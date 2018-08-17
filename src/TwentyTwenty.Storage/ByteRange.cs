using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyTwenty.Storage
{
    public class ByteRange
    {
        public ByteRange(long start, long? end)
        {
            this.Start = start;
            this.End = end;
        }

        public long Start { get; set; }
        public long? End { get; set; }


    }
}
