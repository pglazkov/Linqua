﻿using System;
using Linqua.Service.Models;
using Microsoft.WindowsAzure.MobileServices;

namespace Linqua.DataObjects
{
	public class ClientEntry
	{
		public static ClientEntry CreateNew(string text = null)
		{
			var result = new ClientEntry
			{
				CreatedAt = DateTimeOffset.Now,
				Text = text,
                TranslationState = TranslationState.Pending
			};

			return result;
		}

		public ClientEntry()
		{
			
		}

		public string Id { get; set; }

		public string Text { get; set; }

		public string Definition { get; set; }

        public TranslationState TranslationState { get; set; }

		public bool IsLearnt { get; set; }

		[CreatedAt]
		public DateTimeOffset? CreatedAt { get; set; }

		#region Equality Members

		protected bool Equals(ClientEntry other)
		{
			return string.Equals(Id, other.Id) && string.Equals(Text, other.Text) && string.Equals(Definition, other.Definition) && Equals(TranslationState, other.TranslationState) && IsLearnt.Equals(other.IsLearnt) && CreatedAt.Equals(other.CreatedAt);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ClientEntry)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Id != null ? Id.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Definition != null ? Definition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TranslationState.GetHashCode();
                hashCode = (hashCode * 397) ^ IsLearnt.GetHashCode();
				hashCode = (hashCode * 397) ^ CreatedAt.GetHashCode();
				return hashCode;
			}
		}

		#endregion

	}
}