using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.Config
{
    public interface IHasInternalLog
    {
        IInternalLog InternalLog { get; set; }
    }
}
