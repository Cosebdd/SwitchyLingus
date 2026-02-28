using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SwitchyLingus.Core;
using SwitchyLingus.Core.Config;
using SwitchyLingus.UI.Properties;

namespace SwitchyLingus.UI.ViewModel
{
    internal class OptionsViewModel : INotifyPropertyChanged
    {
        private readonly Options _optionsDialog;
        private readonly LanguageProfileItemsManager _itemsManager;
        private ContextMenuItem? _selectedItem;
        private bool _runOnStartup;

        public OptionsViewModel(LanguageProfileItemsManager itemsManager, Options optionsDialog)
        {
            _itemsManager = itemsManager;
            _optionsDialog = optionsDialog;
            
            _runOnStartup = StartupManager.IsRunOnStartup();

            CreateNewProfileCommand = CreateNewProfile();
            EditProfileCommand = EditProfile();
            RemoveProfileCommand = RemoveProfile();
            RecreateMainProfileCommand = CreateRecreateMainProfileCommand();
        }

        public ContextMenuItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                EditProfileCommand.RaiseCanExecuteChanged();
                RemoveProfileCommand.RaiseCanExecuteChanged();
                OnPropertyChanged();
            }
        }

        public bool RunOnStartup
        {
            get => _runOnStartup;
            set
            {
                StartupManager.SetRunOnStartup(value);
                _runOnStartup = StartupManager.IsRunOnStartup();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ContextMenuItem> ProfileItems => _itemsManager.ProfileItems;

        public ICommand CreateNewProfileCommand { get; }

        public BasicCommand EditProfileCommand { get; }

        public BasicCommand RemoveProfileCommand { get; }

        public ICommand RecreateMainProfileCommand { get; }

        private BasicCommand CreateNewProfile()
        {
            return new BasicCommand(() =>
            {
                var vm = new LayoutSelectionWizardViewModel(ProfileItems);
                var wizard = new LayoutSelectionWizard
                {
                    DataContext = vm,
                    Owner = _optionsDialog,
                    Title = "Create New Profile"
                };
                if (wizard.ShowDialog() != true) return;

                var profile = vm.BuildProfile();
                _itemsManager.CreateLangProfileContextMenuItem(profile);
                AppConfig.CurrentConfig.AddProfile(profile);
            });
        }

        private BasicCommand RemoveProfile()
        {
            return new BasicCommand(() =>
            {
                VerifyThat.IsNotNull(SelectedItem?.Name);
                AppConfig.CurrentConfig.RemoveProfile(SelectedItem.Name);
                _itemsManager.ProfileItems.Remove(SelectedItem);
            }, 
            () => SelectedItem?.Name != null && !SelectedItem.IsImmutable);
        }

        private ICommand CreateRecreateMainProfileCommand()
        {
            return new BasicCommand(() =>
            {
                var dialog = new ConfirmDialog(
                    "Recreate the main profile using your current language settings?",
                    "Recreate main profile")
                {
                    Owner = _optionsDialog
                };
                if (dialog.ShowDialog() != true) return;

                var newProfile = AppConfig.CurrentConfig.RecreateMainProfile();
                var mainItem = _itemsManager.ProfileItems.First(i => i.IsImmutable);
                _itemsManager.UpdateLangProfileContextMenuItem(mainItem, newProfile);
            });
        }

        private BasicCommand EditProfile()
        {
            return new BasicCommand(() =>
                {
                    VerifyThat.IsNotNull(SelectedItem?.Name);
                    var oldName = SelectedItem.Name;
                    var existingProfile = AppConfig.CurrentConfig.LanguageProfiles[oldName];

                    var vm = new LayoutSelectionWizardViewModel(ProfileItems, existingProfile);
                    var wizard = new LayoutSelectionWizard
                    {
                        DataContext = vm,
                        Owner = _optionsDialog,
                        Title = "Edit Profile"
                    };
                    if (wizard.ShowDialog() != true) return;

                    var updatedProfile = vm.BuildProfile();
                    AppConfig.CurrentConfig.UpdateProfile(oldName, updatedProfile);
                    _itemsManager.UpdateLangProfileContextMenuItem(SelectedItem, updatedProfile);
                },
                () => SelectedItem?.Name != null && !SelectedItem.IsImmutable);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}