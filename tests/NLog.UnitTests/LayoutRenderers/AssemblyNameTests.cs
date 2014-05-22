using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NLog.LayoutRenderers;
using NLog.Targets;
using NUnit.Framework;

using NLog;
using NLog.Config;
using NLog.Layouts;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests.LayoutRenderers {
    [TestFixture]
    public class AssemblyNameTests : NLogTestBase {
        private readonly AssemblyName assemblyName = new AssemblyName("ExampleAssembly, Version=1.0.0.0, Culture=en, PublicKeyToken=a5d015c7d5a0b012");

        [Test]
        public void SinglePropertyWithoutShowDisplayNamesTest() {
            //var assemblyName = Assembly.GetEntryAssembly().GetName();

            AssertLayoutRendererOutput("${assembly-name:format=name:Property=TestAssembly}", assemblyName.Name);
            AssertLayoutRendererOutput("${assembly-name:format=version:Property=TestAssembly}", assemblyName.Version.ToString());
            AssertLayoutRendererOutput("${assembly-name:format=culture:Property=TestAssembly}", assemblyName.CultureInfo.TwoLetterISOLanguageName);
            AssertLayoutRendererOutput("${assembly-name:format=PublicKeyToken:Property=TestAssembly}", assemblyName.GetPublicKeyToken().ToString());
        }

        [Test]
        public void SinglePropertyWithShowDisplayNamesTest() {
            AssertLayoutRendererOutput("${assembly-name:format=name:ShowDisplayNames=True:Property=TestAssembly}", "Name=" + assemblyName.Name);
            AssertLayoutRendererOutput("${assembly-name:format=version:ShowDisplayNames=True:Property=TestAssembly}", "Version=" + assemblyName.Version.ToString());
            AssertLayoutRendererOutput("${assembly-name:format=culture:ShowDisplayNames=True:Property=TestAssembly}", "Culture=" + assemblyName.CultureInfo.TwoLetterISOLanguageName);
            AssertLayoutRendererOutput("${assembly-name:format=PublicKeyToken:ShowDisplayNames=True:Property=TestAssembly}", "PublicKeyToken=" + assemblyName.GetPublicKeyToken().ToString());
        }

        [Test]
        public void MultiplePropertyWithoutShowDisplayNamesTest() {
            AssertLayoutRendererOutput("${assembly-name:format=name,version,culture:Property=TestAssembly}",
                assemblyName.Name + ", " + assemblyName.Version.ToString() + ", " + assemblyName.CultureInfo.TwoLetterISOLanguageName);
            AssertLayoutRendererOutput("${assembly-name:format=version,name:Property=TestAssembly}",
                assemblyName.Version.ToString() + ", " + assemblyName.Name);
            AssertLayoutRendererOutput("${assembly-name:format=culture, PublicKeyToken:Property=TestAssembly}",
                assemblyName.CultureInfo.TwoLetterISOLanguageName + ", " + assemblyName.GetPublicKeyToken().ToString());
        }
        [Test]
        public void MultiplePropertyWithShowDisplayNamesTest() {
            AssertLayoutRendererOutput("${assembly-name:format=name,version,culture:ShowDisplayNames=True:Property=TestAssembly}",
                "Name=" + assemblyName.Name + ", " + 
                "Version=" + assemblyName.Version.ToString() + ", " + 
                "Culture=" + assemblyName.CultureInfo.TwoLetterISOLanguageName);
            AssertLayoutRendererOutput("${assembly-name:format=version,name:ShowDisplayNames=True:Property=TestAssembly}",
                "Version=" + assemblyName.Version.ToString() + ", " + 
                "Name=" + assemblyName.Name);
            AssertLayoutRendererOutput("${assembly-name:format=culture, PublicKeyToken:ShowDisplayNames=True:Property=TestAssembly}",
                "Culture=" + assemblyName.CultureInfo.TwoLetterISOLanguageName + ", " + 
                "PublicKeyToken=" + assemblyName.GetPublicKeyToken().ToString());
        }

        [Test]
        public void CheckForNullAssembly() {
            AssertLayoutRendererOutput("${assembly-name:format=name,version,culture:ShowDisplayNames=True:Property=EmptyAssembly}", "");
            AssertLayoutRendererOutput("${assembly-name:format=name,version,culture:Property=EmptyAssembly}", "");
        }

        [Test]
        public void TestCustomSeparator() {
            var separator = "-";
            AssertLayoutRendererOutput("${assembly-name:format=name,version:Separator=-:Property=TestAssembly}",
                String.Format("{1}{0}{2}", separator,assemblyName.Name, assemblyName.Version.ToString()));
            separator = "";
            AssertLayoutRendererOutput("${assembly-name:format=name,version:Separator=:Property=TestAssembly}",
                String.Format("{1}{0}{2}", separator,assemblyName.Name, assemblyName.Version.ToString()));
            separator = " - ";
            AssertLayoutRendererOutput("${assembly-name:format=name,version:Separator= - :Property=TestAssembly}",
                String.Format("{1}{0}{2}",separator, assemblyName.Name, assemblyName.Version.ToString()));
            AssertLayoutRendererOutput("${assembly-name:format=name,version:Separator= - :ShowDisplayNames=True:Property=TestAssembly}",
                String.Format("Name={1}{0}Version={2}", separator, assemblyName.Name, assemblyName.Version.ToString()));
            AssertLayoutRendererOutput("${assembly-name:format=name:Separator= - :Property=TestAssembly}",
                assemblyName.Name);
        }


    }
}
