using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using WindowsCommander.Core;

namespace WindowsCommander.Models
{
	public class HeaderItem : NotifyPropertyChanged
	{
		#region NotifyProperties

		public double Width
		{
			get { return _Width; }
			set
			{
				_Width = value;
				Changed("Width");
			}
		}
		double _Width;

		#endregion

		#region Properties

		public string Title { get; private set; }

		public string CellTemplateSelector { get; private set; }
		public bool IsSortDescending { get; set; }
		public HorizontalAlignment HorizontalAlignment { get; private set; }
		public Func<Item, Item, int> SortPredicate { get; private set; }
		public bool IsVisible { get; private set; }

		#endregion

		#region Constructor

		public HeaderItem(string title, double width, Func<Item, Item, int> sortPredicate, string celltemplateSelector,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, bool isSortDescending = false, bool isVisible = true)
		{
			Title = title;
			Width = width;
			CellTemplateSelector = celltemplateSelector;
			SortPredicate = sortPredicate;
			IsSortDescending = isSortDescending;
			HorizontalAlignment = horizontalAlignment;
			IsVisible = isVisible;
		}

		#endregion
	}
}
