using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Hosts.Web
{

    [Options("IdentityClient")]
    public class IdentityClientOptions : IOptions
    {
        public string AuthorityUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SignOutUri { get; set; }
    }
}
