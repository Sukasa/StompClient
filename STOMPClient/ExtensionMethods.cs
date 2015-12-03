using System;
using System.Collections.Generic;

namespace STOMP
{
    static class ExtensionMethods
    {
        public static void ForEach<T>(this IEnumerable<T> Enum, Action<T> Lambda)
        {
            foreach (T _ in Enum)
                Lambda(_);
        }
    }
}
