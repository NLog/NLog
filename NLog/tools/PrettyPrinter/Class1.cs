using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace CSharpPrettyPrint
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Class1
    {
        static string[] csharpKeywords = {
                                             "class",
                                             "abstract",
                                             "event",
                                             "new",
                                             "struct",
                                             "as",
                                             "explicit",
                                             "null",
                                             "switch",
                                             "base",
                                             "extern",
                                             "object",
                                             "this",
                                             "bool",
                                             "false",
                                             "operator",
                                             "throw",
                                             "break",
                                             "finally",
                                             "out",
                                             "true",
                                             "byte",
                                             "fixed",
                                             "override",
                                             "try",
                                             "case",
                                             "float",
                                             "params",
                                             "typeof",
                                             "catch",
                                             "for",
                                             "private",
                                             "uint",
                                             "char",
                                             "foreach",
                                             "protected",
                                             "ulong",
                                             "checked",
                                             "goto",
                                             "public",
                                             "unchecked",
                                             "if",
                                             "readonly",
                                             "unsafe",
                                             "const",
                                             "implicit",
                                             "ref",
                                             "ushort",
                                             "continue",
                                             "in",
                                             "return",
                                             "using",
                                             "decimal",
                                             "int",
                                             "sbyte",
                                             "virtual",
                                             "default",
                                             "interface",
                                             "sealed",
                                             "volatile",
                                             "delegate",
                                             "internal",
                                             "short",
                                             "void",
                                             "do",
                                             "is",
                                             "sizeof",
                                             "while",
                                             "double",
                                             "lock",
                                             "stackalloc",
                                             "else",
                                             "long",
                                             "static",
                                             "enum",
                                             "namespace",
                                             "string",
        };

        static string[] jscriptKeywords = {
                                              "class", "break", "delete", "function", "return", "typeof",
                                              "case", "do", "if", "switch", "var",
                                              "catch", "else", "in", "this", "void",
                                              "continue", "false", "instanceof", "throw", "while",
                                              "debugger", "finally", "new", "true", "with",
                                              "default", "for", "null", "try", 

                                              "abstract", "double", "goto", "native", "static",
                                              "boolean", "enum", "implements", "package", "super",
                                              "byte", "export", "import", "private", "synchronized",
                                              "char", "extends", "int", "protected", "throws",
                                              "final", "interface", "public", "transient",
                                              "const", "float", "long", "short", "volatile", 
                                          };

        static string MarkKeyword(string language, string l, string k)
        {
            Regex regex = new Regex("\\b"+k+"\\b");
            return regex.Replace(l, "<span class='k'>" + k + "</span>");
        }

        static string PrettyPrintSegment(string language, string line, int p0, int p1, char mode)
        {
            string l0 = line.Substring(p0, p1 - p0);
            l0 = l0.Replace("&", "&amp;");
            l0 = l0.Replace("<", "&lt;");
            l0 = l0.Replace(">", "&gt;");
            // l0 = l0.Replace(" ", "&#160;");
            l0 = l0.Replace("'", "&apos;");
            l0 = l0.Replace("\"", "&quot;");

            switch (mode)
            {
                case 'S':
                    return "<span class='s'>" + l0 + "</span>";
                case 'C': /* comment
                           * 
                           *  */
                    return "<span class='c'>" + l0 + "</span>";
                case 'R':
                    return "<span class='r'>" + l0 + "</span>";
            }

            int p = l0.IndexOf("//");
            string l1 = "";
            if (p >= 0)
            {
                l1 = l0.Substring(p);
                l0 = l0.Substring(0, p);

                l1 = "<span class='c'>" + l1 + "</span>";
            }

            switch (language)
            {
                case "csharp":
                    foreach (string kwd in csharpKeywords)
                    {
                        l0 = MarkKeyword(language, l0, kwd);
                    }
                    break;

                case "jscript":
                    foreach (string kwd in jscriptKeywords)
                    {
                        l0 = MarkKeyword(language, l0, kwd);
                    }
                    break;
            }

            l0 += l1;

            return l0;
        }

        static void WriteAttributes(TextWriter output, XmlTextReader xtr)
        {
            for (int i = 0; i < xtr.AttributeCount; i++)
            {
                xtr.MoveToAttribute(i);
                if (xtr.Prefix == "xml")
                    continue;
                output.Write(" <span class='a'>{0}</span>=<span class='at'>\"{1}\"</span>", xtr.Name, xtr.Value);
            }
            xtr.MoveToElement();
        }

        static void PrettyPrintXml(TextWriter output, TextReader input)
        {
            XmlTextReader xtr = new XmlTextReader(input);
            xtr.WhitespaceHandling = WhitespaceHandling.All;
            while (xtr.Read())
            {
                switch (xtr.NodeType)
                {
                    case XmlNodeType.Element:
                        output.Write("<span class='b'>&lt;</span>");
                        output.Write("<span class='e'>");
                        output.Write(xtr.Name);
                        output.Write("</span>");
                        WriteAttributes(output, xtr);
                        output.Write("<span class='b'>");
                        if (xtr.IsEmptyElement)
                            output.Write("/");
                        output.Write("&gt;</span>");
                        break;

                    case XmlNodeType.EndElement:
                        output.Write("<span class='b'>&lt;/</span>");
                        output.Write("<span class='e'>");
                        output.Write(xtr.Name);
                        output.Write("</span>");
                        output.Write("<span class='b'>");
                        output.Write("&gt;</span>");
                        break;

                    case XmlNodeType.Comment:
                        output.Write("<span class='c'>&lt;!--");
                        output.Write(xtr.Value);
                        output.Write("--&gt;</span>");
                        break;

                    case XmlNodeType.XmlDeclaration:
                        output.Write("<span class='b'>&lt;?</span>");
                        output.Write("<span class='x'>xml&#160;{0}</span>", xtr.Value);
                        output.Write("&#160;<span class='b'>?&gt;</span>");
                        break;

                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Whitespace:
                        output.Write(xtr.Value);
                        break;

                    default:
                        throw new Exception("Unsupported XML node type: " + xtr.NodeType);
                }
            }
        }

        static void PrettyPrint(string language, TextWriter output, TextReader input)
        {
            string line;
            char mode = 'P';

            while ((line = input.ReadLine()) != null)
            {
                int p;
                int p0 = 0;

                string line2 = "";

                for (int i = 0; i < line.Length; ++i)
                {
                    char c = line[i];
                    char c2 = (char)0;
                    if (i + 1 < line.Length)
                        c2 = line[i + 1];

                    switch (mode)
                    {
                        case 'C': // comment
                            if (c == '*' && c2 == '/')
                            {
                                i += 2;
                                line2 += PrettyPrintSegment(language, line, p0, i, mode);
                                p0 = i;
                                mode = 'P';
                            }
                            break;

                        case 'S': // string
                            if (c == '\\')
                            {
                                i++;
                                break;
                            }
                            if (c == '"')
                            {
                                i++;
                                line2 += PrettyPrintSegment(language, line, p0, i, mode);
                                p0 = i;
                                mode = 'P';
                            }
                            break;

                        case 'R': // char
                            if (c == '\\')
                            {
                                i++;
                                break;
                            }
                            if (c == '\'')
                            {
                                i++;
                                line2 += PrettyPrintSegment(language, line, p0, i, mode);
                                p0 = i;
                                mode = 'P';
                            }
                            break;

                        case 'P': // plain
                            if (c == '"')
                            {
                                line2 += PrettyPrintSegment(language, line, p0, i, mode);
                                p0 = i;
                                mode = 'S';
                            } 
                            else if (c == '\'')
                            {
                                line2 += PrettyPrintSegment(language, line, p0, i, mode);
                                p0 = i;
                                mode = 'R';
                            } 
                            else if (c == '/' && c2 == '*')
                            {
                                line2 += PrettyPrintSegment(language, line, p0, i, mode);
                                p0 = i;
                                mode = 'C';
                            }
                            break;
                    }
                }

                line2 += PrettyPrintSegment(language, line, p0, line.Length, mode);

                line = line2;
                
                output.WriteLine(line);
            }
        }
    
        static void Usage(string error)
        {
            Console.WriteLine(error);
            Console.WriteLine("Usage: PrettyPrinter.exe -l csharp|cpp|jscript|vb|xml -i infile -o outfile -m xml|html -s stylesheet.css");
        }

        [STAThread]
        static int Main(string[] args)
        {
            string language = "csharp";
            string stylesheet = "style.css";
            string mode = "html";
            string inputFile = null;
            string outputFile = null;

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-l" && i + 1 < args.Length)
                {
                    language = args[++i];
                    continue;
                }
                if (args[i] == "-i" && i + 1 < args.Length)
                {
                    inputFile = args[++i];
                    continue;
                }

                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputFile = args[++i];
                    continue;
                }

                if (args[i] == "-s" && i + 1 < args.Length)
                {
                    stylesheet = args[++i];
                    continue;
                }

                if (args[i] == "-m" && i + 1 < args.Length)
                {
                    mode = args[++i];
                    continue;
                }

                Usage("Unknown parameter: " + args[i]);
                return 1;
            }

            if (inputFile == null)
            {
                Usage("No input file given.");
                return 1;
            }

            if (outputFile == null)
            {
                Usage("No output file given.");
                return 1;
            }

            using (StreamReader infile = File.OpenText(inputFile))
            {
                using (StreamWriter outfile = File.CreateText(outputFile))
                {
                    if (mode == "html")
                    {
                        outfile.WriteLine("<html>");
                        outfile.WriteLine("<head>");
                        outfile.Write("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />", stylesheet);
                        outfile.WriteLine("</head>");
                        outfile.WriteLine("<body class=\"example\">");
                    }
                    outfile.Write("<pre xml:space='preserve' class='{0}'>", language);
                    if (language == "xml")
                        PrettyPrintXml(outfile, infile);
                    else
                        PrettyPrint(language, outfile, infile);
                    outfile.Write("</pre>");
                    if (mode == "html")
                    {
                        outfile.WriteLine("</body>");
                        outfile.WriteLine("</html>");
                    }
                }
            }
            return 0;
        }
    }
}
