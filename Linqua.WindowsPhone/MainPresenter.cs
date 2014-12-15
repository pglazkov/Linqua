﻿using System;
using System.Composition;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Framework;
using JetBrains.Annotations;
using Linqua.Events;

namespace Linqua
{
	[UsedImplicitly]
	public class MainPresenter : PresenterBase<MainViewModel, IMainView>
	{
		protected override IViewModel CreateViewModel()
		{
			return CompositionManager.Current.GetInstance<MainViewModel>();
		}

		[Import]
		public IEventLocator EventLocator { get; set; }

		protected override void OnInitializedOverride()
		{
			EventLocator.GetEvent<AddWordRequestedEvent>().ObserveOnDispatcher().Subscribe(e => OnNewWordRequested());
		}

		private void OnNewWordRequested()
		{
			View.Navigate(typeof(NewWordPage));
		}

		protected override void OnViewLoadedFirstTimeOverride()
		{
			ViewModel.AddWordCommand.CanExecuteChanged += OnAddWordCommandCanExecuteChanged;
			View.AddWordButton.Click += OnAddWordButtonClick;
		}

		private void OnAddWordButtonClick(object sender, RoutedEventArgs routedEventArgs)
		{
			if (ViewModel.AddWordCommand.CanExecute(null))
			{
				ViewModel.AddWordCommand.Execute(null);
			}
		}

		private void OnAddWordCommandCanExecuteChanged(object sender, EventArgs e)
		{
			View.AddWordButton.IsEnabled = ViewModel.AddWordCommand.CanExecute(null);
		}
	}
}