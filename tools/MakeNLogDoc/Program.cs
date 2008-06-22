using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NLog;
using System.Xml;
using NLog.Config;
using System.ComponentModel;
using NLog.Layouts;
using NLog.Targets;

namespace MakeNLogDoc
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDirectory = ".";
            if (args.Length > 0)
                outputDirectory = args[0];

            using (XmlTextWriter output = new XmlTextWriter(Console.Out))
            //using (XmlWriter output = XmlWriter.Create("NLogDoc.xml"))
            {
                output.Formatting = Formatting.Indented;
                DumpAssembly(output, typeof(Logger).Assembly);
            }
        }

        static void DumpAssembly(XmlWriter output, Assembly assembly)
        {
            output.WriteStartElement("assembly");

            foreach (Type t in assembly.GetExportedTypes())
            {
                TargetAttribute ta = (TargetAttribute)Attribute.GetCustomAttribute(t, typeof(TargetAttribute), false);
                if (ta != null)
                {
                    DumpTarget(output, t, ta);
                }
            }

            output.WriteEndElement();
        }

        static void DumpTarget(XmlWriter output, Type t, TargetAttribute ta)
        {
            output.WriteStartElement("target");
            output.WriteAttributeString("name", ta.Name);
            output.WriteAttributeString("wrapper", XmlConvert.ToString(ta.IsWrapper));
            output.WriteAttributeString("compound", XmlConvert.ToString(ta.IsCompound));
            output.WriteStartElement("properties");
            DumpProperties(output, t, new string[] { "WrappedTarget" });
            output.WriteEndElement();
            output.WriteEndElement();
        }

        static void DumpProperties(XmlWriter output, Type t, ICollection<string> ignoreNames)
        {
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (typeof(Layout).IsAssignableFrom(pi.PropertyType))
                    continue;

                if (ignoreNames.Contains(pi.Name))
                    continue;

                output.WriteStartElement("property");
                output.WriteAttributeString("name", pi.Name);
                output.WriteAttributeString("type", pi.PropertyType.Name);
                DefaultValueAttribute defVal = (DefaultValueAttribute)Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute));
                if (defVal != null)
                {
                    output.WriteAttributeString("defaultValue", defVal.Value.ToString());
                }

                if (pi.IsDefined(typeof(RequiredParameterAttribute), false))
                    output.WriteAttributeString("required", "true");
                output.WriteEndElement();
            }
        }
    }
}
