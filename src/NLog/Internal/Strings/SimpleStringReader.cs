// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System;

    /// <summary>
    /// Simple character tokenizer.
    /// </summary>

#if DEBUG
    [System.Diagnostics.DebuggerDisplay("{" + nameof(CurrentState) + "}")]
#endif
    internal class SimpleStringReader
    {
        private readonly string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleStringReader" /> class.
        /// </summary>
        /// <param name="text">The text to be tokenized.</param>
        public SimpleStringReader(string text)
        {
            _text = text;
            Position = 0;
        }

        /// <summary>
        /// Current position in <see cref="Text"/>
        /// </summary>
        internal int Position { get; set; }

        /// <summary>
        /// Full text to be parsed
        /// </summary>
        internal string Text => _text;

#if DEBUG
        internal static string BuildCurrentState(string done, char current, string todo)
            => $"done: '{done}'.   current: '{current}'.   todo: '{todo}'";
        internal string CurrentState
        {
            get
            {
                var current = (char)Peek();
                if (Position < 0 || Position > Text.Length)
                {
                    return BuildCurrentState(done: "INVALID_CURRENT_STATE", current: char.MaxValue, todo: "INVALID_CURRENT_STATE");
                }
                var done = Substring(0, Position);
                var todo = ((Position < _text.Length - 1) ? Text.Substring(Position + 1) : "");
                return BuildCurrentState(done: done, current: current, todo: todo);
            }
        }
#endif

        /// <summary>
        /// Check current char while not changing the position.
        /// </summary>
        /// <returns></returns>
        internal int Peek()
        {
            if (Position < _text.Length)
            {
                return _text[Position];
            }

            return -1;
        }

        /// <summary>
        /// Read the current char and change position
        /// </summary>
        /// <returns></returns>
        internal int Read()
        {
            if (Position < _text.Length)
            {
                return _text[Position++];
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
            return _text.Substring(startIndex, endIndex - startIndex);
        }

        internal string ReadUntilMatch(Func<int, bool> charFinder)
        {
            int ch;
            int startPosition = Position;
            while ((ch = Peek()) != -1)
            {
                if (charFinder(ch))
                {
                    return Substring(startPosition, Position);
                }

                Read();
            }

            return Substring(startPosition, Position);
        }
    }
}