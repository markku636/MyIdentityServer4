using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServerAspNetIdentity
{
   public class ProfileService: IProfileService
   {
      /// <summary>
      /// 获取用户的Claims
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      public Task GetProfileDataAsync(ProfileDataRequestContext context) {
         // 添加claim
         context.IssuedClaims = new List<Claim> {
            new Claim("xType","xValue")
        };
         return Task.CompletedTask;
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