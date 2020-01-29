using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using WindowHandle = System.IntPtr;

namespace Videfix
{
	public struct WindowPlacement
	{
		private static readonly int Length = Marshal.SizeOf<WindowPlacement>();

		private int length;
		public int flags;
		public int showCmd;
		public System.Drawing.Point ptMinPosition;
		public System.Drawing.Point ptMaxPosition;
		public System.Drawing.Rectangle rcNormalPosition;

		public static WindowPlacement GetPlacement(WindowHandle windowHandle)
		{
			if (!GetWindowPlacement(windowHandle, out var placement))
				throw new System.ComponentModel.Win32Exception();

			return placement;
		}

		public void SetPlacement(WindowHandle windowHandle)
		{
			length = Length;
			flags = 0;
			//showCmd = (showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : showCmd);
			if (!SetWindowPlacement(windowHandle, ref this))
				throw new System.ComponentModel.Win32Exception();
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowPlacement(WindowHandle windowHandle, out WindowPlacement windowPlacement);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPlacement(WindowHandle hWnd, [In] ref WindowPlacement windowPlacement);
	}
}