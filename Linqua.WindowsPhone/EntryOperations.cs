﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using Linqua.DataObjects;
using Linqua.Events;
using Linqua.Persistence;
using Linqua.Translation;
using Linqua.UI;
using MetroLog;

namespace Linqua
{
	[Export(typeof(IEntryOperations))]
    public class EntryOperations : IEntryOperations
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<EntryOperations>();

		private readonly ICompositionFactory compositionFactory;
		private readonly IDataStore storage;
		private readonly IEventAggregator eventAggregator;
		private readonly IStatusBusyService statusBusyService;
		private readonly Lazy<ITranslationService> translator;

		[ImportingConstructor]
		public EntryOperations(
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

		public async Task DeleteEntryAsync(EntryViewModel entry)
		{
			using (statusBusyService.Busy(CommonBusyType.Deleting))
			{
				await storage.DeleteEntry(entry.Entry);
			}

			eventAggregator.Publish(new EntryDeletedEvent(entry));
		}

		public async Task UpdateEntryIsLearnedAsync(EntryViewModel entry)
		{
			Guard.Assert(entry.Entry != null, "entry.Entry != null");

			await storage.UpdateEntry(entry.Entry);

			eventAggregator.Publish(new EntryUpdatedEvent(entry.Entry));
			eventAggregator.Publish(new EntryIsLearntChangedEvent(entry));
		}

		public async Task UpdateEntryAsync(ClientEntry entry)
		{
			Guard.Assert(entry != null, "entry != null");

			await storage.UpdateEntry(entry);

			eventAggregator.Publish(new EntryUpdatedEvent(entry));
		}

		public async Task<string> TranslateEntryItemAsync(ClientEntry entry, IEnumerable<EntryViewModel> viewModelsToUpdate)
		{
			string translation = null;

			viewModelsToUpdate.ForEach(x => x.IsTranslating = true);

			try
			{
				try
				{
					if (Log.IsDebugEnabled)
						Log.Debug("Trying to find an existing entry with Text=\"{0}\".", entry.Text);

					var existingEntry = await storage.LookupByExample(entry);

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
						Log.Debug("Detecting language of \"{0}\"", entry.Text);

					var entryLanguage = await translator.Value.DetectLanguageAsync(entry.Text);

					if (Log.IsDebugEnabled)
						Log.Debug("Detected language: " + entryLanguage);

					if (Log.IsDebugEnabled)
						Log.Debug("Translating \"{0}\" from \"{1}\" to \"{2}\"", entry.Text, entryLanguage, "en");

					translation = await translator.Value.TranslateAsync(entry.Text, entryLanguage, "en");

					if (Log.IsDebugEnabled)
						Log.Debug("Translation: \"{0}\"", translation);
				}

				viewModelsToUpdate.ForEach(x => x.Definition = translation);

				entry.Definition = translation;
				
			}
			catch (Exception ex)
			{
				if (Log.IsErrorEnabled)
					Log.Error("An error occured while trying to translate an entry.", ex);
			}
			finally
			{
				viewModelsToUpdate.ForEach(x => x.IsTranslating = false);
			}

			return translation;
		}
    }
}