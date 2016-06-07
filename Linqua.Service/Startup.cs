using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin;
using Owin;
using Configuration = Linqua.Service.Migrations.Configuration;

[assembly: OwinStartup(typeof(Linqua.Service.Startup))]

namespace Linqua.Service
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            new MobileAppConfiguration().UseDefaultConfiguration().ApplyTo(config);

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //Database.SetInitializer(new LinquaInitializer());

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<LinquaContext, Configuration>());
            //var migrator = new DbMigrator(new Configuration());
            //migrator.Update("AddIndexOnTextColumn");

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            // See: http://www.systemsabuse.com/2015/12/04/local-debugging-with-user-authentication-of-an-azure-mobile-app-service/
            if (string.IsNullOrEmpty(settings.HostName))
            {
                // This middleware is intended to be used locally for debugging. By default, HostName will
                // only have a value when running in an App Service application.
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }
            app.UseWebApi(config);
        }
    }
}
