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

using System;
using System.Collections.Generic;
using Astraia.Common;

namespace Astraia
{
    [Serializable]
    public abstract class Feature<TEntity> : Agent<TEntity> where TEntity : Entity
    {
        private readonly Dictionary<int, Xor.Float> features = new Dictionary<int, Xor.Float>();

        public float GetFloat(int key)
        {
            features.TryAdd(key, 0);
            return features[key];
        }

        public void SetFloat(int key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = value;
        }

        public void AddFloat(int key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += value;
        }

        public void SubFloat(int key, float value)
        {
            features.TryAdd(key, 0);
            features[key] -= value;
        }

        public int GetInt(int key)
        {
            features.TryAdd(key, 0);
            return (int)features[key];
        }

        public void SetInt(int key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = (int)value;
        }

        public void AddInt(int key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += (int)value;
        }

        public void SubInt(int key, float value)
        {
            features.TryAdd(key, 0);
            features[key] -= (int)value;
        }

        public override void Enqueue()
        {
            features.Clear();
        }
    }
}