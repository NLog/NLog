using System;
using System.IO;
using System.Text;
using System.Xml;

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

            throw new NotImplementedException();
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