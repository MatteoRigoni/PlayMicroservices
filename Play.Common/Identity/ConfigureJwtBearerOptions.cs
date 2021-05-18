using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Play.Catalog.Common.Settings;

namespace Play.Common.Identity
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {

        private readonly IConfiguration _configuration;

        public ConfigureJwtBearerOptions(IConfiguration configuration)
        {
            _configuration = configuration;   
        }
        public void Configure(string name, JwtBearerOptions options)
        {
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                var serviceSettings = _configuration.GetSection(nameof(ServiceSettings))
                    .Get<ServiceSettings>();

                 options.Authority = serviceSettings.Authority;
                 options.Audience = serviceSettings.ServiceName;
                 options.MapInboundClaims = false;
                 options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters(){
                     NameClaimType = "name",
                     RoleClaimType = "role"
                 };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }
}