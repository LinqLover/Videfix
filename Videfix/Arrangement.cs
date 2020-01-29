using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Videfix
{
	[DataContract]
	public class Arrangement {
		public Arrangement() : this(Enumerable.Empty<WindowInfo>())
		{
		}

		/// <exception cref="ArgumentNullException"><paramref name="windowInfos"/> is <c>null</c>.</exception>
		public Arrangement(IEnumerable<WindowInfo> windowInfos)
		{
			WindowInfos = windowInfos?.ToArray() ?? throw new ArgumentNullException(nameof(windowInfos));
		}

		[DataMember]
		public WindowInfo[] WindowInfos { get; private set; }
	}
}