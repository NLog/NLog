using System;
using System.Text;

using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;
using NLog.Targets.Compound;

using System.Xml;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Xml.Schema;
using NLog.Conditions;
using System.ComponentModel;
using System.Globalization;
using NLog.Layouts;
using NLog.Targets;

namespace MakeNLogXSD
{
    class Program
    {
        static Hashtable _typeDumped = new Hashtable();
        static XmlDocument _docXml = new XmlDocument();

        static string XmlDefaultValue(object v)
        {
            if (v is bool)
            {
                return XmlConvert.ToString((bool)v);
            }
            else
            {
                return Convert.ToString(v, CultureInfo.InvariantCulture);
            }
        }

        static string FindAnnotation(PropertyInfo pi)
        {
            string xpath = "//class[@id='T:" + pi.DeclaringType.FullName + "']/property[@name='" + pi.Name + "']/documentation/summary";
            XmlNode n = _docXml.SelectSingleNode(xpath);
            if (n != null)
            {
                string suffix = "";

                DefaultValueAttribute dva = (DefaultValueAttribute)Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute));
                if (dva != null)
                    suffix += " Default value is: " + XmlDefaultValue(dva.Value);

                return n.InnerText + suffix;
            }
            else
                return null;
        }

        static string SimpleTypeName(Type t)
        {
            if (t == typeof(byte))
                return "xs:byte";
            if (t == typeof(int))
                return "xs:integer";
            if (t == typeof(long))
                return "xs:long";
            if (t == typeof(string))
                return "xs:string";
            if (t == typeof(bool))
                return "xs:boolean";

            TargetAttribute ta = (TargetAttribute)Attribute.GetCustomAttribute(t, typeof(TargetAttribute));
            if (ta != null)
                return ta.Name;

            LayoutAttribute la = (LayoutAttribute)Attribute.GetCustomAttribute(t, typeof(LayoutAttribute));
            if (la != null)
                return la.Name;

            return t.Name;
        }

        static string MakeCamelCase(string s)
        {
            if (s.Length < 1)
                return s.ToLower();

            int firstLower = s.Length;
            for (int i = 0; i < s.Length; ++i)
            {
                if (Char.IsLower(s[i]))
                {
                    firstLower = i;
                    break;
                }
            }

            if (firstLower == 0)
                return s;

            // DBType
            if (firstLower != 1 && firstLower != s.Length)
                firstLower--;
            return s.Substring(0, firstLower).ToLower() + s.Substring(firstLower);
        }

        static void DumpEnum(XmlTextWriter xtw, Type t)
        {
            xtw.WriteStartElement("xs:simpleType");
            xtw.WriteAttributeString("name", SimpleTypeName(t));

            xtw.WriteStartElement("xs:restriction");
            xtw.WriteAttributeString("base", "xs:string");

            foreach (FieldInfo fi in t.GetFields())
            {
                if (fi.Name.IndexOf("__") >= 0)
                    continue;
                xtw.WriteStartElement("xs:enumeration");
                xtw.WriteAttributeString("value", fi.Name);
                xtw.WriteEndElement();
            }

            xtw.WriteEndElement(); // xs:restriction
            xtw.WriteEndElement(); // xs:simpleType
        }

        static void DumpType(XmlTextWriter xtw, Type t)
        {
            if (_typeDumped[t] != null)
                return;

            _typeDumped[t] = t;

            if (t.IsArray)
                return;

            if (t.IsPrimitive)
                return;

            if (t == typeof(string))
                return;

            if (t.IsEnum)
            {
                DumpEnum(xtw, t);
                return;
            }

            ArrayList typesToDump = new ArrayList();

            typesToDump.Add(t.BaseType);

            xtw.WriteStartElement("xs:complexType");
            xtw.WriteAttributeString("name", SimpleTypeName(t));
            if (t.IsAbstract)
                xtw.WriteAttributeString("abstract", "true");

            if (t.BaseType != typeof(object) || t.GetInterface("NLog.ILayout") != null)
            {
                xtw.WriteStartElement("xs:complexContent");
                xtw.WriteStartElement("xs:extension");
                if (t.BaseType != typeof(object))
                {
                    xtw.WriteAttributeString("base", SimpleTypeName(t.BaseType));
                }
                else
                {
                    xtw.WriteAttributeString("base", "ILayout");
                }
            }

            xtw.WriteStartElement("xs:choice");
            xtw.WriteAttributeString("minOccurs", "0");
            xtw.WriteAttributeString("maxOccurs", "unbounded");

            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.DeclaringType != t)
                    continue;

                ArrayParameterAttribute apa = (ArrayParameterAttribute)Attribute.GetCustomAttribute(pi, typeof(ArrayParameterAttribute));
                if (apa != null)
                {
                    xtw.WriteStartElement("xs:element");
                    xtw.WriteAttributeString("name", apa.ElementName);
                    xtw.WriteAttributeString("type", SimpleTypeName(apa.ItemType));
                    xtw.WriteAttributeString("minOccurs", "0");
                    xtw.WriteAttributeString("maxOccurs", "unbounded");
                    xtw.WriteEndElement();
                    typesToDump.Add(apa.ItemType);
                }
                else if (pi.PropertyType.IsValueType || pi.PropertyType.IsEnum || pi.PropertyType == typeof(string) || typeof(Layout).IsAssignableFrom(pi.PropertyType))
                {
                    if (pi.CanWrite && pi.CanRead && ((pi.GetSetMethod().Attributes & MethodAttributes.ReuseSlot) == 0) && (pi.Name != "Layout" || !typeof(TargetWithLayout).IsAssignableFrom(pi.DeclaringType)))
                    {
                        xtw.WriteStartElement("xs:element");
                        xtw.WriteAttributeString("name", MakeCamelCase(pi.Name));
                        xtw.WriteAttributeString("type", SimpleTypeName(pi.PropertyType));
                        xtw.WriteAttributeString("minOccurs", "0");
                        xtw.WriteAttributeString("maxOccurs", "1");
                        xtw.WriteEndElement();
                    }
                }
            }

            if (t == typeof(WrapperTargetBase))
            {
                xtw.WriteStartElement("xs:element");
                xtw.WriteAttributeString("name", "target");
                xtw.WriteAttributeString("type", "Target");
                xtw.WriteAttributeString("minOccurs", "1");
                xtw.WriteAttributeString("maxOccurs", "1");
                xtw.WriteEndElement();

                xtw.WriteStartElement("xs:element");
                xtw.WriteAttributeString("name", "wrapper-target");
                xtw.WriteAttributeString("type", "WrapperTargetBase");
                xtw.WriteAttributeString("minOccurs", "1");
                xtw.WriteAttributeString("maxOccurs", "1");
                xtw.WriteEndElement();

                xtw.WriteStartElement("xs:element");
                xtw.WriteAttributeString("name", "compound-target");
                xtw.WriteAttributeString("type", "CompoundTargetBase");
                xtw.WriteAttributeString("minOccurs", "1");
                xtw.WriteAttributeString("maxOccurs", "1");
                xtw.WriteEndElement();
            }

            if (t == typeof(CompoundTargetBase))
            {
                xtw.WriteStartElement("xs:element");
                xtw.WriteAttributeString("name", "target");
                xtw.WriteAttributeString("type", "Target");
                xtw.WriteAttributeString("minOccurs", "1");
                xtw.WriteAttributeString("maxOccurs", "unbounded");
                xtw.WriteEndElement();

                xtw.WriteStartElement("xs:element");
                xtw.WriteAttributeString("name", "wrapper-target");
                xtw.WriteAttributeString("type", "WrapperTargetBase");
                xtw.WriteAttributeString("minOccurs", "1");
                xtw.WriteAttributeString("maxOccurs", "1");
                xtw.WriteEndElement();

                xtw.WriteStartElement("xs:element");
                xtw.WriteAttributeString("name", "compound-target");
                xtw.WriteAttributeString("type", "CompoundTargetBase");
                xtw.WriteAttributeString("minOccurs", "1");
                xtw.WriteAttributeString("maxOccurs", "1");
                xtw.WriteEndElement();
            }

            xtw.WriteEndElement();

            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.DeclaringType != t)
                    continue;

                if (pi.IsDefined(typeof(ArrayParameterAttribute), false))
                    continue;

                if (!pi.PropertyType.IsValueType && !pi.PropertyType.IsEnum && pi.PropertyType != typeof(string))
                    continue;

                if (!pi.CanWrite)
                    continue;

                xtw.WriteStartElement("xs:attribute");
                xtw.WriteAttributeString("name", MakeCamelCase(pi.Name));
                xtw.WriteAttributeString("type", SimpleTypeName(pi.PropertyType));
                DefaultValueAttribute dva = (DefaultValueAttribute)Attribute.GetCustomAttribute(pi, typeof(DefaultValueAttribute));
                if (dva != null)
                    xtw.WriteAttributeString("default", XmlDefaultValue(dva.Value));

                string annotation = FindAnnotation(pi);
                if (annotation != null)
                {
                    xtw.WriteStartElement("xs:annotation");
                    xtw.WriteStartElement("xs:documentation");
                    xtw.WriteString(annotation);
                    xtw.WriteEndElement();
                    xtw.WriteEndElement();
                }

                typesToDump.Add(pi.PropertyType);
                xtw.WriteEndElement();
            }

            /*
            if (typeof(Target).IsAssignableFrom(t))
            {
                TargetAttribute ta = (TargetAttribute)Attribute.GetCustomAttribute(t, typeof(TargetAttribute));
                if (ta != null && !ta.IgnoresLayout)
                {
                    xtw.WriteStartElement("xs:attribute");
                    xtw.WriteAttributeString("name", "layout");
                    xtw.WriteAttributeString("type", "NLogLayout");
                    xtw.WriteEndElement();
                }
            }
           */

            if (t.BaseType != typeof(object) || t.GetInterface("NLog.ILayout") != null)
            {
                xtw.WriteEndElement(); // xs:extension
                xtw.WriteEndElement(); // xs:complexContent
            }

            xtw.WriteEndElement(); // xs:complexType

            foreach (Type ttd in typesToDump)
            {
                DumpType(xtw, ttd);
            }
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MakeNLogXSD outputfile.xsd path_to_doc.xml");
                return 1;
            }

            try
            {
                _docXml.Load(args[1]);

                for (int i = 2; i < args.Length; ++i)
                {
                    try
                    {
                        Assembly asm = Assembly.Load(args[i]);
                        NLogFactories.ScanAssembly(asm, "");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WARNING: {0}", ex.Message);
                    }
                }

                StringWriter sw = new StringWriter();

                sw.Write("<root xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">");

                XmlTextWriter xtw = new XmlTextWriter(sw);
                xtw.Namespaces = false;
                xtw.Formatting = Formatting.Indented;

                _typeDumped[typeof(object)] = 1;
                _typeDumped[typeof(Target)] = 1;
                _typeDumped[typeof(TargetWithLayout)] = 1;
                _typeDumped[typeof(TargetWithLayoutHeaderAndFooter)] = 1;
                _typeDumped[typeof(Layout)] = 1;

                foreach (Type targetType in NLogFactories.TargetFactory.RegisteredItems)
                {
                    DumpType(xtw, targetType);
                }
                foreach (Type t in NLogFactories.FilterFactory.RegisteredItems)
                {
                    DumpType(xtw, t);
                }
                foreach (Type t in NLogFactories.LayoutFactory.RegisteredItems)
                {
                    DumpType(xtw, t);
                }
                xtw.Flush();
                sw.Write("</root>");
                sw.Flush();

                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(sw.ToString());

                using (Stream templateStream = Assembly.GetEntryAssembly().GetManifestResourceStream("MakeNLogXSD.TemplateNLog.xsd"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(templateStream);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("", "http://www.w3.org/2001/XMLSchema");

                    XmlNode n = doc.SelectSingleNode("//types-go-here");

                    foreach (XmlElement el in doc2.DocumentElement.ChildNodes)
                    {
                        XmlNode importedNode = doc.ImportNode(el, true);
                        n.ParentNode.InsertBefore(importedNode, n);
                    }
                    n.ParentNode.RemoveChild(n);

                    n = doc.SelectSingleNode("//filters-go-here");
                    foreach (Type t in NLogFactories.FilterFactory.RegisteredItems)
                    {
                        FilterAttribute fa = (FilterAttribute)Attribute.GetCustomAttribute(t, typeof(FilterAttribute));
                        XmlElement el = doc.CreateElement("xs:element", XmlSchema.Namespace);
                        el.SetAttribute("name", fa.Name);
                        el.SetAttribute("type", SimpleTypeName(t));
                        n.ParentNode.InsertBefore(el, n);
                    }
                    n.ParentNode.RemoveChild(n);

                    Console.WriteLine("Saving schema to: {0}", args[0]);
                    doc.Save(args[0]);
                    return 0;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }
    }
}
