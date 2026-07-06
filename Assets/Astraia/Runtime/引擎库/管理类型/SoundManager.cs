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
    [Serializable]
    public class SoundManager : Singleton<SoundManager>, IEvent<OnAfterUpdate>
    {
        private static List<AudioSource> audioData = new List<AudioSource>();
        private static bool isPlaying;

        [SerializeField] private AudioSource musicMain;
        [SerializeField] private int musicVolume;
        [SerializeField] private int audioVolume;

        public int MusicVolume
        {
            get => musicVolume;
            set
            {
                if (musicMain)
                {
                    musicMain.volume = value * 0.01F;
                }

                musicVolume = value;
                JsonManager.Save(musicVolume, nameof(MusicVolume));
            }
        }

        public int AudioVolume
        {
            get => audioVolume;
            set
            {
                foreach (var source in audioData)
                {
                    source.volume = value * 0.01F;
                }

                audioVolume = value;
                JsonManager.Save(audioVolume, nameof(AudioVolume));
            }
        }

        public override void Dequeue()
        {
            isPlaying = true;
            MusicVolume = JsonManager.Load(nameof(MusicVolume), 100);
            AudioVolume = JsonManager.Load(nameof(AudioVolume), 100);
        }

        public override void Enqueue()
        {
            Instance = null;
            audioData.Clear();
        }

        public void Execute(OnAfterUpdate message)
        {
            if (isPlaying)
            {
                for (var i = audioData.Count - 1; i >= 0; i--)
                {
                    if (audioData[i])
                    {
                        if (!audioData[i].isPlaying)
                        {
                            StopInternal(audioData[i]);
                        }
                    }
                    else
                    {
                        StopInternal(audioData[i]);
                    }
                }
            }
        }

        public static void Play(string name)
        {
            Instance?.PlayInternal(name);
        }

        public static void Pause()
        {
            Instance?.PauseInternal();
        }

        public static void Stop()
        {
            Instance?.StopInternal();
        }

        public static void PlayAll()
        {
            Instance?.PlayAllInternal();
        }

        public static void PauseAll()
        {
            Instance?.PauseAllInternal();
        }

        public static void StopAll()
        {
            Instance?.StopAllInternal();
        }

        public static void Stop(AudioSource source)
        {
            Instance?.StopInternal(source);
        }

        public static AudioSource Load(string name)
        {
            return Instance?.LoadInternal(name);
        }

        public static AudioSource Loop(string name)
        {
            return Instance?.LoopInternal(name);
        }

        public static AudioSource Load(string name, float cooldown)
        {
            return Instance?.LoadInternal(name, cooldown);
        }

        private AudioSource PlayInternal(string name, float cooldown)
        {
            var result = GlobalSetting.AUDIOS.Format(name);
            var source = PoolManager.Show<AudioSource>(result, cooldown);
            source.clip = AssetManager.Load<AudioClip>(result);
            source.volume = AudioVolume * 0.01F;
            source.Play();
            audioData.Add(source);
            return source;
        }

        private void PlayInternal(string name)
        {
            var result = GlobalSetting.AUDIOS.Format(name);
            musicMain.clip = AssetManager.Load<AudioClip>(result);
            musicMain.volume = musicVolume * 0.01F;
            musicMain.Play();
        }

        private void PauseInternal()
        {
            musicMain?.Pause();
        }

        private void StopInternal()
        {
            musicMain?.Stop();
        }

        private void PlayAllInternal()
        {
            foreach (var source in audioData)
            {
                source.Play();
            }

            isPlaying = true;
        }

        private void PauseAllInternal()
        {
            foreach (var source in audioData)
            {
                source.Pause();
            }

            isPlaying = false;
        }

        private void StopAllInternal()
        {
            for (var i = audioData.Count - 1; i >= 0; i--)
            {
                StopInternal(audioData[i]);
            }
        }

        private void StopInternal(AudioSource source)
        {
            if (source)
            {
                source.Stop();
                PoolManager.Hide(source);
            }

            audioData.Remove(source);
        }

        private AudioSource LoadInternal(string name)
        {
            var source = PlayInternal(name, 0);
            source.loop = false;
            return source;
        }

        private AudioSource LoopInternal(string name)
        {
            var source = PlayInternal(name, 0);
            source.loop = true;
            return source;
        }

        private AudioSource LoadInternal(string name, float cooldown)
        {
            var source = PlayInternal(name, cooldown);
            source.loop = false;
            return source;
        }
    }
}