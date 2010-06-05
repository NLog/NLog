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

        public string Stylesheet { get; set; }

        public string OutputDirectory { get; set; }

        public string BaseDirectory { get; set; }

        public string FileSuffix { get; set; }

        public string Mode { get; set; }

        private XElement inputDocument;
        private XslCompiledTransform transform = new XslCompiledTransform();

        public void Generate()
        {
            transform = new XslCompiledTransform();
            transform.Load(this.Stylesheet);

            LoadInputFile();
            GenerateKind("target", "Target");
            GenerateKind("layout-renderer", "Layout Renderer");
            GenerateKind("filter", "Filter");
            GenerateKind("layout", "Layout");

            GenerateMergedPage("target", "Target", "targets");
            GenerateMergedPage("target", "Target", "wrapper-targets");
            GenerateMergedPage("layout-renderer", "Layout Renderer", "layout-renderers");
            GenerateMergedPage("wrapper-layout-renderer", "Layout Renderer", "wrapper-layout-renderers");
            GenerateMergedPage("layout", "Layout", "layouts");
            GenerateMergedPage("filter", "Filter", "filters");
        }

        private void GenerateKind(string kind, string kindName)
        {
            Dictionary<string, string> name2slug = new Dictionary<string, string>();
            Directory.CreateDirectory(this.OutputDirectory);

            foreach (var it in inputDocument.Elements("type").Where(c => c.Attribute("kind").Value == kind).Select(c => new { Name = c.Attribute("name").Value, Slug = c.Attribute("slug").Value, Title = c.Attribute("title").Value }))
            {
                GenerateSinglePage(kind, kindName, it.Name, it.Slug, it.Title);
            }
        }

        private void GenerateSinglePage(string kind, string kindName, string name, string slug, string title)
        {
            string filename = Path.Combine(this.OutputDirectory, slug + "." + this.FileSuffix);

            Dictionary<string, XElement> inputXml = new Dictionary<string, XElement>();

            var type = inputDocument.Elements("type").Where(c => c.Attribute("kind").Value == kind && c.Attribute("name").Value == name).Single();

            InsertExamples(type);

            using (var reader = inputDocument.CreateReader())
            {
                using (XmlWriter writer = XmlWriter.Create(filename + ".tmp", new XmlWriterSettings { OmitXmlDeclaration = true }))
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

            PostProcessFile(filename + ".tmp");
            RewriteIfChanged(filename);
        }

        private void RewriteIfChanged(string filename)
        {
            if (File.Exists(filename) && File.ReadAllText(filename + ".tmp") == File.ReadAllText(filename))
            {
                File.Delete(filename + ".tmp");
                Console.WriteLine("{0} is unchanged.", filename);
                return;
            }

            Console.WriteLine("{0} has been updated.", filename);
            File.Delete(filename);
            File.Move(filename + ".tmp", filename);
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

        private void GenerateMergedPage(string kind, string kindName, string slug)
        {
            string filename = Path.Combine(this.OutputDirectory, slug + "." + this.FileSuffix);

            using (XmlWriter writer = XmlWriter.Create(filename + ".tmp", new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                using (var reader = inputDocument.CreateReader())
                {
                    XsltArgumentList arguments = new XsltArgumentList();
                    arguments.AddParam("kind", "", kind);
                    arguments.AddParam("kindName", "", kindName);
                    arguments.AddParam("name", "", "");
                    arguments.AddParam("slug", "", slug);
                    arguments.AddParam("mode", "", this.Mode);
                    transform.Transform(reader, arguments, writer);
                }
            }

            PostProcessFile(filename + ".tmp");
            RewriteIfChanged(filename);
        }

        private void PostProcessFile(string filename)
        {
            string prefix = "<div class=\"generated-doc\" xmlns=\"http://www.w3.org/1999/xhtml\">";
            string suffix = "</div>";
            string content = File.ReadAllText(filename);
            if (content.StartsWith(prefix) && content.EndsWith(suffix))
            {
                content = content.Substring(prefix.Length, content.Length - prefix.Length - suffix.Length);
                File.WriteAllText(filename, content);
            }
        }

        private void LoadInputFile()
        {
            inputDocument = XElement.Load(this.InputFile);
        }
    }
}
