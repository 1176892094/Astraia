using System;
using System.Collections;
using System.Collections.Generic;

namespace Astraia.Core
{
    public class Dictionary<TKey> : IDisposable
    {
        private readonly Dictionary<Type, IDictionary> Items = new Dictionary<Type, IDictionary>();

        public void Set<T>(TKey key, T value)
        {
            GetDict<T>()[key] = value;
        }

        public void Add<T>(TKey key, T value)
        {
            GetDict<T>().Add(key, value);
        }

        public void Remove<T>(TKey key)
        {
            GetDict<T>().Remove(key);
        }

        public bool ContainsKey<T>(TKey key)
        {
            return GetDict<T>().ContainsKey(key);
        }

        public bool TryGetValue<T>(TKey key, out T value)
        {
            return GetDict<T>().TryGetValue(key, out value);
        }

        public ICollection<TKey> GetKeys<T>()
        {
            return GetDict<T>().Keys;
        }

        public ICollection<T> GetValues<T>()
        {
            return GetDict<T>().Values;
        }

        public void Clear<T>()
        {
            GetDict<T>().Clear();
        }

        public void Dispose()
        {
            foreach (var item in Items.Values)
            {
                item.Clear();
            }

            Items.Clear();
        }

        private Dictionary<TKey, T> GetDict<T>()
        {
            if (!Items.TryGetValue(typeof(T), out var items))
            {
                items = new Dictionary<TKey, T>();
                Items.Add(typeof(T), items);
            }

            return (Dictionary<TKey, T>)items;
        }
    }
}