using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;

namespace ModernSlavery.IdentityServer4.Classes
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {

        private readonly IUserRepository _userRepository;

        public CustomResourceOwnerPasswordValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            User user = await _userRepository.FindByEmailAsync(context.UserName);

            if (user != null && await _userRepository.CheckPasswordAsync(user, context.Password))
            {
                context.Result = new GrantValidationResult(user.UserId.ToString(), OidcConstants.AuthenticationMethods.Password);
            }
        }

    }
}
