﻿using System;
using System.Composition;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.Persistence;
using Linqua.Translation;

namespace Linqua.UI
{
	public class EntryEditViewModel : EntryViewModel
	{
		private readonly IDataStore storage;
		private readonly IStatusBusyService statusBusyService;
		private readonly IEntryOperations entryOperations;
		private Lazy<ITranslationService> translator;
		private bool isLoadingData;

		public EntryEditViewModel(IEventAggregator eventAggregator)
			: base(eventAggregator)
		{
			TranslateCommand = new DelegateCommand(() => TranslateAsync().FireAndForget(), CanTranslate);
			SaveCommand = new DelegateCommand(() => SaveAsync().FireAndForget(), CanSave);
			CancelCommand = new DelegateCommand(Cancel);
		}

		[ImportingConstructor]
		public EntryEditViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			IEntryOperations entryOperations,
			Lazy<ITranslationService> translator)
			: this(eventAggregator)
		{
			Guard.NotNull(compositionFactory, () => compositionFactory);
			Guard.NotNull(storage, () => storage);
			Guard.NotNull(eventAggregator, () => eventAggregator);
			Guard.NotNull(statusBusyService, () => statusBusyService);
			Guard.NotNull(translator, () => translator);
			Guard.NotNull(entryOperations, () => entryOperations);

			this.storage = storage;
			this.statusBusyService = statusBusyService;
			this.entryOperations = entryOperations;
			this.translator = translator;

			EventAggregator = eventAggregator;
			CompositionFactory = compositionFactory;
		}

		public DelegateCommand TranslateCommand { get; private set; }
		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }

		public IEntryEditView View { get; set; }

		public bool IsLoadingData
		{
			get { return isLoadingData; }
			private set
			{
				if (value == isLoadingData) return;
				isLoadingData = value;
				RaisePropertyChanged();
			}
		}

		public async Task InitializeAsync([NotNull] string entryId)
		{
			Guard.NotNullOrEmpty(entryId, () => entryId);

			using (statusBusyService.Busy())
			{
				IsLoadingData = true;

				try
				{
					await storage.InitializeAsync();

					Entry = await storage.LookupById(entryId);
				}
				finally
				{
					IsLoadingData = false;
				}
			}
		}

		protected override void SetIsLearnt(bool value)
		{
			Entry.IsLearnt = value;

			RaisePropertyChanged(() => IsLearnt);
			RaisePropertyChanged(() => IsLearnStatusText);
		}

		private async Task SaveAsync()
		{
			using (statusBusyService.Busy(CommonBusyType.Saving))
			{
				await entryOperations.UpdateEntryAsync(Entry);

				View.NavigateBack();
			}
		}

		private bool CanSave()
		{
			return true;
		}

		private void Cancel()
		{
			View.NavigateBack();
		}

		private async Task TranslateAsync()
		{
			await entryOperations.TranslateEntryItemAsync(Entry, new[] { this });
		}

		private bool CanTranslate()
		{
			return !string.IsNullOrEmpty(Text) && !IsTranslating;
		}

		protected override void OnTextChangedOverride()
		{
			base.OnTextChangedOverride();

			TranslateCommand.RaiseCanExecuteChanged();
		}

		protected override void OnIsTranslatingChangedOverride()
		{
			base.OnIsTranslatingChangedOverride();

			TranslateCommand.RaiseCanExecuteChanged();
		}
	}
}