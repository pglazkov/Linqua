﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua.UI
{
	public class EntryViewModel : ViewModelBase
	{
		private readonly IEventAggregator eventAggregator;
		private bool isTranslating;
		private ClientEntry entry;

		protected EntryViewModel([NotNull] IEventAggregator eventAggregator)
		{
			Guard.NotNull(eventAggregator, () => eventAggregator);

			this.eventAggregator = eventAggregator;

			eventAggregator.GetEvent<EntryUpdatedEvent>()
			               .Where(x => x.Entry.Id == Entry.Id)
						   .ObserveOnDispatcher()
			               .SubscribeWeakly(this, (this_, e) => this_.OnEntryUpdated(e));

			DeleteCommand = new DelegateCommand(() => DeleteSelfAsync().FireAndForget());
			QuickEditCommand = new DelegateCommand(() => QuickEditSelfAsync().FireAndForget());
			EditCommand = new DelegateCommand(() => EditSelfAsync().FireAndForget());
		}

		public EntryViewModel(ClientEntry entry, [NotNull] IEventAggregator eventAggregator)
			: this(eventAggregator)
		{
			Guard.NotNull(entry, () => entry);

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
				RaisePropertyChanged(() => Text);
				OnTextChangedOverride();
				RaisePropertyChanged(() => DateAdded);
				RaisePropertyChanged(() => IsLearnt);
				RaisePropertyChanged(() => IsLearnStatusText);
				RaisePropertyChanged(() => Definition);
				RaisePropertyChanged(() => IsDefinitionVisible);
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

		public DateTime DateAdded
		{
			get { return Entry != null ? Entry.CreatedAt.GetValueOrDefault().DateTime : DateTime.MinValue; }
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
				RaisePropertyChanged(() => IsDefinitionVisible);

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
				RaisePropertyChanged();
				RaisePropertyChanged(() => IsDefinitionVisible);
			}
		}

		public bool IsDefinitionVisible
		{
			get { return !string.IsNullOrEmpty(Definition) || IsTranslating; }
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

			RaisePropertyChanged(() => IsLearnt);
			RaisePropertyChanged(() => IsLearnStatusText);
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

		private Task QuickEditSelfAsync()
		{
			Guard.Assert(Entry != null, "Entry != null");

			EventAggregator.Publish(new EntryQuickEditRequestedEvent(this));

			return Task.FromResult(true);
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