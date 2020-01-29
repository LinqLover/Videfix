using Microsoft.VisualStudio.TestTools.UnitTesting;
using Videfix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HelperLib.SystemHelpers.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace Videfix.Tests
{
	[TestClass()]
	public class VidefixTests
	{
		[TestMethod()]
		public void VidefixTest()
		{
			if (MessageBox.Show("Ready to dare the test?", nameof(VidefixTests), MessageBoxButton.OKCancel) != MessageBoxResult.OK)
				Assert.Inconclusive("User denied to test");

			var oldArrangement = VidefixCore.Save();

			MessageBox.Show("Now you can move something before you click here.");

			VidefixCore.Apply(oldArrangement);

			var newArrangement = VidefixCore.Save();

			Assert.IsTrue(oldArrangement.WindowInfos.Length == newArrangement.WindowInfos.Length);
			Assert.IsTrue(EnumerableHelper.SequenceIsCongruent(oldArrangement.WindowInfos, newArrangement.WindowInfos));

			MessageBox.Show("Test done.");
		}

		[TestMethod]
		public void SerializationTest()
		{
			if (MessageBox.Show("Ready to dare the test?", nameof(VidefixTests), MessageBoxButton.OKCancel) != MessageBoxResult.OK)
				Assert.Inconclusive("User denied to test");

			string filePath = Path.GetTempFileName();


			var savedArrangement = VidefixCore.Save();

			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				var serializer = new DataContractSerializer(savedArrangement.GetType());
				serializer.WriteObject(fileStream, savedArrangement);
			}

			MessageBox.Show("Now you can move something before you click here.");


			Arrangement readArrangement;
			using (var fileStream = new FileStream(filePath, FileMode.Open))
			{
				var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
				var serializer = new DataContractSerializer(savedArrangement.GetType());
				readArrangement = (Arrangement)serializer.ReadObject(reader, true);
			}

			VidefixCore.Apply(readArrangement);

			var newArrangement = VidefixCore.Save();

			Assert.IsTrue(newArrangement.WindowInfos.Length == readArrangement.WindowInfos.Length);
			Assert.IsTrue(EnumerableHelper.SequenceIsCongruent(newArrangement.WindowInfos, readArrangement.WindowInfos));

			MessageBox.Show("Test done.");
		}
	}
}