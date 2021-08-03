using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WindowsCommander.Extensions;
using WindowsCommander.Models;

namespace WindowsCommander
{
	class Directory
	{
		public static void GetItems(string path)
		{
			var directoryInfo = new DirectoryInfo(path);

			var directories = directoryInfo.GetDirectories()
				.Where(n => n.ShowHidden(false))
				.OrderBy(n => n.Name)
				.Select((n, i) => new DirectoryItem(i++, n.FullName, n.Name, n.LastWriteTime, n.IsHidden()));

		}
	}
}
