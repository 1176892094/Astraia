// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:31
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using System.Text;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    public abstract class Attribute<TOwner, T> : Agent<TOwner> where TOwner : Component
    {
        private readonly Dictionary<T, Variable<float>> attributes = new Dictionary<T, Variable<float>>();

        internal float Get(T key)
        {
            if (!attributes.ContainsKey(key))
            {
                attributes.Add(key, 0);
            }

            return attributes[key].Value;
        }

        internal void Set(T key, float value)
        {
            if (!attributes.ContainsKey(key))
            {
                attributes.Add(key, 0);
            }

            attributes[key] = value;
        }

        protected override void OnHide()
        {
            attributes.Clear();
        }

        public override string ToString()
        {
            var builder = HeapManager.Dequeue<StringBuilder>();
            foreach (var attribute in attributes)
            {
                builder.AppendFormat("{0} : {1}", attribute.Key, attribute.Value);
            }

            var message = builder.ToString();
            builder.Length = 0;
            HeapManager.Enqueue(builder);
            return message;
        }
    }
}