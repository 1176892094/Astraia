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
        private static AudioState audioState;

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

        internal static void Update()
        {
            if (audioState == AudioState.Play)
            {
                for (int i = audioLoop.Count - 1; i >= 0; i--)
                {
                    if (!audioLoop[i].isPlaying)
                    {
                        Stop(audioLoop[i]);
                    }
                }
            }
        }

        public static void Play(string name, AudioState state)
        {
            if (!Instance) return;
            switch (state)
            {
                case AudioState.Play:
                    var result = GlobalSetting.Audio.Format(name);
                    var source = Instance.source;
                    source.clip = AssetManager.Load<AudioClip>(result);
                    source.volume = MusicVolume;
                    source.loop = true;
                    source.Play();
                    break;
                case AudioState.Pause:
                    Instance.source.Pause();
                    break;
                case AudioState.Stop:
                    Instance.source.Stop();
                    break;
            }
        }

        public static AudioSource Play(string name, AudioMode mode = AudioMode.Once)
        {
            if (!Instance) return null;
            var result = GlobalSetting.Audio.Format(name);
            var source = LoadPool(result).Load();
            source.transform.SetParent(null);
            source.gameObject.SetActive(true);
            source.clip = AssetManager.Load<AudioClip>(result);
            source.loop = mode == AudioMode.Loop;
            source.volume = AudioVolume;
            source.Play();
            audioLoop.Add(source);
            return source;
        }

        public static void Stop(AudioState state)
        {
            if (!Instance) return;
            switch (state)
            {
                case AudioState.Play:
                    audioState = state;
                    foreach (var audio in audioLoop) audio.Play();
                    break;
                case AudioState.Pause:
                    audioState = state;
                    foreach (var audio in audioLoop) audio.Pause();
                    break;
                case AudioState.Stop:
                    foreach (var audio in audioLoop) Stop(audio);
                    break;
            }
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
            audioLoop.Remove(source);
            source.gameObject.SetActive(false);
            source.transform.SetParent(pool.transform);
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
    }
}