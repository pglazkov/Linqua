using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Web.Http;
using AutoMapper;
using Linqua.Service.DataObjects;
using Linqua.Service.Migrations;
using Linqua.Service.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace Linqua.Service
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //Database.SetInitializer(new LinquaInitializer());

            var migrator = new DbMigrator(new Configuration());
            migrator.Update();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entry, ClientEntry>();
                cfg.CreateMap<ClientEntry, Entry>();
            });

            // Uncomment this in order to debug authentication locally
            //config.SetIsHosted(true);
        }
    }

    public class LinquaInitializer : ClearDatabaseSchemaIfModelChanges<LinquaContext>
    {
        protected override void Seed(LinquaContext context)
        {
            List<Entry> todoItems = new List<Entry>
            {
                new Entry {Id = Guid.NewGuid().ToString(), Text = "Word 1", Definition = "Definition 1"},
                new Entry {Id = Guid.NewGuid().ToString(), Text = "Word 2", Definition = "Definition 2"},
            };

            foreach (Entry todoItem in todoItems)
            {
                context.Set<Entry>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}