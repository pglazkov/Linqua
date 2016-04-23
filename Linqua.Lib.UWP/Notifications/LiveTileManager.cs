using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Framework;
using JetBrains.Annotations;
using Linqua.Persistence;
using MetroLog;
using NotificationsExtensions.TileContent;

namespace Linqua.Notifications
{
    [Export(typeof(ILiveTileManager))]
    public class LiveTileManager : ILiveTileManager
    {
        private readonly IBackendServiceClient backendServiceClient;
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<LiveTileManager>();

        [ImportingConstructor]
        public LiveTileManager([NotNull] IBackendServiceClient backendServiceClient)
        {
            Guard.NotNull(backendServiceClient, nameof(backendServiceClient));

            this.backendServiceClient = backendServiceClient;
        }

        public async Task UpdateTileAsync()
        {
            var dataTiles = await CreateDataTilesAsync();

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

            if (dataTiles.Count > 0)
            {
                tileUpdater.Clear();
                tileUpdater.EnableNotificationQueue(true);

                foreach (var dataTile in dataTiles)
                {
                    tileUpdater.Update(dataTile);
                }
            }
            else
            {
                tileUpdater.Clear();
                tileUpdater.EnableNotificationQueue(false);

                if (Log.IsDebugEnabled)
                    Log.Debug("There is no data for the tile update.");
            }
        }

        [NotNull]
        private async Task<List<TileNotification>> CreateDataTilesAsync()
        {
            var result = new List<TileNotification>();

            var randomEntries = await backendServiceClient.GetRandomEntries(5);

            foreach (var randomEntry in randomEntries)
            {
                var tileHeading = randomEntry.Text;
                var tileText = randomEntry.Definition;

                var wideTile = TileContentFactory.CreateTileWide310x150Text01();
                wideTile.TextHeading.Text = tileHeading;
                wideTile.TextBody1.Text = tileText;

                var squareTile = TileContentFactory.CreateTileSquare150x150Text01();
                squareTile.TextHeading.Text = tileHeading;
                squareTile.TextBody1.Text = tileText;

                wideTile.Square150x150Content = squareTile;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(wideTile.ToString());

                var notification = new TileNotification(xmlDocument);

                result.Add(notification);
            }

            return result;
        }
    }
}