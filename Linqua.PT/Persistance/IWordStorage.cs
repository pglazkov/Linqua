using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.Model;

namespace Linqua.Persistance
{
	public interface IWordStorage
	{
		[NotNull]
		Task<IEnumerable<Word>> LoadAllWords();
	}
}