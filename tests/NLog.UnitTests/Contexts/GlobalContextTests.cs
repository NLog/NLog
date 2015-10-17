using NLog.Context;
using Xunit;

#pragma warning disable 0618
namespace NLog.UnitTests.Contexts
{
    /// <summary>
    /// Summary description for GlobalContextTests
    /// </summary>
    public class GlobalContextTests
    {
        [Fact]
        public void GlobalContextTest1()
        {
            GlobalContext.Instance.Clear();
            Assert.False(GlobalContext.Instance.Contains("foo"));
            Assert.Equal(string.Empty, GlobalContext.Instance.Get("foo", null));

            Assert.False(GlobalContext.Instance.Contains("foo2"));
            Assert.Equal(string.Empty, GlobalContext.Instance.Get("foo2", null));

            GlobalContext.Instance["foo"] = "bar";
            GlobalContext.Instance["foo2"] = "bar2";

            Assert.True(GlobalContext.Instance.Contains("foo"));
            Assert.Equal("bar", GlobalContext.Instance.Get("foo", null));

            GlobalContext.Instance.Remove("foo");
            Assert.False(GlobalContext.Instance.Contains("foo"));
            Assert.Equal(string.Empty, GlobalContext.Instance.Get("foo", null));

            Assert.True(GlobalContext.Instance.Contains("foo2"));
            Assert.Equal("bar2", GlobalContext.Instance.Get("foo2", null));
        }

        [Fact]
        public void GlobalContextTest2()
        {
            GlobalDiagnosticsContext.Clear();
            Assert.False(GlobalDiagnosticsContext.Contains("foo"));
            Assert.Equal(string.Empty, GlobalDiagnosticsContext.Get("foo"));
            Assert.False(GlobalDiagnosticsContext.Contains("foo2"));
            Assert.Equal(string.Empty, GlobalDiagnosticsContext.Get("foo2"));

            GlobalDiagnosticsContext.Set("foo", "bar");
            GlobalDiagnosticsContext.Set("foo2", "bar2");

            Assert.True(GlobalDiagnosticsContext.Contains("foo"));
            Assert.Equal("bar", GlobalDiagnosticsContext.Get("foo"));

            GlobalDiagnosticsContext.Remove("foo");
            Assert.False(GlobalDiagnosticsContext.Contains("foo"));
            Assert.Equal(string.Empty, GlobalDiagnosticsContext.Get("foo"));

            Assert.True(GlobalDiagnosticsContext.Contains("foo2"));
            Assert.Equal("bar2", GlobalDiagnosticsContext.Get("foo2"));
        }

        [Fact]
        public void GlobalContextTest3()
        {
            GDC.Clear();
            Assert.False(GDC.Contains("foo"));
            Assert.Equal(string.Empty, GDC.Get("foo"));
            Assert.False(GDC.Contains("foo2"));
            Assert.Equal(string.Empty, GDC.Get("foo2"));

            GDC.Set("foo", "bar");
            GDC.Set("foo2", "bar2");

            Assert.True(GDC.Contains("foo"));
            Assert.Equal("bar", GDC.Get("foo"));

            GDC.Remove("foo");
            Assert.False(GDC.Contains("foo"));
            Assert.Equal(string.Empty, GDC.Get("foo"));

            Assert.True(GDC.Contains("foo2"));
            Assert.Equal("bar2", GDC.Get("foo2"));
        }
    }
}
