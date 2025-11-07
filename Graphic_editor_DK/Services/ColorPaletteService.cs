using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Graphic_editor_DK.Services
{
    public class ColorPaletteService : INotifyPropertyChanged
    {
        private Color _selectedStrokeColor = Colors.Black;
        private Color _selectedFillColor = Colors.Transparent;

        public ObservableCollection<Color> RecentColors { get; } = new ObservableCollection<Color>();

        public Color SelectedStrokeColor
        {
            get => _selectedStrokeColor;
            set
            {
                _selectedStrokeColor = value;
                OnPropertyChanged();
                AddRecentColor(value);
            }
        }

        public Color SelectedFillColor
        {
            get => _selectedFillColor;
            set
            {
                _selectedFillColor = value;
                OnPropertyChanged();
                AddRecentColor(value);
            }
        }

        public void AddRecentColor(Color color)
        {
            if (!RecentColors.Contains(color))
            {
                RecentColors.Insert(0, color);
                if (RecentColors.Count > 10)
                    RecentColors.RemoveAt(RecentColors.Count - 1);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}