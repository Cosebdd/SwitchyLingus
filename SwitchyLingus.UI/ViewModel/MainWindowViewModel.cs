using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SwitchyLingus.Core.Config;
using SwitchyLingus.Core.Extension;

namespace SwitchyLingus.UI.ViewModel
{
    public class MainWindowViewModel
    {
        private readonly Lock _lockObject = new Lock();

        private readonly AppConfig _config = AppConfig.CurrentConfig;
        private readonly LanguageProfileItemsManager _itemsManager;
        private Options? _openedOptionsDialog;

        public MainWindowViewModel()
        {
            _itemsManager = new LanguageProfileItemsManager();
            CreateProfileMenuItems();
            OptionsCommand = ShowOptionsDialogCommand();
            ExitCommand = ExitApplicationCommand();
            _openedOptionsDialog = null;
        }

        private ICommand ExitApplicationCommand()
        {
            return new BasicCommand(() => { Application.Current.Shutdown(); });
        }

        public ObservableCollection<ContextMenuItem> ProfileItems => _itemsManager.ProfileItems;

        public ICommand OptionsCommand { get; }
        public ICommand ExitCommand { get; }

        private ICommand ShowOptionsDialogCommand()
        {
            return new BasicCommand(() =>
            {
                lock (_lockObject)
                {
                    if (_openedOptionsDialog == null)
                    {
                        _openedOptionsDialog = new Options();
                        _openedOptionsDialog.DataContext = new OptionsViewModel(_itemsManager, _openedOptionsDialog);
                        _openedOptionsDialog.Closed += (sender, args) => { _openedOptionsDialog = null; };
                        _openedOptionsDialog.Show();
                    }
                    else
                    {
                        _openedOptionsDialog.Activate();
                    }
                }
            });
        }

        private void CreateProfileMenuItems()
        {
            var installedProfileName = AppConfig.CurrentConfig.GetInstalledLanguageProfileName();
            
            _config.LanguageProfiles.Values
                .ForEach(p => _itemsManager.CreateLangProfileContextMenuItem(p, installedProfileName));
        }
    }
}