using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCommand
{
    public interface IDataCache
    {
        object Get(string key);

        void Set(string key, object value, DateTimeOffset absoluteExpiration);

        void Set(string key, object value, TimeSpan slidingExpiration);

        void Remove(string key);
    }
}
