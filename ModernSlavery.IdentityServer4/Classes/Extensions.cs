using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Data;

namespace ModernSlavery.IdentityServer4.Classes
{
    public static class CustomIdentityServerBuilderExtensions
    {

        public static IIdentityServerBuilder AddCustomUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.AddProfileService<CustomProfileService>();
            builder.AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();

            return builder;
        }

    }
}
