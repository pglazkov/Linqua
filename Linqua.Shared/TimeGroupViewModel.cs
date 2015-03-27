using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Framework;
using JetBrains.Annotations;

namespace Linqua
{
	public class TimeGroupViewModel<TItemViewModel> : ViewModelBase
		where TItemViewModel : ViewModelBase
	{
		private ObservableCollection<TItemViewModel> items;
		private IDictionary<TItemViewModel, bool> itemIsMatchFilterStatuses = new Dictionary<TItemViewModel, bool>();

		public TimeGroupViewModel([NotNull] string groupName)
		{
			Guard.NotNullOrEmpty(groupName, () => groupName);

			GroupName = groupName;
		}

		public string GroupName { get; private set; }

		public ObservableCollection<TItemViewModel> Items
		{
			get { return items; }
			set
			{
				if (items == value)
				{
					return;
				}

				items = value;

				items = value;
				itemIsMatchFilterStatuses = items.ToDictionary(x => x, x => true);
			}
		}

		public void NotifyIsMatch(TItemViewModel item, bool isMatch)
		{
			itemIsMatchFilterStatuses[item] = isMatch;

			RaisePropertyChanged(() => IsFilterNoMatch);
		}

		public bool IsFilterNoMatch
		{
			get { return !itemIsMatchFilterStatuses.Any(x => x.Value); }
		}
	}
}
