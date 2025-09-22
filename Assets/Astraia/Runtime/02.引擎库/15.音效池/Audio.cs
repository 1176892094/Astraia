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

    public static class AudioManager
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
                for (var i = audioLoop.Count - 1; i >= 0; i--)
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
            if (!Instance)
            {
                return;
            }

            switch (state)
            {
                case AudioState.Play:
                    var result = GlobalSetting.Audio.Format(name);
                    var source = Instance.source;
                    source.clip = AssetManager.Load<AudioClip>(result);
                    source.loop = true;
                    source.volume = MusicVolume;
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
            if (!Instance)
            {
                return null;
            }

            var result = GlobalSetting.Audio.Format(name);
            var source = PoolManager.GetAudio(result);
            source.clip = AssetManager.Load<AudioClip>(result);
            source.loop = mode == AudioMode.Loop;
            source.volume = AudioVolume;
            source.Play();
            audioLoop.Add(source);
            return source;
        }

        public static void Stop(AudioState state)
        {
            if (!Instance)
            {
                return;
            }

            switch (state)
            {
                case AudioState.Play:
                    audioState = state;
                    foreach (var audio in audioLoop)
                    {
                        audio.Play();
                    }

                    break;
                case AudioState.Pause:
                    audioState = state;
                    foreach (var audio in audioLoop)
                    {
                        audio.Pause();
                    }

                    break;
                case AudioState.Stop:
                    foreach (var audio in audioLoop)
                    {
                        Stop(audio);
                    }

                    break;
            }
        }

        public static void Stop(AudioSource source)
        {
            if (!Instance)
            {
                return;
            }

            source.Stop();
            audioLoop.Remove(source);
            PoolManager.Hide(source.gameObject);
        }
    }
}