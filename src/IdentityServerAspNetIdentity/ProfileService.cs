using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity
{
    public class ProfileService: IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        /// <summary>
        /// 获取用户的Claims
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context) {
            // 添加claim

            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList(); //  default

            var customCliams = context.Subject.Claims.ToList();

            List<string> requestedClaimTypes = new List<string>() { "email", "name",  "image" };

            //var newClaims = customCliams.Where(type => customCliams.Contains(type)).ToList();

            var  newClaims = customCliams.Where(claim => requestedClaimTypes.Contains(claim.Type)).ToList();

            // Add custom claims in token here based on user properties or any other source
            //claims.Add(new Claim("employee_id", user.EmployeeId ?? string.Empty));

            context.IssuedClaims.AddRange(newClaims);

         //context.IssuedClaims.Add(new Claim("image", "xValue"));

         //return Task.CompletedTask;
      }

        /// <summary>
        /// identity server需要确定用户是否有效
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context) {
            // 这里写你的逻辑，IsActive表示通过还是不通过，true表示通过
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}