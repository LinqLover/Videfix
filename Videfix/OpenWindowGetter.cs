using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using HelperLib;
using HelperLib.SystemHelpers.Diagnostics;
using ProcessHandle = System.IntPtr;
using ThreadHandle = System.Int32;
using WindowHandle = System.IntPtr;
using Win32Error = BetterWin32Errors.Win32Error;

namespace Videfix
{
	/// <summary>Contains functionality to get all the open windows.</summary>
	/// <remarks>
	/// CREDITS: https://www.tcx.be/blog/2006/list-open-windows/
	/// </remarks>
	public
	static class OpenWindowGetter
	{
		/// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
		/// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
		public static IDictionary<WindowHandle, WindowKey> GetOpenWindows()
		{
			var shellWindow = GetShellWindow();
			var windows = new Dictionary<WindowHandle, WindowKey>();
			var processNames = new Dictionary<ThreadHandle, string>();

			EnumWindows(delegate (WindowHandle hWnd, int lParam)
			{
				if (hWnd == shellWindow) return true;
				if (!IsWindowVisible(hWnd)) return true;

				int length = GetWindowTextLength(hWnd);
				if (length == 0) return true;

				var builder = new StringBuilder(length);
				GetWindowText(hWnd, builder, length + 1);

				if (GetWindowThreadProcessId(hWnd, out var processHandle) is 0)
					throw new Win32Exception();
				var processId = (ThreadHandle)processHandle;
				if (!processNames.TryGetValue(processId, out var processName))
					try
					{
						processName = Process.GetProcessById(processId).GetMainModuleFileName();
						processNames.Add(processId, processName);
					}
					catch (InvalidOperationException)
					{
					}
					catch (Win32Exception exception) when ((uint)exception.ErrorCode == (uint)Win32Error.E_FAIL)
					{
					}
				if (!string.IsNullOrEmpty(processName))
					windows.Add(hWnd, new WindowKey(processName, builder.ToString()));
				return true;

			}, 0);

			return windows;
		}

		private delegate bool EnumWindowsProc(WindowHandle windowHandle, int lParam);

		[DllImport("USER32.DLL")]
		private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

		[DllImport("USER32.DLL")]
		private static extern int GetWindowText(WindowHandle windowHandle, StringBuilder stringBuilder, int maxCount);

		/// <summary>
		/// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window. 
		/// http://msdn.microsoft.com/en-us/library/ms633522(VS.85).aspx
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern ThreadHandle GetWindowThreadProcessId(WindowHandle hWnd, out ProcessHandle processId);

		[DllImport("USER32.DLL")]
		private static extern int GetWindowTextLength(WindowHandle hWnd);

		[DllImport("USER32.DLL")]
		private static extern bool IsWindowVisible(WindowHandle hWnd);

		[DllImport("USER32.DLL")]
		private static extern WindowHandle GetShellWindow();
	}
}
