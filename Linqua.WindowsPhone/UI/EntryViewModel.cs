using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Service.Models;

namespace Linqua.UI
{
	public class EntryViewModel : ViewModelBase
	{
		private readonly IEventAggregator eventAggregator;
		private bool isTranslating;
		private ClientEntry entry;

		protected EntryViewModel([NotNull] IEventAggregator eventAggregator)
		{
			Guard.NotNull(eventAggregator, nameof(eventAggregator));

			this.eventAggregator = eventAggregator;

			eventAggregator.GetEvent<EntryUpdatedEvent>()
			               .Where(x => x.Entry.Id == Entry.Id)
						   .ObserveOnDispatcher()
			               .SubscribeWeakly(this, (this_, e) => this_.OnEntryUpdated(e));

			DeleteCommand = new DelegateCommand(() => DeleteSelfAsync().FireAndForget(), CanDeleteSelf);
			QuickEditCommand = new DelegateCommand(() => QuickEditSelfAsync().FireAndForget(), CanQuickEditSelf);
			EditCommand = new DelegateCommand(() => EditSelfAsync().FireAndForget(), CanEditSelf);
		}

		public EntryViewModel(ClientEntry entry, [NotNull] IEventAggregator eventAggregator)
			: this(eventAggregator)
		{
			Guard.NotNull(entry, nameof(entry));

			Entry = entry;
		}


		public DelegateCommand DeleteCommand { get; private set; }
		public DelegateCommand QuickEditCommand { get; private set; }
		public DelegateCommand EditCommand { get; private set; }

		private IEntryOperations EntryOperations
		{
			get { return CompositionFactory.Create<IEntryOperations>(); }
		}

		public ClientEntry Entry
		{
			get { return entry; }
			protected set
			{
				if (Equals(value, entry)) return;
				entry = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(Text));
				OnTextChangedOverride();
				RaisePropertyChanged(nameof(DateAdded));
				RaisePropertyChanged(nameof(IsLearnt));
				RaisePropertyChanged(nameof(IsLearnStatusText));
				RaisePropertyChanged(nameof(Definition));
				RaisePropertyChanged(nameof(IsDefinitionVisible));

				DeleteCommand.RaiseCanExecuteChanged();
				QuickEditCommand.RaiseCanExecuteChanged();
				EditCommand.RaiseCanExecuteChanged();

				OnIsTranslatingChangedOverride();
				OnEntryChangedOverride();
			}
		}

		public string Text
		{
			get { return Entry != null ? Entry.Text : string.Empty; }
			set
			{
				if (Equals(value, Text))
				{
					return;
				}

				Guard.Assert(Entry != null, "Entry != null");

				Entry.Text = value;
				RaisePropertyChanged();
				OnTextChangedOverride();
			}
		}

	    public string LanguageCode
	    {
	        get { return Entry != null ? Entry.TextLanguageCode : string.Empty; }
	    }

        public DateTime DateAdded
		{
			get { return Entry != null ? Entry.ClientCreatedAt.LocalDateTime : DateTime.MinValue; }
		}

		public bool IsLearnt
		{
			get { return Entry != null && Entry.IsLearnt; }
			set
			{
				if (Equals(IsLearnt, value))
				{
					return;
				}

				Guard.Assert(Entry != null, "Entry != null");

				SetIsLearnt(value);
			}
		}

		public string IsLearnStatusText
		{
			get
			{
				var result = IsLearnt ? Resources.GetString("EntryViewModel_LearntStatus") : Resources.GetString("EntryViewModel_NotLearntStatus");

				return result;
			}
		}

		public bool IsTranslating
		{
			get { return isTranslating; }
			set
			{
				if (value.Equals(isTranslating)) return;
				isTranslating = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(IsDefinitionVisible));
                RaisePropertyChanged(nameof(LanguageCode));

				OnIsTranslatingChangedOverride();
			}
		}

		public string Definition
		{
			get { return Entry != null ? Entry.Definition : string.Empty; }
			set
			{
				if (Equals(Definition, value))
				{
					return;
				}

				Guard.Assert(Entry != null, "Entry != null");

				Entry.Definition = value;
                Entry.TranslationState = TranslationState.Manual;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(IsDefinitionVisible));
                RaisePropertyChanged(nameof(LanguageCode));
			}
		}

		public bool IsDefinitionVisible
		{
			get { return !string.IsNullOrEmpty(Definition) || IsTranslating; }
		}

	    public string NoDefinitionText
	    {
	        get
	        {
                return Resources.GetString("NoTraslationText");
            }
	    }

        private bool CanDeleteSelf()
		{
			return Entry != null;
		}

		private async Task DeleteSelfAsync()
		{
			Guard.Assert(Entry != null, "Entry != null");

			var confirmed = await DialogService.ShowConfirmation(
				Resources.GetString("EntryViewModel_DeleteConfirmationTitle"),
				string.Format(Resources.GetString("EntryViewModel_DeleteConfirmationTextTemplate"), Text),
				okCommandText: Resources.GetString("EntryViewModel_DeleteConfirmationOkButtonText"));

			if (confirmed)
			{
				await EntryOperations.DeleteEntryAsync(this);

				OnDeleted();
			}
		}

		public async Task UnlearnAsync()
		{
			await UpdateIsLearntAsync(false, showConfirmation: false);
		}

		protected virtual void SetIsLearnt(bool value)
		{
			UpdateIsLearntAsync(value).FireAndForget();
		}

		private async Task UpdateIsLearntAsync(bool value, bool showConfirmation = true)
		{
			bool confirmed = !showConfirmation;

			if (showConfirmation)
			{
				if (value)
				{
					confirmed = await DialogService.ShowConfirmation(
						Resources.GetString("EntryViewModel_MarkLearnedConfirmationTitle"),
						Resources.GetString("EntryViewModel_MarkLearnedConfirmationText"),
						okCommandText: Resources.GetString("EntryViewModel_MarkLearnedConfirmationOk"));
				}
				else
				{
					confirmed = await DialogService.ShowConfirmation(
						Resources.GetString("EntryViewModel_UnMarkLearnedConfirmationTitle"),
						Resources.GetString("EntryViewModel_UnMarkLearnedConfirmationText"),
						okCommandText: Resources.GetString("EntryViewModel_UnMarkLearnedConfirmationOk"));
				}
			}

			if (confirmed)
			{
				Entry.IsLearnt = value;

				await EntryOperations.UpdateEntryIsLearnedAsync(this);
			}

			RaisePropertyChanged(nameof(IsLearnt));
			RaisePropertyChanged(nameof(IsLearnStatusText));
		}

		protected virtual void OnDeleted()
		{
		}

		protected virtual void OnEntryChangedOverride()
		{

		}

		private void OnEntryUpdated(EntryUpdatedEvent e)
		{
			Entry = e.Entry;

			RaisePropertyChanged("");
		}

		private bool CanQuickEditSelf()
		{
			return Entry != null;
		}

		private Task QuickEditSelfAsync()
		{
			Guard.Assert(Entry != null, "Entry != null");

			EventAggregator.Publish(new EntryQuickEditRequestedEvent(this));

			return Task.FromResult(true);
		}

		private bool CanEditSelf()
		{
			return Entry != null;
		}

		private Task EditSelfAsync()
		{
			Guard.Assert(Entry != null, "Entry != null");

			EventAggregator.Publish(new EntryEditRequestedEvent(Entry.Id));

			return Task.FromResult(true);
		}

		protected virtual void OnIsTranslatingChangedOverride()
		{
		}

		protected virtual void OnTextChangedOverride()
		{
		}
	}
}