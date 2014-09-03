using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole
{
    public static class CollectionExtensions
    {
        public static Task<T> TakeAsync<T>(this BlockingCollection<T> collection)
        {
            return Task.Run(() => collection.Take());
        }
    }
}
