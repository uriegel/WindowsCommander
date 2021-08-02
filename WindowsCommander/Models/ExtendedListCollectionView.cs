using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WindowsCommander.Models
{
	public class ExtendedListCollectionView : ListCollectionView
	{
		#region NotifyProperties

		public bool IsActive
		{
			get { return _IsActive; }
			set
			{
				_IsActive = value;
				Changed();
			}
		}
		bool _IsActive;

		public new object CurrentItem
		{
			get { return _CurrentItem; }
			set
			{
				_CurrentItem = value;
				Changed();
				CurrentChanged?.Invoke(this, EventArgs.Empty);
			}
		}
		object _CurrentItem;

		#endregion

		#region Properties

		public IList SelectedItems { get; set; }
		
		public new IComparer CustomSort
		{
			get => base.CustomSort;
			set
			{
				base.CustomSort = value;
				SortCriteriumChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Events

		public event EventHandler SortCriteriumChanged;
		public new event EventHandler CurrentChanged;
		protected override event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Event Handlers

		#endregion

		#region Constructor

		public ExtendedListCollectionView(IList list)
			: base(list)
		{
			SelectedItems = new ObservableCollection<object>();
			(SourceCollection as INotifyCollectionChanged).CollectionChanged += ExtendedListCollectionView_CollectionChanged;
		}

		void ExtendedListCollectionView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if ((SourceCollection as IList).Count == 1)
					CurrentItem = (SourceCollection as IList)[0];
			}
		}

		#endregion

		#region Methods

		//public IEnumerable<Item> GetSelectedItems()
		//{
		//	var result = SelectedItems.OfType<Item>().Where(n => n is not ParentItem);
		//	if (result.Count() == 0)
		//		result = Enumerable.Repeat(CurrentItem as Item, 1);
		//	return result;
		//}
		protected void Changed([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion
	}
}
