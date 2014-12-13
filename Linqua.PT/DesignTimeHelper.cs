using System;
using System.Collections.Generic;
using Linqua.Model;

namespace Linqua
{
	public static class DesignTimeHelper
	{
		public static IEnumerable<Word> FakeWords
		{
			get
			{
				yield return new Word("Aankomst");
				yield return new Word("Voordelig");
				yield return new Word("Zender");
				yield return new Word("toiletborstel");
				yield return new Word("beide");
				yield return new Word("Toch");
				yield return new Word("verantwoord");
				yield return new Word("Zou");
				yield return new Word("eigen");
				yield return new Word("Betaalwijze");
				yield return new Word("terug");
				yield return new Word("Krijgt");
				yield return new Word("Gewoon");
				yield return new Word("Steeds");
				yield return new Word("Bewaar");
				yield return new Word("Klaar");
				yield return new Word("Alles");
				yield return new Word("Eigen");
				yield return new Word("Belangrijk");
				yield return new Word("Vervalen");
				yield return new Word("Het ongemak");
				yield return new Word("echter");
			}
		}
	}
}