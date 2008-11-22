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
using System.Xml.Xsl;
using System.IO;

namespace MakeNLogDoc
{
    class Program
    {
        static bool TryGetMemberDoc(XmlDocument doc, string id, out XmlElement element)
        {
            element = (XmlElement)doc.SelectSingleNode("/doc/members/member[@name='" + id + "']");
            return element != null;
        }

        static void DumpApiDocs(XmlWriter writer, string kind, IEnumerable<Type> types, XmlDocument comments)
        {
            foreach (Type type in types)
            {
                writer.WriteStartElement("type");
                writer.WriteAttributeString("kind", kind);
                writer.WriteAttributeString("id", type.FullName);
                string name;
                if (TryGetTypeNameFromNameAttribute(type, out name))
                {
                    writer.WriteAttributeString("name", name);
                }

                XmlElement memberDoc;
                if (TryGetMemberDoc(comments, "T:" + type.FullName, out memberDoc))
                {
                    writer.WriteStartElement("doc");
                    memberDoc.WriteContentTo(writer);
                    writer.WriteEndElement();
                }

                foreach (PropertyInfo propInfo in type.GetProperties())
                {
                    writer.WriteStartElement("property");
                    writer.WriteAttributeString("name", propInfo.Name);
                    writer.WriteAttributeString("type", propInfo.PropertyType.FullName);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        private static bool TryGetTypeNameFromNameAttribute(Type type, out string name)
        {
            foreach (Attribute a in type.GetCustomAttributes(false))
            {
                NameAttributeBase nab = a as NameAttributeBase;
                if (nab != null)
                {
                    name = nab.Name;
                    return true;
                }
            }
            name = null;
            return false;
        }

        static void Main(string[] args)
        {
            XmlDocument comments = new XmlDocument();
            comments.Load("NLog.xml");

            Assembly assembly = Assembly.Load("NLog");
            using (XmlWriter writer = XmlWriter.Create("NLog.doc.xml", new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartElement("types");
                DumpApiDocs(writer, "target", GetTargetTypes(assembly), comments);
                writer.WriteEndElement();
            }
        }

        private static IEnumerable<Type> GetTargetTypes(Assembly assembly)
        {
            foreach (Type t in assembly.GetTypes())
            {
                if (t.IsDefined(typeof(TargetAttribute), false))
                    yield return t;
            }
        }
    }
}
