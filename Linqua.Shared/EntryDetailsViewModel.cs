using System;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Persistence;
using Linqua.Translation;

namespace Linqua
{
	public class EntryDetailsViewModel : ViewModelBase
	{
		private readonly IDataStore storage;
		private readonly IStatusBusyService statusBusyService;
		private Lazy<ITranslationService> translator;
		private bool isLoadingData;
		private ClientEntry entry;
		private bool isTranslating;

		public EntryDetailsViewModel()
		{

		}

		public EntryDetailsViewModel(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			Lazy<ITranslationService> translator)
			: this()
		{
			Guard.NotNull(compositionFactory, () => compositionFactory);
			Guard.NotNull(storage, () => storage);
			Guard.NotNull(eventAggregator, () => eventAggregator);
			Guard.NotNull(statusBusyService, () => statusBusyService);
			Guard.NotNull(translator, () => translator);

			this.storage = storage;
			this.statusBusyService = statusBusyService;
			this.translator = translator;

			EventAggregator = eventAggregator;
			CompositionFactory = compositionFactory;
		}

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

		public ClientEntry Entry
		{
			get { return entry; }
			private set
			{
				if (Equals(value, entry)) return;
				entry = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => EntryText);
				RaisePropertyChanged(() => Definition);
				RaisePropertyChanged(() => IsDefinitionVisible);
			}
		}

		public string EntryText
		{
			get { return Entry != null ? Entry.Text : string.Empty; }
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

				EventAggregator.Publish(new EntryDefinitionChangedEvent(Entry));
			}
		}

		public bool IsDefinitionVisible
		{
			get { return !string.IsNullOrEmpty(Definition) || IsTranslating; }
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
					Entry = await storage.LookupById(entryId);
				}
				finally
				{
					IsLoadingData = false;
				}
			}
		}
	}
}
