using System;
using System.Collections.Specialized;
using NLogViewer.Parsers;
using System.Windows.Forms;

namespace NLogViewer.Configuration
{
    public interface IWizardPropertyPage<T>
    {
        T TargetObject { get; set; }
    }
}
