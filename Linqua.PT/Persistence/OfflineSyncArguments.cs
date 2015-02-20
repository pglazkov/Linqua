﻿using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
	public class OfflineSyncArguments
	{
		public static readonly OfflineSyncArguments Default = new OfflineSyncArguments();

		[CanBeNull]
		public EntryQuery Query { get; set; }

		public bool PurgeCache { get; set; }
	}
}
