namespace MergeApiXml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class NLogApiMerger
    {
        private List<KeyValuePair<string, string>> releases = new List<KeyValuePair<string, string>>();

        public XDocument Result { get; set; }

        public void AddRelease(string name, string baseDir)
        {
            if (Directory.Exists(baseDir))
            {
                releases.Add(new KeyValuePair<string, string>(name, baseDir));
            }
        }

        public void Merge()
        {
            var resultDoc = new XDocument(
                new XProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"style.xsl\""),
                new XElement("types"));

            foreach (var kvp in releases)
            {
                string releaseName = kvp.Key;
                string basePath = kvp.Value;

                foreach (string frameworkDir in Directory.GetDirectories(basePath))
                {
                    string apiFile = Path.Combine(frameworkDir, "API/NLog.api");
                    if (File.Exists(apiFile))
                    {
                        string frameworkName = Path.GetFileName(frameworkDir);
                        Console.WriteLine("Loading {0}", apiFile);
                        XElement apiDoc = XElement.Load(apiFile);
                        FixWhitespace(apiDoc);
                        MergeApiFile(resultDoc.Root, apiDoc, releaseName, frameworkName);
                    }
                }
            }

            foreach (var typeElement in resultDoc.Root.Elements("type"))
            {
                this.SortProperties(typeElement);
            }

            this.PostProcessSupportedIn(resultDoc.Root);
            this.Result = resultDoc;
        }

        private void FixWhitespace(XElement xmlElement)
        {
            foreach (var node in xmlElement.DescendantNodes())
            {
                XElement el = node as XElement;
                if (el != null)
                {
                    FixWhitespace(el);
                    continue;
                }

                XText txt = node as XText;
                if (txt != null)
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

        private void PostProcessSupportedIn(XElement rootElement)
        {
        }

        private void SortProperties(XElement typeElement)
        {
            var propertyElements = typeElement.Elements("property").ToList();
            foreach (var prop in propertyElements)
            {
                prop.Remove();
            }

            propertyElements.Sort((p1, p2) =>
                {
                    string cat1 = (string)p1.Attribute("category") ?? "Other";
                    string cat2 = (string)p2.Attribute("category") ?? "Other";

                    int v1 = GetCategoryValue(cat1);
                    int v2 = GetCategoryValue(cat2);

                    if (v1 != v2)
                    {
                        return v1 - v2;
                    }

                    return string.Compare(cat1, cat2, StringComparison.OrdinalIgnoreCase);
                });

            typeElement.Add(propertyElements);
        }

        private int GetCategoryValue(string categoryName)
        {
            switch (categoryName)
            {
                case "General Options":
                    return 0;

                case "Layout Options":
                    return 10;

                default:
                    return 100;
            }
        }

        private void MergeApiFile(XElement resultDoc, XElement apiDoc, string releaseName, string frameworkName)
        {
            AddSupportedIn(resultDoc, releaseName, frameworkName);

            MergeTypes(resultDoc, releaseName, frameworkName, apiDoc);
        }

        private void MergeTypes(XElement resultDoc, string releaseName, string frameworkName, XElement apiDoc)
        {
            foreach (var type in apiDoc.Elements("type"))
            {
                string kind = (string)type.Attribute("kind");
                string name = (string)type.Attribute("name");

                var mergedElement = resultDoc.Elements("type").Where(c => (string)c.Attribute("kind") == kind && string.Equals(name, (string)c.Attribute("name"), StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                if (mergedElement == null)
                {
                    Console.WriteLine("kind: {0} name: {1}", kind, name);
                    mergedElement = new XElement("type");
                    mergedElement.Add(new XAttribute("kind", kind));
                    mergedElement.Add(new XAttribute("name", name));
                    resultDoc.Add(mergedElement);
                }

                this.AddSupportedIn(mergedElement, releaseName, frameworkName);
                this.MergeDoc(mergedElement, type);
                this.MergeAttributes(mergedElement, type);
                this.MergeProperties(mergedElement, releaseName, frameworkName, type);
            }
        }

        private void MergeAttributes(XElement mergedElement, XElement sourceElement)
        {
            foreach (var attrib in sourceElement.Attributes())
            {
                mergedElement.SetAttributeValue(attrib.Name, attrib.Value);
            }
        }

        private void MergeDoc(XElement mergedElement, XElement sourceElement)
        {
            var oldDoc = mergedElement.Element("doc");
            var sourceDoc = sourceElement.Element("doc");
            if (oldDoc != null)
            {
                oldDoc.ReplaceWith(sourceDoc);
            }
            else
            {
                mergedElement.Add(sourceDoc);
            }
        }

        private void MergeProperties(XElement mergedType, string releaseName, string frameworkName, XElement inputType)
        {
            foreach (var property in inputType.Elements("property"))
            {
                string name = (string)property.Attribute("name");
                var mergedProperty = mergedType.Elements("property").Where(c => string.Equals((string)c.Attribute("name"), name, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                if (mergedProperty == null)
                {
                    mergedProperty = new XElement("property", new XAttribute("name", name));
                    mergedType.Add(mergedProperty);
                }

                mergedProperty.SetAttributeValue("name", (string)property.Attribute("name"));

                this.AddSupportedIn(mergedProperty, releaseName, frameworkName);
                this.MergeDoc(mergedProperty, property);
                this.MergeAttributes(mergedProperty, property);
                this.MergeEnumValues(mergedProperty, releaseName, frameworkName, property);

                var elementType = property.Element("elementType");
                if (elementType != null)
                {
                    var mergedElementType = mergedProperty.Element("elementType");
                    if (mergedElementType == null)
                    {
                        mergedElementType = new XElement("elementType");
                        mergedProperty.Add(mergedElementType);
                    }

                    this.AddSupportedIn(mergedElementType, releaseName, frameworkName);
                    this.MergeAttributes(mergedElementType, elementType);
                    this.MergeProperties(mergedElementType, releaseName, frameworkName, elementType);
                }
            }
        }

        private void MergeEnumValues(XElement mergedProperty, string releaseName, string frameworkName, XElement property)
        {
            foreach (var enumElement in property.Elements("enum"))
            {
                string enumName = (string)enumElement.Attribute("name");
                var mergedEnum = mergedProperty.Elements("enum").Where(c => (string)c.Attribute("name") == enumName).SingleOrDefault();
                if (mergedEnum == null)
                {
                    mergedEnum = new XElement("enum");
                    mergedProperty.Add(mergedEnum);
                }

                this.AddSupportedIn(mergedEnum, releaseName, frameworkName);
                this.MergeDoc(mergedEnum, enumElement);
                this.MergeAttributes(mergedEnum, enumElement);
            }
        }

        private void AddSupportedIn(XElement element, string releaseName, string frameworkName)
        {
            var supportedIn = element.Element("supported-in");
            if (supportedIn == null)
            {
                supportedIn = new XElement("supported-in");
                element.Add(supportedIn);
            }

            supportedIn.Add(new XElement("release", new XAttribute("name", releaseName), new XAttribute("framework", frameworkName)));
        }
    }
}
