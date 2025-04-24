using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Commander.Controllers;
using Commander.Controls.ColumnViewHeader;
using Commander.Extensions;

namespace Commander;

public partial class TestControl : UserControl
{
    public TestControl()
    {
        InitializeComponent();
        DataContext = new ColumnViewContext();
        griddie.ColumnViewContext = DataContext as ColumnViewContext;
        griddie.HeaderItems =
        [
            new HeaderItem("Name"),
            new HeaderItem("Datum"),
            new HeaderItem("Größe", TextAlignment.Right)
        ];
        var items = Directory.GetItems(@"c:\windows\system32");
        PeopleListView.ItemsSource = items;
    }

    void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => (sender as TextBox)?.SelectAll());
        e.Handled = true;
    }

    void story_Completed(object? sender, EventArgs e)
    {
        var story = (Storyboard)FindResource("WaterRipples");
        story.Completed -= story_Completed;
        PeopleListView.Effect = null;
    }

    void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Return:

                ////(DataContext as ItemViewContext).ChangePath.Execute((sender as TextBox).Text);
                //var items = Directory.GetItems((sender as TextBox).Text);
                //this.listView.ItemsSource = items;
                //e.Handled = true;
                //liste.FocusItem((DataContext as ItemViewContext).View.CurrentItem as Item, true);


                Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                {
                    var ripple = new WaterRipple
                    {
                        Amplitude = 10,
                        RatioControl = 2,
                        Frequency = 35
                    };
                    PeopleListView.Effect = ripple;
                    var story = (Storyboard)FindResource("WaterRipples");
                    story.Completed += story_Completed;
                    story.Begin();
                });


                break;
        }
    }

    void PeopleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.AddedItems.OfType<Item>();
        var toRemove = selected.Where(n => n.Name == "@AdvancedKeySettingsNotification.png");
        PeopleListView.SelectedItems.Remove(toRemove.FirstOrDefault());
    }

    void PeopleListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var lbi = (e.OriginalSource as DependencyObject)?.FindAncestorOrSelf<ListBoxItem>();
        if (lbi != null)
        { 
            PeopleListView.UpdateLayout(); // Ensure layout is up to date
            PeopleListView.ScrollIntoView(lbi); // Ensure the item is visible
            //ListViewItem container = (ListViewItem)PeopleListView.ItemContainerGenerator.ContainerFromItem(lbi);
            Keyboard.Focus(lbi);

            e.Handled = true;
        }
    }

    private void ListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {

    }
}

class Directory
{
    public static IEnumerable<Item> GetItems(string path)
    {
        var directoryInfo = new DirectoryInfo(path);

        var directories = directoryInfo.GetFiles()
            .OrderBy(n => n.Name)
            .Select((n, i) => new DirectoryItem(i++, n.FullName, n.Name, n.LastWriteTime, false, n.Length));
        return directories;
    }
}

class DirectoryItem : Item
{
    #region Properties

    public int Order { get; private set; }

    public bool IsHidden { get; protected set; }

    public long Size { get; protected set; }

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

    public DirectoryItem(int order, string path, string name, DateTime dateTime, bool isHidden, long size)
        : base(name, path, dateTime)
    {
        Order = order;
        IsHidden = isHidden;
        Size = size;
    }

    #endregion

    #region Methods

    public static DirectoryItem Create(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        return new DirectoryItem(0, path, di.Name, di.LastAccessTime, false, 0);
    }

    #endregion
}

static class DirectoryInfoExtensions
{
    public static bool IsHidden(this DirectoryInfo directoryInfo)
        => (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

    public static bool ShowHidden(this DirectoryInfo directoryInfo, bool show)
        => show || !directoryInfo.IsHidden();
}

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

    protected Item() { }

    #endregion
}

public class NotifyPropertyChanged : INotifyPropertyChanged
{
    #region INotifyPropertyChanged Members

    /// <summary>
    /// Hit Hilfe dieses Ereignisses werden Änderungen signalisiert
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Wenn sich etwas in der Klasse ändert, muss diese Methode aufgerufen werden, damit die Signalisierung erfolgt
    /// </summary>
    /// <param name="propertyName">Der Name des Properties, welches sich geändert hat</param>
    protected void Changed([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}

