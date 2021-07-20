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

		void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				User32.PostMessage(windowHandle, User32.WindowMessage.WM_NCLBUTTONDOWN, new IntPtr(2), IntPtr.Zero);
		}

		void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
				Maximize_Click(this, e);
		}

		void Window_Loaded(object sender, RoutedEventArgs e) => windowHandle = new WindowInteropHelper(this).Handle;

		void Minimize_Click(object sender, MouseButtonEventArgs e)
			=> WindowState = WindowState.Minimized;
		void Maximize_Click(object sender, MouseButtonEventArgs e)
			=> WindowState = WindowState == WindowState.Normal
				? WindowState.Maximized
				: WindowState.Normal;

		void Close_Click(object sender, MouseButtonEventArgs e)
			=> Close();

		IntPtr windowHandle;
	}
}

// TODO Icon
// TODO Save Windows State