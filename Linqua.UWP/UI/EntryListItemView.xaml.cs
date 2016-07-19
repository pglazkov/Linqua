using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Framework;
using JetBrains.Annotations;

namespace Linqua.UI
{
    public partial class EntryListItemView : UserControl, IEntryListItemView, INotifyPropertyChanged
    {
        public EntryListItemView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public EntryListItemViewModel ViewModel => (EntryListItemViewModel)DataContext;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            OnPropertyChanged(nameof(ViewModel));

            var newVm = (EntryListItemViewModel)args.NewValue;

            if (newVm != null)
            {
                newVm.View = this;
            }
        }

        public void Focus()
        {
            var ownerItemsControl = this.GetFirstAncestorOfType<ItemsControl>();

            if (ownerItemsControl != null && ViewModel != null)
            {
                var listView = ownerItemsControl as ListView;

                Control listItemContainer = null;

                if (listView != null)
                {
                    listItemContainer = listView.ContainerFromItem(ViewModel) as Control;
                }
                else if (ownerItemsControl.ItemContainerGenerator != null)
                {
                    listItemContainer = ownerItemsControl.ContainerFromItem(ViewModel) as Control;
                }

                if (listItemContainer != null)
                {
                    listItemContainer.Focus(FocusState.Programmatic);
                }
            }
        }

        #region PropertyChanged Event

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}