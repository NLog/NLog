using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace BuildDocPages
{
    public class DocPageGenerator
    {
        public DocPageGenerator()
        {
        }

        public string InputFile { get; set; }

        public string OutputDirectory { get; set; }

        public string BaseDirectory { get; set; }

        public string FileSuffix { get; set; }

        public string Mode { get; set; }

        private XElement inputDocument;
        private XslCompiledTransform transform = new XslCompiledTransform();

        public void Generate()
        {
            transform = new XslCompiledTransform();
            transform.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "style.xsl"));

            LoadInputFile();
            GenerateKind("target", "Target");
            GenerateKind("layout-renderer", "Layout Renderer");
            GenerateKind("filter", "Filter");
            GenerateKind("layout", "Layout");
        }

        private void GenerateKind(string kind, string kindName)
        {
            Dictionary<string, string> name2slug = new Dictionary<string, string>();
            Directory.CreateDirectory(this.OutputDirectory + "/" + kind);

            foreach (var it in inputDocument.Elements("type").Where(c => c.Attribute("kind").Value == kind).Select(c => new { Name = c.Attribute("name").Value, Slug = c.Attribute("slug").Value, Title = c.Attribute("title").Value }))
            {
                GenerateSinglePage(kind, kindName, it.Name, it.Slug, it.Title);
            }

            GenerateMergedPage(kind, kindName);
        }

        private void GenerateSinglePage(string kind, string kindName, string name, string slug, string title)
        {
            string filename = Path.Combine(this.OutputDirectory, kind + "/" + name + "." + this.FileSuffix);

            Console.WriteLine("Generating {0}", filename);

            Dictionary<string, XElement> inputXml = new Dictionary<string, XElement>();

            var type = inputDocument.Elements("type").Where(c => c.Attribute("kind").Value == kind && c.Attribute("name").Value == name).Single();

            InsertExamples(type);

            using (var reader = type.CreateReader())
            {
                using (XmlWriter writer = XmlWriter.Create(filename, new XmlWriterSettings { OmitXmlDeclaration = true }))
                {
                    XsltArgumentList arguments = new XsltArgumentList();
                    arguments.AddParam("kind", "", kind);
                    arguments.AddParam("kindName", "", kindName);
                    arguments.AddParam("name", "", name);
                    arguments.AddParam("slug", "", slug);
                    arguments.AddParam("mode", "", this.Mode);
                    transform.Transform(reader, arguments, writer);
                }
            }

            // File.WriteAllText(Path.ChangeExtension(filename, ".title"), title);
        }

        private void InsertExamples(XElement type)
        {
            foreach (var code in type.Descendants("code").Where(c=>c.Attribute("source") != null))
            {
                string fullName = Path.Combine(this.BaseDirectory, code.Attribute("source").Value);
                if (File.Exists(fullName))
                {
                    string language;

                    switch (code.Attribute("lang").Value.ToLowerInvariant())
                    {
                        case "c#":
                            language = "CSharp";
                            break;

                        case "xml":
                            language = "XML";
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    string content = File.ReadAllText(fullName);

                    content = SyntaxHighlighting(content, language);
                    var pre = new XElement("pre", content);

                    code.AddBeforeSelf(pre);
                }
                else
                {
                    Console.WriteLine("Warning: {0} not found.", fullName);
                }
            }
        }

        private string SyntaxHighlighting(string content, string language)
        {
            if (language == "XML")
            {
                return content.Replace("<", "&lt;").Replace(">", "&gt;");

            }

            return content;
        }

        private void GenerateMergedPage(string kind, string kindName)
        {
            string filename = Path.Combine(this.OutputDirectory, kind + "/merged." + this.FileSuffix);
            string xhtml = "http://www.w3.org/1999/xhtml";

            using (XmlWriter writer = XmlWriter.Create(filename, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("html", xhtml);
                writer.WriteStartElement("head", xhtml);
                writer.WriteStartElement("link", xhtml);
                writer.WriteAttributeString("type", "text/css");
                writer.WriteAttributeString("rel", "stylesheet");
                writer.WriteAttributeString("href", "../../../../style.css");
                writer.WriteEndElement(); // link
                writer.WriteEndElement(); // head
                writer.WriteStartElement("body");

                foreach (var it in inputDocument.Elements("type").Where(c => c.Attribute("kind").Value == kind).Select(c => new { Name = c.Attribute("name").Value, Slug = c.Attribute("slug").Value, Title = c.Attribute("title").Value }))
                {
                    var type = inputDocument.Elements("type").Where(c => c.Attribute("kind").Value == kind && c.Attribute("name").Value == it.Name).Single();

                    InsertExamples(type);

                    writer.WriteStartElement("hr");
                    writer.WriteEndElement();
                    writer.WriteElementString("h4", it.Slug);
                    using (var reader = type.CreateReader())
                    {
                        XsltArgumentList arguments = new XsltArgumentList();
                        arguments.AddParam("kind", "", kind);
                        arguments.AddParam("kindName", "", kindName);
                        arguments.AddParam("name", "", it.Name);
                        arguments.AddParam("slug", "", it.Slug);
                        arguments.AddParam("mode", "", "plain");
                        transform.Transform(reader, arguments, writer);
                    }
                }
                writer.WriteEndElement(); // body
                writer.WriteEndElement(); // html
            }
        }

        private void LoadInputFile()
        {
            inputDocument = XElement.Load(this.InputFile);
        }
    }
}
