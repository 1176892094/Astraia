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

namespace Astraia
{
    [Serializable]
    public abstract class Feature<TEntity> : Module<TEntity> where TEntity : Entity
    {
        private readonly Dictionary<Enum, Xor.Float> features = new Dictionary<Enum, Xor.Float>();

        public float GetFloat(Enum key, float value = 0)
        {
            features.TryAdd(key, value);
            return features[key];
        }

        public void SetFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = value;
        }

        public void AddFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += value;
        }

        public void SubFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] -= value;
        }

        public int GetInt(Enum key, float value = 0)
        {
            features.TryAdd(key, value);
            return (int)features[key];
        }

        public void SetInt(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = (int)value;
        }

        public void AddInt(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += (int)value;
        }

        public void SubInt(Enum key, float value)
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