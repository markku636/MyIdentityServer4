using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServerAspNetIdentity
{
public class CustomerProfileService : IProfileService
{
    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        //获取IdentityServer给我们定义的Cliams和我们在SignAsync添加的Claims
        var claims = context.Subject.Claims.ToList();

        context.IssuedClaims = claims;

        return Task.CompletedTask;
    }
    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}

}
