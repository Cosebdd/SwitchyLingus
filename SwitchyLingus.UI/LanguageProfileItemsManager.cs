using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SwitchyLingus.Core;
using SwitchyLingus.Core.Extension;
using SwitchyLingus.Core.Model;

namespace SwitchyLingus.UI
{
    internal class LanguageProfileItemsManager
    {
        public ObservableCollection<ContextMenuItem> ProfileItems { get; set; }

        public LanguageProfileItemsManager()
        {
            ProfileItems = new ObservableCollection<ContextMenuItem>();
        }

        public void CreateLangProfileContextMenuItem(LanguageProfile profile, string? currentLanguageProfileName = null)
        {
            var item = new ContextMenuItem()
            {
                Name = profile.Name,
                IsChecked = currentLanguageProfileName?.Equals(profile.Name) ?? false,
                IsImmutable = profile.IsMainProfile
            };

            item.ItemCommand = SetProfileCommand(profile, item);
            ProfileItems.Add(item);
        }

        public void UpdateLangProfileContextMenuItem(ContextMenuItem item, LanguageProfile profile)
        {
            item.Name = profile.Name;
            item.ItemCommand = SetProfileCommand(profile, item);
        }

        private ICommand SetProfileCommand(LanguageProfile languageProfile, ContextMenuItem item)
        {
            return new BasicCommand(() =>
                {
                    var previouslyChecked = ProfileItems.FirstOrDefault(i => i.IsChecked);
                    ProfileItems.ForEach(i => i.IsChecked = false);
                    item.IsChecked = true;
                    Task.Run(() =>
                    {
                        try
                        {
                            LanguageProfileSetter.SetProfile(languageProfile);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                item.IsChecked = false;
                                if (previouslyChecked != null)
                                    previouslyChecked.IsChecked = true;
                            });
                        }
                    });
                }
            );
        }
    }
}