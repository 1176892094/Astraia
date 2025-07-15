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
    public abstract class Feature<T> : Source
    {
        private readonly Dictionary<T, Variable<float>> features = new Dictionary<T, Variable<float>>();

        private float Get(T key)
        {
            features.TryAdd(key, 0);
            return features[key].Value;
        }

        private void Set(T key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = value;
        }

        public float GetFloat(T key)
        {
            return Get(key);
        }

        public void SetFloat(T key, float value)
        {
            Set(key, value);
        }

        public void AddFloat(T key, float value)
        {
            SetFloat(key, GetFloat(key) + value);
        }

        public void SubFloat(T key, float value)
        {
            SetFloat(key, GetFloat(key) - value);
        }

        public int GetInt(T key)
        {
            return (int)Get(key);
        }

        public void SetInt(T key, float value)
        {
            Set(key, (int)value);
        }

        public void AddInt(T key, float value)
        {
            SetInt(key, GetInt(key) + (int)value);
        }

        public void SubInt(T key, float value)
        {
            SetInt(key, GetInt(key) - (int)value);
        }

        public override void OnFade()
        {
            features.Clear();
        }
    }
}