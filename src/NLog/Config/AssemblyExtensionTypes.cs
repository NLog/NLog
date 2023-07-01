// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Config
{
    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    internal static class AssemblyExtensionTypes
    {
        public static void RegisterTypes(ConfigurationItemFactory factory)
        {
            #pragma warning disable CS0618 // Type or member is obsolete
            factory.RegisterTypeProperties<NLog.Targets.TargetWithContext.TargetWithContextLayout>(() => null);
            factory.RegisterTypeProperties<NLog.Layouts.CsvLayout.CsvHeaderLayout>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionAndExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionExceptionExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionLayoutExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionLevelExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionLiteralExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionLoggerNameExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionMessageExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionMethodExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionNotExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionOrExpression>(() => null);
            factory.RegisterTypeProperties<NLog.Conditions.ConditionRelationalExpression>(() => null);
            factory.RegisterType<NLog.Config.LoggingRule>();
            factory.FilterFactory.RegisterType<NLog.Filters.ConditionBasedFilter>("when");
            factory.FilterFactory.RegisterType<NLog.Filters.WhenContainsFilter>("whenContains");
            factory.FilterFactory.RegisterType<NLog.Filters.WhenEqualFilter>("whenEqual");
            factory.FilterFactory.RegisterType<NLog.Filters.WhenNotContainsFilter>("whenNotContains");
            factory.FilterFactory.RegisterType<NLog.Filters.WhenNotEqualFilter>("whenNotEqual");
            factory.FilterFactory.RegisterType<NLog.Filters.WhenRepeatedFilter>("whenRepeated");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.AllEventPropertiesLayoutRenderer>("all-event-properties");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.AppDomainLayoutRenderer>("appdomain");
#if !NETSTANDARD
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.AppSettingLayoutRenderer>("appsetting");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.AssemblyVersionLayoutRenderer>("assembly-version");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.BaseDirLayoutRenderer>("basedir");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.CallSiteFileNameLayoutRenderer>("callsite-filename");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.CallSiteLayoutRenderer>("callsite");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.CallSiteLineNumberLayoutRenderer>("callsite-linenumber");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.CounterLayoutRenderer>("counter");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.CurrentDirLayoutRenderer>("currentdir");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.DateLayoutRenderer>("date");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.DbNullLayoutRenderer>("db-null");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.DirectorySeparatorLayoutRenderer>("dir-separator");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.EnvironmentLayoutRenderer>("environment");
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.EnvironmentUserLayoutRenderer>("environment-user");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-properties");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-property");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-context");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exceptiondata");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exception-data");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ExceptionLayoutRenderer>("exception");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.FileContentsLayoutRenderer>("file-contents");
            factory.RegisterTypeProperties<NLog.LayoutRenderers.FuncThreadAgnosticLayoutRenderer>(() => null);
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.GarbageCollectorInfoLayoutRenderer>("gc");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.GdcLayoutRenderer>("gdc");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.GuidLayoutRenderer>("guid");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.HostNameLayoutRenderer>("hostname");
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.IdentityLayoutRenderer>("identity");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.InstallContextLayoutRenderer>("install-context");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("level");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("loglevel");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LiteralLayoutRenderer>("literal");
            factory.RegisterTypeProperties<NLog.LayoutRenderers.LiteralWithRawValueLayoutRenderer>(() => null);
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LocalIpAddressLayoutRenderer>("local-ip");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Log4JXmlEventLayoutRenderer>("log4jxmlevent");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("loggername");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("logger");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.LongDateLayoutRenderer>("longdate");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.MachineNameLayoutRenderer>("machinename");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.MdcLayoutRenderer>("mdc");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.MdlcLayoutRenderer>("mdlc");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.MessageLayoutRenderer>("message");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.NdcLayoutRenderer>("ndc");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.NdlcLayoutRenderer>("ndlc");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.NdlcTimingLayoutRenderer>("ndlctiming");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.NewLineLayoutRenderer>("newline");
#if !NETSTANDARD1_3
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.NLogDirLayoutRenderer>("nlogdir");
#endif
#if !NETSTANDARD1_3
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessDirLayoutRenderer>("processdir");
#endif
#if !NETSTANDARD1_3
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessIdLayoutRenderer>("processid");
#endif
#if !NETSTANDARD1_3
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessInfoLayoutRenderer>("processinfo");
#endif
#if !NETSTANDARD1_3
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessNameLayoutRenderer>("processname");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessTimeLayoutRenderer>("processtime");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextIndentLayoutRenderer>("scopeindent");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("scopenested");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("scopeproperty");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("scopetiming");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.SequenceIdLayoutRenderer>("sequenceid");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ShortDateLayoutRenderer>("shortdate");
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderApplicationDataLayoutRenderer>("userApplicationDataDir");
#endif
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderCommonApplicationDataLayoutRenderer>("commonApplicationDataDir");
#endif
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderLayoutRenderer>("specialfolder");
#endif
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderLocalApplicationDataLayoutRenderer>("userLocalApplicationDataDir");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.StackTraceLayoutRenderer>("stacktrace");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.TempDirLayoutRenderer>("tempdir");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ThreadIdLayoutRenderer>("threadid");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.ThreadNameLayoutRenderer>("threadname");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.TicksLayoutRenderer>("ticks");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.TimeLayoutRenderer>("time");
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.TraceActivityIdLayoutRenderer>("activityid");
#endif
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.VariableLayoutRenderer>("var");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("cached");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("Cached");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("ClearCache");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("CachedSeconds");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("filesystem-normalize");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("FSNormalize");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("json-encode");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("JsonEncode");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("left");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("Truncate");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("lowercase");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("Lowercase");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("ToLower");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("norawvalue");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("NoRawValue");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("Object-Path");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("ObjectPath");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.OnExceptionLayoutRendererWrapper>("onexception");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.OnHasPropertiesLayoutRendererWrapper>("onhasproperties");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("pad");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("Padding");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("PadCharacter");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("FixedLength");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("AlignmentOnTruncation");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper>("replace");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("replace-newlines");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("ReplaceNewLines");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.RightLayoutRendererWrapper>("right");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.Rot13LayoutRendererWrapper>("rot13");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.SubstringLayoutRendererWrapper>("substring");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("trim-whitespace");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("TrimWhiteSpace");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("uppercase");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("Uppercase");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("ToUpper");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UrlEncodeLayoutRendererWrapper>("url-encode");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("whenEmpty");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("WhenEmpty");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("when");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("When");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("wrapline");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("WrapLine");
            factory.LayoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("xml-encode");
            factory.AmbientRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("XmlEncode");
            factory.LayoutFactory.RegisterType<NLog.Layouts.CompoundLayout>("CompoundLayout");
            factory.RegisterType<NLog.Layouts.CsvColumn>();
            factory.LayoutFactory.RegisterType<NLog.Layouts.CsvLayout>("CsvLayout");
            factory.LayoutFactory.RegisterType<NLog.Layouts.JsonArrayLayout>("JsonArrayLayout");
            factory.RegisterType<NLog.Layouts.JsonAttribute>();
            factory.LayoutFactory.RegisterType<NLog.Layouts.JsonLayout>("JsonLayout");
            factory.LayoutFactory.RegisterType<NLog.Layouts.LayoutWithHeaderAndFooter>("LayoutWithHeaderAndFooter");
            factory.LayoutFactory.RegisterType<NLog.Layouts.Log4JXmlEventLayout>("Log4JXmlEventLayout");
            factory.LayoutFactory.RegisterType<NLog.Layouts.SimpleLayout>("SimpleLayout");
            factory.RegisterType<NLog.Layouts.ValueTypeLayoutInfo>();
            factory.RegisterType<NLog.Layouts.XmlAttribute>();
            factory.LayoutFactory.RegisterType<NLog.Layouts.XmlLayout>("XmlLayout");
            factory.TargetFactory.RegisterType<NLog.Targets.ChainsawTarget>("Chainsaw");
#if !NETSTANDARD1_3
            factory.TargetFactory.RegisterType<NLog.Targets.ColoredConsoleTarget>("ColoredConsole");
#endif
#if !NETSTANDARD1_3
            factory.RegisterType<NLog.Targets.ConsoleRowHighlightingRule>();
#endif
#if !NETSTANDARD1_3
            factory.TargetFactory.RegisterType<NLog.Targets.ConsoleTarget>("Console");
#endif
#if !NETSTANDARD1_3
            factory.RegisterType<NLog.Targets.ConsoleWordHighlightingRule>();
#endif
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.TargetFactory.RegisterType<NLog.Targets.DebuggerTarget>("Debugger");
#endif
            factory.TargetFactory.RegisterType<NLog.Targets.DebugSystemTarget>("DebugSystem");
            factory.TargetFactory.RegisterType<NLog.Targets.DebugTarget>("Debug");
#if !NETSTANDARD
            factory.TargetFactory.RegisterType<NLog.Targets.EventLogTarget>("EventLog");
#endif
            factory.TargetFactory.RegisterType<NLog.Targets.FileTarget>("File");
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            factory.TargetFactory.RegisterType<NLog.Targets.MailTarget>("Mail");
            factory.TargetFactory.RegisterType<NLog.Targets.MailTarget>("Email");
            factory.TargetFactory.RegisterType<NLog.Targets.MailTarget>("Smtp");
            factory.TargetFactory.RegisterType<NLog.Targets.MailTarget>("SmtpClient");
#endif
            factory.TargetFactory.RegisterType<NLog.Targets.MemoryTarget>("Memory");
            factory.RegisterType<NLog.Targets.MethodCallParameter>();
            factory.TargetFactory.RegisterType<NLog.Targets.MethodCallTarget>("MethodCall");
            factory.TargetFactory.RegisterType<NLog.Targets.NetworkTarget>("Network");
            factory.RegisterType<NLog.Targets.NLogViewerParameterInfo>();
            factory.TargetFactory.RegisterType<NLog.Targets.NLogViewerTarget>("NLogViewer");
            factory.TargetFactory.RegisterType<NLog.Targets.NullTarget>("Null");
            factory.RegisterType<NLog.Targets.TargetPropertyWithContext>();
#if !NETSTANDARD1_3
            factory.TargetFactory.RegisterType<NLog.Targets.TraceTarget>("Trace");
            factory.TargetFactory.RegisterType<NLog.Targets.TraceTarget>("TraceSystem");
#endif
            factory.TargetFactory.RegisterType<NLog.Targets.WebServiceTarget>("WebService");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.AsyncTargetWrapper>("AsyncWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.AutoFlushTargetWrapper>("AutoFlushWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.BufferingTargetWrapper>("BufferingWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.FallbackGroupTarget>("FallbackGroup");
            factory.RegisterType<NLog.Targets.Wrappers.FilteringRule>();
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.FilteringTargetWrapper>("FilteringWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.GroupByTargetWrapper>("GroupByWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.LimitingTargetWrapper>("LimitingWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.PostFilteringTargetWrapper>("PostFilteringWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.RandomizeGroupTarget>("RandomizeGroup");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.RepeatingTargetWrapper>("RepeatingWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.RetryingTargetWrapper>("RetryingWrapper");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.RoundRobinGroupTarget>("RoundRobinGroup");
            factory.TargetFactory.RegisterType<NLog.Targets.Wrappers.SplitGroupTarget>("SplitGroup");
            factory.TimeSourceFactory.RegisterType<NLog.Time.AccurateLocalTimeSource>("AccurateLocal");
            factory.TimeSourceFactory.RegisterType<NLog.Time.AccurateUtcTimeSource>("AccurateUTC");
            factory.TimeSourceFactory.RegisterType<NLog.Time.FastLocalTimeSource>("FastLocal");
            factory.TimeSourceFactory.RegisterType<NLog.Time.FastUtcTimeSource>("FastUTC");
            factory.ConditionMethodFactory.RegisterOneParameter("length", (logEvent,arg1) => NLog.Conditions.ConditionMethods.Length(arg1?.ToString()));
            factory.ConditionMethodFactory.RegisterTwoParameters("equals", (logEvent,arg1,arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()));
            factory.ConditionMethodFactory.RegisterTwoParameters("strequals", (logEvent,arg1,arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()));
            factory.ConditionMethodFactory.RegisterThreeParameters("strequals", (logEvent,arg1,arg2,arg3) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.ConditionMethodFactory.RegisterTwoParameters("contains", (logEvent,arg1,arg2) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString()));
            factory.ConditionMethodFactory.RegisterThreeParameters("contains", (logEvent,arg1,arg2,arg3) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.ConditionMethodFactory.RegisterTwoParameters("starts-with", (logEvent,arg1,arg2) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString()));
            factory.ConditionMethodFactory.RegisterThreeParameters("starts-with", (logEvent,arg1,arg2,arg3) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.ConditionMethodFactory.RegisterTwoParameters("ends-with", (logEvent,arg1,arg2) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString()));
            factory.ConditionMethodFactory.RegisterThreeParameters("ends-with", (logEvent,arg1,arg2,arg3) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.ConditionMethodFactory.RegisterTwoParameters("regex-matches", (logEvent,arg1,arg2) => NLog.Conditions.ConditionMethods.RegexMatches(arg1?.ToString(), arg2?.ToString()));
            factory.ConditionMethodFactory.RegisterThreeParameters("regex-matches", (logEvent,arg1,arg2,arg3) => NLog.Conditions.ConditionMethods.RegexMatches(arg1?.ToString(), arg2?.ToString(), arg3?.ToString() ?? string.Empty));

            #pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}