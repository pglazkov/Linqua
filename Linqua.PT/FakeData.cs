using System;
using System.Collections.Generic;
using Linqua.DataObjects;

namespace Linqua
{
	public static class FakeData
	{
		private static List<ClientEntry> fakeWords = new List<ClientEntry>
		{
			ClientEntry.CreateNew("Aankomst"),
			ClientEntry.CreateNew("Voordelig"),
			ClientEntry.CreateNew("Zender"),
			ClientEntry.CreateNew("toiletborstel"),
			ClientEntry.CreateNew("beide"),
			ClientEntry.CreateNew("Toch"),
			ClientEntry.CreateNew("verantwoord"),
			ClientEntry.CreateNew("Zou"),
			ClientEntry.CreateNew("eigen"),
			ClientEntry.CreateNew("Betaalwijze"),
			ClientEntry.CreateNew("terug"),
			ClientEntry.CreateNew("Krijgt"),
			ClientEntry.CreateNew("Gewoon"),
			ClientEntry.CreateNew("Steeds"),
			ClientEntry.CreateNew("Bewaar"),
			ClientEntry.CreateNew("Klaar"),
			ClientEntry.CreateNew("Alles"),
			ClientEntry.CreateNew("Eigen"),
			ClientEntry.CreateNew("Belangrijk"),
			ClientEntry.CreateNew("Vervalen"),
			ClientEntry.CreateNew("Het ongemak"),
			ClientEntry.CreateNew("echter")
		};

		public static List<ClientEntry> FakeWords
		{
			get { return fakeWords; }
		}
	}
}