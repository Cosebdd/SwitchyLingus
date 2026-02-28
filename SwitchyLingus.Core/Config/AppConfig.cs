using System.Diagnostics;
using System.Runtime.Versioning;
using SwitchyLingus.Core.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SwitchyLingus.Core.Config
{
    [SupportedOSPlatform("windows")]
    public class AppConfig
    {
        private const string MainProfileName = "MainProfile";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true,
            WriteIndented = true,
        };

        private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SwitchyLingus");

        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

        public IDictionary<string, LanguageProfile> LanguageProfiles => InternalAppConfig.LanguageProfiles;

        public static AppConfig CurrentConfig { get; } = new AppConfig();

        internal InternalAppConfig InternalAppConfig { get; }

        private AppConfig()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);
                
                var jsonConfig = File.ReadAllText(ConfigPath);
                InternalAppConfig = JsonSerializer.Deserialize<InternalAppConfig>(jsonConfig, SerializerOptions) ??
                                    throw new Exception("Unable to deserialize config file.");

                if (InternalAppConfig.LanguageProfiles == null
                    || InternalAppConfig.LanguageProfiles.Count == 0
                    || string.IsNullOrEmpty(InternalAppConfig.WinUserLanguageType))
                    throw new Exception("Invalid config file");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Recreating basic config file");
                var basicConfig = BasicConfigCreator.CreateBasicConfig(MainProfileName);
                InternalAppConfig = basicConfig;
                SaveConfig();
            }
        }

        public string? GetInstalledLanguageProfileName()
        {
            var installedKeyboards = LanguageProfileGetter
                .ListInstalledLanguages()
                .SelectMany(l => l.InputMethods)
                .ToHashSet();

            var keyboardsByProfile = LanguageProfiles
                .Select(p => new
                {
                    Name = p.Key,
                    InputMethodTips = p.Value.Languages.SelectMany(l => l.InputMethods).ToHashSet()
                });

            return keyboardsByProfile.FirstOrDefault(g => installedKeyboards.SetEquals(g.InputMethodTips))?.Name;
        }

        public void AddProfile(LanguageProfile profile)
        {
            InternalAppConfig.LanguageProfiles.Add(profile.Name, profile);
            SaveConfig();
        }

        public void RemoveProfile(string profileName)
        {
            ValidateProfile(profileName);

            InternalAppConfig.LanguageProfiles.Remove(profileName);

            SaveConfig();
        }

        public void UpdateProfile(string oldName, LanguageProfile updatedProfile)
        {
            ValidateProfile(oldName);

            if (oldName != updatedProfile.Name)
                InternalAppConfig.LanguageProfiles.Remove(oldName);

            InternalAppConfig.LanguageProfiles[updatedProfile.Name] = updatedProfile;
            SaveConfig();
        }

        public LanguageProfile RecreateMainProfile()
        {
            var newProfile = LanguageProfileGetter.GetMainProfileFromPowershell(MainProfileName, out _);

            InternalAppConfig.LanguageProfiles[MainProfileName] = newProfile;
            SaveConfig();
            return newProfile;
        }

        private void ValidateProfile(string profileName)
        {
            if (!InternalAppConfig.LanguageProfiles.TryGetValue(profileName, out var profile))
                throw new Exception($"Profile {profileName} does not exist");

            VerifyThat.IsNot(profile.IsMainProfile, "Main profile can't be removed or updated");
        }

        private void SaveConfig()
        {
            var jsonConfig = JsonSerializer.Serialize(InternalAppConfig, SerializerOptions);
            File.WriteAllText(ConfigPath, jsonConfig);
        }
    }
}