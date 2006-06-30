using System;
using System.Collections.Specialized;
using NLogViewer.Parsers;

namespace NLogViewer.Configuration
{
    public interface IWizardConfigurable
    {
        IWizardPage GetWizardPage();
    }
}
