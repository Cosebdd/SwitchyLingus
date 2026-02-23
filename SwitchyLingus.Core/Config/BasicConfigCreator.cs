using System.Runtime.Versioning;
using SwitchyLingus.Core.Model;

namespace SwitchyLingus.Core.Config
{
    [SupportedOSPlatform("windows")]
    internal static class BasicConfigCreator
    {
        public static InternalAppConfig CreateBasicConfig(string name)
        {
            var langProfile = LanguageProfileGetter.GetMainProfileFromPowershell(name, out var type);
            VerifyThat.IsNotNull(type.AssemblyQualifiedName);
            var languageProfiles = new Dictionary<string, LanguageProfile>(){{langProfile.Name, langProfile}};

            return new InternalAppConfig()
            {
                LanguageProfiles = languageProfiles,
                WinUserLanguageType = type.AssemblyQualifiedName,
            };
        }
    }
}