using Domain.Identities;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using IdentityServer4.Extensions;

namespace IdentityServerAspNetIdentity;

public class ProfileService(ILogger<ProfileService> logger, UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> ClaimsFactory) : DefaultProfileService(logger)
{
    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        context.LogProfileRequest(this.Logger);

        var user = await userManager.FindByIdAsync(context.Subject.GetSubjectId());
        var claims = await ClaimsFactory.CreateAsync(user);

        context.AddRequestedClaims(claims.Claims);

        //context.RequestedClaimTypes = claims.Claims;

        var list = context.Subject.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
        context.AddRequestedClaims(context.Subject.Claims);
        context.LogIssuedClaims(this.Logger);
        


        //context.LogProfileRequest(logger);

        //// 这里也可以查询用户的claims
        //var id = context.Subject.GetSubjectId();

        //var userClaims = await GetUserClaimsAsync(id);

        //if (context.RequestedClaimTypes.Any())
        //{
        //    var filterClaims = context.FilterClaims(userClaims);
        //    context.IssuedClaims.AddRange(filterClaims);
        //}
        //else
        //{
        //    context.IssuedClaims.AddRange(userClaims);
        //}

        //context.IssuedClaims.AddRange(context.Subject.Claims);

        //context.LogIssuedClaims(logger);
    }

    public override async Task IsActiveAsync(IsActiveContext context)
    {
        Logger.LogDebug("IsActive called from: {caller}", context.Caller);

        var user = await userManager.FindByIdAsync(context.Subject.GetSubjectId());

        context.IsActive = user?.LockoutEnabled ?? false;
    }

    private async Task<IEnumerable<Claim>> GetUserClaimsAsync(string id)
    {
        var claims = new List<Claim>();
        var user = await userManager.FindByIdAsync(id) ?? throw new Exception("获取Claim时查找不到用户");
        var roles = await userManager.GetRolesAsync(user);

        if (roles.Any())
        {
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
        }

        claims.Add(new Claim(JwtClaimTypes.Email, user.Email ?? string.Empty));

        return claims;
    }
}