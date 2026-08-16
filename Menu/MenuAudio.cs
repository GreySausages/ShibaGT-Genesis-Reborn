using System.Collections;
using System.IO;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using UnityEngine;
using UnityEngine.Networking;

namespace ShibaGTGenesisReborn.Menu
{
    public static class MenuAudio
    {
        public static AudioClip clickClip;
        public static AudioSource audioSource;
        private static bool isInitialized;

        private const string AudioUrl = "https://github.com/incharilla1/assets/raw/refs/heads/main/button-click.mp3";
        private static string AudioPath => Path.Combine(ModsLib.GenesisDirectory, "button-click.mp3");

        public static void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            if (Main.Instance != null)
                Main.Instance.StartCoroutine(LoadAudio());
            else if (Plugin.Instance != null)
                CoroutineManager.RunCoroutine(LoadAudio());
            else if (CoroutineManager.instance != null)
                CoroutineManager.RunCoroutine(LoadAudio());
        }

        private static IEnumerator LoadAudio()
        {
            if (File.Exists(AudioPath))
            {
                using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + AudioPath, AudioType.MPEG);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) 
                    clickClip = DownloadHandlerAudioClip.GetContent(request);
            }
            else
            {
                using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(AudioUrl, AudioType.MPEG);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    clickClip = DownloadHandlerAudioClip.GetContent(request);
                    File.WriteAllBytes(AudioPath, request.downloadHandler.data);
                }
            }
        }

        public static void PlayClickSound(float volume = 1.25f)
        {
            if (clickClip == null) return;

            if (audioSource == null)
            {
                GameObject host = Main.Instance != null ? Main.Instance.gameObject : (GorillaTagger.Instance != null ? GorillaTagger.Instance.gameObject : null);
                if (host != null)
                {
                    audioSource = host.GetComponent<AudioSource>() ?? host.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 0f;
                }
            }

            if (audioSource != null)
                audioSource.PlayOneShot(clickClip, volume);
        }
    }
}
