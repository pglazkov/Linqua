﻿using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using Framework;
using Framework.MefExtensions;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
{
    public class Bootstrapper
    {
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<Bootstrapper>();

        public void Run(App application)
        {
			if (Log.IsInfoEnabled)
				Log.Info("Bootstrapper sequence started.");

            var mvvmConventions = new ConventionBuilder();

            mvvmConventions.ForTypesDerivedFrom<ViewModelBase>()
                           .Export();

            ViewLocator.BuildMefConventions(mvvmConventions);

            var configuration = new ContainerConfiguration()
                .WithAssembly(typeof (App).GetTypeInfo().Assembly, mvvmConventions)
                .WithAssembly(typeof (AppPortable).GetTypeInfo().Assembly, mvvmConventions)
                .WithAssembly(typeof (FrameworkPortable).GetTypeInfo().Assembly)
                .WithAssembly(typeof (FrameworkPhone).GetTypeInfo().Assembly)
                .WithProvider(new DefaultExportDescriptorProvider());
			
            var container = configuration.CreateContainer();

			CompositionManager.Initialize(container);

            App.CompositionManager = CompositionManager.Current;

			if (Log.IsInfoEnabled)
				Log.Info("Bootstrapper sequence completed.");
        }
    }
}