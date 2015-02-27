using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NLog.Internal
{
    /// <summary>
    /// Stream helpers
    /// </summary>
    public static class StreamHelpers
    {

        /// <summary>
        /// Copy stream input to output. Skip the first bytes
        /// </summary>
        /// <param name="input">stream to read from</param>
        /// <param name="output">stream to write to</param>
        /// <param name="offset">first bytes to skip (optional)</param>
        public static void CopyWithOffset(this Stream input, Stream output, int offset)
        {

            if (offset < 0)
            {
                throw new ArgumentException("negative offset");
            }

          
           //skip offset
            input.Seek(offset, SeekOrigin.Current);


            byte[] buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {

                output.Write(buffer, 0, read);
               
            }
        }

      

    }
}
