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
#pragma warning disable CS0618 // Type or member is obsolete
        public static void RegisterTargetTypes(ConfigurationItemFactory factory, bool checkTypeExists)
        {
            var targetFactory = factory.GetTargetFactory();

            factory.RegisterTypeProperties<NLog.Targets.TargetWithContext.TargetWithContextLayout>(() => null);
#if NETFRAMEWORK
            targetFactory.RegisterType<NLog.Targets.EventLogTarget>("EventLog", checkTypeExists);
#endif
            targetFactory.RegisterType<NLog.Targets.ColoredConsoleTarget>("ColoredConsole", checkTypeExists);
            factory.RegisterType<NLog.Targets.ConsoleRowHighlightingRule>();
            targetFactory.RegisterType<NLog.Targets.ConsoleTarget>("Console", checkTypeExists);
            factory.RegisterType<NLog.Targets.ConsoleWordHighlightingRule>();
            targetFactory.RegisterType<NLog.Targets.DebuggerTarget>("Debugger", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.DebugSystemTarget>("DebugSystem", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.DebugTarget>("Debug", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.FileTarget>("File", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.MemoryTarget>("Memory", checkTypeExists);
            factory.RegisterType<NLog.Targets.MethodCallParameter>();
            targetFactory.RegisterType<NLog.Targets.MethodCallTarget>("MethodCall", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.NullTarget>("Null", checkTypeExists);
            factory.RegisterType<NLog.Targets.TargetPropertyWithContext>();
            targetFactory.RegisterType<NLog.Targets.Wrappers.AsyncTargetWrapper>("AsyncWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.AutoFlushTargetWrapper>("AutoFlushWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.BufferingTargetWrapper>("BufferingWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.FallbackGroupTarget>("FallbackGroup", checkTypeExists);
            factory.RegisterType<NLog.Targets.Wrappers.FilteringRule>();
            targetFactory.RegisterType<NLog.Targets.Wrappers.FilteringTargetWrapper>("FilteringWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.GroupByTargetWrapper>("GroupByWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.LimitingTargetWrapper>("LimitingWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.PostFilteringTargetWrapper>("PostFilteringWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.RandomizeGroupTarget>("RandomizeGroup", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.RepeatingTargetWrapper>("RepeatingWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.RetryingTargetWrapper>("RetryingWrapper", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.RoundRobinGroupTarget>("RoundRobinGroup", checkTypeExists);
            targetFactory.RegisterType<NLog.Targets.Wrappers.SplitGroupTarget>("SplitGroup", checkTypeExists);
        }

        public static void RegisterLayoutTypes(ConfigurationItemFactory factory, bool checkTypeExists)
        {
            var layoutFactory = factory.GetLayoutFactory();

            factory.RegisterTypeProperties<NLog.Layouts.CsvLayout.CsvHeaderLayout>(() => null);
            layoutFactory.RegisterType<NLog.Layouts.CompoundLayout>("CompoundLayout", checkTypeExists);
            factory.RegisterType<NLog.Layouts.CsvColumn>();
            layoutFactory.RegisterType<NLog.Layouts.CsvLayout>("CsvLayout", checkTypeExists);
            layoutFactory.RegisterType<NLog.Layouts.JsonArrayLayout>("JsonArrayLayout", checkTypeExists);
            factory.RegisterType<NLog.Layouts.JsonAttribute>();
            layoutFactory.RegisterType<NLog.Layouts.JsonLayout>("JsonLayout", checkTypeExists);
            layoutFactory.RegisterType<NLog.Layouts.LayoutWithHeaderAndFooter>("LayoutWithHeaderAndFooter", checkTypeExists);
            layoutFactory.RegisterType<NLog.Layouts.SimpleLayout>("SimpleLayout", checkTypeExists);
            factory.RegisterType<NLog.Layouts.ValueTypeLayoutInfo>();
            factory.RegisterType<NLog.Layouts.XmlAttribute>();
            layoutFactory.RegisterType<NLog.Layouts.XmlLayout>("XmlLayout", checkTypeExists);
        }

        public static void RegisterLayoutRendererTypes(ConfigurationItemFactory factory, bool checkTypeExists)
        {
            var layoutRendererFactory = factory.GetLayoutRendererFactory();
            var ambientPropertyFactory = factory.GetAmbientPropertyFactory();

#if NETFRAMEWORK
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.AppSettingLayoutRenderer>("appsetting", checkTypeExists);
#endif
            factory.RegisterTypeProperties<NLog.LayoutRenderers.LiteralWithRawValueLayoutRenderer>(() => null);
            factory.RegisterTypeProperties<NLog.LayoutRenderers.FuncLayoutRenderer>(() => null);
            factory.RegisterTypeProperties<NLog.LayoutRenderers.FuncThreadAgnosticLayoutRenderer>(() => null);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.AllEventPropertiesLayoutRenderer>("alleventproperties", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.AppDomainLayoutRenderer>("appdomain", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.AssemblyVersionLayoutRenderer>("assemblyversion", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.BaseDirLayoutRenderer>("basedir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.CallSiteFileNameLayoutRenderer>("callsitefilename", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.CallSiteLayoutRenderer>("callsite", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.CallSiteLineNumberLayoutRenderer>("callsitelinenumber", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.CounterLayoutRenderer>("counter", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.CurrentDirLayoutRenderer>("currentdir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.DateLayoutRenderer>("date", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.DateTimeOffsetLayoutRenderer>("datetimeoffset", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.DbNullLayoutRenderer>("dbnull", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.DirectorySeparatorLayoutRenderer>("dirseparator", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.EnvironmentLayoutRenderer>("environment", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.EnvironmentUserLayoutRenderer>("environmentuser", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("eventproperties", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("eventproperty", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("eventcontext", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exceptiondata", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ExceptionLayoutRenderer>("exception", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.GarbageCollectorInfoLayoutRenderer>("gc", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.GdcLayoutRenderer>("gdc", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.GuidLayoutRenderer>("guid", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.HostNameLayoutRenderer>("hostname", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.IdentityLayoutRenderer>("identity", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.InstallContextLayoutRenderer>("installcontext", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("level", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("loglevel", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.LiteralLayoutRenderer>("literal", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("loggername", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("logger", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.LongDateLayoutRenderer>("longdate", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.MachineNameLayoutRenderer>("machinename", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.MessageLayoutRenderer>("message", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.NewLineLayoutRenderer>("newline", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.NLogDirLayoutRenderer>("nlogdir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessDirLayoutRenderer>("processdir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessIdLayoutRenderer>("processid", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessInfoLayoutRenderer>("processinfo", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessNameLayoutRenderer>("processname", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ProcessTimeLayoutRenderer>("processtime", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextIndentLayoutRenderer>("scopeindent", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("scopenested", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("ndc", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("ndlc", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("scopeproperty", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("mdc", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("mdlc", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("scopetiming", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("ndlctiming", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.SequenceIdLayoutRenderer>("sequenceid", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ShortDateLayoutRenderer>("shortdate", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderApplicationDataLayoutRenderer>("userApplicationDataDir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderCommonApplicationDataLayoutRenderer>("commonApplicationDataDir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderLayoutRenderer>("specialfolder", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.SpecialFolderLocalApplicationDataLayoutRenderer>("userLocalApplicationDataDir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.StackTraceLayoutRenderer>("stacktrace", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.TempDirLayoutRenderer>("tempdir", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ThreadIdLayoutRenderer>("threadid", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.ThreadNameLayoutRenderer>("threadname", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.TicksLayoutRenderer>("ticks", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.TimeLayoutRenderer>("time", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.VariableLayoutRenderer>("var", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("cached", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("Cached", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("ClearCache", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("CachedSeconds", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("filesystemnormalize", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("FSNormalize", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("jsonencode", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("JsonEncode", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("left", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("Truncate", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("lowercase", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("Lowercase", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("ToLower", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("norawvalue", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("NoRawValue", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("ObjectPath", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("ObjectPath", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.OnExceptionLayoutRendererWrapper>("onexception", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.OnHasPropertiesLayoutRendererWrapper>("onhasproperties", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("pad", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("Padding", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("PadCharacter", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("FixedLength", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("AlignmentOnTruncation", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper>("replace", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("replacenewlines", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("ReplaceNewLines", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.RightLayoutRendererWrapper>("right", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.Rot13LayoutRendererWrapper>("rot13", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.SubstringLayoutRendererWrapper>("substring", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("trimwhitespace", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("TrimWhiteSpace", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("uppercase", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("Uppercase", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("ToUpper", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.UrlEncodeLayoutRendererWrapper>("urlencode", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("whenEmpty", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("WhenEmpty", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("when", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("When", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("wrapline", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("WrapLine", checkTypeExists);
            layoutRendererFactory.RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("xmlencode", checkTypeExists);
            ambientPropertyFactory.RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("XmlEncode", checkTypeExists);
        }

        public static void RegisterFilterTypes(ConfigurationItemFactory factory, bool checkTypeExists)
        {
            var filterFactory = factory.GetFilterFactory();
            filterFactory.RegisterType<NLog.Filters.ConditionBasedFilter>("when", checkTypeExists);
            filterFactory.RegisterType<NLog.Filters.WhenContainsFilter>("whenContains", checkTypeExists);
            filterFactory.RegisterType<NLog.Filters.WhenEqualFilter>("whenEqual", checkTypeExists);
            filterFactory.RegisterType<NLog.Filters.WhenNotContainsFilter>("whenNotContains", checkTypeExists);
            filterFactory.RegisterType<NLog.Filters.WhenNotEqualFilter>("whenNotEqual", checkTypeExists);
            filterFactory.RegisterType<NLog.Filters.WhenRepeatedFilter>("whenRepeated", checkTypeExists);
        }

        public static void RegisterTimeSourceTypes(ConfigurationItemFactory factory, bool checkTypeExists)
        {
            var timeSourceFactory = factory.GetTimeSourceFactory();
            timeSourceFactory.RegisterType<NLog.Time.AccurateLocalTimeSource>("AccurateLocal", checkTypeExists);
            timeSourceFactory.RegisterType<NLog.Time.AccurateUtcTimeSource>("AccurateUTC", checkTypeExists);
            timeSourceFactory.RegisterType<NLog.Time.FastLocalTimeSource>("FastLocal", checkTypeExists);
            timeSourceFactory.RegisterType<NLog.Time.FastUtcTimeSource>("FastUTC", checkTypeExists);
        }

        public static void RegisterConditionTypes(ConfigurationItemFactory factory, bool checkTypeExists)
        {
            var conditionMethodFactory = factory.GetConditionMethodFactory();
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
            if (!checkTypeExists || !conditionMethodFactory.CheckTypeAliasExists("length"))
                conditionMethodFactory.RegisterOneParameter("length", (logEvent, arg1) => NLog.Conditions.ConditionMethods.Length(arg1?.ToString()));
            if (!checkTypeExists || !conditionMethodFactory.CheckTypeAliasExists("equals"))
                conditionMethodFactory.RegisterTwoParameters("equals", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
            if (!checkTypeExists || !conditionMethodFactory.CheckTypeAliasExists("strequals"))
            {
                conditionMethodFactory.RegisterTwoParameters("strequals", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
                conditionMethodFactory.RegisterThreeParameters("strequals", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
            }
            if (!checkTypeExists || !conditionMethodFactory.CheckTypeAliasExists("contains"))
            {
                conditionMethodFactory.RegisterTwoParameters("contains", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString()) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
                conditionMethodFactory.RegisterThreeParameters("contains", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
            }
            if (!checkTypeExists || !conditionMethodFactory.CheckTypeAliasExists("starts-with"))
            {
                conditionMethodFactory.RegisterTwoParameters("starts-with", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString()) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
                conditionMethodFactory.RegisterThreeParameters("starts-with", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
            }
            if (!checkTypeExists || !conditionMethodFactory.CheckTypeAliasExists("ends-with"))
            {
                conditionMethodFactory.RegisterTwoParameters("ends-with", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString()) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
                conditionMethodFactory.RegisterThreeParameters("ends-with", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true) ? Conditions.ConditionExpression.BoxedTrue : Conditions.ConditionExpression.BoxedFalse);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
