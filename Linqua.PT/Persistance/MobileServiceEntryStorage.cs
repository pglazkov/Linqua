using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices;

namespace Linqua.Persistance
{
	[Export(typeof(IEntryStorage))]
	public class MobileServiceEntryStorage : IEntryStorage
	{
		private static readonly MobileServiceClient MobileService = new MobileServiceClient(
		  "http://localhost:59988"
);
		// Use this constructor instead after publishing to the cloud
		// public static MobileServiceClient MobileService = new MobileServiceClient(
		//      "https://linqua.azure-mobile.net/",
		//      "veBcEvMWjGNePbAKosRSIQzJGiTrfc50"
		//);


		public async Task<IEnumerable<ClientEntry>> LoadAllEntries()
		{
			try
			{
				return await MobileService.GetTable<ClientEntry>().ToListAsync();
			}
			catch (Exception e)
			{
				throw;
			}
		}
	}
}