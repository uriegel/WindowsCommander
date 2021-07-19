using PInvoke;

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace WindowCommander
{
	public partial class MainWindow : Window
	{
		public MainWindow() => InitializeComponent();

		void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
			=> User32.PostMessage(windowHandle, User32.WindowMessage.WM_NCLBUTTONDOWN, new IntPtr(2), IntPtr.Zero);

		void Window_Loaded(object sender, RoutedEventArgs e) => windowHandle = new WindowInteropHelper(this).Handle;

		IntPtr windowHandle;
	}
}

// TODO Hover in Window Control Buttons
// TODO Click in Window Control Buttons
// TODO Icon
// TODO Dblclk in Title
// TODO Save Windows State