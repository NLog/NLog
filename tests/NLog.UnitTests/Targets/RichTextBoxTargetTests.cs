// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !MONO && !SILVERLIGHT && !__IOS__

namespace NLog.UnitTests.Targets
{
    using System.IO;
    using System.Text;
    using NLog.Config;
    using NLog.Targets;
    using System.Windows.Forms;
    using System.Drawing;
    using Xunit;

    public class RichTextBoxTargetTests : NLogTestBase
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.RichTextBoxTargetTests");

        [Fact]
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

            var form = target.TargetForm;

            Assert.True(target.CreatedForm);
            Assert.True(form.Name.StartsWith("NLog"));
            Assert.Equal(FormWindowState.Normal, form.WindowState);
            Assert.Equal("NLog", form.Text);
            Assert.Equal(300, form.Width);
            Assert.Equal(200, form.Height);

            MemoryStream ms = new MemoryStream();
            target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
            string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

            Assert.True(target.CreatedForm);

            var result = rtfText;
            Assert.True(result.Contains(@"{\colortbl ;\red255\green255\blue255;\red255\green0\blue0;\red255\green165\blue0;\red0\green0\blue0;\red128\green128\blue128;\red169\green169\blue169;}"));
            Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\b\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
            Assert.True(result.Contains(@"\cf2\highlight1\i Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
            Assert.True(result.Contains(@"\cf3\ul\b0\i0 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
            Assert.True(result.Contains(@"\cf4\ulnone Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
            Assert.True(result.Contains(@"\cf5 Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
            Assert.True(result.Contains(@"\cf6\i Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
            Assert.True(result.Contains(@"\cf0\highlight0\i0\f1\par"));
            Assert.True(result.Contains(@"}"));

            LogManager.Configuration = null;
            Assert.Null(target.TargetForm);
            Application.DoEvents();
            Assert.True(form.IsDisposed);
        }

        [Fact]
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

                var form = target.TargetForm;

                MemoryStream ms = new MemoryStream();
                target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
                string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

                Assert.True(target.CreatedForm);

                var result = rtfText;
                Assert.True(result.Contains(@"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;}"));
                Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"\cf0\highlight0\f1\par"));
                Assert.True(result.Contains(@"}"));
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
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

                var form = target.TargetForm;

                MemoryStream ms = new MemoryStream();
                target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
                string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

                Assert.True(target.CreatedForm);

                var result = rtfText;
                Assert.True(result.Contains(@"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red128\green0\blue0;}"));
                Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"\cf3 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"\cf1 Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"\cf3 Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"\cf0\highlight0\f1\par"));
                Assert.True(result.Contains(@"}"));
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void CustomWordRowColoringTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    WordColoringRules =
                    {
                        new RichTextBoxWordColoringRule("zzz", "Red", "Empty"),
                        new RichTextBoxWordColoringRule("aaa", "Green", "Empty"),
                    }
                };

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                logger.Fatal("Test zzz");
                logger.Error("Foo xxx");
                logger.Warn("Bar yyy");
                logger.Info("Test aaa");
                logger.Debug("Foo zzz");
                logger.Trace("Bar ccc");

                Application.DoEvents();

                var form = target.TargetForm;

                MemoryStream ms = new MemoryStream();
                target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
                string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

                Assert.True(target.CreatedForm);

                // "zzz" string will be highlighted

                var result = rtfText;
                Assert.True(result.Contains(@"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red255\green0\blue0;\red0\green128\blue0;}"));
                Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test \cf3\f1 zzz\cf1\f0\par"));
                Assert.True(result.Contains(@"Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo xxx\par"));
                Assert.True(result.Contains(@"Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar yyy\par"));
                Assert.True(result.Contains(@"Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test \cf4\f1 aaa\cf1\f0\par"));
                Assert.True(result.Contains(@"Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo \cf3\f1 zzz\cf1\f0\par"));
                Assert.True(result.Contains(@"Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar ccc\par"));
                Assert.True(result.Contains(@"\cf0\highlight0\f1\par"));
                Assert.True(result.Contains(@"}"));
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void RichTextBoxTargetDefaultsTest()
        {
            var target = new RichTextBoxTarget();
            Assert.False(target.UseDefaultRowColoringRules);
            Assert.Equal(0, target.WordColoringRules.Count);
            Assert.Equal(0, target.RowColoringRules.Count);
            Assert.Null(target.FormName);
            Assert.Null(target.ControlName);
        }

        [Fact]
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

                var form = target.TargetForm;
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                for (int i = 0; i < 100; ++i)
                {
                    logger.Info("Test");
                    Application.DoEvents();
                    Assert.Equal(target.TargetRichTextBox.SelectionStart, target.TargetRichTextBox.TextLength);
                    Assert.Equal(target.TargetRichTextBox.SelectionLength, 0);
                }
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
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

                Assert.Equal(0, target.MaxLines);
                target.MaxLines = 7;

                var form = target.TargetForm;
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                for (int i = 0; i < 100; ++i)
                {
                    logger.Info("Test {0}", i);
                }

                Application.DoEvents();
                string expectedText = "Test 93\nTest 94\nTest 95\nTest 96\nTest 97\nTest 98\nTest 99\n";

                Assert.Equal(expectedText, target.TargetRichTextBox.Text);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
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
            Assert.Equal(expectedRules.Length, actualRules.Count);
            for (int i = 0; i < expectedRules.Length; ++i)
            {
                Assert.Equal(expectedRules[i].BackgroundColor, actualRules[i].BackgroundColor);
                Assert.Equal(expectedRules[i].FontColor, actualRules[i].FontColor);
                Assert.Equal(expectedRules[i].Condition.ToString(), actualRules[i].Condition.ToString());
                Assert.Equal(expectedRules[i].Style, actualRules[i].Style);
            }
        }

        [Fact]
        public void ActiveFormTest()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;

                RichTextBox rtb = new RichTextBox();
                rtb.Dock = DockStyle.Fill;
                rtb.Name = "Control1";
                form.Controls.Add(rtb);
                form.Shown += (sender, e) =>
                    {
                        target.Initialize(null);
                        form.Activate();
                        Application.DoEvents();
                        Assert.Same(form, target.TargetForm);
                        Assert.Same(rtb, target.TargetRichTextBox);
                        form.Close();
                    };

                form.Show();
                Application.DoEvents();
            }
        }

        [Fact]
        public void ActiveFormTest2()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                FormName = "MyForm2",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;

                RichTextBox rtb = new RichTextBox();
                rtb.Dock = DockStyle.Fill;
                rtb.Name = "Control1";
                form.Controls.Add(rtb);
                form.Show();
                using (Form form1 = new Form())
                {
                    form1.Name = "MyForm2";
                    RichTextBox rtb2 = new RichTextBox();
                    rtb2.Dock = DockStyle.Fill;
                    rtb2.Name = "Control1";
                    form1.Controls.Add(rtb2);
                    form1.Show();
                    form1.Activate();

                    target.Initialize(null);
                    Assert.Same(form1, target.TargetForm);
                    Assert.Same(rtb2, target.TargetRichTextBox);
                }
            }
        }

        [Fact]
        public void ActiveFormNegativeTest1()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;

                //RichTextBox rtb = new RichTextBox();
                //rtb.Dock = DockStyle.Fill;
                //rtb.Name = "Control1";
                //form.Controls.Add(rtb);
                form.Show();
                try
                {
                    target.Initialize(null);
                    Assert.True(false, "Expected exception.");
                }
                catch (NLogConfigurationException ex)
                {
                    Assert.Equal("Rich text box control 'Control1' cannot be found on form 'MyForm1'.", ex.Message);
                }
            }
        }

        [Fact]
        public void ActiveFormNegativeTest2()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
            };

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;
                form.Show();

                try
                {
                    target.Initialize(null);
                    Assert.True(false, "Expected exception.");
                }
                catch (NLogConfigurationException ex)
                {
                    Assert.Equal("Rich text box control name must be specified for RichTextBoxTarget.", ex.Message);
                }
            }
        }
    }
}

#endif