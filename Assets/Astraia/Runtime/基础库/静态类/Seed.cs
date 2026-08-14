// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 19:08:48
// # Recently: 2026-08-14 19:21:48
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    public static class Seed
    {
        [ThreadStatic]
        private static Random random;

        private static Random Random => random ??= new Random(Environment.TickCount);

        public static int sign => Random.Next(2) == 0 ? 1 : -1;

        public static float value => (float)Random.NextDouble();

        public static int Next()
        {
            return Random.Next();
        }

        public static int Next(int max)
        {
            return Random.Next(max);
        }

        public static int Next(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static int NextSign(int min, int max)
        {
            return Random.Next(min, max) * sign;
        }

        public static Fixation Next(Fixation max)
        {
            return new Fixation(Random.Next(max.value));
        }

        public static Fixation Next(Fixation min, Fixation max)
        {
            return new Fixation(Random.Next(min.value, max.value));
        }

        public static Fixation NextSign(Fixation min, Fixation max)
        {
            return new Fixation(Random.Next(min.value, max.value)) * sign;
        }

        public static void NextBytes(byte[] bytes)
        {
            Random.NextBytes(bytes);
        }

        public static T Next<T>() where T : unmanaged, Enum
        {
            return Enum<T>.Values[Random.Next(Enum<T>.Values.Length)];
        }

        public static T Next<T>(T maxValue) where T : unmanaged, Enum
        {
            return Enum<T>.Values[Random.Next(maxValue.Index() + 1)];
        }

        public static T Next<T>(T minValue, T maxValue) where T : unmanaged, Enum
        {
            return Enum<T>.Values[Random.Next(minValue.Index(), maxValue.Index() + 1)];
        }

        public static T[] Array<T>() where T : unmanaged, Enum
        {
            return Enum<T>.Values;
        }

        public static int Count<T>() where T : unmanaged, Enum
        {
            return Enum<T>.Values.Length;
        }

        public static int Index<T>(this T value) where T : unmanaged, Enum
        {
            return Enum<T>.Indices[value];
        }

        private static class Enum<T> where T : unmanaged, Enum
        {
            public static readonly Dictionary<T, int> Indices;
            public static readonly T[] Values;

            static Enum()
            {
                Values = (T[])Enum.GetValues(typeof(T));
                Indices = new Dictionary<T, int>(Values.Length);
                for (var i = 0; i < Values.Length; i++)
                {
                    Indices[Values[i]] = i;
                }
            }
        }
    }
}