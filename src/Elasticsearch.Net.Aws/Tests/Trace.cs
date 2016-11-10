using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public static class Trace
    {
#if DOTNETCORE
        public static void WriteLine(string line) => Console.WriteLine(line);
        public static void Write(string line) => Console.Write(line);
#else
        public static void WriteLine(string line) => System.Diagnostics.Trace.WriteLine(line);
        public static void Write(string line) => System.Diagnostics.Trace.Write(line);
#endif
    }
}
