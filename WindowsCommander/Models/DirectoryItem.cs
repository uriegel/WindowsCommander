using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsCommander.Models
{
	class DirectoryItem : Item
	{
		#region Properties

		public int Order { get; private set; }

		public bool IsHidden { get; protected set; }

		#endregion

		#region Constructor

		//public DirectoryItem(int order, string path, string name, DateTime dateTime)
		//	: base(name, path, dateTime)
		//{
		//	Order = order;
		//	// TODO: Exceptions vermeiden
		//	try
		//	{
		//		if (Directory.Exists(path))
		//		{
		//			var di = new DirectoryInfo(path);
		//			IsHidden = di.IsHidden();
		//		}
		//	}
		//	catch { }
		//}

		public DirectoryItem(int order, string path, string name, DateTime dateTime, bool isHidden)
			: base(name, path, dateTime)
		{
			Order = order;
			IsHidden = isHidden;
		}

		#endregion

		#region Methods

		public static DirectoryItem Create(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			return new DirectoryItem(0, path, di.Name, di.LastAccessTime, false);
		}

		#endregion
	}
}
