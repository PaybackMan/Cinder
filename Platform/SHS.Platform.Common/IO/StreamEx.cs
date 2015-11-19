using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamEx
    {
        /// <summary>
        /// Copies the contents of the current stream from its current position 
        /// into destination at its current position.  If destination supports seeking, 
        /// will move the position of destination back to current position after copying, 
        /// preparing it for reading the newly copied data.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyTo(this Stream source, Stream destination, Func<bool> readPredicate, bool moveBackToCurrent = true)
        {
            long totalRead = 0;
            int read = 0;
            var buffer = new byte[4096];
            do
            {
                read = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, read);
                totalRead += read;
            } while( read > 0 && readPredicate() );

            if (moveBackToCurrent && destination.CanSeek)
            {
                destination.Seek(-totalRead, SeekOrigin.Current);
            }
        }
    }
}
