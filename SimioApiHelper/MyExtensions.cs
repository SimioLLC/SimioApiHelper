using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimioApiHelper
{
    public static class MyExtensions
    {
        public static KeyValuePair<TKey, TValue> GetEntry<TKey, TValue>
    (this IDictionary<TKey, TValue> dictionary,
     TKey key)
        {
            return new KeyValuePair<TKey, TValue>(key, dictionary[key]);
        }
    }
}
