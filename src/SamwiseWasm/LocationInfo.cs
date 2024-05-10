using System;
using System.Collections.Generic;

namespace Peevo.Samwise.Wasm
{
    public struct LocationInfo
    {
        public string file;
        public int lineStart;
        public int lineEnd;

        public LocationInfo(string file, int lineStart, int lineEnd) 
        {
            this.file = file;
            this.lineStart = lineStart;
            this.lineEnd = lineEnd;
        }
    }

}