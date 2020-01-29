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
	internal static class DesktopManager
	{
		static DesktopManager()
		{
			Initialize();
		}

		internal static IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
		internal static IVirtualDesktopManager VirtualDesktopManager;
		internal static IApplicationViewCollection ApplicationViewCollection;
		internal static IVirtualDesktopPinnedApps VirtualDesktopPinnedApps;

		internal static void Initialize()
		{
			var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));
			VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(Guids.CLSID_VirtualDesktopManagerInternal, typeof(IVirtualDesktopManagerInternal).GUID);
			VirtualDesktopManager = (IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_VirtualDesktopManager));
			ApplicationViewCollection = (IApplicationViewCollection)shell.QueryService(typeof(IApplicationViewCollection).GUID, typeof(IApplicationViewCollection).GUID);
			VirtualDesktopPinnedApps = (IVirtualDesktopPinnedApps)shell.QueryService(Guids.CLSID_VirtualDesktopPinnedApps, typeof(IVirtualDesktopPinnedApps).GUID);
		}

		/// <summary>
		/// get desktop with index
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
		internal static IVirtualDesktop GetDesktop(int index)
		{
			int count = VirtualDesktopManagerInternal.GetCount();
			if (index < 0 || index >= count)
				throw new ArgumentOutOfRangeException("index");
			VirtualDesktopManagerInternal.GetDesktops(out var desktops);
			desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out object objdesktop);
			Marshal.ReleaseComObject(desktops);
			return (IVirtualDesktop)objdesktop;
		}

		/// <summary>
		/// get index of desktop
		/// </summary>
		internal static int GetDesktopIndex(IVirtualDesktop desktop)
		{
			int index = -1;
			var IdSearch = desktop.GetId();
			VirtualDesktopManagerInternal.GetDesktops(out var desktops);
			for (int i = 0; i < VirtualDesktopManagerInternal.GetCount(); i++)
			{
				desktops.GetAt(i, typeof(IVirtualDesktop).GUID, out object objdesktop);
				if (IdSearch.CompareTo(((IVirtualDesktop)objdesktop).GetId()) == 0)
				{
					index = i;
					break;
				}
			}
			Marshal.ReleaseComObject(desktops);
			return index;
		}

		/// <summary>
		/// get application view to window handle
		/// </summary>
		internal static IApplicationView GetApplicationView(this IntPtr hWnd)
		{
			ApplicationViewCollection.GetViewForHwnd(hWnd, out var view);
			return view;
		}

		/// <summary>
		/// get Application ID to window handle
		/// </summary>
		internal static string GetAppId(IntPtr hWnd)
		{
			hWnd.GetApplicationView().GetAppUserModelId(out string appId);
			return appId;
		}
	}
}