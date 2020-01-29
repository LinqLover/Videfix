using System;
using System.Runtime.InteropServices;

/// <remarks>
/// CREDITS: https://github.com/MScholtes/VirtualDesktop
/// 
/// MIT License
/// 
/// Copyright(c) 2017 Markus Scholtes
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </remarks>
namespace MScholtes.VirtualDesktop
{
	public class Desktop
	{
		private IVirtualDesktop ivd;
		private Desktop(IVirtualDesktop desktop) { ivd = desktop; }

		/// <summary>
		/// Get hash
		/// </summary>
		public override int GetHashCode()
		{
			return ivd.GetHashCode();
		}

		/// <summary>
		/// Compares with object
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Desktop desk && ReferenceEquals(ivd, desk.ivd);
		}

		/// <summary>
		/// Returns the number of desktops
		/// </summary>
		public static int Count => DesktopManager.VirtualDesktopManagerInternal.GetCount();

		/// <summary>
		/// Returns current desktop
		/// </summary>
		public static Desktop Current => new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());

		/// <summary>
		/// Create desktop object from index
		/// </summary>
		/// <param name="index">0..Count-1</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
		public static Desktop FromIndex(int index)
		{
			return new Desktop(DesktopManager.GetDesktop(index));
		}

		/// <summary>
		/// Creates desktop object on which window <paramref name="hWnd"/> is displayed
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="hWnd"/> is <c>default</c>.</exception>
		public static Desktop FromWindow(IntPtr hWnd)
		{
			try
			{
				return FromWindowInternal(hWnd);
			}
			catch (COMException exception) when ((uint)exception.HResult == 0x800706BA)
			{
				DesktopManager.Initialize();
				return FromWindowInternal(hWnd);
			}
		}

		private static Desktop FromWindowInternal(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			try
			{
				var id = DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
				if (id == default)
					return default;
				return new Desktop(DesktopManager.VirtualDesktopManagerInternal.FindDesktop(ref id));
			}
			catch (COMException exception) when ((uint)exception.HResult == 0x8002802B)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns index of desktop object or -1 if not found
		/// </summary>
		/// <param name="desktop"></param>
		/// <returns></returns>
		public static int FromDesktop(Desktop desktop)
		{
			return DesktopManager.GetDesktopIndex(desktop.ivd);
		}

		/// <summary>
		/// Create a new desktop
		/// </summary>
		public static Desktop Create()
		{
			return new Desktop(DesktopManager.VirtualDesktopManagerInternal.CreateDesktop());
		}

		/// <summary>
		/// Destroy desktop and switch to <paramref name="fallback"/>
		/// </summary>
		/// <param name="fallback">if no fallback is given use desktop to the left except for desktop 0.</param>
		public void Remove(Desktop fallback = null)
		{
			IVirtualDesktop fallbackdesktop;
			if (fallback == null)
			{ // if no fallback is given use desktop to the left except for desktop 0.
				var dtToCheck = new Desktop(DesktopManager.GetDesktop(0));
				if (Equals(dtToCheck))
				{ // desktop 0: set fallback to second desktop (= "right" desktop)
					DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 4, out fallbackdesktop); // 4 = RightDirection
				}
				else
				{ // set fallback to "left" desktop
					DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 3, out fallbackdesktop); // 3 = LeftDirection
				}
			}
			else
				// set fallback desktop
				fallbackdesktop = fallback.ivd;

			DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(ivd, fallbackdesktop);
		}

		/// <summary>
		/// Returns <true> if this desktop is the current displayed one
		/// </summary>
		public bool IsVisible
		{
			get { return ReferenceEquals(ivd, DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop()); }
		}

		/// <summary>
		/// Make this desktop visible
		/// </summary>
		public void MakeVisible()
		{
			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(ivd);
		}

		/// <summary>
		/// Returns desktop at the left of this one, null if none 
		/// </summary>
		public Desktop Left
		{
			get
			{
				int hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 3, out var desktop); // 3 = LeftDirection
				if (hr == 0)
					return new Desktop(desktop);
				else
					return null;
			}
		}

		/// <summary>
		/// Returns desktop at the right of this one, null if none
		/// </summary>
		public Desktop Right
		{
			get
			{
				int hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 4, out var desktop); // 4 = RightDirection
				if (hr == 0)
					return new Desktop(desktop);
				else
					return null;
			}
		}

		/// <summary>
		/// Move window <paramref name="hWnd"/> to this desktop
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="hWnd"/> is <c>null</c>.</exception>
		public void MoveWindow(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			GetWindowThreadProcessId(hWnd, out int processId);

			if (System.Diagnostics.Process.GetCurrentProcess().Id == processId)
			{ // window of process
				try // the easy way (if we are owner)
				{
					DesktopManager.VirtualDesktopManager.MoveWindowToDesktop(hWnd, ivd.GetId());
				}
				catch // window of process, but we are not the owner
				{
					DesktopManager.ApplicationViewCollection.GetViewForHwnd(hWnd, out var view);
					DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
				}
			}
			else
			{ // window of other process
				DesktopManager.ApplicationViewCollection.GetViewForHwnd(hWnd, out var view);
				DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
			}
		}

		/// <summary>
		/// Returns true if window <paramref name="hWnd"/> is on this desktop
		/// </summary>
		/// <param name="hWnd"></param>
		public bool HasWindow(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			return ivd.GetId() == DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
		}

		/// <summary>
		/// Returns true if window <paramref name="hWnd"/> is pinned to all desktops
		/// </summary>
		public static bool IsWindowPinned(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			return DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(hWnd.GetApplicationView());
		}

		/// <summary>
		/// pin window <paramref name="hWnd"/> to all desktops
		/// </summary>
		public static void PinWindow(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			var view = hWnd.GetApplicationView();
			if (!DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(view))
			{ // pin only if not already pinned
				DesktopManager.VirtualDesktopPinnedApps.PinView(view);
			}
		}

		/// <summary>
		/// unpin window <paramref name="hWnd"/> from all desktops 
		/// </summary>
		public static void UnpinWindow(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			var view = hWnd.GetApplicationView();
			if (DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(view))
			{ // unpin only if not already unpinned
				DesktopManager.VirtualDesktopPinnedApps.UnpinView(view);
			}
		}

		/// <summary>
		/// Returns true if application for window <paramref name="hWnd"/> is pinned to all desktops 
		/// </summary>
		public static bool IsApplicationPinned(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			return DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(DesktopManager.GetAppId(hWnd));
		}

		/// <summary>
		/// pin application for window <paramref name="hWnd"/> to all desktops
		/// </summary>
		public static void PinApplication(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			string appId = DesktopManager.GetAppId(hWnd);
			if (!DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(appId))
			{ // pin only if not already pinned
				DesktopManager.VirtualDesktopPinnedApps.PinAppID(appId);
			}
		}

		/// <summary>
		/// unpin application for window <paramref name="hWnd"/> from all desktops
		/// </summary>
		public static void UnpinApplication(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
				throw new ArgumentNullException();
			var view = hWnd.GetApplicationView();
			string appId = DesktopManager.GetAppId(hWnd);
			if (DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(appId))
			{ // unpin only if already pinned
				DesktopManager.VirtualDesktopPinnedApps.UnpinAppID(appId);
			}
		}

		/// <summary>Get process id to window handle</summary>
		[DllImport("user32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
	}
}