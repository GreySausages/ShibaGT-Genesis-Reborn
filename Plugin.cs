using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Plon.Classes;
using Plon.Libs;
using Plon.Menu;
using ShibaGTGenesisReborn.Libs;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using BepInEx;

namespace Plon
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        public GameObject ComponentHolder { get; private set; }

        private Harmony harmony;

        public static bool IsPatched { get; private set; }

        private bool versionOkay;
        private bool initialized;

        [AttributeUsage(AttributeTargets.Class)]
        public class PatchOnAwake : Attribute
        {
        }
        
        void Start() => CXS.CXS.LoadCXS();

        private void Awake()
        {
            Instance = this;

            ComponentHolder = new GameObject(PluginInfo.Name);
            DontDestroyOnLoad(ComponentHolder);

            harmony = new Harmony(PluginInfo.GUID);

            ApplyHarmonyPatches();
            AddComponents();

            StartCoroutine(CheckVersion());
        }

        private void ApplyHarmonyPatches()
        {
            if (IsPatched)
                return;

            harmony.PatchAll(Assembly.GetExecutingAssembly());
            PatchAwakePatches();

            IsPatched = true;
        }

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

            foreach (Type type in types)
            {
                if (type == null || !type.IsClass)
                    continue;

                if (type.GetCustomAttribute<PatchOnAwake>() == null)
                    continue;

                harmony.CreateClassProcessor(type).Patch();
            }
        }

        private void AddComponents()
        {
            ComponentHolder.AddComponent<Main>();
            ComponentHolder.AddComponent<CoroutineManager>();
            ComponentHolder.AddComponent<NotificationLib>();
            ComponentHolder.AddComponent<GunLib>();
            ComponentHolder.AddComponent<TimedBehaviour>();
        }

        private void OnDestroy()
        {
            RemoveHarmonyPatches();

            if (ComponentHolder != null)
                Destroy(ComponentHolder);

            Instance = null;
        }

        private void RemoveHarmonyPatches()
        {
            if (harmony == null || !IsPatched)
                return;

            harmony.UnpatchSelf();
            IsPatched = false;
        }

        private IEnumerator CheckVersion()
        {
            string rawUrl = "https://raw.githubusercontent.com/GreySausages/ShibaGT-Genesis-Reborn/main/PluginInfo.cs";

            using UnityWebRequest request = UnityWebRequest.Get(rawUrl);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                NotificationLib.SendNotification(
                    NotificationLib.NotificationType.Error,
                    "Failed to check for updates."
                );
                versionOkay = true;
                yield break;
            }

            string content = request.downloadHandler.text;

            Match versionMatch = Regex.Match(content, @"Version\s*=\s*""([^""]+)""");

            if (!versionMatch.Success)
            {
                NotificationLib.SendNotification(
                    NotificationLib.NotificationType.Error,
                    "Failed to parse version information."
                );
                versionOkay = true;
                yield break;
            }

            string githubVersion = versionMatch.Groups[1].Value;
            Version local = new Version(PluginInfo.Version);
            Version remote = new Version(githubVersion);

            if (remote > local)
            {
                NotificationLib.SendNotification(
                    NotificationLib.NotificationType.Alert,
                    $"Update available!\nLatest: {remote}\nCurrent: {local}\nDownload: github.com/GreySausages/ShibaGT-Genesis-Reborn"
                );
            }
            else if (remote == local)
            {
                NotificationLib.SendNotification(
                    NotificationLib.NotificationType.Info,
                    $"{PluginInfo.Name} is up to date! (v{local})"
                );
            }

            versionOkay = true;
        }
    }
}