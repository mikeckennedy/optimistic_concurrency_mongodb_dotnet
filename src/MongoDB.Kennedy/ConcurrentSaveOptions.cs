using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Kennedy
{
	public enum ConcurrentSaveOptions
	{
		ProtectServerChanges = 1,
		OverwriteServerChanges = 2
	}
}
