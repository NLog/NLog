using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace NLog.Internal
{
    internal static class EncodingHelpers
    {
        /// <summary>
        /// Fix encoding so it has/hasn't a BOM (Byte-order-mark)
        /// </summary>
        /// <param name="encoding">encoding to be converted</param>
        /// <param name="includeBOM">should we include the BOM (Byte-order-mark) for UTF? <c>true</c> for BOM, <c>false</c> for no BOM</param>
        /// <returns>new or current encoding</returns>
        /// <remarks>.net has default a BOM included with UTF-8</remarks>
        public static Encoding ConvertEncodingBOM([NotNull] this Encoding encoding, bool includeBOM)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");
            if (!includeBOM)
            {
                //default .net uses BOM, so we have to create a new instance to exlucde this.
                if (encoding.EncodingName == Encoding.UTF8.EncodingName)
                {
                    return new UTF8Encoding(false);

                }
            }

            return encoding;
        }
    }
}
