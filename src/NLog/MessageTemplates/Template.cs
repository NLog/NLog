// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.MessageTemplates
{
    /// <summary>
    /// A mesage template
    /// </summary>
    internal class Template
    {
        /// <summary>The original template string.</summary>
        /// <remarks>This is the key passed to structured targets.</remarks>     
        public string Value { get; }

        /// <summary>The list of literal parts, useful for string rendering.
        /// It indicates the number of characters from the original string to print,
        /// then there's a hole with how many chars to skip.</summary>
        /// <example>
        /// "Hello {firstName} {lastName}!"
        /// -------------------------------------
        /// ║P     |S          ║P|S         ║P|S║
        /// ║6     |11         ║1|10        ║1|0║
        /// ║Hello |{firstName}║ |{lastName}║!║
        /// 
        /// "{x} * 2 = {2x}"
        /// --------------------
        /// ║P|S  ║P      |S   ║
        /// ║0|3  ║7      |4   ║
        ///   ║{x}║ * 2 = |{2x}║
        /// 
        /// The tricky part is escaped braces. They are represented by a skip = 0,
        /// which is interpreted as "move one char forward, no hole".
        /// 
        /// "Escaped }} is fun."
        /// ----------------------
        /// ║P        |S║P       |S║
        /// ║9        |0║8       |0║
        /// ║Escaped }|}║ is fun.|║
        /// </example>
        public Literal[] Literals { get; }

        /// <summary> This list of holes. It's used both to fill the string rendering
        /// and to send values along the template to structured targets.</summary>
        public Hole[] Holes { get; }

        /// <summary>Indicates whether the template should be interpreted as positional 
        /// (all holes are numbers) or named.</summary>
        public bool IsPositional { get; }

        /// <summary>
        /// Create a template, which is already parsed
        /// </summary>
        /// <param name="template"></param>
        /// <param name="isPositional"></param>
        /// <param name="literals"></param>
        /// <param name="holes"></param>
        public Template(string template, bool isPositional, List<Literal> literals, List<Hole> holes)
        {
            Value = template;
            IsPositional = isPositional;
            // Using arrays is important! It's the only CLR type that will give us a no-copy access to 
            // the structs contained in the array.
            Literals = literals.ToArray();
            Holes = holes.ToArray();
        }

        /// <summary>
        /// Create a template, which is already parsed
        /// </summary>
        /// <param name="template"></param>
        /// <param name="isPositional"></param>
        /// <param name="literals"></param>
        /// <param name="holes"></param>
        public Template(string template, bool isPositional, Literal[] literals, Hole[] holes)
        {
            Value = template;
            IsPositional = isPositional;
            Literals = literals;
            Holes = holes;
        }

        /// <summary>This is for testing only: recreates <see cref="Value"/> from the parsed data.</summary>
        public string Rebuild()
        {
            var sb = new StringBuilder(Value.Length);
            int pos = 0;
            int h = 0;
            foreach (var literal in Literals)
            {
                sb.Append(Value, pos, literal.Print);
                pos += literal.Print;
                if (literal.Skip == 0)
                {
                    // 0 means escaping or end of string without hole.
                    if (pos < Value.Length)
                        sb.Append(Value[pos++]);
                }
                else
                {
                    pos += literal.Skip;
                    RebuildHole(sb, ref Holes[h++]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// This is for testing only: rebuilds the hole
        /// </summary>
        /// <param name="sb">Add to this string builder</param>
        /// <param name="hole">ref for performance</param>
        private static void RebuildHole(StringBuilder sb, ref Hole hole)
        {
            if (hole.CaptureType == CaptureType.Normal)
                sb.Append('{');
            else if (hole.CaptureType == CaptureType.Serialize)
                sb.Append("{@");
            else  // hole.CaptureType == CaptureType.Stringification
                sb.Append("{$");

            sb.Append(hole.Name);

            if (hole.Alignment != 0)
                sb.Append(',').Append(hole.Alignment);

            if (hole.Format != null)
                sb.Append(':').Append(hole.Format.Replace("{", "{{").Replace("}", "}}")); // rebuild of the escaped brackets in format

            sb.Append('}');
        }
    }
}