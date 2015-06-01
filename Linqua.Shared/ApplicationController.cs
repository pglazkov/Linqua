using System;
using System.Composition;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using Linqua.Events;
using Linqua.Persistence;
using Linqua.Translation;
using MetroLog;

namespace Linqua
{
	[Export(typeof(IApplicationController))]
    public class ApplicationController : IApplicationController
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<ApplicationController>();

		private readonly ICompositionFactory compositionFactory;
		private readonly IDataStore storage;
		private readonly IEventAggregator eventAggregator;
		private readonly IStatusBusyService statusBusyService;
		private readonly Lazy<ITranslationService> translator;

		[ImportingConstructor]
		public ApplicationController(
			ICompositionFactory compositionFactory,
			IDataStore storage,
			IEventAggregator eventAggregator,
			IStatusBusyService statusBusyService,
			Lazy<ITranslationService> translator)
		{
			this.compositionFactory = compositionFactory;
			this.storage = storage;
			this.eventAggregator = eventAggregator;
			this.statusBusyService = statusBusyService;
			this.translator = translator;
		}

		public void OnIsLearntChanged(EntryViewModel entry)
		{
			Guard.Assert(entry.Entry != null, "e.EntryViewModel.Entry != null");

			storage.UpdateEntry(entry.Entry).FireAndForget();

			eventAggregator.Publish(new EntryIsLearntChangedEvent(entry));
		}

		public async Task TranslateEntryItemAsync(EntryViewModel entryItem)
		{
			entryItem.IsTranslating = true;

			try
			{
				string translation = null;

				try
				{
					if (Log.IsDebugEnabled)
						Log.Debug("Trying to find an existing entry with Text=\"{0}\".", entryItem.Text);

					var existingEntry = await storage.LookupByExample(entryItem.Entry);

					if (existingEntry != null && !string.IsNullOrWhiteSpace(existingEntry.Definition))
					{
						if (Log.IsDebugEnabled)
							Log.Debug("Found existing entry with translation: \"{0}\". Entry ID: {1}", existingEntry.Definition, existingEntry.Id);

						translation = existingEntry.Definition;
					}
				}
				catch (Exception ex)
				{
					if (Log.IsErrorEnabled)
						Log.Error("An error occured while trying to find an existing entry.", ex);
				}

				if (string.IsNullOrEmpty(translation))
				{
					if (Log.IsDebugEnabled)
						Log.Debug("Detecting language of \"{0}\"", entryItem.Entry.Text);

					var entryLanguage = await translator.Value.DetectLanguageAsync(entryItem.Entry.Text);

					if (Log.IsDebugEnabled)
						Log.Debug("Detected language: " + entryLanguage);

					if (Log.IsDebugEnabled)
						Log.Debug("Translating \"{0}\" from \"{1}\" to \"{2}\"", entryItem.Entry.Text, entryLanguage, "en");

					translation = await translator.Value.TranslateAsync(entryItem.Entry.Text, entryLanguage, "en");

					if (Log.IsDebugEnabled)
						Log.Debug("Translation: \"{0}\"", translation);
				}

				entryItem.Definition = translation;

				await storage.UpdateEntry(entryItem.Entry);
			}
			catch (Exception ex)
			{
				if (Log.IsErrorEnabled)
					Log.Error("An error occured while trying to translate an entry.", ex);
			}
			finally
			{
				entryItem.IsTranslating = false;
			}
		}
    }
}
