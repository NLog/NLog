using System;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

class Program
{
    static string[] files = { 
        "Benchmark.xml",
        "Benchmark-log4net-withformat.xml"
        // "Benchmark-log4net.xml",
        // "Benchmark-log4net-concrete-logger.xml",
    };

    static List<List<double>> timings = new List<List<double>>();

    static void Main(string[] args)
    {
        XmlDocument doc = new XmlDocument();
        for (int i = 0; i < files.Length; ++i)
        {
            doc.Load(files[i]);
            List<double> tt = new List<double>();

            foreach (XmlElement timing in doc.SelectNodes("//timing"))
            {
                double val = Convert.ToDouble(timing.GetAttribute("nanosecondsPerLog"), CultureInfo.InvariantCulture);
                tt.Add(val);
            }
            timings.Add(tt);
        }
        int ordinal = 0;

        foreach (XmlElement el in doc.SelectNodes("//timing"))
        {
            string section = ((XmlElement)el.ParentNode).GetAttribute("logger");

            if (ordinal % 4 == 0)
            {
                Console.WriteLine();
                Console.WriteLine("\"{0}\"\t\"NLog\"\t\"Log4Net\"", section);
            }
            Console.Write("\"{0}\"", el.GetAttribute("name"));
            for (int i = 0; i < files.Length; ++i)
            {
                Console.Write("\t{0}",Math.Round(timings[i][ordinal],3));
            }
            Console.WriteLine();
            ordinal++;
        }
    }
}
