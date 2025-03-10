//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        public static void RegisterTargetTypes(ConfigurationItemFactory factory)
        {
            factory.RegisterTypeProperties<NLog.Targets.TargetWithContext.TargetWithContextLayout>(() => null);
#if NETFRAMEWORK
            factory.GetTargetFactory().RegisterType<NLog.Targets.EventLogTarget>("EventLog");
#endif
            factory.GetTargetFactory().RegisterType<NLog.Targets.ColoredConsoleTarget>("ColoredConsole");
            factory.RegisterType<NLog.Targets.ConsoleRowHighlightingRule>();
            factory.GetTargetFactory().RegisterType<NLog.Targets.ConsoleTarget>("Console");
            factory.RegisterType<NLog.Targets.ConsoleWordHighlightingRule>();
            factory.GetTargetFactory().RegisterType<NLog.Targets.DebuggerTarget>("Debugger");
            factory.GetTargetFactory().RegisterType<NLog.Targets.DebugSystemTarget>("DebugSystem");
            factory.GetTargetFactory().RegisterType<NLog.Targets.DebugTarget>("Debug");
            factory.GetTargetFactory().RegisterType<NLog.Targets.FileTarget>("File");
            factory.GetTargetFactory().RegisterType<NLog.Targets.MemoryTarget>("Memory");
            factory.RegisterType<NLog.Targets.MethodCallParameter>();
            factory.GetTargetFactory().RegisterType<NLog.Targets.MethodCallTarget>("MethodCall");
            factory.GetTargetFactory().RegisterType<NLog.Targets.NullTarget>("Null");
            factory.RegisterType<NLog.Targets.TargetPropertyWithContext>();
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.AsyncTargetWrapper>("AsyncWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.AutoFlushTargetWrapper>("AutoFlushWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.BufferingTargetWrapper>("BufferingWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.FallbackGroupTarget>("FallbackGroup");
            factory.RegisterType<NLog.Targets.Wrappers.FilteringRule>();
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.FilteringTargetWrapper>("FilteringWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.GroupByTargetWrapper>("GroupByWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.LimitingTargetWrapper>("LimitingWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.PostFilteringTargetWrapper>("PostFilteringWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RandomizeGroupTarget>("RandomizeGroup");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RepeatingTargetWrapper>("RepeatingWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RetryingTargetWrapper>("RetryingWrapper");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RoundRobinGroupTarget>("RoundRobinGroup");
            factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.SplitGroupTarget>("SplitGroup");
        }

        public static void RegisterLayoutTypes(ConfigurationItemFactory factory)
        {
            factory.RegisterTypeProperties<NLog.Layouts.CsvLayout.CsvHeaderLayout>(() => null);
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.CompoundLayout>("CompoundLayout");
            factory.RegisterType<NLog.Layouts.CsvColumn>();
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.CsvLayout>("CsvLayout");
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.JsonArrayLayout>("JsonArrayLayout");
            factory.RegisterType<NLog.Layouts.JsonAttribute>();
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.JsonLayout>("JsonLayout");
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.LayoutWithHeaderAndFooter>("LayoutWithHeaderAndFooter");
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.SimpleLayout>("SimpleLayout");
            factory.RegisterType<NLog.Layouts.ValueTypeLayoutInfo>();
            factory.RegisterType<NLog.Layouts.XmlAttribute>();
            factory.GetLayoutFactory().RegisterType<NLog.Layouts.XmlLayout>("XmlLayout");
        }

        public static void RegisterLayoutRendererTypes(ConfigurationItemFactory factory)
        {
#if NETFRAMEWORK
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AppSettingLayoutRenderer>("appsetting");
#endif
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AllEventPropertiesLayoutRenderer>("all-event-properties");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AppDomainLayoutRenderer>("appdomain");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AssemblyVersionLayoutRenderer>("assembly-version");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.BaseDirLayoutRenderer>("basedir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CallSiteFileNameLayoutRenderer>("callsite-filename");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CallSiteLayoutRenderer>("callsite");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CallSiteLineNumberLayoutRenderer>("callsite-linenumber");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CounterLayoutRenderer>("counter");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CurrentDirLayoutRenderer>("currentdir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.DateLayoutRenderer>("date");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.DbNullLayoutRenderer>("db-null");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.DirectorySeparatorLayoutRenderer>("dir-separator");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EnvironmentLayoutRenderer>("environment");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EnvironmentUserLayoutRenderer>("environment-user");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-properties");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-property");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-context");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exceptiondata");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exception-data");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ExceptionLayoutRenderer>("exception");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.FileContentsLayoutRenderer>("file-contents");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.GarbageCollectorInfoLayoutRenderer>("gc");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.GdcLayoutRenderer>("gdc");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.GuidLayoutRenderer>("guid");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.HostNameLayoutRenderer>("hostname");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.IdentityLayoutRenderer>("identity");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.InstallContextLayoutRenderer>("install-context");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("level");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("loglevel");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LiteralLayoutRenderer>("literal");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("loggername");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("logger");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LongDateLayoutRenderer>("longdate");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.MachineNameLayoutRenderer>("machinename");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.MessageLayoutRenderer>("message");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.NewLineLayoutRenderer>("newline");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.NLogDirLayoutRenderer>("nlogdir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessDirLayoutRenderer>("processdir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessIdLayoutRenderer>("processid");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessInfoLayoutRenderer>("processinfo");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessNameLayoutRenderer>("processname");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessTimeLayoutRenderer>("processtime");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextIndentLayoutRenderer>("scopeindent");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("scopenested");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("ndc");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("ndlc");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("scopeproperty");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("mdc");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("mdlc");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("scopetiming");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("ndlctiming");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SequenceIdLayoutRenderer>("sequenceid");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ShortDateLayoutRenderer>("shortdate");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderApplicationDataLayoutRenderer>("userApplicationDataDir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderCommonApplicationDataLayoutRenderer>("commonApplicationDataDir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderLayoutRenderer>("specialfolder");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderLocalApplicationDataLayoutRenderer>("userLocalApplicationDataDir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.StackTraceLayoutRenderer>("stacktrace");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.TempDirLayoutRenderer>("tempdir");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ThreadIdLayoutRenderer>("threadid");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ThreadNameLayoutRenderer>("threadname");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.TicksLayoutRenderer>("ticks");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.TimeLayoutRenderer>("time");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.VariableLayoutRenderer>("var");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("cached");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("Cached");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("ClearCache");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("CachedSeconds");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("filesystem-normalize");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("FSNormalize");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("json-encode");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("JsonEncode");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("left");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("Truncate");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("lowercase");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("Lowercase");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("ToLower");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("norawvalue");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("NoRawValue");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("Object-Path");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("ObjectPath");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.OnExceptionLayoutRendererWrapper>("onexception");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.OnHasPropertiesLayoutRendererWrapper>("onhasproperties");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("pad");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("Padding");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("PadCharacter");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("FixedLength");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("AlignmentOnTruncation");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper>("replace");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("replace-newlines");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("ReplaceNewLines");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.RightLayoutRendererWrapper>("right");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.Rot13LayoutRendererWrapper>("rot13");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.SubstringLayoutRendererWrapper>("substring");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("trim-whitespace");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("TrimWhiteSpace");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("uppercase");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("Uppercase");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("ToUpper");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UrlEncodeLayoutRendererWrapper>("url-encode");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("whenEmpty");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("WhenEmpty");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("when");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("When");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("wrapline");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("WrapLine");
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("xml-encode");
            factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("XmlEncode");
        }

        public static void RegisterFilterTypes(ConfigurationItemFactory factory)
        {
            factory.GetFilterFactory().RegisterType<NLog.Filters.ConditionBasedFilter>("when");
            factory.GetFilterFactory().RegisterType<NLog.Filters.WhenContainsFilter>("whenContains");
            factory.GetFilterFactory().RegisterType<NLog.Filters.WhenEqualFilter>("whenEqual");
            factory.GetFilterFactory().RegisterType<NLog.Filters.WhenNotContainsFilter>("whenNotContains");
            factory.GetFilterFactory().RegisterType<NLog.Filters.WhenNotEqualFilter>("whenNotEqual");
            factory.GetFilterFactory().RegisterType<NLog.Filters.WhenRepeatedFilter>("whenRepeated");
        }

        public static void RegisterTimeSourceTypes(ConfigurationItemFactory factory)
        {
            factory.GetTimeSourceFactory().RegisterType<NLog.Time.AccurateLocalTimeSource>("AccurateLocal");
            factory.GetTimeSourceFactory().RegisterType<NLog.Time.AccurateUtcTimeSource>("AccurateUTC");
            factory.GetTimeSourceFactory().RegisterType<NLog.Time.FastLocalTimeSource>("FastLocal");
            factory.GetTimeSourceFactory().RegisterType<NLog.Time.FastUtcTimeSource>("FastUTC");
        }

        public static void RegisterConditionTypes(ConfigurationItemFactory factory)
        {
            factory.GetConditionMethodFactory().RegisterOneParameter("length", (logEvent, arg1) => NLog.Conditions.ConditionMethods.Length(arg1?.ToString()));
            factory.GetConditionMethodFactory().RegisterTwoParameters("equals", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()));
            factory.GetConditionMethodFactory().RegisterTwoParameters("strequals", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()));
            factory.GetConditionMethodFactory().RegisterThreeParameters("strequals", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.GetConditionMethodFactory().RegisterTwoParameters("contains", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString()));
            factory.GetConditionMethodFactory().RegisterThreeParameters("contains", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.GetConditionMethodFactory().RegisterTwoParameters("starts-with", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString()));
            factory.GetConditionMethodFactory().RegisterThreeParameters("starts-with", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            factory.GetConditionMethodFactory().RegisterTwoParameters("ends-with", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString()));
            factory.GetConditionMethodFactory().RegisterThreeParameters("ends-with", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
        }
    }
}
