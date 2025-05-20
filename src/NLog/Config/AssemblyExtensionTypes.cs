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
        public static void RegisterTargetTypes(ConfigurationItemFactory factory, bool skipCheckExists)
        {
            factory.RegisterTypeProperties<NLog.Targets.TargetWithContext.TargetWithContextLayout>(() => null);
#if NETFRAMEWORK
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("EventLog"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.EventLogTarget>("EventLog");
#endif
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("ColoredConsole"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.ColoredConsoleTarget>("ColoredConsole");
            factory.RegisterType<NLog.Targets.ConsoleRowHighlightingRule>();
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("Console"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.ConsoleTarget>("Console");
            factory.RegisterType<NLog.Targets.ConsoleWordHighlightingRule>();
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("Debugger"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.DebuggerTarget>("Debugger");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("DebugSystem"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.DebugSystemTarget>("DebugSystem");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("Debug"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.DebugTarget>("Debug");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("File"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.FileTarget>("File");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("Memory"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.MemoryTarget>("Memory");
            factory.RegisterType<NLog.Targets.MethodCallParameter>();
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("MethodCall"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.MethodCallTarget>("MethodCall");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("Null"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.NullTarget>("Null");
            factory.RegisterType<NLog.Targets.TargetPropertyWithContext>();
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("AsyncWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.AsyncTargetWrapper>("AsyncWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("AutoFlushWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.AutoFlushTargetWrapper>("AutoFlushWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("BufferingWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.BufferingTargetWrapper>("BufferingWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("FallbackGroup"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.FallbackGroupTarget>("FallbackGroup");
            factory.RegisterType<NLog.Targets.Wrappers.FilteringRule>();
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("FilteringWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.FilteringTargetWrapper>("FilteringWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("GroupByWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.GroupByTargetWrapper>("GroupByWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("LimitingWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.LimitingTargetWrapper>("LimitingWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("PostFilteringWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.PostFilteringTargetWrapper>("PostFilteringWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("RandomizeGroup"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RandomizeGroupTarget>("RandomizeGroup");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("RepeatingWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RepeatingTargetWrapper>("RepeatingWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("RetryingWrapper"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RetryingTargetWrapper>("RetryingWrapper");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("RoundRobinGroup"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.RoundRobinGroupTarget>("RoundRobinGroup");
            if (skipCheckExists || !factory.GetTargetFactory().CheckTypeAliasExists("SplitGroup"))
                factory.GetTargetFactory().RegisterType<NLog.Targets.Wrappers.SplitGroupTarget>("SplitGroup");
        }

        public static void RegisterLayoutTypes(ConfigurationItemFactory factory, bool skipCheckExists)
        {
            factory.RegisterTypeProperties<NLog.Layouts.CsvLayout.CsvHeaderLayout>(() => null);
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("CompoundLayout"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.CompoundLayout>("CompoundLayout");
            factory.RegisterType<NLog.Layouts.CsvColumn>();
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("CsvLayout"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.CsvLayout>("CsvLayout");
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("JsonArrayLayout"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.JsonArrayLayout>("JsonArrayLayout");
            factory.RegisterType<NLog.Layouts.JsonAttribute>();
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("JsonLayout"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.JsonLayout>("JsonLayout");
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("LayoutWithHeaderAndFooter"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.LayoutWithHeaderAndFooter>("LayoutWithHeaderAndFooter");
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("SimpleLayout"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.SimpleLayout>("SimpleLayout");
            factory.RegisterType<NLog.Layouts.ValueTypeLayoutInfo>();
            factory.RegisterType<NLog.Layouts.XmlAttribute>();
            if (skipCheckExists || !factory.GetLayoutFactory().CheckTypeAliasExists("XmlLayout"))
                factory.GetLayoutFactory().RegisterType<NLog.Layouts.XmlLayout>("XmlLayout");
        }

        public static void RegisterLayoutRendererTypes(ConfigurationItemFactory factory, bool skipCheckExists)
        {
#if NETFRAMEWORK
            factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AppSettingLayoutRenderer>("appsetting");
#endif
            factory.RegisterTypeProperties<NLog.LayoutRenderers.LiteralWithRawValueLayoutRenderer>(() => null);
            factory.RegisterTypeProperties<NLog.LayoutRenderers.FuncLayoutRenderer>(() => null);
            factory.RegisterTypeProperties<NLog.LayoutRenderers.FuncThreadAgnosticLayoutRenderer>(() => null);
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("all-event-properties"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AllEventPropertiesLayoutRenderer>("all-event-properties");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("appdomain"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AppDomainLayoutRenderer>("appdomain");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("assembly-version"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.AssemblyVersionLayoutRenderer>("assembly-version");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("basedir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.BaseDirLayoutRenderer>("basedir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("callsite-filename"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CallSiteFileNameLayoutRenderer>("callsite-filename");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("callsite"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CallSiteLayoutRenderer>("callsite");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("callsite-linenumber"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CallSiteLineNumberLayoutRenderer>("callsite-linenumber");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("counter"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CounterLayoutRenderer>("counter");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("currentdir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.CurrentDirLayoutRenderer>("currentdir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("date"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.DateLayoutRenderer>("date");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("db-null"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.DbNullLayoutRenderer>("db-null");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("dir-separator"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.DirectorySeparatorLayoutRenderer>("dir-separator");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("environment"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EnvironmentLayoutRenderer>("environment");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("environment-user"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EnvironmentUserLayoutRenderer>("environment-user");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("event-properties"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-properties");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("event-property"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-property");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("event-context"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.EventPropertiesLayoutRenderer>("event-context");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("exceptiondata"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exceptiondata");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("exception-data"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ExceptionDataLayoutRenderer>("exception-data");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("exception"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ExceptionLayoutRenderer>("exception");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("gc"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.GarbageCollectorInfoLayoutRenderer>("gc");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("gdc"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.GdcLayoutRenderer>("gdc");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("guid"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.GuidLayoutRenderer>("guid");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("hostname"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.HostNameLayoutRenderer>("hostname");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("identity"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.IdentityLayoutRenderer>("identity");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("install-context"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.InstallContextLayoutRenderer>("install-context");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("level"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("level");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("loglevel"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LevelLayoutRenderer>("loglevel");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("literal"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LiteralLayoutRenderer>("literal");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("loggername"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("loggername");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("logger"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LoggerNameLayoutRenderer>("logger");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("longdate"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.LongDateLayoutRenderer>("longdate");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("machinename"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.MachineNameLayoutRenderer>("machinename");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("message"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.MessageLayoutRenderer>("message");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("newline"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.NewLineLayoutRenderer>("newline");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("nlogdir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.NLogDirLayoutRenderer>("nlogdir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("processdir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessDirLayoutRenderer>("processdir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("processid"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessIdLayoutRenderer>("processid");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("processinfo"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessInfoLayoutRenderer>("processinfo");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("processname"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessNameLayoutRenderer>("processname");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("processtime"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ProcessTimeLayoutRenderer>("processtime");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("scopeindent"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextIndentLayoutRenderer>("scopeindent");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("scopenested"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("scopenested");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("ndc"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("ndc");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("ndlc"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextNestedStatesLayoutRenderer>("ndlc");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("scopeproperty"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("scopeproperty");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("mdc"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("mdc");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("mdlc"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextPropertyLayoutRenderer>("mdlc");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("scopetiming"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("scopetiming");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("ndlctiming"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ScopeContextTimingLayoutRenderer>("ndlctiming");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("sequenceid"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SequenceIdLayoutRenderer>("sequenceid");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("shortdate"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ShortDateLayoutRenderer>("shortdate");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("userApplicationDataDir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderApplicationDataLayoutRenderer>("userApplicationDataDir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("commonApplicationDataDir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderCommonApplicationDataLayoutRenderer>("commonApplicationDataDir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("specialfolder"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderLayoutRenderer>("specialfolder");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("userLocalApplicationDataDir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.SpecialFolderLocalApplicationDataLayoutRenderer>("userLocalApplicationDataDir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("stacktrace"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.StackTraceLayoutRenderer>("stacktrace");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("tempdir"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.TempDirLayoutRenderer>("tempdir");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("threadid"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ThreadIdLayoutRenderer>("threadid");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("threadname"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.ThreadNameLayoutRenderer>("threadname");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("ticks"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.TicksLayoutRenderer>("ticks");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("time"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.TimeLayoutRenderer>("time");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("var"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.VariableLayoutRenderer>("var");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("cached"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("cached");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("Cached"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("Cached");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("ClearCache"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("ClearCache");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("CachedSeconds"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper>("CachedSeconds");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("filesystem-normalize"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("filesystem-normalize");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("FSNormalize"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.FileSystemNormalizeLayoutRendererWrapper>("FSNormalize");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("json-encode"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("json-encode");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("JsonEncode"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper>("JsonEncode");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("left"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("left");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("Truncate"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LeftLayoutRendererWrapper>("Truncate");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("lowercase"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("lowercase");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("Lowercase"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("Lowercase");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("ToLower"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.LowercaseLayoutRendererWrapper>("ToLower");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("norawvalue"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("norawvalue");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("NoRawValue"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.NoRawValueLayoutRendererWrapper>("NoRawValue");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("Object-Path"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("Object-Path");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("ObjectPath"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ObjectPathRendererWrapper>("ObjectPath");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("onexception"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.OnExceptionLayoutRendererWrapper>("onexception");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("onhasproperties"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.OnHasPropertiesLayoutRendererWrapper>("onhasproperties");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("pad"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("pad");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("Padding"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("Padding");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("PadCharacter"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("PadCharacter");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("FixedLength"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("FixedLength");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("AlignmentOnTruncation"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.PaddingLayoutRendererWrapper>("AlignmentOnTruncation");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("replace"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper>("replace");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("replace-newlines"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("replace-newlines");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("ReplaceNewLines"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.ReplaceNewLinesLayoutRendererWrapper>("ReplaceNewLines");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("right"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.RightLayoutRendererWrapper>("right");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("rot13"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.Rot13LayoutRendererWrapper>("rot13");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("substring"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.SubstringLayoutRendererWrapper>("substring");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("trim-whitespace"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("trim-whitespace");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("TrimWhiteSpace"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.TrimWhiteSpaceLayoutRendererWrapper>("TrimWhiteSpace");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("uppercase"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("uppercase");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("Uppercase"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("Uppercase");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("ToUpper"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UppercaseLayoutRendererWrapper>("ToUpper");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("url-encode"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.UrlEncodeLayoutRendererWrapper>("url-encode");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("whenEmpty"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("whenEmpty");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("WhenEmpty"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenEmptyLayoutRendererWrapper>("WhenEmpty");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("when"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("when");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("When"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WhenLayoutRendererWrapper>("When");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("wrapline"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("wrapline");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("WrapLine"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.WrapLineLayoutRendererWrapper>("WrapLine");
            if (skipCheckExists || !factory.GetLayoutRendererFactory().CheckTypeAliasExists("xml-encode"))
                factory.GetLayoutRendererFactory().RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("xml-encode");
            if (skipCheckExists || !factory.GetAmbientPropertyFactory().CheckTypeAliasExists("XmlEncode"))
                factory.GetAmbientPropertyFactory().RegisterType<NLog.LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper>("XmlEncode");
        }

        public static void RegisterFilterTypes(ConfigurationItemFactory factory, bool skipCheckExists)
        {
            if (skipCheckExists || !factory.GetFilterFactory().CheckTypeAliasExists("when"))
                factory.GetFilterFactory().RegisterType<NLog.Filters.ConditionBasedFilter>("when");
            if (skipCheckExists || !factory.GetFilterFactory().CheckTypeAliasExists("whenContains"))
                factory.GetFilterFactory().RegisterType<NLog.Filters.WhenContainsFilter>("whenContains");
            if (skipCheckExists || !factory.GetFilterFactory().CheckTypeAliasExists("whenEqual"))
                factory.GetFilterFactory().RegisterType<NLog.Filters.WhenEqualFilter>("whenEqual");
            if (skipCheckExists || !factory.GetFilterFactory().CheckTypeAliasExists("whenNotContains"))
                factory.GetFilterFactory().RegisterType<NLog.Filters.WhenNotContainsFilter>("whenNotContains");
            if (skipCheckExists || !factory.GetFilterFactory().CheckTypeAliasExists("whenNotEqual"))
                factory.GetFilterFactory().RegisterType<NLog.Filters.WhenNotEqualFilter>("whenNotEqual");
            if (skipCheckExists || !factory.GetFilterFactory().CheckTypeAliasExists("whenRepeated"))
                factory.GetFilterFactory().RegisterType<NLog.Filters.WhenRepeatedFilter>("whenRepeated");
        }

        public static void RegisterTimeSourceTypes(ConfigurationItemFactory factory, bool skipCheckExists)
        {
            if (skipCheckExists || !factory.GetTimeSourceFactory().CheckTypeAliasExists("AccurateLocal"))
                factory.GetTimeSourceFactory().RegisterType<NLog.Time.AccurateLocalTimeSource>("AccurateLocal");
            if (skipCheckExists || !factory.GetTimeSourceFactory().CheckTypeAliasExists("AccurateUTC"))
                factory.GetTimeSourceFactory().RegisterType<NLog.Time.AccurateUtcTimeSource>("AccurateUTC");
            if (skipCheckExists || !factory.GetTimeSourceFactory().CheckTypeAliasExists("FastLocal"))
                factory.GetTimeSourceFactory().RegisterType<NLog.Time.FastLocalTimeSource>("FastLocal");
            if (skipCheckExists || !factory.GetTimeSourceFactory().CheckTypeAliasExists("FastUTC"))
                factory.GetTimeSourceFactory().RegisterType<NLog.Time.FastUtcTimeSource>("FastUTC");
        }

        public static void RegisterConditionTypes(ConfigurationItemFactory factory, bool skipCheckExists)
        {
            if (skipCheckExists || !factory.GetConditionMethodFactory().CheckTypeAliasExists("length"))
                factory.GetConditionMethodFactory().RegisterOneParameter("length", (logEvent, arg1) => NLog.Conditions.ConditionMethods.Length(arg1?.ToString()));
            if (skipCheckExists || !factory.GetConditionMethodFactory().CheckTypeAliasExists("equals"))
                factory.GetConditionMethodFactory().RegisterTwoParameters("equals", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()));
            if (skipCheckExists || !factory.GetConditionMethodFactory().CheckTypeAliasExists("strequals"))
            {
                factory.GetConditionMethodFactory().RegisterTwoParameters("strequals", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString()));
                factory.GetConditionMethodFactory().RegisterThreeParameters("strequals", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.Equals2(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            }
            if (skipCheckExists || !factory.GetConditionMethodFactory().CheckTypeAliasExists("contains"))
            {
                factory.GetConditionMethodFactory().RegisterTwoParameters("contains", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString()));
                factory.GetConditionMethodFactory().RegisterThreeParameters("contains", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.Contains(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            }
            if (skipCheckExists || !factory.GetConditionMethodFactory().CheckTypeAliasExists("starts-with"))
            {
                factory.GetConditionMethodFactory().RegisterTwoParameters("starts-with", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString()));
                factory.GetConditionMethodFactory().RegisterThreeParameters("starts-with", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.StartsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            }
            if (skipCheckExists || !factory.GetConditionMethodFactory().CheckTypeAliasExists("ends-with"))
            {
                factory.GetConditionMethodFactory().RegisterTwoParameters("ends-with", (logEvent, arg1, arg2) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString()));
                factory.GetConditionMethodFactory().RegisterThreeParameters("ends-with", (logEvent, arg1, arg2, arg3) => NLog.Conditions.ConditionMethods.EndsWith(arg1?.ToString(), arg2?.ToString(), arg3 is bool ignoreCase ? ignoreCase : true));
            }
        }
    }
}
