using SwitchyLingus.Core.Model;

namespace SwitchyLingus.Core.Config
{
    internal class InternalAppConfig
    {
        public required IDictionary<string, LanguageProfile> LanguageProfiles { get; init; }
        public required string WinUserLanguageType { get; init; }
    }
}