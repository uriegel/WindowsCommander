using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsCommander
{
	enum WindowMessage
	{
		WM_NCLBUTTONDOWN = 0xA1
	}

	static class PInvoke
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PostMessage(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);
	}
}
