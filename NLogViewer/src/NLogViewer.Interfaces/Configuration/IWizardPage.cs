using System;
using System.Collections.Specialized;
using NLogViewer.Parsers;
using System.Windows.Forms;

namespace NLogViewer.Configuration
{
    public interface IWizardPage
    {
        string Title { get; set; }
        string Label1 { get; set; }
        string Label2 { get; set; }
        Control Control { get; }
        bool NextButtonIsDefault { get; }
        void ActivatePage();
        bool ValidatePage();
    }
}
