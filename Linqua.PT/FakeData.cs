using System;
using System.Collections.Generic;
using Linqua.DataObjects;

namespace Linqua
{
	public static class FakeData
	{
		public static IEnumerable<ClientEntry> FakeWords
		{
			get
			{
				yield return new ClientEntry("Aankomst");
				yield return new ClientEntry("Voordelig");
				yield return new ClientEntry("Zender");
				yield return new ClientEntry("toiletborstel");
				yield return new ClientEntry("beide");
				yield return new ClientEntry("Toch");
				yield return new ClientEntry("verantwoord");
				yield return new ClientEntry("Zou");
				yield return new ClientEntry("eigen");
				yield return new ClientEntry("Betaalwijze");
				yield return new ClientEntry("terug");
				yield return new ClientEntry("Krijgt");
				yield return new ClientEntry("Gewoon");
				yield return new ClientEntry("Steeds");
				yield return new ClientEntry("Bewaar");
				yield return new ClientEntry("Klaar");
				yield return new ClientEntry("Alles");
				yield return new ClientEntry("Eigen");
				yield return new ClientEntry("Belangrijk");
				yield return new ClientEntry("Vervalen");
				yield return new ClientEntry("Het ongemak");
				yield return new ClientEntry("echter");
			}
		}
	}
}