using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using BepInEx;
using GorillaLocomotion;

namespace ShibaGTGenesisReborn
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        public GameObject ComponentHolder { get; private set; }

        private Harmony harmony;
        private bool versionOkay;
        private bool initialized;
        private bool isRunning = true;
        private Coroutine versionLoopCoroutine;

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class PatchOnAwake : Attribute { }

        private void PatchAwakePatches()
        {
            Type[] types;

            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                if (type == null || !type.IsClass)
                    continue;

                if (type.GetCustomAttribute<PatchOnAwake>() == null)
                    continue;

                harmony.CreateClassProcessor(type).Patch();
            }
        }

        private void Awake()
        {
            Instance = this;

            ComponentHolder = new GameObject(PluginInfo.Name);
            DontDestroyOnLoad(ComponentHolder);

            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        private void Start()
        {
            CXS.CXS.LoadCXS();

            harmony = new Harmony(PluginInfo.GUID);

            harmony.PatchAll();
            PatchAwakePatches();

            ComponentHolder.AddComponent<Main>();
            ComponentHolder.AddComponent<CoroutineManager>();
            ComponentHolder.AddComponent<NotificationLib>();
            ComponentHolder.AddComponent<TimedBehaviour>();
            ComponentHolder.AddComponent<NetworkingLibrary>();

            StartCoroutine(StartVersionCheck());
        }

        private void OnPlayerSpawned()
        {
            if (initialized || !isRunning)
                return;

            initialized = true;

            if (ComponentHolder != null && ComponentHolder.GetComponent<InputHandler>() == null)
                ComponentHolder.AddComponent<InputHandler>();

            versionLoopCoroutine = StartCoroutine(WaitForVersionThenStartLoop());
        }

        private void OnDestroy()
        {
            isRunning = false;

            if (versionLoopCoroutine != null)
            {
                StopCoroutine(versionLoopCoroutine);
                versionLoopCoroutine = null;
            }

            StopAllCoroutines();

            harmony?.UnpatchSelf();

            if (ComponentHolder != null)
            {
                Destroy(ComponentHolder);
                ComponentHolder = null;
            }

            Instance = null;
        }

        private IEnumerator WaitForVersionThenStartLoop()
        {
            while (!versionOkay && isRunning)
                yield return null;

            if (isRunning)
                versionLoopCoroutine = StartCoroutine(VersionLoop());
        }

        private IEnumerator StartVersionCheck()
        {
            yield return CheckVersion(true);
        }

        private IEnumerator VersionLoop()
        {
            while (isRunning)
            {
                yield return new WaitForSeconds(300f);

                if (!isRunning)
                    yield break;

                yield return CheckVersion(false);
            }
        }

        private IEnumerator CheckVersion(bool startup)
        {
            string rawUrl = "https://raw.githubusercontent.com/GreySausages/ShibaGT-Genesis-Reborn/main/PluginInfo.cs";

            UnityWebRequest request = UnityWebRequest.Get(rawUrl);

            try
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (startup)
                    {
                        NotificationLib.SendNotification(
                            NotificationLib.NotificationType.Error,
                            "Unable to connect to update servers."
                        );
                    }

                    versionOkay = true;
                    yield break;
                }

                string content = request.downloadHandler.text;

                Match versionMatch = Regex.Match(content, @"Version\s*=\s*""([^""]+)""");

                if (!versionMatch.Success)
                {
                    if (startup)
                    {
                        NotificationLib.SendNotification(
                            NotificationLib.NotificationType.Error,
                            "Failed to parse version information."
                        );
                    }

                    versionOkay = true;
                    yield break;
                }

                string githubVersion = versionMatch.Groups[1].Value;
                Version local = new Version(PluginInfo.Version);
                Version remote = new Version(githubVersion);

                if (remote > local)
                {
                    if (startup)
                    {
                        NotificationLib.SendNotification(
                            NotificationLib.NotificationType.Alert,
                            $"Update available!\nLatest: {remote}\nCurrent: {local}\nDownload: github.com/GreySausages/ShibaGT-Genesis-Reborn"
                        );
                    }

                    versionOkay = true;
                }
                else if (remote == local)
                {
                    if (startup)
                    {
                        NotificationLib.SendNotification(
                            NotificationLib.NotificationType.Info,
                            $"{PluginInfo.Name} is up to date! (v{local})"
                        );
                    }

                    versionOkay = true;
                }
                else
                {
                    if (startup)
                    {
                        NotificationLib.SendNotification(
                            NotificationLib.NotificationType.Error,
                            $"Modified or invalid {PluginInfo.Name} detected. Please download the official version."
                        );
                    }

                    versionOkay = false;
                }
            }
            finally
            {
                request.Dispose();
            }
        }
    }
}