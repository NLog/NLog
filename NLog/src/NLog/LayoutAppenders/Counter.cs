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
using System.Collections;
using System.Globalization;

namespace NLog.LayoutAppenders
{
    [LayoutAppender("counter")]
    public class CounterLayoutAppender : LayoutAppender
    {
        private int _value = 1;
        private string _sequence = null;
        private int _increment = 1;

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public int Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }

        public string Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }
        
        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 32;
        }
        
        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
        {
            int v;

            if (_sequence != null) {
                v = GetNextSequenceValue(Sequence, Value, Increment);
            } else {
                v = _value;
                _value += _increment;
            }

            builder.Append(ApplyPadding(v.ToString(Culture)));
        }

        private static Hashtable _sequences = new Hashtable();

        private static int GetNextSequenceValue(string sequenceName, int defaultValue, int increment) {
            lock (_sequences) {
                object v = _sequences[sequenceName];
                int val;

                if (v == null) {
                    val = defaultValue;
                } else {
                    val = (int)v;
                }

                int retVal = val;
                
                val += increment;
                _sequences[sequenceName] = val;
                return retVal;
            }
        }
    }
}
