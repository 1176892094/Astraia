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
using UnityEngine;

namespace Astraia.Common
{
    public static partial class AudioManager
    {
        public static float musicValue
        {
            get
            {
                if (GlobalManager.settings == null)
                {
                    GlobalManager.settings = new AudioSetting();
                    JsonManager.Load(GlobalManager.settings, nameof(AudioManager));
                }

                return GlobalManager.settings.musicValue;
            }
            set
            {
                GlobalManager.settings.musicValue = value;
                GlobalManager.Instance.sounds.volume = value;
                JsonManager.Save(GlobalManager.settings, nameof(AudioManager));
            }
        }

        public static float audioValue
        {
            get
            {
                if (GlobalManager.settings == null)
                {
                    GlobalManager.settings = new AudioSetting();
                    JsonManager.Load(GlobalManager.settings, nameof(AudioManager));
                }

                return GlobalManager.settings.audioValue;
            }
            set
            {
                GlobalManager.settings.audioValue = value;
                foreach (var audioSource in GlobalManager.audioData)
                {
                    audioSource.volume = value;
                }

                JsonManager.Save(GlobalManager.settings, nameof(AudioManager));
            }
        }

        public static async void PlayMain(string name, Action<AudioSource> action = null)
        {
            if (!GlobalManager.Instance) return;
            var target = GlobalSetting.GetAudioPath(name);
            var source = GlobalManager.Instance.sounds;
            source.clip = await AssetManager.Load<AudioClip>(target);
            source.loop = true;
            source.volume = musicValue;
            action?.Invoke(source);
            source.Play();
        }

        public static async void PlayLoop(string name, Action<AudioSource> action = null)
        {
            if (!GlobalManager.Instance) return;
            var target = GlobalSetting.GetAudioPath(name);
            var source = LoadPool(target).Dequeue();
            GlobalManager.audioData.Add(source);
            source.transform.SetParent(null);
            source.gameObject.SetActive(true);
            source.clip = await AssetManager.Load<AudioClip>(target);
            source.loop = true;
            source.volume = audioValue;
            action?.Invoke(source);
            source.Play();
        }

        public static async void PlayOnce(string name, Action<AudioSource> action = null)
        {
            if (!GlobalManager.Instance) return;
            var target = GlobalSetting.GetAudioPath(name);
            var source = LoadPool(target).Dequeue();
            GlobalManager.audioData.Add(source);
            source.transform.SetParent(null);
            source.gameObject.SetActive(true);
            source.clip = await AssetManager.Load<AudioClip>(target);
            source.loop = false;
            source.volume = audioValue;
            action?.Invoke(source);
            source.Play();
            source.Wait(source.clip.length).OnUpdate(() => StopLoop(source));
        }

        public static void StopMain(bool pause = true)
        {
            if (!GlobalManager.Instance) return;
            if (pause)
            {
                GlobalManager.Instance.sounds.Pause();
            }
            else
            {
                GlobalManager.Instance.sounds.Stop();
            }
        }

        public static void StopLoop(AudioSource source)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.poolGroup.TryGetValue(source.name, out var pool))
            {
                pool = new GameObject(Service.Text.Format("Pool - {0}", source.name));
                pool.transform.SetParent(GlobalManager.Instance.transform);
                GlobalManager.poolGroup.Add(source.name, pool);
            }

            source.Stop();
            source.gameObject.SetActive(false);
            source.transform.SetParent(pool.transform);
            GlobalManager.audioData.Remove(source);
            LoadPool(source.name).Enqueue(source);
        }

        private static AudioPool LoadPool(string path)
        {
            if (GlobalManager.poolData.TryGetValue(path, out var pool))
            {
                return (AudioPool)pool;
            }

            pool = new AudioPool(typeof(AudioSource), path);
            GlobalManager.poolData.Add(path, pool);
            return (AudioPool)pool;
        }

        internal static void Dispose()
        {
            GlobalManager.audioData.Clear();
        }
    }
}