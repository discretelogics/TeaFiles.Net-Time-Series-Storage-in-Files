// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime.Layout
{
    [TestClass]
    public unsafe class ByteSearcherTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        bool Match0(byte[] searchSpace, byte[] searchPattern)
        {
            fixed (byte* b = searchSpace)
            fixed (byte* s = searchPattern)
            {
                return ByteSearcher.StartsWith(b, s, searchPattern.Length);
            }
        }

        [TestMethod]
        public void MatchesTest()
        {
            this.Match0(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {4}).Should().Be.False();
            this.Match0(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {1}).Should().Be.True();
            this.Match0(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {0}).Should().Be.False();
            this.Match0(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {4, 6}).Should().Be.False();
            this.Match0(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {7, 8, 9, 0}).Should().Be.False();
            this.Match0(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {1, 2, 3, 4, 5}).Should().Be.True();
        }

        int MatchN(byte[] searchSpace, byte[] searchPattern)
        {
            fixed (byte* b = searchSpace)
            fixed (byte* s = searchPattern)
            {
                return ByteSearcher.GetPosition(b, s, searchSpace.Length, searchPattern.Length);
            }
        }

        [TestMethod]
        public void MatchNTest()
        {
            this.MatchN(new byte[] {1, 2}, new byte[] {2}).Should().Be(1);
            this.MatchN(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {4}).Should().Be(3);
            this.MatchN(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {1}).Should().Be(0);
            this.MatchN(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {0}).Should().Be(9);
            Executing.This(() => this.MatchN(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {4, 6})).Should().Throw<InternalErrorException>();
            this.MatchN(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {7, 8, 9, 0}).Should().Be(6);
            this.MatchN(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, new byte[] {1, 2, 3, 4, 5}).Should().Be(0);
        }
    }
}
