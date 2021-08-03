using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsCommander.Extensions
{
	static class DirectoryInfoExtensions
	{
		public static bool IsHidden(this DirectoryInfo directoryInfo)
			=>(directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

		public static bool ShowHidden(this DirectoryInfo directoryInfo, bool show)
			=> show || !directoryInfo.IsHidden();
	}
}
