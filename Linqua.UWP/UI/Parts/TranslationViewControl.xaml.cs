using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using JetBrains.Annotations;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua.UI
{
    public sealed partial class TranslationViewControl : UserControl, INotifyPropertyChanged
    {
        public TranslationViewControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        public EntryViewModel ViewModel => (EntryViewModel)DataContext;

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            OnPropertyChanged(nameof(ViewModel));
        }

        #region PropertyChanged Event

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}