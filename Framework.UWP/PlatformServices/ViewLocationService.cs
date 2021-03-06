﻿using System.Composition;

namespace Framework.PlatformServices
{
    [Export(typeof(IViewLocationService))]
    [Shared]
    public class ViewLocationService : IViewLocationService
    {
        public ViewLocationService()
        {
        }

        public object LocateForViewModel(IViewModel viewModel)
        {
            return ViewLocator.Locate(viewModel);
        }
    }
}