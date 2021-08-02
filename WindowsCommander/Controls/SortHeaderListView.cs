using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using WindowsCommander.Core;
using WindowsCommander.Models;

namespace WindowsCommander
{
    public class SortHeaderListView : ListView
    {
        #region Properties

        public HeaderItem CurrentHeader
        {
            get => selectedSortColumn.Tag as HeaderItem;
        }

        public ExtendedListCollectionView ExtendedListCollectionView { get; set; }

        #endregion

        #region Routed Event Handlers

        void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null || headerClicked.Role != GridViewColumnHeaderRole.Normal)
                return;

            try
            {
                if (selectedSortColumn != null)
                    AdornerLayer.GetAdornerLayer(selectedSortColumn).Remove(currentAdorner);
            }
            catch { }

            HeaderItem headerItem = (headerClicked.Tag as HeaderItem);
            if (headerItem.SortPredicate == null)
                return;

            bool isSortDescending = headerItem.IsSortDescending;
            if (selectedSortColumn == headerClicked)
                isSortDescending = !headerItem.IsSortDescending;
            selectedSortColumn = headerClicked;
            headerItem.IsSortDescending = isSortDescending;
            try
            {
                noScrollIntoViewWhileSettingSortColumns = true;
                SetSortCriterium(headerItem.SortPredicate, headerItem.IsSortDescending);
            }
            finally
            {
                noScrollIntoViewWhileSettingSortColumns = false;
            }

            currentAdorner = new SortAdorner(selectedSortColumn, headerItem.IsSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending);
            AdornerLayer.GetAdornerLayer(selectedSortColumn).Add(currentAdorner);
        }

        void selectedSortColumn_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as GridViewColumnHeader).Loaded -= selectedSortColumn_Loaded;
            HeaderItem headerItem = selectedSortColumn.Tag as HeaderItem;
            currentAdorner = new SortAdorner(selectedSortColumn, headerItem.IsSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending);
            AdornerLayer.GetAdornerLayer(selectedSortColumn).Add(currentAdorner);
        }

        #endregion

        #region Methods

        public void InitializeHeaders(HeaderItem[] headerItems, HeaderItem selectedItem)
        {
            try
            {
                (View as GridView).Columns.Clear();
                foreach (HeaderItem header in headerItems)
                {
                    GridViewColumn col = new GridViewColumn();
                    col.Width = header.Width;
                    GridViewColumnHeader columnHeader = new GridViewColumnHeader();
                    columnHeader.Content = header.Title;
                    columnHeader.Click += columnHeader_Click;
                    columnHeader.HorizontalContentAlignment = header.HorizontalAlignment;
                    if (columnHeader.HorizontalContentAlignment == System.Windows.HorizontalAlignment.Right)
                        columnHeader.Padding = new Thickness(0, 0, 18, 0);
                    columnHeader.Tag = header;
                    col.Header = columnHeader;
                    col.CellTemplateSelector = (DataTemplateSelector)FindResource(header.CellTemplateSelector);
                    (View as GridView).Columns.Add(col);
                    if (header.IsVisible)
                    {
                        if (header == selectedItem)
                            InitializeSort(columnHeader, selectedItem);
                    }
                    else
                        col.Width = 0;
                }
            }
            catch { }
        }

        public HeaderItem[] GetHeaders()
            => (View as GridView).Columns.Select(n => (n.Header as GridViewColumnHeader).Tag as HeaderItem).ToArray();

        public void EnableColumn(int index, bool isEnabled = true)
            => (View as GridView).Columns[index].Width = isEnabled 
                ? (((View as GridView).Columns[index].Header as GridViewColumnHeader).Tag as HeaderItem).Width 
                : 0;

        public new bool SetSelectedItems(IEnumerable selectedItems) => base.SetSelectedItems(selectedItems);

        public void FocusItem(Item item, bool force = false)
        {
            if (noScrollIntoViewWhileSettingSortColumns)
                return;
            ScrollIntoView(item);
            if (ExtendedListCollectionView.IsActive || force)
            {
                ListViewItem lvi = ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (lvi != null)
                    lvi.Focus();
            }
        }

        void InitializeSort(GridViewColumnHeader columnHeader, HeaderItem headerItem)
        {
            selectedSortColumn = columnHeader;
            selectedSortColumn.Loaded += selectedSortColumn_Loaded;
        }

        void SetSortCriterium(Func<Item, Item, int> sortPredicate, bool isDescending)
        {
            //View.CustomSort = new itemSorter(sortPredicate, isDescending);
            //View.Refresh();
        }


        #endregion

        #region Fields

        GridViewColumnHeader selectedSortColumn;
        SortAdorner currentAdorner;
        bool noScrollIntoViewWhileSettingSortColumns;

        #endregion
    }
}
