using System;
using WindowHandle = System.IntPtr;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HelperLib.SystemHelpers;
using MScholtes.VirtualDesktop;

namespace Videfix
{
	[DataContract]
	public class WindowInfo
	{
		public WindowInfo(WindowKey key, int desktopIndex, WindowPlacement placement)
		{
			Key = key ?? throw new ArgumentNullException(nameof(key));
			DesktopIndex = desktopIndex >= 0 ? desktopIndex : default;
			Placement = placement;
		}

		[DataMember]
		public WindowKey Key { get; private set; }

		[DataMember]
		protected int? DesktopIndex { get; private set; }

		[DataMember]
		protected WindowPlacement Placement { get; private set; }

		public override bool Equals(object obj)
		 => obj is WindowInfo second
		 && Equals(second);

		public virtual bool Equals(WindowInfo second)
		 => GetType() == second.GetType()
		 && Key.Equals(second.Key)
		 && DesktopIndex == second.DesktopIndex;

		public override int GetHashCode()
		 => (Key, DesktopIndex).GetHashCode();

		public void Apply(WindowHandle windowHandle)
		{
			if (DesktopIndex.TryGetValue(out var desktopIndex))
			{
				var desktop = desktopIndex < Desktop.Count
					? Desktop.FromIndex(desktopIndex)
					: Desktop.Create();
				desktop.MoveWindow(windowHandle);
			}

			Placement.SetPlacement(windowHandle);
		}

		public static bool TrySave(WindowKey windowKey, WindowHandle windowHandle, out WindowInfo windowInfo)
		{
			var desktop = Desktop.FromWindow(windowHandle);
			if (desktop == default)
			{
				// TODO: Are there any windows whose placement we could save or set?
				windowInfo = default;
				return false;
			}

			var windowPlacement = WindowPlacement.GetPlacement(windowHandle);

			windowInfo = new WindowInfo(windowKey, Desktop.FromDesktop(desktop), windowPlacement);
			return true;
		}
	}
}