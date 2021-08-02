using System;
using WindowsCommander.Core;

namespace WindowsCommander.Models
{
	public class Item : NotifyPropertyChanged
	{
		#region NotifyProperties

		public string Name
		{
			get => _Name;
			set
			{
				_Name = value;
				Changed();
			}
		}
		string _Name;

		public DateTime DateTime
		{
			get => _DateTime; 
			set
			{
				_DateTime = value;
				Changed();
			}
		}
		DateTime _DateTime;

		#endregion

		#region Properties
		public string Path { get; protected set; }

		#endregion

		#region Constructor

		public Item(string name, string path, DateTime dateTime)
		{
			Name = name;
			Path = path;
			DateTime = dateTime;
		}

		protected Item() {}

		#endregion
	}
}
