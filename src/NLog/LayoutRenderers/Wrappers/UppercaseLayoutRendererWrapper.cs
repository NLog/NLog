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

namespace NLog.LayoutRenderers.Wrappers
{
    using System.ComponentModel;
    using System.Globalization;
    using NLog.Config;
    using NLog.Internal.Pooling.Pools;
    using System.Text;
    


    /// <summary>
    /// Converts the result of another layout output to upper case.
    /// </summary>
    /// <example>
    /// ${uppercase:${level}} //[DefaultParameter]
    /// ${uppercase:Inner=${level}} 
    /// ${level:uppercase} // [AmbientProperty]
    /// </example>
    [LayoutRenderer("uppercase")]
    [AmbientProperty("Uppercase")]
    [ThreadAgnostic]
    public sealed class UppercaseLayoutRendererWrapper : WrapperLayoutRendererBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="UppercaseLayoutRendererWrapper" /> class.
        /// </summary>
        public UppercaseLayoutRendererWrapper()
        {
            this.Culture = CultureInfo.InvariantCulture;
            this.Uppercase = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether upper case conversion should be applied.
        /// </summary>
        /// <value>A value of <c>true</c> if upper case conversion should be applied otherwise, <c>false</c>.</value>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(true)]
        public bool Uppercase { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Transformation Options' order='10' />
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Padded and trimmed string.</returns>
        protected override string Transform(string text)
        {
            if (!this.Uppercase)
            {
                return text;
            }

            return text.ToUpper(this.Culture);
        }

#if NET4_5
        private void Transform(StringBuilder builder)
        {
            int length = builder.Length;
            var resultArray = this.LoggingConfiguration.PoolFactory.Get<CharArrayPool, char[]>().Get(length);
          
            builder.CopyTo(0, resultArray, 0, length);
            for (int x = 0; x < length; x++)
            {
                char c = resultArray[x];
                resultArray[x] = char.ToUpper(c, this.Culture);
            }
            builder.Length = 0;
            builder.Append(resultArray, 0, length);
            
            this.LoggingConfiguration.PutBack(resultArray);
        }


        /// <summary>
        /// Renders the inner layout to the given string builder.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="logEvent">The log event to render.</param>
        protected override void RenderInner(StringBuilder builder, LogEventInfo logEvent)
        {
            StringBuilder innerBuilder = this.Inner.RenderBuilder(logEvent);
            if (this.LoggingConfiguration.PoolingEnabled())
            {
                this.Transform(innerBuilder);

                var resultArray = this.LoggingConfiguration.PoolFactory.Get<CharArrayPool, char[]>().Get(innerBuilder.Length);

                innerBuilder.CopyTo(0, resultArray, 0, innerBuilder.Length);
                builder.Append(resultArray, 0, innerBuilder.Length);

                this.LoggingConfiguration.PutBack(resultArray);
                this.LoggingConfiguration.PutBack(innerBuilder);
            }
            else
            {
                if (this.Uppercase)
                {
                    string result = this.Transform(innerBuilder.ToString());
                    builder.Append(result);
                }
                else
                {
                    builder.Append(innerBuilder);
                }
            }
        }
#endif
    }
}
