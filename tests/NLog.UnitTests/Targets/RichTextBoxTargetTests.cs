// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System.Diagnostics;

#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.UnitTests.Targets
{
    using System;
    using System.IO;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    using System.Threading;
    using System.Windows.Forms;
    using System.Drawing;

    [TestClass]
    public class RichTextBoxTargetTests : NLogTestBase
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.RichTextBoxTargetTests");

        [TestMethod]
        public void SimpleRichTextBoxTargetTest()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            logger.Fatal("Test");
            logger.Error("Foo");
            logger.Warn("Bar");
            logger.Info("Test");
            logger.Debug("Foo");
            logger.Trace("Bar");

            Application.DoEvents();

            var form = target.Form;

            Assert.IsTrue(target.CreatedForm);
            Assert.IsTrue(form.Name.StartsWith("NLog"));
            Assert.AreEqual(FormWindowState.Normal, form.WindowState);
            Assert.AreEqual("NLog", form.Text);
            Assert.AreEqual(300, form.Width);
            Assert.AreEqual(200, form.Height);

            MemoryStream ms = new MemoryStream();
            target.RichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
            string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

            Assert.IsTrue(target.CreatedForm);

            string expectedRtf = @"{\colortbl ;\red255\green255\blue255;\red255\green0\blue0;\red255\green165\blue0;\red0\green0\blue0;\red128\green128\blue128;\red169\green169\blue169;}
\viewkind4\uc1\pard\cf1\highlight2\b\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
\cf2\highlight1\i Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
\cf3\ul\b0\i0 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf4\ulnone Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
\cf5 Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
\cf6\i Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf0\highlight0\i0\f1\par
}";
            Assert.IsTrue(rtfText.Contains(expectedRtf), "Invalid RTF: " + rtfText);

            LogManager.Configuration = null;
            Assert.IsNull(target.Form);
            Application.DoEvents();
            Assert.IsTrue(form.IsDisposed);
        }

        [TestMethod]
        public void NoColoringTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                };

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                logger.Fatal("Test");
                logger.Error("Foo");
                logger.Warn("Bar");
                logger.Info("Test");
                logger.Debug("Foo");
                logger.Trace("Bar");

                Application.DoEvents();

                var form = target.Form;

                MemoryStream ms = new MemoryStream();
                target.RichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
                string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

                Assert.IsTrue(target.CreatedForm);

                string expectedRtf = @"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;}
\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf0\highlight0\f1\par
}";
                Assert.IsTrue(rtfText.Contains(expectedRtf), "Invalid RTF: " + rtfText);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [TestMethod]
        public void CustomRowColoringTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    RowColoringRules =
                    {
                        new RichTextBoxRowColoringRule("starts-with(message, 'B')", "Maroon", "Empty"),
                    }
                };

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                logger.Fatal("Test");
                logger.Error("Foo");
                logger.Warn("Bar");
                logger.Info("Test");
                logger.Debug("Foo");
                logger.Trace("Bar");

                Application.DoEvents();

                var form = target.Form;

                MemoryStream ms = new MemoryStream();
                target.RichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
                string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

                Assert.IsTrue(target.CreatedForm);

                string expectedRtf = @"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red128\green0\blue0;}
\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
\cf3 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf1 Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
\cf3 Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf0\highlight0\f1\par
}";
                Assert.IsTrue(rtfText.Contains(expectedRtf), "Invalid RTF: " + rtfText);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [TestMethod]
        public void RichTextBoxTargetDefaultsTest()
        {
            var target = new RichTextBoxTarget();
            Assert.IsFalse(target.UseDefaultRowColoringRules);
            Assert.AreEqual(0, target.WordColoringRules.Count);
            Assert.AreEqual(0, target.RowColoringRules.Count);
            Assert.IsNull(target.FormName);
            Assert.IsNull(target.ControlName);
        }

        [TestMethod]
        public void AutoScrollTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    AutoScroll = true,
                };

                var form = target.Form;
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                for (int i = 0; i < 100; ++i)
                {
                    logger.Info("Test");
                    Application.DoEvents();
                    Assert.AreEqual(target.RichTextBox.SelectionStart, target.RichTextBox.TextLength);
                    Assert.AreEqual(target.RichTextBox.SelectionLength, 0);
                }
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [TestMethod]
        public void MaxLinesTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    AutoScroll = true,
                };

                Assert.AreEqual(0, target.MaxLines);
                target.MaxLines = 7;

                var form = target.Form;
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                for (int i = 0; i < 100; ++i)
                {
                    logger.Info("Test {0}", i);
                }

                Application.DoEvents();
                string expectedText = "Test 93\nTest 94\nTest 95\nTest 96\nTest 97\nTest 98\nTest 99\n";

                Assert.AreEqual(expectedText, target.RichTextBox.Text);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [TestMethod]
        public void ColoringRuleDefaults()
        {
            var expectedRules = new[]
            {
                new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic),
                new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline),
                new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic),
            };

            var actualRules = RichTextBoxTarget.DefaultRowColoringRules;
            Assert.AreEqual(expectedRules.Length, actualRules.Count);
            for (int i = 0; i < expectedRules.Length; ++i)
            {
                Assert.AreEqual(expectedRules[i].BackgroundColor, actualRules[i].BackgroundColor);
                Assert.AreEqual(expectedRules[i].FontColor, actualRules[i].FontColor);
                Assert.AreEqual(expectedRules[i].Condition.ToString(), actualRules[i].Condition.ToString());
                Assert.AreEqual(expectedRules[i].Style, actualRules[i].Style);
            }
        }
    }
}

#endif