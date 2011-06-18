// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace MakeNLogXSD
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    public class XsdFileGenerator
    {
        private static XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

        private HashSet<string> typesEmitted;
        private XElement template;
        private XElement typesGoHere;
        private XElement filtersGoHere;

        public XsdFileGenerator()
        {
            this.template = LoadTemplateXSD();

            this.filtersGoHere = template.Descendants("filters-go-here").Single();
            this.typesGoHere = template.Descendants("types-go-here").Single();

            this.typesEmitted = new HashSet<string>();
        }

        public string TargetNamespace { get; set; }

        public void ProcessApiFile(string apiFile)
        {
            Console.WriteLine("Processing API file '{0}'", Path.GetFullPath(apiFile));
            XElement api = XElement.Load(apiFile);

            foreach (var type in api.Elements("type"))
            {
                string typeName = (string)type.Attribute("name");

                string baseType = null;
                switch ((string)type.Attribute("kind"))
                {
                    case "target":
                        baseType = "Target";
                        if ((string)type.Attribute("iswrapper") == "1")
                        {
                            baseType = "WrapperTargetBase";
                        }
                        if ((string)type.Attribute("iscompound") == "1")
                        {
                            baseType = "CompoundTargetBase";
                        }
                        break;

                    case "filter":
                        baseType = "Filter";
                        break;

                    case "layout":
                        baseType = "Layout";
                        break;
                }

                if (baseType == null)
                {
                    continue;
                }

                var typeElement = new XElement(xsd + "complexType",
                    new XAttribute("name", (string)type.Attribute("name")),
                    new XElement(xsd + "complexContent",
                        new XElement(xsd + "extension",
                            new XAttribute("base", baseType),
                            new XElement(xsd + "choice",
                                new XAttribute("minOccurs", "0"),
                                new XAttribute("maxOccurs", "unbounded"),
                                GetPropertyElements(type)),
                            GetAttributeElements(type))));

                typesGoHere.AddBeforeSelf(typeElement);

                foreach (var enumProperty in type.Descendants("property").Where(c => (string)c.Attribute("type") == "Enum"))
                {
                    string enumType = (string)enumProperty.Attribute("enumType");
                    if (!typesEmitted.Contains(enumType))
                    {
                        typesEmitted.Add(enumType);
                        typesGoHere.AddBeforeSelf(GenerateEnumType(enumProperty));
                    }
                }

                foreach (var elementType in type.Descendants("elementType"))
                {
                    string collectionElementType = (string)elementType.Attribute("name");
                    if (!typesEmitted.Contains(collectionElementType))
                    {
                        typesEmitted.Add(collectionElementType);

                        typesGoHere.AddBeforeSelf(new XElement(xsd + "complexType",
                            new XAttribute("name", collectionElementType),
                            new XElement(xsd + "choice",
                                new XAttribute("minOccurs", "0"),
                                new XAttribute("maxOccurs", "unbounded"),
                                GetPropertyElements(elementType)),
                            GetAttributeElements(elementType)));
                    }
                }

                if (baseType == "Filter")
                {
                    filtersGoHere.AddBeforeSelf(new XElement(xsd + "element",
                        new XAttribute("name", typeName),
                        new XAttribute("type", typeName)));
                }
            }
        }


        public void SaveResult(string outputFile)
        {
            template.Attribute("targetNamespace").Value = this.TargetNamespace;
            template.Attribute("xmlns").Value = this.TargetNamespace;
            filtersGoHere.Remove();
            typesGoHere.Remove();

            Console.WriteLine("Saving '{0}'", Path.GetFullPath(outputFile));
            template.Save(outputFile);
        }

        private XElement LoadTemplateXSD()
        {
            XElement xElement;

            using (var stream = this.GetType().Assembly.GetManifestResourceStream(this.GetType(), "TemplateXSD.xml"))
            {
                using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = true }))
                {
                    xElement = XElement.Load(reader);
                }
            }

            return xElement;
        }

        private static IEnumerable<XElement> GetAttributeElements(XElement type)
        {
            return type.Elements("property").Select(GetAttributeElement).ToList();
        }

        private static XElement GetAttributeElement(XElement propertyElement)
        {
            var result = new XElement(xsd + "attribute",
                new XAttribute("name", (string)propertyElement.Attribute("camelName")));

            var propertyType = (string)propertyElement.Attribute("type");
            if (propertyType == "Collection")
            {
                return null;
            }

            if (propertyType == "Enum")
            {
                string enumType = (string)propertyElement.Attribute("enumType");

                result.Add(new XAttribute("type", enumType));
            }
            else
            {
                result.Add(new XAttribute("type", GetXsdType(propertyType, true)));
            }

            var doc = propertyElement.Element("doc");
            if (doc != null)
            {
                var summary = doc.Element("summary");
                if (summary != null)
                {
                    result.Add(new XElement(xsd + "annotation",
                        new XElement(xsd + "documentation",
                            summary.Value)));
                }
            }

            return result;
        }

        private static XElement GenerateEnumType(XElement enumProperty)
        {
            var enumerationValues = enumProperty.Elements("enum")
                .Select(c => new XElement(xsd + "enumeration",
                                 new XAttribute("value", (string)c.Attribute("name"))));

            return new XElement(xsd + "simpleType",
                new XAttribute("name", (string)enumProperty.Attribute("enumType")),
                new XElement(xsd + "restriction",
                    new XAttribute("base", "xs:string"),
                    enumerationValues));
        }

        private static IEnumerable<XElement> GetPropertyElements(XElement type)
        {
            var results = new List<XElement>();
            foreach (var propertyElement in type.Elements("property"))
            {
                results.Add(GetPropertyElement(propertyElement));
            }

            return results;
        }

        private static XElement GetPropertyElement(XElement propertyElement)
        {
            var result = new XElement(xsd + "element",
                new XAttribute("name", (string)propertyElement.Attribute("camelName")),
                new XAttribute("minOccurs", "0"));

            string propertyType = (string)propertyElement.Attribute("type");
            if (propertyType == "Collection")
            {
                result.Add(new XAttribute("maxOccurs", "unbounded"));
                var elementType = propertyElement.Element("elementType");
                result.Attribute("name").Value = (string)elementType.Attribute("elementTag");
                result.Add(new XAttribute("type", (string)elementType.Attribute("name")));
            }
            else if (propertyType == "Enum")
            {
                string enumType = (string)propertyElement.Attribute("enumType");

                result.Add(new XAttribute("maxOccurs", "1"));
                result.Add(new XAttribute("type", enumType));
            }
            else
            {
                result.Add(new XAttribute("maxOccurs", "1"));
                result.Add(new XAttribute("type", GetXsdType(propertyType, false)));
            }

            return result;
        }

        private static string GetXsdType(string apiTypeName, bool attribute)
        {
            switch (apiTypeName)
            {
                case "Layout":
                    return attribute ? "SimpleLayoutAttribute" : "Layout";

                case "Condition":
                    return "Condition";

                case "String":
                    return "xs:string";

                case "Integer":
                    return "xs:integer";

                case "Long":
                    return "xs:long";

                case "Byte":
                    return "xs:byte";

                case "Boolean":
                    return "xs:boolean";

                case "Encoding":
                    return "xs:string";

                case "Culture":
                    return "xs:string";

                case "Char":
                    return "xs:string";

                case "System.Type":
                    return "xs:string";

                case "System.Uri":
                    return "xs:anyURI";

                default:
                    throw new NotSupportedException("Unknown API type '" + apiTypeName + "'.");
            }
        }
    }
}
