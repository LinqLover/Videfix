using Microsoft.VisualStudio.TestTools.UnitTesting;
using Videfix;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Videfix.Tests
{
	[TestClass()]
	public class OpenWindowGetterTests
	{
		[TestMethod()]
		public void GetOpenWindowsTest()
		{
			var openWindows = OpenWindowGetter.GetOpenWindows();
			Assert.IsTrue(openWindows.Any());
		}
	}
}