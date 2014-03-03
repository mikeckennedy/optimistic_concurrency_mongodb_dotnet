using System;
using System.Linq;

namespace MongoDB.Kennedy
{
	public enum ConcurrentSaveOptions
	{
		ProtectServerChanges = 1,
		OverwriteServerChanges = 2
	}
}