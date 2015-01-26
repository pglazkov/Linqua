using System;
using System.Collections.Generic;
using Linqua.DataObjects;

namespace Linqua
{
	public static class FakeData
	{
		private static List<ClientEntry> fakeWords = new List<ClientEntry>
		{
			new ClientEntry("Aankomst"),
			new ClientEntry("Voordelig"),
			new ClientEntry("Zender"),
			new ClientEntry("toiletborstel"),
			new ClientEntry("beide"),
			new ClientEntry("Toch"),
			new ClientEntry("verantwoord"),
			new ClientEntry("Zou"),
			new ClientEntry("eigen"),
			new ClientEntry("Betaalwijze"),
			new ClientEntry("terug"),
			new ClientEntry("Krijgt"),
			new ClientEntry("Gewoon"),
			new ClientEntry("Steeds"),
			new ClientEntry("Bewaar"),
			new ClientEntry("Klaar"),
			new ClientEntry("Alles"),
			new ClientEntry("Eigen"),
			new ClientEntry("Belangrijk"),
			new ClientEntry("Vervalen"),
			new ClientEntry("Het ongemak"),
			new ClientEntry("echter")
		};

		public static List<ClientEntry> FakeWords
		{
			get { return fakeWords; }
		}
	}
}