using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity
{
   public class ImplicitProfileService: IProfileService
   {
      public Task GetProfileDataAsync(ProfileDataRequestContext context) {
         //获取IdentityServer给我们定义的Cliams和我们在SignAsync添加的Claims
         

         // 在自定义验证：ResourceOwnerValidator中填写的claims的内容
         var claims = context.Subject.Claims.ToList();

         // 设置返回的claims的值
         context.IssuedClaims = claims.ToList();

         return Task.CompletedTask;
      }
      public Task IsActiveAsync(IsActiveContext context) {
         context.IsActive = true;
         return Task.CompletedTask;
      }
   }

}
