namespace DumpApiXml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public class DocFileBuilder
    {
        private static readonly Dictionary<string, string> simpleTypeNames = new Dictionary<string, string>()
        {
            { typeof(string).FullName, "String" },
            { typeof(int).FullName, "Integer" },
            { typeof(long).FullName, "Long" },
            { typeof(bool).FullName, "Boolean" },
            { typeof(char).FullName, "Char" },
            { typeof(byte).FullName, "Byte" },
            { typeof(short).FullName, "Integer" },
            { typeof(CultureInfo).FullName, "Culture" },
            { typeof(Encoding).FullName, "Encoding" },
            { "NLog.Layouts.Layout", "Layout" },
            { "NLog.Targets.Target", "Target" },
            { "NLog.Targets.LineEndingMode", "LineEndingMode" },
            { "NLog.Conditions.ConditionExpression", "Condition" },
            { "NLog.Filters.FilterResult", "FilterResult" },
            { "NLog.Layout", "Layout" },
            { "NLog.Target", "Target" },
            { "NLog.ConditionExpression", "Condition" },
            { "NLog.FilterResult", "FilterResult" },
        };

        private readonly List<Assembly> assemblies = new List<Assembly>();
        private readonly List<XmlDocument> comments = new List<XmlDocument>();

        public void Build(string outputFile)
        {
            using XmlWriter writer = XmlWriter.Create(outputFile, new XmlWriterSettings
            {
                Indent = true
            });
            Build(writer);
        }

        public void Build(XmlWriter writer)
        {
            writer.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='style.xsl'");
            writer.WriteStartElement("types");
            DumpApiDocs(writer, "target", "NLog.Targets.TargetAttribute", "", " target");
            DumpApiDocs(writer, "layout", "NLog.Layouts.LayoutAttribute", "", "");
            DumpApiDocs(writer, "layout-renderer", "NLog.LayoutRenderers.LayoutRendererAttribute", "${", "}");
            DumpApiDocs(writer, "filter", "NLog.Filters.FilterAttribute", "", " filter");

            DumpApiDocs(writer, "target", "NLog.TargetAttribute", "", " target");
            DumpApiDocs(writer, "layout", "NLog.LayoutAttribute", "", "");
            DumpApiDocs(writer, "layout-renderer", "NLog.LayoutRendererAttribute", "${", "}");
            DumpApiDocs(writer, "filter", "NLog.FilterAttribute", "", " filter");

            DumpApiDocs(writer, "time-source", "NLog.Time.TimeSourceAttribute", "", " time source");
            writer.WriteEndElement();
        }

        public void DisableComments()
        {
            comments.Clear();
        }

        public void LoadAssembly(string fileName)
        {
            assemblies.Add(Assembly.LoadFrom(fileName));
        }

        public void LoadComments(string commentsFile)
        {
            var commentsDoc = new XmlDocument();
            commentsDoc.Load(commentsFile);
            FixWhitespace(commentsDoc.DocumentElement);
            comments.Add(commentsDoc);
        }

        private void FixWhitespace(XmlElement xmlElement)
        {
            foreach (var node in xmlElement.ChildNodes)
            {
                if (node is XmlElement el)
                {
                    FixWhitespace(el);
                    continue;
                }

                if (node is XmlText txt)
                {
                    txt.Value = FixWhitespace(txt.Value);
                }
            }
        }

        private string FixWhitespace(string p)
        {
            p = p.Replace("\n", " ");
            p = p.Replace("\r", " ");
            string oldP = "";

            while (oldP != p)
            {
                oldP = p;
                p = p.Replace("  ", " ");
            }

            return p;
        }

        private static string GetTypeName(Type type)
        {
            type = GetUnderlyingType(type);

            if (simpleTypeNames.TryGetValue(type.FullName, out string simpleName))
            {
                return simpleName;
            }

            return type.FullName;
        }

        private static Type GetUnderlyingType(Type type)
        {
            if (type.IsGenericType && type.Name.StartsWith("Layout"))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            return type;
        }

        private static bool IsCollection(PropertyInfo prop, out Type elementType, out string elementName)
        {

            if (TryGetFirstTwoArgumentForAttribute(prop, "NLog.Config.ArrayParameterAttribute", out object v1, out object v2))
            {
                elementType = (Type)v1;
                elementName = (string)v2;
                return true;
            }
            else
            {
                elementName = null;
                elementType = null;
                return false;
            }
        }

        private static bool TryGetFirstArgumentForAttribute(Type type, string attributeTypeName, out object value)
        {
            foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(type))
            {
                if (cad.Constructor.DeclaringType.FullName == attributeTypeName)
                {
                    value = cad.ConstructorArguments[0].Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static bool TryGetFirstTwoArgumentForAttribute(PropertyInfo propInfo, string attributeTypeName, out object value1, out object value2)
        {
            foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(propInfo))
            {
                if (cad.Constructor.DeclaringType.FullName == attributeTypeName)
                {
                    value1 = cad.ConstructorArguments[0].Value;
                    value2 = cad.ConstructorArguments[1].Value;
                    return true;
                }
            }

            value1 = value2 = null;
            return false;
        }

        private static bool TryGetTypeNameFromNameAttribute(Type type, string attributeTypeName, out string name)
        {
            if (TryGetFirstArgumentForAttribute(type, attributeTypeName, out object val))
            {
                name = (string)val;
                return true;
            }

            name = null;
            return false;
        }

        private string Capitalize(string p)
        {
            return p.Substring(0, 1).ToUpper() + p.Substring(1);
        }

        private void DumpApiDocs(XmlWriter writer, string kind, string attributeTypeName, string titlePrefix, string titleSuffix)
        {
            foreach (Type type in GetTypesWithAttribute(attributeTypeName).OrderBy(t => t.Name))
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                writer.WriteStartElement("type");
                writer.WriteAttributeString("kind", kind);
                writer.WriteAttributeString("assembly", type.Assembly.GetName().Name);
                writer.WriteAttributeString("clrType", type.FullName);

                if (TryGetTypeNameFromNameAttribute(type, attributeTypeName, out string name))
                {
                    writer.WriteAttributeString("name", name);
                }
                writer.WriteAttributeString("slug", GetSlug(name, kind));
                writer.WriteAttributeString("title", titlePrefix + name + titleSuffix);

                if (InheritsFrom(type, "CompoundTargetBase"))
                {
                    writer.WriteAttributeString("iscompound", "1");
                }

                if (InheritsFrom(type, "WrapperTargetBase"))
                {
                    writer.WriteAttributeString("iswrapper", "1");
                }

                if (InheritsFrom(type, "WrapperLayoutRendererBase"))
                {
                    writer.WriteAttributeString("iswrapper", "1");
                }

                if (TryGetTypeNameFromNameAttribute(type, "NLog.LayoutRenderers.AmbientPropertyAttribute", out string ambientPropName))
                {
                    writer.WriteAttributeString("ambientProperty", ambientPropName);
                }

                DumpTypeMembers(writer, type);

                writer.WriteEndElement();
            }
        }

        private bool InheritsFrom(Type type, string typeName)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                if (t.Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private void DumpTypeMembers(XmlWriter writer, Type type)
        {
            var categories = new Dictionary<string, int>();

            categories["General Options"] = 10;
            categories["Layout Options"] = 20;

            if (TryGetMemberDoc("T:" + type.FullName, out XmlElement memberDoc))
            {
                writer.WriteStartElement("doc");
                memberDoc.WriteContentTo(writer);
                writer.WriteEndElement();

                foreach (XmlElement element in memberDoc.SelectNodes("docgen/categories/category"))
                {
                    string categoryName = element.GetAttribute("name");
                    int order = Convert.ToInt32(element.GetAttribute("order"));

                    categories[categoryName] = order;
                }
            }

            var property2Category = new Dictionary<PropertyInfo, string>();
            var propertyOrderWithinCategory = new Dictionary<PropertyInfo, int>();
            var propertyDoc = new Dictionary<PropertyInfo, XmlElement>();

            var propertyInfos = GetProperties(type).ToList();
            foreach (PropertyInfo propInfo in propertyInfos)
            {
                string category = null;
                int order = 100;

                if (HasAttribute(propInfo, "NLog.Config.NLogConfigurationIgnorePropertyAttribute"))
                {
                    Console.WriteLine("SKIP {0}.{1}, it has [NLogConfigurationIgnoreProperty]", type.Name, propInfo.Name);
                    continue;
                }

                if (HasAttribute(propInfo, "System.ObsoleteAttribute"))
                {
                    Console.WriteLine("SKIP [Obsolete] {0}.{1}", type.Name, propInfo.Name);
                    continue;
                }

                if (TryGetMemberDoc("P:" + propInfo.DeclaringType.FullName + "." + propInfo.Name, out memberDoc))
                {
                    propertyDoc.Add(propInfo, memberDoc);

                    var docgen = (XmlElement)memberDoc.SelectSingleNode("docgen");
                    if (docgen != null)
                    {
                        category = docgen.GetAttribute("category");
                        order = Convert.ToInt32(docgen.GetAttribute("order"));
                    }
                }

                if (string.IsNullOrEmpty(category))
                {
                    Console.WriteLine("WARNING: Property {0}.{1} does not have <docgen /> element defined.",
                        propInfo.DeclaringType.Name, propInfo.Name);

                    category = "Other";
                }

                if (!categories.TryGetValue(category, out _))
                {
                    categories.Add(category, 100 + categories.Count);
                }

                //Console.WriteLine("p: {0} cat: {1} order: {2}", propInfo.Name, category, order);
                property2Category[propInfo] = category;
                propertyOrderWithinCategory[propInfo] = order;
            }

            if (categories.Count == 0)
                return;

            object configInstance = null;
            try
            {
                if (!type.IsAbstract)
                {
                    configInstance = Activator.CreateInstance(type);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAILED to create default instance of {0} - {1}", type.Name, ex.ToString());
            }

            foreach (string category in categories.OrderBy(c => c.Value).Select(c => c.Key))
            {
                string categoryName = category;

                foreach (PropertyInfo propInfo in propertyInfos
                    .Where(p => property2Category.ContainsKey(p) && propertyOrderWithinCategory.ContainsKey(p))
                        .Where(p => property2Category[p] == categoryName).OrderBy(
                            p => propertyOrderWithinCategory[p]).ThenBy(pi => pi.Name))
                {

                    writer.WriteStartElement("property");
                    writer.WriteAttributeString("name", propInfo.Name);
                    writer.WriteAttributeString("camelName", MakeCamelCase(propInfo.Name));

                    if (TryGetPropertyDefaultValue(configInstance, propInfo, out string defaultValue))
                    {
                        writer.WriteAttributeString("defaultValue", defaultValue);
                    }

                    writer.WriteAttributeString("category", categoryName);

                    if (HasAttribute(propInfo, "NLog.Config.RequiredParameterAttribute"))
                    {
                        writer.WriteAttributeString("required", "1");
                    }
                    else
                    {
                        writer.WriteAttributeString("required", "0");
                    }

                    if (propInfo.Name == "Encoding")
                    {
                        writer.WriteAttributeString("type", "Encoding");
                    }
                    else if (HasAttribute(propInfo, "NLog.Config.AcceptsLayoutAttribute") ||
                             propInfo.Name == "Layout")
                    {
                        writer.WriteAttributeString("type", "Layout");
                    }
                    else if (HasAttribute(propInfo, "NLog.Config.AcceptsConditionAttribute") ||
                             propInfo.Name == "Condition")
                    {
                        writer.WriteAttributeString("type", "Condition");
                    }
                    else if (IsCollection(propInfo, out Type elementType, out string elementTag))
                    {
                        writer.WriteAttributeString("type", "Collection");
                        writer.WriteStartElement("elementType");
                        writer.WriteAttributeString("name", GetTypeName(elementType));
                        writer.WriteAttributeString("elementTag", elementTag);
                        if (elementType != type)
                        {
                            DumpTypeMembers(writer, elementType);
                        }
                        writer.WriteEndElement();
                    }
                    else
                    {
                        var underlyingType = GetUnderlyingType(propInfo.PropertyType);
                        var typeName = GetTypeName(underlyingType);
                        if (underlyingType.IsEnum)
                        {
                            writer.WriteAttributeString("type", "Enum");
                            writer.WriteAttributeString("enumType", typeName);
                            foreach (FieldInfo fi in
                                underlyingType.GetFields(BindingFlags.Static | BindingFlags.Public))
                            {
                                writer.WriteStartElement("enum");
                                writer.WriteAttributeString("name", fi.Name);
                                if (
                                    TryGetMemberDoc(
                                        "F:" + underlyingType.FullName.Replace("+", ".") + "." + fi.Name,
                                        out memberDoc))
                                {
                                    writer.WriteStartElement("doc");
                                    memberDoc.WriteContentTo(writer);
                                    writer.WriteEndElement();
                                }

                                writer.WriteEndElement();
                            }
                        }
                        else
                        {
                            writer.WriteAttributeString("type", typeName);
                        }
                    }

                    if (TryGetMemberDoc("P:" + propInfo.DeclaringType.FullName + "." + propInfo.Name, out memberDoc))
                    {
                        writer.WriteStartElement("doc");
                        memberDoc.WriteContentTo(writer);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
        }

        private static bool TryGetPropertyDefaultValue(object configInstance, PropertyInfo propInfo, out string defaultValue)
        {
            try
            {
                object propertyValue = configInstance != null ? propInfo.GetValue(configInstance) : null;
                if (propertyValue is Enum)
                {
                    defaultValue = propertyValue.ToString();
                    return true;
                }

                if (propertyValue is Encoding encoding)
                {
                    defaultValue = encoding.WebName;
                    return true;
                }

                var underlyingType = GetUnderlyingType(propInfo.PropertyType);
                if (propertyValue != null && underlyingType != null && !ReferenceEquals(underlyingType, propInfo.PropertyType) && !underlyingType.Equals(propertyValue?.GetType()))
                {
                    try
                    {
                        if (underlyingType.IsEnum)
                        {
                            var enumStringValue = propertyValue?.ToString();
                            defaultValue = Enum.Parse(underlyingType, enumStringValue, true).ToString();
                            return true;
                        }
                        else if (typeof(IConvertible).IsAssignableFrom(underlyingType))
                        {
                            var convStringValue = propertyValue?.ToString();
                            propertyValue = Convert.ChangeType(convStringValue, underlyingType);
                        }
                        else
                        {
                            defaultValue = null;
                            return false;
                        }
                    }
                    catch
                    {
                        defaultValue = null;
                        return false;
                    }
                }

                IConvertible convertibleValue = propertyValue as IConvertible;
                if (convertibleValue == null && propertyValue != null)
                {
                    if (propertyValue is System.Collections.IEnumerable)
                    {
                        defaultValue = string.Empty;
                        return true;
                    }

                    convertibleValue = Convert.ToString(propertyValue, CultureInfo.InvariantCulture);
                }

                switch (convertibleValue?.GetTypeCode() ?? TypeCode.Empty)
                {
                    case TypeCode.Boolean: defaultValue = XmlConvert.ToString((bool)convertibleValue); return true;
                    case TypeCode.DateTime: defaultValue = XmlConvert.ToString((DateTime)convertibleValue, XmlDateTimeSerializationMode.RoundtripKind); return true;
                    case TypeCode.Int16: defaultValue = XmlConvert.ToString((Int16)convertibleValue); return true;
                    case TypeCode.Int32: defaultValue = XmlConvert.ToString((Int32)convertibleValue); return true;
                    case TypeCode.Int64: defaultValue = XmlConvert.ToString((Int64)convertibleValue); return true;
                    case TypeCode.UInt16: defaultValue = XmlConvert.ToString((UInt16)convertibleValue); return true;
                    case TypeCode.UInt32: defaultValue = XmlConvert.ToString((UInt32)convertibleValue); return true;
                    case TypeCode.UInt64: defaultValue = XmlConvert.ToString((UInt64)convertibleValue); return true;
                    case TypeCode.Double: defaultValue = XmlConvert.ToString((Double)convertibleValue); return true;
                    case TypeCode.Single: defaultValue = XmlConvert.ToString((Single)convertibleValue); return true;
                    case TypeCode.Decimal: defaultValue = XmlConvert.ToString((Decimal)convertibleValue); return true;
                    case TypeCode.Char: defaultValue = XmlConvert.ToString((Char)convertibleValue); return true;
                    case TypeCode.Empty: defaultValue = null; return false;
                }

                defaultValue = convertibleValue?.ToString();
                return defaultValue != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAILED to lookup default value for property {0}-{1} - {2}", configInstance?.GetType(), propInfo?.Name, ex.ToString());
                defaultValue = null;
                return false;
            }
        }

        private void FixupElement(XmlElement element)
        {
            var summary = (XmlElement)element.SelectSingleNode("summary");
            summary?.InnerXml = FixupSummaryXml(summary.InnerXml);

            foreach (XmlElement code in element.SelectNodes("//code[@src]"))
            {
                code.SetAttribute("source", code.GetAttribute("src"));
                code.RemoveAttribute("src");
            }
        }

        private string FixupSummaryXml(string xml)
        {
            xml = xml.Trim();
            xml = ReplaceAndCapitalize(xml, "Gets or sets a value indicating ", "Indicates ");
            xml = ReplaceAndCapitalize(xml, "Gets or sets the ", "");
            xml = ReplaceAndCapitalize(xml, "Gets or sets a ", "");
            xml = ReplaceAndCapitalize(xml, "Gets or sets ", "");
            xml = ReplaceAndCapitalize(xml, "Gets the ", "The ");
            return xml;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties()
                .Where(c => IncludeProperty(c)).OrderBy(p => p.Name);
        }

        private string GetSlug(string name, string kind)
        {
            switch (kind)
            {
                case "target":
                    return name + "_target";

                case "layout-renderer":
                    return name + "_layout_renderer";

                case "layout":
                    return name;

                case "filter":
                    return name + "_filter";

                case "time-source":
                    return name + "_time_source";
            }

            string slugBase;

            if (name == name.ToUpperInvariant())
            {
                slugBase = name.ToLowerInvariant();
            }
            else
            {
                name = name.Replace("NLog", "Nlog");
                name = name.Replace("Log4J", "Log4j");

                var sb = new StringBuilder();
                for (int i = 0; i < name.Length; ++i)
                {
                    if (Char.IsUpper(name[i]) && i > 0)
                    {
                        sb.Append("_");
                    }
                    sb.Append(name[i]);
                }

                slugBase = sb.ToString();
            }

            if (kind == "layout")
            {
                return slugBase;
            }
            else
            {
                return slugBase + "-" + kind;
            }
        }

        private IEnumerable<Type> GetTypesWithAttribute(string attributeTypeName)
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type t in assembly.SafeGetTypes())
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
            try
            {
                foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(type))
                {
                    if (cad.Constructor.DeclaringType.FullName == attributeTypeName)
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool HasAttribute(PropertyInfo propertyInfo, string attributeTypeName)
        {
            try
            {
                foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(propertyInfo))
                {
                    if (cad.Constructor.DeclaringType.FullName == attributeTypeName)
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool IncludeProperty(PropertyInfo pi)
        {
            if (pi.CanRead && pi.CanWrite && pi.GetSetMethod() != null && pi.GetSetMethod().IsPublic)
            {
                if (pi.Name == "CultureInfo")
                {
                    return false;
                }

                if (pi.Name == "WrappedTarget")
                {
                    return false;
                }

                if ((pi.PropertyType.FullName.StartsWith("NLog.ILayout") || pi.PropertyType.FullName.StartsWith("NLog.Layout")) && pi.Name != "Layout" && pi.Name.EndsWith("Layout"))
                {
                    return false;
                }

                if (pi.PropertyType.FullName.StartsWith("NLog.Conditions.ConditionExpression") && pi.Name != "Condition" && pi.Name.EndsWith("Condition"))
                {
                    return false;
                }

                if (pi.Name.StartsWith("Compiled"))
                {
                    return false;
                }

                return true;
            }

            if (HasAttribute(pi, "NLog.Config.ArrayParameterAttribute"))
            {
                return true;
            }

            return false;
        }

        private string MakeCamelCase(string s)
        {
            if (s.Length <= 2)
            {
                return s.ToLowerInvariant();
            }

            if (s.ToUpperInvariant() == s)
            {
                return s.ToLowerInvariant();
            }

            if (s.StartsWith("DB"))
            {
                return "db" + s.Substring(2);
            }

            return s.Substring(0, 1).ToLowerInvariant() + s.Substring(1);
        }

        private string ReplaceAndCapitalize(string xml, string pattern, string replacement)
        {
            if (xml.StartsWith(pattern))
            {
                return Capitalize(xml.Replace(pattern, replacement));
            }
            return xml;
        }

        private bool TryGetMemberDoc(string id, out XmlElement element)
        {
            foreach (XmlDocument doc in comments)
            {
                element = (XmlElement)doc.SelectSingleNode("/doc/members/member[@name='" + id + "']");
                if (element != null)
                {
                    FixupElement(element);
                    return true;
                }
            }

            element = null;
            return false;
        }
    }

    public static class AssemblyExt
    {
        /// <summary>
        /// Gets all usable exported types from the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan.</param>
        /// <returns>Usable types from the given assembly.</returns>
        /// <remarks>Types which cannot be loaded are skipped.</remarks>
        public static Type[] SafeGetTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                foreach (var ex in typeLoadException.LoaderExceptions)
                {
                    //InternalLogger.Warn(ex, "Type load exception.");
                }

                var loadedTypes = new List<Type>();
                foreach (var t in typeLoadException.Types)
                {
                    if (t != null)
                    {
                        loadedTypes.Add(t);
                    }
                }

                return loadedTypes.ToArray();
            }
        }
    }
}
