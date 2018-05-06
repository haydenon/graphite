using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration
{
    public static class SettingsUserSecrets
    {
        public static IConfigurationBuilder AddUserSecretsToSettings(this IConfigurationBuilder settings, string id)
        {
            return settings.AddUserSecrets(id);
        }
    }
}
