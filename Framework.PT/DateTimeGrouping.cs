using System;
using System.Collections.Generic;
using Framework.PlatformServices;

namespace Framework
{
	public static class DateTimeGrouping
	{
		private class GroupInfo
		{
			public GroupInfo()
			{
			}

			public GroupInfo(string key, string englishName)
			{
				Key = key;
				EnglishName = englishName;
			}

			public string Key { get; set; }
			public string EnglishName { get; set; }
		}

		private static readonly List<GroupInfo> GroupInfos = new List<GroupInfo>
		{
			new GroupInfo("Older", "Older"),
			new GroupInfo("LastMonth", "Last month"),
			new GroupInfo("4WeeksAgo", "4 weeks ago"),
			new GroupInfo("3WeeksAgo", "3 weeks ago"), 
			new GroupInfo("2WeeksAgo", "2 weeks ago"),
			new GroupInfo("LastWeek", "Last week"),
			new GroupInfo("Monday", "Monday"),
			new GroupInfo("Tuesday", "Tuesday"),
			new GroupInfo("Wednesday", "Wednesday"),
			new GroupInfo("Thursday", "Thursday"),
			new GroupInfo("Friday", "Friday"),
			new GroupInfo("Saturday", "Saturday"),
			new GroupInfo("Sunday", "Sunday"),
			new GroupInfo("Yesterday", "Yesterday"),
			new GroupInfo("Today", "Today"),
			new GroupInfo("Tomorrow", "Tomorrow"),
			new GroupInfo("Monday2", "Monday"),
			new GroupInfo("Tuesday2", "Tuesday"),
			new GroupInfo("Wednesday2", "Wednesday"),
			new GroupInfo("Thursday2", "Thursday"),
			new GroupInfo("Friday2", "Friday"),
			new GroupInfo("Saturday2", "Saturday"),
			new GroupInfo("Sunday2", "Sunday"),
			new GroupInfo("NextWeek", "Next week"),
			new GroupInfo("In2Weeks", "In 2 weeks"),
			new GroupInfo("In3Weeks", "In 3 weeks"),
			new GroupInfo("In4Weeks", "In 4 weeks"),
			new GroupInfo("NextMonth", "Next month"),
			new GroupInfo("Newer", "Newer")
		};

		private static IStringResourceManager resources;

		private static IStringResourceManager Resources
		{
			get { return resources ?? (resources = CompositionManager.Current.GetInstance<IStringResourceManager>()); }
		}

		private static string GetGroupName(int index)
		{
			var groupInfo = GroupInfos[index];

			if (Resources != null)
			{
				return Resources.GetString("TimeGroup_" + groupInfo.Key, groupInfo.EnglishName);
			}

			return groupInfo.EnglishName;
		}

		private static readonly int GroupIndexShift = GroupInfos.Count / 2;

		public static DateTimeGroupInfo GetGroup(DateTime target, DateTime reference)
		{
			Func<int, DateTimeGroupInfo> result = i => new DateTimeGroupInfo(i, GetGroupName(i + GroupIndexShift));

			int sourceWeakDay = (Convert.ToInt32(target.DayOfWeek) + 6) % 7;
			int refWeakDay = (Convert.ToInt32(reference.DayOfWeek) + 6) % 7;

			int ts = (reference.Date - target.Date).Days;

			if (ts == 0)
			{
				return result(0); // same day
			}
			if (ts == 1)
			{
				return result(-1); // day before
			}

			if (ts == -1)
			{
				return result(1); // day after
			}

			// same week 
			if ((ts > 1) && (ts <= refWeakDay))
			{
				return result(sourceWeakDay - 8);
			}

			if ((-ts > 1) && (-ts <= 6 - refWeakDay))
			{
				return result(sourceWeakDay + 2);
			}

			// previous / next weeks
			for (int nIdx = 0; nIdx <= 3; nIdx++)
			{
				if ((ts > refWeakDay + nIdx * 7) &&
					(ts <= refWeakDay + (nIdx + 1) * 7))
				{
					if (reference.Month == target.Month)
					{
						return result(-(9 + nIdx));
					}
					else
					{
						return result(-13);
					}
				}
				if ((-ts > 6 - refWeakDay + nIdx * 7) &&
					(-ts <= 6 - refWeakDay + (nIdx + 1) * 7))
				{
					if (reference.Month == target.Month)
					{
						return result((9 + nIdx));
					}
					else
					{
						return result(13);
					}
				}
			}

			if (Math.Abs(target.Month - reference.Month) == 1)
			{
				return result(Math.Sign(ts) * (-13));
			}

			return result(Math.Sign(ts) * (-14));
		}

		public static string OlderGroupName
		{
			get { return GetGroupName(0); }
		}
	}
}