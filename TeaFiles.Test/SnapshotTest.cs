// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.

#if false

    // copyright discretelogics © 2011
using System;
using System.IO;
using System.Linq;
using TeaTime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace DiscreteLogics.TeaFile.Tests
{
	[TestClass]
	public class SnapshotTest
	{
		[TestMethod]
		public void SnapshotReturnsCorrectValue()
		{
			const string filename = "snapshotreturnscorrectvalue.tea";
			using (var tf = TeaFile<int>.Create(filename))
			{
				tf.Write(Enumerable.Range(3, 5));
			}

			var s = TeaTime.TeaFile.GetSnapshot(filename);

			s.First.Values[0].Should().Be(3);
			s.Last.Values[0].Should().Be(7);
			s.Count.Should().Be(5);
		}
	}
}

#endif
