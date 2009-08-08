using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Globalization;

namespace MakeNLogDoc
{
    public class DocFileBuilder
    {
        private List<Assembly> assemblies = new List<Assembly>();
        private List<string> referenceAssemblyDirectories = new List<string>();
        private List<XmlDocument> comments = new List<XmlDocument>();

        private static Dictionary<string, string> simpleTypeNames = new Dictionary<string, string>()
        {
            { typeof(string).FullName, "String" },
            { typeof(int).FullName, "Integer" },
            { typeof(bool).FullName, "Boolean" },
            { typeof(char).FullName, "Char" },
            { typeof(CultureInfo).FullName, "Culture" },
            { typeof(Encoding).FullName, "Encoding" },
            { "NLog.Layouts.Layout", "Layout" },
            { "NLog.Targets.Target", "Target" },
            { "NLog.Conditions.ConditionExpression", "Condition" },
            { "NLog.Filters.FilterResult", "FilterResult" },
        };


        public void LoadAssembly(string fileName)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
            try
            {
                assemblies.Add(Assembly.ReflectionOnlyLoadFrom(fileName));
            }
            finally
            {
                //AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
            }
        }

        public void AddReferenceDirectory(string dir)
        {
            referenceAssemblyDirectories.Add(dir);
        }

        public void LoadComments(string commentsFile)
        {
            var commentsDoc = new XmlDocument();
            commentsDoc.Load(commentsFile);
            comments.Add(commentsDoc);
        }
        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName asmName = new AssemblyName(args.Name);
            string assemblyFileName = asmName.Name + ".dll";
            foreach (string refDir in referenceAssemblyDirectories)
            {
                string fileName = Path.Combine(refDir, assemblyFileName);
                if (File.Exists(fileName))
                {
                    return Assembly.ReflectionOnlyLoadFrom(fileName);
                }
            }
            Console.WriteLine("Could not resolve: {0}", assemblyFileName);

            throw new FileNotFoundException(assemblyFileName + " not found in reference assembly.");
        }

        public void Build(string outputFile)
        {
            using (XmlWriter writer = XmlWriter.Create(outputFile, new XmlWriterSettings { Indent = true }))
            {
                Build(writer);
            }
        }

        public void Build(XmlWriter writer)
        {
            writer.WriteStartElement("types");
            DumpApiDocs(writer, "target", "NLog.Targets.TargetAttribute");
            DumpApiDocs(writer, "layout", "NLog.Layouts.LayoutAttribute");
            DumpApiDocs(writer, "lr", "NLog.LayoutRenderers.LayoutRendererAttribute");
            DumpApiDocs(writer, "filter", "NLog.Filters.FilterAttribute");
            writer.WriteEndElement();
        }

        private IEnumerable<Type> GetTypesWithAttribute(string attributeTypeName)
        {
            foreach (Assembly assembly in this.assemblies)
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (HasAttribute(t, attributeTypeName))
                    {
                        yield return t;
                    }
                }
            }
        }

        private bool HasAttribute(Type type, string attributeTypeName)
        {
            foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(type))
            {
                if (cad.Constructor.DeclaringType.FullName == attributeTypeName)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetMemberDoc(string id, out XmlElement element)
        {
            foreach (XmlDocument doc in comments)
            {
                element = (XmlElement)doc.SelectSingleNode("/doc/members/member[@name='" + id + "']");
                if (element != null)
                {
                    return true;
                }
            }

            element = null;
            return false;
        }

        private void DumpApiDocs(XmlWriter writer, string kind, string attribteTypeName)
        {
            foreach (Type type in GetTypesWithAttribute(attribteTypeName))
            {
                writer.WriteStartElement("type");
                writer.WriteAttributeString("kind", kind);
                writer.WriteAttributeString("id", type.FullName);
                string name;
                if (TryGetTypeNameFromNameAttribute(type, attribteTypeName, out name))
                {
                    writer.WriteAttributeString("name", name);
                }

                XmlElement memberDoc;
                if (TryGetMemberDoc("T:" + type.FullName, out memberDoc))
                {
                    writer.WriteStartElement("doc");
                    memberDoc.WriteContentTo(writer);
                    writer.WriteEndElement();
                }

                foreach (PropertyInfo propInfo in type.GetProperties())
                {
                    writer.WriteStartElement("property");
                    writer.WriteAttributeString("name", propInfo.Name);
                    writer.WriteAttributeString("type", GetTypeName(propInfo.PropertyType));
                    if (IsCollection(propInfo.PropertyType))
                    {
                        writer.WriteAttributeString("collection", "true");
                    }
                    if (TryGetMemberDoc("P:" + propInfo.DeclaringType.FullName + "." + propInfo.Name  , out memberDoc))
                    {
                        writer.WriteStartElement("doc");
                        memberDoc.WriteContentTo(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        private static bool IsCollection(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return true;
                }

                throw new NotSupportedException("Type: " + type.FullName);
            }

            return false;
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return GetTypeName(type.GetGenericArguments()[0]);
                }

                throw new NotSupportedException("Type: " + type.FullName);
            }

            string simpleName;

            if (simpleTypeNames.TryGetValue(type.FullName, out simpleName))
            {
                return simpleName;
            }

            return type.FullName;
        }

        private static bool TryGetTypeNameFromNameAttribute(Type type, string attributeTypeName, out string name)
        {
            foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(type))
            {
                if (cad.Constructor.DeclaringType.FullName == attributeTypeName)
                {
                    name = (string)cad.ConstructorArguments[0].Value;
                    return true;
                }
            }

            name = null;
            return false;
        }
    }
}
