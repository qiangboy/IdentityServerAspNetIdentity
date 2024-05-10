using Application;
using Domain.Identities;
using IdentityApi;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //options.SwaggerDoc("IdentityApi", new OpenApiInfo
    //{
    //    Title = "IdentityApi",
    //    Version = "V1",
    //    Description = "IdentityApi"
    //});

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                TokenUrl = new Uri("https://localhost:5001/connect/token"),
                Scopes = new Dictionary<string, string>()
                {
                    {"openid", "openid"},
                    {"profile", "profile"},
                    {"identity.api", "identity.api"}
                }
            }
        }
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

var connString = builder.Configuration.GetConnectionString("Ids4");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddTransient<UserService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

// 覆盖identity框架的认证配置
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://localhost:5001";
        options.RequireHttpsMetadata = true;
        options.Audience = "identity.api";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "name",
            RoleClaimType = "role",
            //ClockSkew = TimeSpan.FromSeconds(100), //每隔n秒验证一次token，默认300秒
            RequireExpirationTime = true, // token必须有超时时间
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("identity.api");
        options.OAuthClientSecret("123456");
        options.OAuthScopes("openid", "profile", "identity.api");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
