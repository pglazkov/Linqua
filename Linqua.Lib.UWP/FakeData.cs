using System;
using System.Collections.Generic;
using Linqua.DataObjects;

namespace Linqua
{
    public static class FakeData
    {
        private static List<Entry> fakeWords = new List<Entry>
        {
            Entry.CreateNew("Aankomst"),
            Entry.CreateNew("Voordelig"),
            Entry.CreateNew("Zender"),
            Entry.CreateNew("toiletborstel"),
            Entry.CreateNew("beide"),
            Entry.CreateNew("Toch"),
            Entry.CreateNew("verantwoord"),
            Entry.CreateNew("Zou"),
            Entry.CreateNew("eigen"),
            Entry.CreateNew("Betaalwijze"),
            Entry.CreateNew("terug"),
            Entry.CreateNew("Krijgt"),
            Entry.CreateNew("Gewoon"),
            Entry.CreateNew("Steeds"),
            Entry.CreateNew("Bewaar"),
            Entry.CreateNew("Klaar"),
            Entry.CreateNew("Alles"),
            Entry.CreateNew("Eigen"),
            Entry.CreateNew("Belangrijk"),
            Entry.CreateNew("Vervalen"),
            Entry.CreateNew("Het ongemak"),
            Entry.CreateNew("echter")
        };

        public static List<Entry> FakeWords
        {
            get { return fakeWords; }
        }
    }
}