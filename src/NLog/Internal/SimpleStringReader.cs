// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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



namespace NLog.Internal
{
    /// <summary>
    /// Simple character tokenizer.
    /// </summary>

#if DEBUG
     [System.Diagnostics.DebuggerDisplay("{CurrentState}")]
#endif
	internal class SimpleStringReader
	{
        private readonly string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleStringReader" /> class.
        /// </summary>
        /// <param name="text">The text to be tokenized.</param>
        public SimpleStringReader(string text)
        {
            this.text = text;
            this.Position = 0;
        }

        /// <summary>
        /// Current position in <see cref="Text"/>
        /// </summary>
        internal int Position { get; set; }

        /// <summary>
        /// Full text to be parsed
        /// </summary>
        internal string Text
        {
            get { return this.text; }
        }

#if DEBUG
        string CurrentState
        {
            get
            {
                var current = (char)Peek();
                var done = Substring(0, Position - 1);
                var todo = ((Position > text.Length) ? Text.Substring(Position + 1) : "");
                return string.Format("done: '{0}'.   current: '{1}'.   todo: '{2}'", done, current, todo);
            }
        }
#endif

        /// <summary>
        /// Check current char while not changing the position.
        /// </summary>
        /// <returns></returns>
        internal int Peek()
        {
            if (this.Position < this.text.Length)
            {
                return this.text[this.Position];
            }

            return -1;
        }

        /// <summary>
        /// Read the current char and change position
        /// </summary>
        /// <returns></returns>
        internal int Read()
        {
            if (this.Position < this.text.Length)
            {
                return this.text[this.Position++];
            }

            return -1;
        }

        /// <summary>
        /// Get the substring of the <see cref="Text"/>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        internal string Substring(int startIndex, int endIndex)
        {
            return this.text.Substring(startIndex, endIndex - startIndex);
        }
    }
}