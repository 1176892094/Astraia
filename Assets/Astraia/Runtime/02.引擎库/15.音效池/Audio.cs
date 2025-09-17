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

using UnityEngine;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class AudioManager
    {
        public static float MusicVolume
        {
            get
            {
                musicVolume = JsonManager.Load(nameof(MusicVolume), 1F);
                return musicVolume;
            }
            set
            {
                if (Instance && Instance.source)
                {
                    Instance.source.volume = value;
                }

                musicVolume = value;
                JsonManager.Save(musicVolume, nameof(MusicVolume));
            }
        }

        public static float AudioVolume
        {
            get
            {
                audioVolume = JsonManager.Load(nameof(AudioVolume), 1F);
                return audioVolume;
            }
            set
            {
                foreach (var audio in audioLoop)
                {
                    audio.volume = value;
                }

                audioVolume = value;
                JsonManager.Save(audioVolume, nameof(AudioVolume));
            }
        }

        public static AudioSource PlayMusic(string name)
        {
            if (!Instance) return null;
            var target = GlobalSetting.GetAudioPath(name);
            var source = Instance.source;
            source.clip = AssetManager.Load<AudioClip>(target);
            source.loop = true;
            source.volume = MusicVolume;
            source.Play();
            return source;
        }

        public static void StopMusic(bool pause = true)
        {
            if (!Instance) return;
            if (pause)
            {
                Instance.source.Pause();
            }
            else
            {
                Instance.source.Stop();
            }
        }

        public static AudioSource PlayLoop(string name)
        {
            if (!Instance) return null;
            var target = GlobalSetting.GetAudioPath(name);
            var source = LoadPool(target).Load();
            audioLoop.Add(source);
            source.transform.SetParent(null);
            source.gameObject.SetActive(true);
            source.clip = AssetManager.Load<AudioClip>(target);
            source.loop = true;
            source.volume = AudioVolume;
            source.Play();
            return source;
        }

        public static AudioSource PlayOnce(string name)
        {
            if (!Instance) return null;
            var target = GlobalSetting.GetAudioPath(name);
            var source = LoadPool(target).Load();
            audioLoop.Add(source);
            source.transform.SetParent(null);
            source.gameObject.SetActive(true);
            source.clip = AssetManager.Load<AudioClip>(target);
            source.loop = false;
            source.volume = AudioVolume;
            source.Play();
            source.Wait(source.clip.length).OnUpdate(() => Stop(source));
            return source;
        }

        public static void Stop(AudioSource source)
        {
            if (!Instance) return;
            if (!poolRoot.TryGetValue(source.name, out var pool))
            {
                pool = new GameObject("Pool - {0}".Format(source.name));
                pool.transform.SetParent(Instance.transform);
                poolRoot.Add(source.name, pool);
            }

            source.Stop();
            source.gameObject.SetActive(false);
            source.transform.SetParent(pool.transform);
            audioLoop.Remove(source);
            LoadPool(source.name).Push(source);
        }

        private static Pool LoadPool(string path)
        {
            if (poolData.TryGetValue(path, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(typeof(AudioSource), path);
            poolData.Add(path, pool);
            return (Pool)pool;
        }

        internal static void Dispose()
        {
            audioLoop.Clear();
        }
    }
}