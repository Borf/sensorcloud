using System;
using System.Collections.Generic;
using System.Text;

namespace NNTP
{
    public class Header
    {
        public long articleNumber;
        public string articleId;
        public Dictionary<string, string> headers;
    }
}
