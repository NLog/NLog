using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Config
{
    /// <summary>
    ///     Provides extensions to the <see cref="LoggingConfiguration" /> class, to enable serialization.
    /// </summary>
    public static class LoggingConfigurationExtensions
    {
        /// <summary>
        ///     Serializes a <paramref name="loggingConfiguration" /> into an xml configuration.
        /// </summary>
        /// <param name="loggingConfiguration">The <see cref="LoggingConfiguration" /> to serialize.</param>
        /// <param name="writer">The <see cref="XmlWriter" /> used to create the xml configuration.</param>
        public static void Serialize(this LoggingConfiguration loggingConfiguration, XmlWriter writer)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteStartDocument();
            writer.WriteStartElement("nlog", @"http://www.nlog-project.org/schemas/NLog.xsd");
            writer.WriteAttributeString("xmlns", "xsi", "", value: @"http://www.w3.org/2001/XMLSchema-instance");

            WriteVariables(loggingConfiguration.Variables, writer);
            WriteTargets(loggingConfiguration.AllTargets, writer);
            WriteRules(loggingConfiguration.LoggingRules, writer);

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private static void WriteRules(IEnumerable<LoggingRule> rules, XmlWriter writer)
        {
            writer.WriteStartElement("rules");

            foreach (LoggingRule rule in rules)
            {
                writer.WriteStartElement("logger");
                writer.WriteAttributeString("name", rule.LoggerNamePattern);

                if (rule.Final)
                {
                    writer.WriteAttributeString("final", "true");
                }

                string targets = string.Join(",", rule.Targets.Select(target => target.Name));
                writer.WriteAttributeString("writeTo", targets);

                if (rule.Levels.Count == 1)
                {
                    writer.WriteAttributeString("level", rule.Levels.First().Name);
                }

                else if (rule.Levels.Count >= LogLevel.AllLoggingLevels.Count())
                {
                }

                else if (rule.Levels.AreContigous(out LogLevel min, out LogLevel max))
                {
                    writer.WriteAttributeString("minlevel", min.Name);
                    writer.WriteAttributeString("maxlevel", max.Name);
                }

                else
                {
                    string levels = string.Join(",", rule.Levels.Select(level => level.Name));
                    writer.WriteAttributeString("levels", min.Name);
                }
            }

            writer.WriteEndElement();
        }

        private static bool AreContigous(this IEnumerable<LogLevel> span, out LogLevel min, out LogLevel max)
        {
            min = null;
            max = null;
            using (IEnumerator<LogLevel> ordered = span.OrderBy(level => level.Ordinal).GetEnumerator())
            {
                if (!ordered.MoveNext() || ordered.Current == null)
                    return false;

                min = ordered.Current;
                int ordinal = min.Ordinal;
                while (ordered.MoveNext())
                {
                    if (ordered.Current == null)
                        return false;

                    if (ordered.Current.Ordinal != ++ordinal)
                        return false;
                    max = ordered.Current;
                }
            }

            return true;
        }

        private static void WriteTargets(IEnumerable<Target> targets, XmlWriter writer)
        {
            writer.WriteStartElement("targets");

            foreach (Target target in targets)
            {
                writer.WriteStartElement("target");
                writer.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance",
                    ResolveConfigurationType(target.GetType()));

                WriteParameters(target, writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void WriteParameters(Target target, XmlWriter writer)
        {
            PropertyInfo[] properties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                // we only care for public read/writable properties. Theese are very likely configuration
                // properties.
                .Where(pi => pi.GetSetMethod(false) != null && pi.GetGetMethod(false) != null)
                .ToArray();

            foreach (PropertyInfo property in properties)
            {
                writer.WriteAttributeString(property.Name, PropertyHelper.GetPropertyAsString(target, property));
            }
        }

       


        private static string ResolveConfigurationType(Type targetType)
        {
        
            var targetAttribute = CustomAttributeExtensions.GetCustomAttribute<TargetAttribute>(targetType);

            if (targetAttribute == null)
                throw new InvalidOperationException(
                    $"Target '{targetType.FullName}' does not have a 'NLog.Targets.TargetAttribute'." +
                    "This is required for this serialization.");

            return targetAttribute.Name;
        }

        private static void WriteVariables(IDictionary<string, SimpleLayout> variables, XmlWriter writer)
        {
            foreach (KeyValuePair<string, SimpleLayout> variable in variables)
            {
                writer.WriteStartElement("variable");
                writer.WriteAttributeString("name", variable.Key);
                writer.WriteAttributeString("value", variable.Value.OriginalText);
                writer.WriteEndElement();
            }
        }


        /// <inheritdoc cref="Serialize(LoggingConfiguration,TextWriter, XmlWriterSettings)" />
        public static void Serialize(this LoggingConfiguration loggingConfiguration, TextWriter writer)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            loggingConfiguration.Serialize(XmlWriter.Create(writer));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,System.Xml.XmlWriter)" />
        /// <paramref name="writer">The <see cref="TextWriter" /> that will contain the xml configuration.</paramref>
        /// <paramref name="settings">
        ///     The <see cref="XmlWriterSettings" /> object used to configure the internal
        ///     <see cref="T:System.Xml.XmlWriter" /> instance. If this is <see langword="null" />, a
        ///     <see cref="T:System.Xml.XmlWriterSettings" /> with default settings is used.
        /// </paramref>
        public static void Serialize(this LoggingConfiguration loggingConfiguration, TextWriter writer,
            XmlWriterSettings settings)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            loggingConfiguration.Serialize(XmlWriter.Create(writer, settings));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,TextWriter, XmlWriterSettings)" />
        public static void Serialize(this LoggingConfiguration loggingConfiguration, StringBuilder stringBuilder)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (stringBuilder == null)
                throw new ArgumentNullException(nameof(stringBuilder));

            loggingConfiguration.Serialize(XmlWriter.Create(stringBuilder));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,System.Xml.XmlWriter)" />
        /// <paramref name="stringBuilder">The <see cref="StringBuilder" /> that will contain the xml configuration.</paramref>
        /// <paramref name="settings">
        ///     The <see cref="XmlWriterSettings" /> object used to configure the internal
        ///     <see cref="T:System.Xml.XmlWriter" /> instance. If this is <see langword="null" />, a
        ///     <see cref="T:System.Xml.XmlWriterSettings" /> with default settings is used.
        /// </paramref>
        public static void Serialize(this LoggingConfiguration loggingConfiguration, StringBuilder stringBuilder,
            XmlWriterSettings settings)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (stringBuilder == null)
                throw new ArgumentNullException(nameof(stringBuilder));

            loggingConfiguration.Serialize(XmlWriter.Create(stringBuilder, settings));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,string, XmlWriterSettings)" />
        public static void Serialize(this LoggingConfiguration loggingConfiguration, string outputFileName)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (outputFileName == null)
                throw new ArgumentNullException(nameof(outputFileName));

            loggingConfiguration.Serialize(XmlWriter.Create(outputFileName));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,System.Xml.XmlWriter)" />
        /// <paramref name="outputFileName">The filename where the xml configuration should be stored.</paramref>
        /// <paramref name="settings">
        ///     The <see cref="XmlWriterSettings" /> object used to configure the internal
        ///     <see cref="T:System.Xml.XmlWriter" /> instance. If this is <see langword="null" />, a
        ///     <see cref="T:System.Xml.XmlWriterSettings" /> with default settings is used.
        /// </paramref>
        public static void Serialize(this LoggingConfiguration loggingConfiguration, string outputFileName,
            XmlWriterSettings settings)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (outputFileName == null)
                throw new ArgumentNullException(nameof(outputFileName));

            loggingConfiguration.Serialize(XmlWriter.Create(outputFileName, settings));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,Stream, XmlWriterSettings)" />
        public static void Serialize(this LoggingConfiguration loggingConfiguration, Stream output)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            loggingConfiguration.Serialize(XmlWriter.Create(output));
        }

        /// <inheritdoc cref="Serialize(LoggingConfiguration,System.Xml.XmlWriter)" />
        /// <paramref name="output">The <see cref="Stream" />to write the xml configuration into.</paramref>
        /// <paramref name="settings">
        ///     The <see cref="XmlWriterSettings" /> object used to configure the internal
        ///     <see cref="T:System.Xml.XmlWriter" /> instance. If this is <see langword="null" />, a
        ///     <see cref="T:System.Xml.XmlWriterSettings" /> with default settings is used.
        /// </paramref>
        public static void Serialize(this LoggingConfiguration loggingConfiguration, Stream output,
            XmlWriterSettings settings)
        {
            if (loggingConfiguration == null)
                throw new ArgumentNullException(nameof(loggingConfiguration));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            loggingConfiguration.Serialize(XmlWriter.Create(output, settings));
        }
    }
}