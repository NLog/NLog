// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;
using NLog.Conditions;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that filters log entries based on a condition.
    /// </summary>
    [Target("FilteringWrapper",IgnoresLayout=true,IsWrapper=true)]
    public class FilteringTargetWrapper: WrapperTargetBase
    {
        private ConditionExpression _condition;

        /// <summary>
        /// Creates a new instance of <see cref="FilteringTargetWrapper"/>.
        /// </summary>
        public FilteringTargetWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="FilteringWrapper"/> and 
        /// initializes the <see cref="WrapperTargetBase.WrappedTarget"/> and
        /// <see cref="Condition"/> properties.
        /// </summary>
        public FilteringTargetWrapper(Target writeTo, string condition)
        {
            WrappedTarget = writeTo;
            Condition = condition;
        }

        [RequiredParameter]
        [AcceptsCondition]
        public string Condition
        {
            get { return _condition.ToString(); }
            set { _condition = ConditionParser.ParseExpression(value); }
        }

        protected internal override void Write(LogEventInfo logEvent)
        {
            object v = _condition.Evaluate(logEvent);
            if (v != null && v is bool && (bool)v)
                WrappedTarget.Write(logEvent);
        }

        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts(layouts);
            _condition.PopulateLayouts(layouts);
        }

    }
}
