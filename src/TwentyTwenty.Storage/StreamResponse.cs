using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TwentyTwenty.Storage
{
    public class StreamResponse
    {
        public StreamResponse(Stream stream, long contentLength = 0)
        {
            this.Stream = stream;
            this.ContentLength = contentLength;
        }

        public Stream Stream { get; set; }

        public long ContentLength { get; set; }
    }
}
