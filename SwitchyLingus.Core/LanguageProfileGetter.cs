using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using SwitchyLingus.Core.Model;
using SwitchyLingus.Core.Unsafe;

namespace SwitchyLingus.Core
{
    [SupportedOSPlatform("windows")]
    public static class LanguageProfileGetter
    {
        internal static LanguageProfile GetMainProfileFromPowershell(string name, out Type winUserLanguageType)
        {
            using var psRunspace = RunspaceFactory.CreateRunspace();
            psRunspace.Open();
            using var psPipeline = psRunspace.CreatePipeline();
            var languages = new List<Language>();

            var command = new Command("Get-WinUserLanguageList");

            psPipeline.Commands.Add(command);

            var results = psPipeline.Invoke();
            dynamic member = results.First().BaseObject;

            winUserLanguageType = member[0].GetType();

            foreach (var mem in member)
            {
                languages.Add(
                    new Language(mem.LanguageTag, mem.InputMethodTips.ToArray()
                    ));
            }

            var languageProfile = new LanguageProfile
            {
                Languages = languages,
                Name = name,
                IsMainProfile = true
            };
            return languageProfile;
        }
        
        internal static IEnumerable<Language> ListInstalledLanguages()
        {
            var keyboards = ListInstalledKeyboards();
            return keyboards.GroupBy(k => k.LanguageTag)
                .Select(g =>
                    new Language(
                        g.Key,
                        g.Select(l => l.InputMethodTip).ToArray())
                );
        }

        private static IEnumerable<KeyboardLayoutInfo> ListInstalledKeyboards()
        {
            var availableLanguages = InputMethod.GetInstalledInputMethods();
            var allKeyboards = KeyboardLayoutEnumerator.AvailableLayouts;

            foreach (var availableLanguage in availableLanguages)
            {
                var tip = availableLanguage.GetInputMethodTip();
                if (tip is not null && allKeyboards.TryGetValue(tip, out var keyboard))
                    yield return keyboard;
            }
        }
    }
}