// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
using System.Text;

namespace NLog.LayoutAppenders
{
    [LayoutAppender("ndc")]
    public class NDCLayoutAppender : LayoutAppender
    {
        private int _topFrames = -1;
        private int _bottomFrames = -1;
        private string _separator = " ";
        
        public int TopFrames
        {
            get { return _topFrames; }
            set { _topFrames = value; }
        }

        public int BottomFrames
        {
            get { return _bottomFrames; }
            set { _bottomFrames = value; }
        }

        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        public override int GetEstimatedBufferSize(LogEventInfo ev) {
            return 0;
        }

        public override void Append(StringBuilder builder, LogEventInfo ev)
        {
            string msg;

            if (TopFrames != -1) {
                msg = NDC.GetTopMessages(TopFrames, Separator);
            } else if (BottomFrames != -1) {
                msg = NDC.GetBottomMessages(BottomFrames, Separator);
            } else {
                msg = NDC.GetAllMessages(Separator);
            }
            builder.Append(ApplyPadding(msg));
        }
    }
}
