using System;
using System.Runtime.Serialization;

namespace Videfix
{
	[DataContract]
	public class WindowKey
	{
		private WindowKey()
		{
		}

		public WindowKey(string processName, string windowTitle)
		{
			ProcessName = processName ?? throw new ArgumentNullException(nameof(processName));
			WindowTitle = windowTitle ?? throw new ArgumentNullException(nameof(windowTitle));
		}

		[DataMember]
		public string ProcessName { get; private set; }
		
		[DataMember]
		public string WindowTitle { get; private set; }

		public override bool Equals(object obj)
		 => obj is WindowKey second
		 && Equals(second);

		public virtual bool Equals(WindowKey second)
		 => GetType() == second.GetType()
		 && (ProcessName, WindowTitle) == (second.ProcessName, second.WindowTitle);

		public override int GetHashCode()
		 => (ProcessName, WindowTitle).GetHashCode();
	}
}