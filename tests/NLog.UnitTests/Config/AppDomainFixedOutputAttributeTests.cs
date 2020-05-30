using NLog.Config;
using System;
using System.Reflection;
using NLog.Internal;
using Xunit;

namespace NLog.UnitTests.Config
{
    public class AppDomainFixedOutputAttributeTests
    {
        [Fact]
        public void IRawValueRenderer_AppDomainFixedOutput_Attribute_NotRequired()
        {
            var allTypes = typeof(IRawValue).Assembly.SafeGetTypes();
            foreach (Type type in allTypes)
            {
                if (typeof(NLog.Internal.IRawValue).IsAssignableFrom(type) && !type.IsInterface)
                {
                    var appDomainFixedOutputAttribute = type.GetCustomAttribute<AppDomainFixedOutputAttribute>();
                    Assert.True(ReferenceEquals(appDomainFixedOutputAttribute, null), $"{type.ToString()} should not be marked with [AppDomainFixedOutput]");
                }
            }
        }
    }
}
