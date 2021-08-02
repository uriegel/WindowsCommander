using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WindowsCommander.Models;

namespace WindowsCommander
{
	/// <summary>
	/// Interaktionslogik für Folder.xaml
	/// </summary>
	public partial class Folder : UserControl
	{
        public Folder()
        {
            InitializeComponent();
            var selectedItem = new HeaderItem("Name", 150, (i1, i2) => string.Compare(i1.Name, i2.Name), "IconNameTemplateSelector");
            var headers = new HeaderItem[]
            {
                selectedItem,
                new HeaderItem("Neu", 150, (i1, i2) => string.Compare(i1.Name, i2.Name), "RenameFileTemplateSelector", HorizontalAlignment=HorizontalAlignment.Left, false, false),
                new HeaderItem("Erw.", 40, null, "ExtensionTemplateSelector"),
                new HeaderItem("Größe", 70, null, "SizeTemplateSelector", HorizontalAlignment.Right),
                new HeaderItem("Datum", 120, null, "DateTimeTemplateSelector", HorizontalAlignment.Left, true),
                new HeaderItem("Version", 80, null, "VersionTemplateSelector")
            };
            InitializeHeaders(headers, selectedItem);
        }

        public void InitializeHeaders(HeaderItem[] headerItems, HeaderItem selectedItem)
        {
            try
            {
                var listViewView = listView.View as GridView;
                listViewView.Columns.Clear();
                foreach (HeaderItem header in headerItems)
                {
					GridViewColumnHeader columnHeader = new()
					{
						Content = header.Title,
						//columnHeader.Click += columnHeader_Click;
						HorizontalContentAlignment = header.HorizontalAlignment
					};
					if (columnHeader.HorizontalContentAlignment == System.Windows.HorizontalAlignment.Right)
                        columnHeader.Padding = new Thickness(0, 0, 18, 0);
                    columnHeader.Tag = header;
                    
                    GridViewColumn col = new()
					{
						Width = header.Width,
                        Header = columnHeader,
                        //col.CellTemplateSelector = (DataTemplateSelector)FindResource(header.CellTemplateSelector);
                    };

                    listViewView.Columns.Add(col);
                    if (header.IsVisible)
                    {
                        //if (header == selectedItem)
                        //    InitializeSort(columnHeader, selectedItem);
                    }
                    else
                        col.Width = 0;
                }
            }
            catch { }
        }
    }
}
