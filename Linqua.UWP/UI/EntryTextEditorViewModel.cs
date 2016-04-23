using System;
using System.Reactive.Linq;
using Framework;
using Framework.PlatformServices;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua.UI
{
    public class EntryTextEditorViewModel : ViewModelBase
    {
        private readonly IEventAggregator eventAggregator;
        private string entryText;
        private bool isBusy;
        private ClientEntry data;

        public EntryTextEditorViewModel()
            : this(DesignTimeDetection.IsInDesignTool ? DesignTimeHelper.EventAggregator : null)
        {
        }

        public EntryTextEditorViewModel(IEventAggregator eventAggregator)
        {
            Guard.NotNull(eventAggregator, nameof(eventAggregator));

            this.eventAggregator = eventAggregator;
            FinishCommand = new DelegateCommand(Finish, CanFinish);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public DelegateCommand FinishCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public ClientEntry Data
        {
            get { return data; }
            set
            {
                if (Equals(value, data)) return;
                data = value;

                if (Data != null)
                {
                    EntryText = Data.Text;
                }
                else
                {
                    EntryText = null;
                }

                IsBusy = false;

                RaisePropertyChanged();
            }
        }

        public string EntryText
        {
            get { return entryText; }
            set
            {
                if (value == entryText) return;
                entryText = value;
                RaisePropertyChanged();
                FinishCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (value.Equals(isBusy)) return;
                isBusy = value;
                RaisePropertyChanged();
                FinishCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanFinish()
        {
            return !string.IsNullOrEmpty(EntryText) && !IsBusy;
        }

        private void Cancel()
        {
            EntryText = string.Empty;

            eventAggregator.Publish(new EntryEditingCancelledEvent());
        }

        private void Finish()
        {
            IsBusy = true;

            if (data == null)
            {
                data = ClientEntry.CreateNew(EntryText);
            }
            else
            {
                Data.Text = EntryText;
            }

            // Schedule publishing the event after a small timeout to allow the layout to update as a result of 
            // hiding the on-screen keyboard when user presses "Save".
            Observable.Timer(TimeSpan.FromSeconds(0.2)).ObserveOnDispatcher().Subscribe(_ => { eventAggregator.Publish(new EntryEditingFinishedEvent(Data)); });
        }

        public void Clear()
        {
            Data = null;
        }
    }
}