// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:29
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Core
{
    using static GlobalManager;

    public static class AudioManager
    {
        public static int MusicVolume
        {
            get => GlobalManager.MusicVolume = JsonManager.Load(nameof(MusicVolume), 100);
            set
            {
                if (Source)
                {
                    Source.volume = value * 0.01F;
                }

                GlobalManager.MusicVolume = value;
                JsonManager.Save(GlobalManager.MusicVolume, nameof(MusicVolume));
            }
        }

        public static int AudioVolume
        {
            get => GlobalManager.AudioVolume = JsonManager.Load(nameof(AudioVolume), 100);
            set
            {
                foreach (var audio in AudioData)
                {
                    audio.volume = value * 0.01F;
                }

                GlobalManager.AudioVolume = value;
                JsonManager.Save(GlobalManager.AudioVolume, nameof(AudioVolume));
            }
        }

        public static AudioState AudioState
        {
            get => GlobalManager.AudioState;
            set
            {
                switch (value)
                {
                    case AudioState.Play:
                        foreach (var audio in AudioData) audio.Play();
                        break;
                    case AudioState.Pause:
                        foreach (var audio in AudioData) audio.Pause();
                        break;
                    case AudioState.Stop:
                        foreach (var audio in AudioData) Stop(audio);
                        break;
                }

                GlobalManager.AudioState = value;
            }
        }

        internal static void Update()
        {
            if (GlobalManager.AudioState == AudioState.Play)
            {
                for (var i = AudioData.Count - 1; i >= 0; i--)
                {
                    if (AudioData[i])
                    {
                        if (!AudioData[i].isPlaying)
                        {
                            Stop(AudioData[i]);
                        }
                    }
                    else
                    {
                        Stop(AudioData[i]);
                    }
                }
            }
        }

        public static void Play(string name)
        {
            if (!Source) return;
            var result = GlobalSetting.AUDIOS.Format(name);
            Source.clip = AssetManager.Load<AudioClip>(result);
            Source.loop = true;
            Source.volume = GlobalManager.MusicVolume * 0.01F;
            Source.Play();
        }

        public static void Pause()
        {
            if (!Source) return;
            Source.Pause();
        }

        public static void Stop()
        {
            if (!Source) return;
            Source.Stop();
        }

        public static AudioSource Load(string name, bool loop = false, float interval = 0)
        {
            var result = GlobalSetting.AUDIOS.Format(name);
            var source = PoolManager.Show(result, interval);
            source.clip = AssetManager.Load<AudioClip>(result);
            source.loop = loop;
            source.volume = GlobalManager.AudioVolume * 0.01F;
            source.Play();
            AudioData.Add(source);
            return source;
        }

        public static void Stop(AudioSource source)
        {
            if (source)
            {
                source.Stop();
                PoolManager.Hide(source);
            }

            AudioData.Remove(source);
        }

        internal static void Dispose()
        {
            AudioData.Clear();
            GlobalManager.AudioState = AudioState.Play;
        }

        private static class PoolManager
        {
            public static AudioSource Show(string path, float interval)
            {
                if (!Instance) return null;
                var item = LoadPool(path).Load(interval);
                item.transform.SetParent(null);
                item.gameObject.SetActive(true);
                return item;
            }

            public static void Hide(AudioSource item)
            {
                if (!Instance || !item) return;
                if (!PoolRoot.TryGetValue(item.name, out var pool))
                {
                    pool = new GameObject("Pool - {0}".Format(item.name)).transform;
                    pool.SetParent(Instance.transform);
                    PoolRoot.Add(item.name, pool);
                }

                item.gameObject.SetActive(false);
                item.transform.SetParent(pool);
                LoadPool(item.name).Push(item);
            }

            private static Pool LoadPool(string path)
            {
                if (!PoolData.TryGetValue(path, out var pool))
                {
                    pool = new Pool(typeof(GameObject), path);
                    PoolData.Add(path, pool);
                }

                return (Pool)pool;
            }

            [Serializable]
            private class Pool : IPool
            {
                private readonly Queue<AudioSource> unused = new Queue<AudioSource>();
                private float waitTime;
                private AudioSource current;
                public Type Type { get; private set; }
                public string Path { get; private set; }
                public int Acquire { get; private set; }
                public int Release { get; private set; }
                public int Dequeue { get; private set; }
                public int Enqueue { get; private set; }

                public Pool(Type type, string path)
                {
                    Type = type;
                    Path = path;
                }

                public AudioSource Load(float interval)
                {
                    if (interval != 0)
                    {
                        if (waitTime > Time.time)
                        {
                            return current;
                        }

                        waitTime = Time.time + interval;
                    }

                    Dequeue++;
                    Acquire++;
                    if (unused.TryDequeue(out var item))
                    {
                        Release--;
                        if (item)
                        {
                            current = item;
                            return item;
                        }

                        Dequeue++;
                        Enqueue++;
                    }

                    item = new GameObject(Path).AddComponent<AudioSource>();
                    current = item;
                    return item;
                }

                public void Push(AudioSource item)
                {
                    Enqueue++;
                    if (!unused.Contains(item))
                    {
                        Acquire--;
                        Release++;
                        unused.Enqueue(item);
                    }
                }

                void IDisposable.Dispose()
                {
                    current = null;
                    unused.Clear();
                }
            }
        }
    }
}