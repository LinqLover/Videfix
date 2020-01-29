using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using HelperLib.SystemHelpers;
using HelperLib.SystemHelpers.Collections.Generic;
using HelperLib.SystemHelpers.Linq;
using MScholtes.VirtualDesktop;
using WindowHandle = System.IntPtr;

namespace Videfix
{
	/// <summary>
	/// Provides methods of the Videfix core service.
	/// </summary>
	public static class VidefixCore
	{
		/// <summary>
		/// Applies the window arrangement from the <paramref name="arrangement"/> parameter to open windows.
		/// </summary>
		/// <param name="arrangement">The target arrangement of windows.</param>
		public static void Apply(Arrangement arrangement)
		{
			var windows = OpenWindowGetter.GetOpenWindows().ToDictionary();

			foreach (var windowInfo in arrangement.WindowInfos)
				if (windows.TryGetSingle(window => window.Value.Equals(windowInfo.Key), out var singleWindow))
				{
					var windowHandle = singleWindow.Key;
					windows.Remove(windowHandle);

					windowInfo.Apply(windowHandle);
				}
		}

		/// <summary>
		/// Applies the window arrangement saved at the <paramref name="filePath"/> parameter to open windows.
		/// </summary>
		/// <param name="filePath">The path to the file the arrangement has been written into.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="filePath"/> is invalid or unavailable.</exception>
		/// <exception cref="NotSupportedException"><paramref name="filePath"/> is not supported.</exception>
		/// <exception cref="System.Security.SecurityException">Caller misses required permissions.</exception>
		/// <exception cref="FileNotFoundException">File cannot be found.</exception>
		/// <exception cref="IOException">I/O error occurred.</exception>
		/// <exception cref="DirectoryNotFoundException">Invalid <paramref name="filePath"/>.</exception>
		/// <exception cref="PathTooLongException"><paramref name="filePath"/> is too long.</exception>
		/// <exception cref="SerializationException">The file is invalid.</exception>
		public static void Apply(string filePath)
		{
			Arrangement arrangement;
			using (var fileStream = new FileStream(filePath, FileMode.Open))
			{
				var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
				var serializer = new DataContractSerializer(typeof(Arrangement));
				arrangement = (Arrangement)serializer.ReadObject(reader, true);
			}

			Apply(arrangement);
		}

		/// <summary>
		/// Collects the current set of open windows and collects a new <see cref="Arrangement"/> object.
		/// </summary>
		public static Arrangement Save()
		{
			var windows = OpenWindowGetter.GetOpenWindows();
			var windowInfos = from window in windows
							  let sef = SideEffectHelper.EliminateSideEffects<WindowKey, WindowHandle, WindowInfo, bool>(WindowInfo.TrySave, window.Value, window.Key)
							  where sef.result
							  select sef.side;

			return new Arrangement(windowInfos);
		}

		/// <summary>
		/// Collects the current set of open windows and writes it at the specified <paramref name="filePath"/>.
		/// </summary>
		/// <param name="filePath">The path where the arrangement will be written into.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="filePath"/> is invalid or unavailable.</exception>
		/// <exception cref="NotSupportedException"><paramref name="filePath"/> is not supported.</exception>
		/// <exception cref="System.Security.SecurityException">Caller misses required permissions.</exception>
		/// <exception cref="FileNotFoundException">File cannot be found.</exception>
		/// <exception cref="IOException">I/O error occurred.</exception>
		/// <exception cref="DirectoryNotFoundException">Invalid <paramref name="filePath"/>.</exception>
		/// <exception cref="PathTooLongException"><paramref name="filePath"/> is too long.</exception>
		/// <exception cref="InvalidDataContractException">Invalid file content.</exception>
		/// <exception cref="InvalidOperationException">Too many open windows.</exception>
		/// <exception cref="System.Runtime.InteropServices.COMException"></exception>
		public static void Save(string filePath)
		{
			var oldArrangement = Save();

			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				var serializer = new DataContractSerializer(oldArrangement.GetType());
				try
				{
					serializer.WriteObject(fileStream, oldArrangement);
				}
				catch (System.ServiceModel.QuotaExceededException exception)
				{
					throw new InvalidOperationException("Too many open windows.", exception);
				}
			}
		}

		public static bool CheckNeedsRestore()
		{
			var openWindows = OpenWindowGetter.GetOpenWindows();
			if (!openWindows.Any())
				return false;
			return /*openWindows
				.Select(window => Desktop.FromWindow(window.Key))
				.All(desktop => desktop.Left is null)*/ true;
		}
	}
}