namespace MergeApiXml
{
    using System;
    using System.IO;
    using System.Linq;

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string releaseDir = args[0];
                string outputDir = Path.Combine(releaseDir, "Website");
                Directory.CreateDirectory(outputDir);
                string outputFile = Path.Combine(releaseDir, "NLogMerged.api.xml");

                var merger = new NLogApiMerger();
                merger.AddRelease("1.0", @"\\MASTER\lab\NLog\1.0\Release");
                merger.AddRelease("2.0", releaseDir);
                merger.Merge();

                // remove properties which have been removed in NLog 2.0
                foreach (var prop in merger.Result.Root.Descendants("property")
                    .Where(c => c.Element("supported-in").Elements("release").All(e => (string)e.Attribute("name") != "2.0"))
                    .ToList())
                {
                    prop.Remove();
                };

                Console.WriteLine("Saving {0}...", outputFile);
                merger.Result.Save(outputFile);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }
    }
}
