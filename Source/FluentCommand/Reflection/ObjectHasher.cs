using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Novus.Common.Extensions;

namespace Novus.Common.Reflection
{
    /// <summary>
    /// A class to support hashing an object by combining the hash code of its properties.
    /// </summary>
    public class ObjectHasher
    {
        private readonly HashSet<int> _objectReferences;

        private const int _seed = 113;
        private const int _step = 397;
        private int _hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectHasher"/> class.
        /// </summary>
        public ObjectHasher()
        {
            _objectReferences = new HashSet<int>();
            _hash = _seed;
        }

        /// <summary>
        /// Returns a hash code for the specified <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The source object to get the hash code from.</param>
        /// <returns>
        /// A hash code for the specified <paramref name="instance"/>. 
        /// </returns>
        public int GetHashCode(object instance)
        {
            // reset
            _hash = _seed;
            _objectReferences.Clear();

            HashValue(instance);
            return _hash;
        }

        private void HashValue(object value)
        {
            if (value == null)
            {
                CombineHash(0);
                return;
            }

            Type type = value.GetType().GetUnderlyingType();

            if (type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Decimal)
                || type == typeof(Guid)
                || type == typeof(Uri))
            {
                CombineHash(value);
                return;
            }

            if (value is IEnumerable)
                HashCollection(value);
            else
                HashObject(value);
        }
        
        private void HashObject(object source)
        {
            if (source == null)
                return;

            // check if this object has already been hashed 
            // using RuntimeHelpers.GetHashCode to get object identity
            int hashCode = RuntimeHelpers.GetHashCode(source);
            if (!_objectReferences.Add(hashCode))
                return;

            var sourceType = source.GetType();
            var sourceAccessor = TypeAccessor.GetAccessor(sourceType);

            // first try to used overriding GetHashCode
            var method = sourceAccessor.FindMethod("GetHashCode", Type.EmptyTypes, BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (method != null && method.MethodInfo.IsOverriding())
            {
                CombineHash(source);
                return;
            }

            // next use property values
            var properties = sourceAccessor.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (IMemberAccessor property in properties)
            {
                if (!property.HasGetter)
                    continue;

                var value = property.GetValue(source);
                HashValue(value);
            }
        }

        private void HashCollection(object value)
        {
            if (value == null)
                return;

            var list = value as IEnumerable;
            if (list == null)
                return;

            // check if this object has already been hashed 
            // using RuntimeHelpers.GetHashCode to get object identity
            int hashCode = RuntimeHelpers.GetHashCode(value);
            if (!_objectReferences.Add(hashCode))
                return;

            foreach (var item in list)
                HashValue(item);
        }

        private void CombineHash(object value)
        {
            _hash = CombineHashCode(_hash, value.GetHashCode());
        }

        private static int CombineHashCode(int start, params int[] values)
        {
            unchecked
            {
                return values.Aggregate(start, (c, v) => (c * _step) ^ v);
            }
        }
    }
}
