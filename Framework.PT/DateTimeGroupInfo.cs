using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Framework
{
	public class DateTimeGroupInfo
	{
		public DateTimeGroupInfo(int orderIndex, [NotNull] string groupName)
		{
			Guard.NotNull(groupName, nameof(groupName));

			OrderIndex = orderIndex;
			GroupName = groupName;
		}

		public int OrderIndex { get; }
		public string GroupName { get; }

		public override string ToString()
		{
			return "[" + OrderIndex + "] " + GroupName;
		}

		#region Equality Members

		protected bool Equals(DateTimeGroupInfo other)
		{
			return OrderIndex == other.OrderIndex && string.Equals(GroupName, other.GroupName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DateTimeGroupInfo)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (OrderIndex * 397) ^ (GroupName != null ? GroupName.GetHashCode() : 0);
			}
		}

		#endregion

	}
}