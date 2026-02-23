using System.Collections;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using SwitchyLingus.Core.Config;
using SwitchyLingus.Core.Model;

namespace SwitchyLingus.Core
{
    [SupportedOSPlatform("windows")]
    public static class LanguageProfileSetter
    {
        public static void SetProfile(LanguageProfile profile)
        {
            using var psRunspace = RunspaceFactory.CreateRunspace();
            psRunspace.Open();
            using var psPipeline = psRunspace.CreatePipeline();

            var command = new Command("Set-WinUserLanguageList");

            var langList = GetLanguageList(profile);

            psPipeline.Commands.Add(command);
            command.Parameters.Add("LanguageList", langList);
            command.Parameters.Add("Force", true);

            psPipeline.Invoke();
        }

        private static IList GetLanguageList(LanguageProfile profile)
        {
            var langType = Type.GetType(AppConfig.CurrentConfig.InternalAppConfig.WinUserLanguageType);

            VerifyThat.IsNotNull(langType);

            return profile.Languages.Select(CreateLanguage).ToList();

            dynamic CreateLanguage(Language language)
            {
                dynamic resultLang = Activator.CreateInstance(langType, language.Tag) 
                                     ?? throw new Exception($"Failed to create an instance of {langType}");
                resultLang.InputMethodTips.Clear();
                resultLang.InputMethodTips.AddRange(language.InputMethods);
                resultLang.Spellchecking = language.Spellchecking;
                resultLang.Handwriting = language.Handwriting;
                return resultLang;
            }
        }
    }
}