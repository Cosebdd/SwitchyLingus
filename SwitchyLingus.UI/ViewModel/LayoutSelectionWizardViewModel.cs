using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SwitchyLingus.Core;
using SwitchyLingus.Core.Extension;
using SwitchyLingus.Core.Model;
using SwitchyLingus.UI.Properties;

namespace SwitchyLingus.UI.ViewModel
{
    internal sealed class LayoutSelectionWizardViewModel : INotifyPropertyChanged
    {
        private readonly IReadOnlyCollection<ContextMenuItem> _existingProfiles;
        private readonly IReadOnlyDictionary<string, KeyboardLayoutInfo> _allLayouts;
        private readonly string _editedProfileName = string.Empty;
        private string _profileName = string.Empty;
        private string _searchText = string.Empty;
        private bool _canSave;
        private KeyboardLayoutInfo? _selectedAvailableLayout;
        private KeyboardLayoutInfo? _selectedChosenLayout;

        public LayoutSelectionWizardViewModel(IReadOnlyCollection<ContextMenuItem> existingProfiles)
        {
            _existingProfiles = existingProfiles;
            _allLayouts = KeyboardLayoutEnumerator.AvailableLayouts;

            AvailableLayouts = new ObservableCollection<KeyboardLayoutInfo>(_allLayouts.Values);
            SelectedLayouts = new ObservableCollection<KeyboardLayoutInfo>();

            AddSelectedLayoutsCommand = new BasicCommand(AddSelectedLayout, () => _selectedAvailableLayout != null);
            RemoveSelectedLayoutsCommand = new BasicCommand(RemoveSelectedLayout, () => _selectedChosenLayout != null);
        }

        public LayoutSelectionWizardViewModel(IReadOnlyCollection<ContextMenuItem> existingProfiles, LanguageProfile profile)
            : this(existingProfiles)
        {
            _profileName = profile.Name;
            _editedProfileName = profile.Name;

            profile
                .Languages
                .SelectMany(l => l.InputMethods)
                .Select(l => _allLayouts[l])
                .ForEach(MoveLayoutToSelected);

            UpdateCanSave();
        }

        public string ProfileName
        {
            get => _profileName;
            set
            {
                _profileName = value;
                OnPropertyChanged();
                UpdateCanSave();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterAvailableLayouts();
            }
        }

        public ObservableCollection<KeyboardLayoutInfo> AvailableLayouts { get; }

        public ObservableCollection<KeyboardLayoutInfo> SelectedLayouts { get; }

        public KeyboardLayoutInfo? SelectedAvailableLayout
        {
            get => _selectedAvailableLayout;
            set
            {
                _selectedAvailableLayout = value;
                OnPropertyChanged();
                AddSelectedLayoutsCommand.RaiseCanExecuteChanged();
            }
        }

        public KeyboardLayoutInfo? SelectedChosenLayout
        {
            get => _selectedChosenLayout;
            set
            {
                _selectedChosenLayout = value;
                OnPropertyChanged();
                RemoveSelectedLayoutsCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanSave
        {
            get => _canSave;
            private set
            {
                _canSave = value;
                OnPropertyChanged();
            }
        }

        public BasicCommand AddSelectedLayoutsCommand { get; }

        public BasicCommand RemoveSelectedLayoutsCommand { get; }

        private void AddSelectedLayout()
        {
            if (_selectedAvailableLayout == null) return;

            var layout = _selectedAvailableLayout;
            MoveLayoutToSelected(layout);
        }

        private void MoveLayoutToSelected(KeyboardLayoutInfo layout)
        {
            AvailableLayouts.Remove(layout);
            SelectedLayouts.Add(layout);
            UpdateCanSave();
        }

        private void RemoveSelectedLayout()
        {
            if (_selectedChosenLayout == null) return;

            var layout = _selectedChosenLayout;
            SelectedLayouts.Remove(layout);
            FilterAvailableLayouts();

            UpdateCanSave();
        }

        private void FilterAvailableLayouts()
        {
            AvailableLayouts.Clear();

            AvailableLayouts.AddRange(
                _allLayouts.Values
                .Except(SelectedLayouts)
                .Where(MatchesFilter));
        }

        private bool MatchesFilter(KeyboardLayoutInfo layout)
        {
            if (string.IsNullOrWhiteSpace(_searchText)) return true;

            var search = _searchText.ToLowerInvariant();
            return layout.DisplayName.ToLowerInvariant().Contains(search) ||
                   layout.LanguageTag.ToLowerInvariant().Contains(search);
        }

        private void UpdateCanSave()
        {
            CanSave = GetCanSave();
        }

        private bool GetCanSave()
        {
            return !string.IsNullOrWhiteSpace(_profileName) && ProfileNameIsUnique() && SelectedLayouts.Count > 0;

            bool ProfileNameIsUnique()
            {
                return _existingProfiles
                    .Where(p => p.Name != _editedProfileName)
                    .Select(p => p.Name)
                    .WhereNotNull()
                    .All(name => !name.Equals(_profileName));
            }
        }

        public LanguageProfile BuildProfile()
        {
            var languages = SelectedLayouts
                .GroupBy(l => l.LanguageTag)
                .Select(g => 
                    new Language(g.Key, g
                        .Select(l => l.InputMethodTip)
                        .ToArray())
                );

            return new LanguageProfile()
            {
                Name = ProfileName,
                Languages = languages.ToList()
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
