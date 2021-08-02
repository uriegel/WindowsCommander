using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WindowsCommander.Core
{
	public class SortAdorner : Adorner
	{
		#region Properties

		public ListSortDirection Direction { get; }

		#endregion

		#region Constructor

		public SortAdorner(UIElement element, ListSortDirection dir)
			: base(element) => Direction = dir;

		#endregion

		#region Methods

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (AdornedElement.RenderSize.Width < 20)
				return;

			drawingContext.PushTransform(
				new TranslateTransform(
				  AdornedElement.RenderSize.Width - 15,
				  (AdornedElement.RenderSize.Height - 5) / 2));

			drawingContext.DrawGeometry(Brushes.Gray, null,
				Direction == ListSortDirection.Ascending ?
				  ascendingGeometry : descendingGeometry);

			drawingContext.Pop();
		}

		#endregion

		#region Fields

		readonly static Geometry ascendingGeometry = Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");
		readonly static Geometry descendingGeometry = Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

		#endregion
	}
}
