using System;
using System.Collections.Generic;
using System.Reflection;
using NLog.Internal.Fakeables;

namespace NLog.UnitTests
{
    public class AppDomainMock : IAppDomain
    {
        public AppDomainMock(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
        }


        public string BaseDirectory { get; set; }
        public string ConfigurationFile { get; set; }
        public IEnumerable<string> PrivateBinPath { get; set; }
        public string FriendlyName { get; set; }
        public int Id { get; set; }
        public IEnumerable<Assembly> GetAssemblies()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> ProcessExit;
        public event EventHandler<EventArgs> DomainUnload;
    }
}