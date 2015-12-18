using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace NLog.Internal
{
    /// <summary>
    /// <see cref="XmlWriter"/> for <see cref="LogEventInfo"/>.
    /// </summary>
    public class LogEventInfoXmlWriter
    {
        /// <summary>
        /// Writes the specified <paramref name="logEventInfo"/> to the <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to write the xml to.</param>
        /// <param name="logEventInfo">The log event information to write.</param>
        public void Write(StringBuilder builder, LogEventInfo logEventInfo)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            };

            using (var xw = XmlWriter.Create(builder, settings))
            {
                Write(xw, logEventInfo);
                xw.Flush();
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="logEventInfo"/> to the <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the xml to.</param>
        /// <param name="logEventInfo">The log event information to write.</param>
        public void Write(XmlWriter writer, LogEventInfo logEventInfo)
        {
            writer.WriteStartElement("LogEvent");

            if (logEventInfo.SequenceID != default(int))
                writer.WriteElementString("SequenceID", XmlConvert.ToString(logEventInfo.SequenceID));

            if (logEventInfo.TimeStamp != default(DateTime))
                writer.WriteElementString("TimeStamp", XmlConvert.ToString(logEventInfo.TimeStamp, XmlDateTimeSerializationMode.RoundtripKind));

            if (logEventInfo.Level != null)
                writer.WriteElementString("Level", logEventInfo.Level.Name);

            if (logEventInfo.LoggerName != null)
                writer.WriteElementString("LoggerName", logEventInfo.LoggerName);

            if (logEventInfo.Message != null)
                writer.WriteElementString("Message", logEventInfo.FormattedMessage);

            if (logEventInfo.Exception != null)
            {
                writer.WriteStartElement("Error");
                WriteError(writer, logEventInfo.Exception);
                writer.WriteEndElement(); // Error 
            }


#if !SILVERLIGHT

            if (logEventInfo.HasStackTrace)
                writer.WriteElementString("StackTrace", logEventInfo.StackTrace.ToString());

#endif

            if (logEventInfo.Properties != null)
            {
                writer.WriteStartElement("Properties");
                WriteProperties(writer, logEventInfo.Properties);
                writer.WriteEndElement(); // Properties 
            }

            writer.WriteEndElement(); // LogEvent
        }

        private void WriteError(XmlWriter writer, Exception error)
        {
            if (error == null)
                return;

            Type type = error.GetType();
            var typeName = type.FullName;

            writer.WriteElementString("TypeName", typeName);

#if !SILVERLIGHT
            var external = error as ExternalException;
            if (external != null)
                writer.WriteElementString("ErrorCode", external.ErrorCode.ToString(CultureInfo.InvariantCulture));

            var method = error.TargetSite;
            if (method != null)
            {
                var assembly = method.Module.Assembly.GetName();

                writer.WriteElementString("MethodName", method.Name);
                if (assembly.Name != null)
                    writer.WriteElementString("ModuleName", assembly.Name);

                writer.WriteElementString("ModuleVersion", assembly.Version.ToString());

            }
#endif

            writer.WriteElementString("Message", error.Message);

#if !SILVERLIGHT
            if (error.Source != null)
                writer.WriteElementString("Source", error.Source);
#endif

            writer.WriteElementString("StackTrace", error.StackTrace);
            writer.WriteElementString("ExceptionText", error.ToString());

            if (error.InnerException == null)
                return;

            writer.WriteStartElement("InnerError");
            WriteError(writer, error.InnerException);
            writer.WriteEndElement(); // InnerError
        }

        private void WriteProperties(XmlWriter writer, IDictionary<object, object> properties)
        {
            foreach (var property in properties)
            {
                writer.WriteStartElement("Property");
                WriteProperty(writer, property);
                writer.WriteEndElement(); // Property 
            }
        }

        private void WriteProperty(XmlWriter writer, KeyValuePair<object, object> property)
        {
            var name = Convert.ToString(property.Key, CultureInfo.InvariantCulture);
            var value = Convert.ToString(property.Value, CultureInfo.InvariantCulture);

            writer.WriteElementString("Name", name);
            writer.WriteElementString("Value", value);
        }

    }
}