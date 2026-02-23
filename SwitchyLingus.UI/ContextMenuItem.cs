using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SwitchyLingus.UI
{
    public sealed class ContextMenuItem : INotifyPropertyChanged
    {
        private string? _name;
        
        private bool _isChecked;
        private bool _isImmutable;


        public string? Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsImmutable
        {
            get => _isImmutable;
            set
            {
                _isImmutable = value;
                OnPropertyChanged();
            }
        }

        public ICommand? ItemCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}