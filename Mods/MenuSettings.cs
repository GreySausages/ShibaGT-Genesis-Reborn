using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static int OutlineIndex;
        public static Color[] outlines =
        {
            Color.blue,
            Color.green,
            Color.red,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            Color.white,
            Color.black,
            new Color(0.06f, 0.06f, 0.06f),
            new Color(1f, 0.5f, 0f),
            new Color(1f, 0.4f, 0.7f),
            new Color(0.5f, 0f, 1f),
            new Color(0.6f, 0.3f, 0f),
            new Color(0.6f, 1f, 0f),
            new Color(0.2f, 1f, 0.5f),
            new Color(1f, 0.2f, 0.2f),
            new Color(0.3f, 0.8f, 1f),
        };

        public static readonly string[] outnames =
        {
            "Blue",
            "Green",
            "Red",
            "Yellow",
            "Cyan",
            "Magenta",
            "White",
            "Black",
            "Dark Grey",
            "Orange",
            "Pink",
            "Purple",
            "Brown",
            "Lime",
            "Mint",
            "Coral",
            "Sky",
        };

        public static float notifcooldown;

        public static void Save()
        {
            string prefsPath = Path.Combine(ModsLib.GenesisDirectory, "Genesis_Saved_Prefs.txt");
            List<string> list = new List<string>();

            foreach (ButtonInfo[] buttonList in Buttons.buttons)
            {
                foreach (ButtonInfo btn in buttonList)
                {
                    if (btn.enabled || btn.isFavorite)
                    {
                        list.Add(btn.isFavorite ? "fav" + btn.buttonText : btn.buttonText);
                    }
                }
            }

            if (Main.what)
            {
                list.Add("SideMagfoar");
            }

            Directory.CreateDirectory(ModsLib.GenesisDirectory);
            File.WriteAllLines(prefsPath, list);
        }

        public static void Load()
        {
            string prefsPath = Path.Combine(ModsLib.GenesisDirectory, "Genesis_Saved_Prefs.txt");
            if (File.Exists(prefsPath))
            {
                string[] shit = File.ReadAllLines(prefsPath);
                foreach (ButtonInfo[] info in Buttons.buttons)
                {
                    foreach (ButtonInfo info1 in info)
                    {
                        if (info1.enabled)
                        {
                            info1.enabled = false;
                            info1?.disableMethod?.Invoke();
                        }
                    }
                }
                foreach (string shit2 in shit)
                {
                    foreach (ButtonInfo[] info in Buttons.buttons)
                    {
                        foreach (ButtonInfo info1 in info)
                        {
                            if (info1.buttonText == shit2 && !info1.enabled)
                            {
                                info1.enabled = true;
                                info1?.enableMethod?.Invoke();
                                info1?.method?.Invoke();
                            }
                            if ("fav" + info1.buttonText == shit2 && !info1.isFavorite)
                            {
                                info1.isFavorite = true;
                                Main.favoriteButtons.Add(info1);
                                Main.UpdateFavoritesCategory();
                            }
                        }
                        if (shit2.Contains("SideMagfoar"))
                        {
                            Main.GetIndex("PPos").overlapText = "Menu Layout: Sides";
                            Main.what = true;
                        }
                    }
                }
            }
        }

        public static void Removeprefs()
        {
            string prefsPath = Path.Combine(ModsLib.GenesisDirectory, "Genesis_Saved_Prefs.txt");
            if (File.Exists(prefsPath))
            {
                File.Delete(prefsPath);
            }
        }

        public static void SwitchPagePos()
        {
            Main.what = !Main.what ? true : false;
            Main.GetIndex("PPos").overlapText = Main.what ? "Menu Layout: Sides" : "Menu Layout: ShibaGT";
        }

        public static void ChangeOutlineColor()
        {
            OutlineIndex = (OutlineIndex + 1) % outlines.Length;
            Main.GetIndex("COC").overlapText = "Outline: " + outnames[OutlineIndex];
            Main.what2 = outlines[OutlineIndex];
        }

        public static void AntiReport()
        {
            if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.LocalPlayer == null) return;

            NetPlayer localPlayer = NetworkSystem.Instance.LocalPlayer;
            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line?.linePlayer == null || (line.linePlayer != localPlayer && line.playerActorNumber != localPlayer.ActorNumber && line.linePlayer.UserId != localPlayer.UserId)) continue;

                if (line.reportInProgress || (line.reportButton != null && line.reportButton.isOn) || (line.reportButtons != null && line.reportButtons.activeInHierarchy))
                    Disconnect();

                if (line.reportButton == null || !line.reportButton.gameObject.activeInHierarchy) continue;

                Vector3 target = line.reportButton.transform.position;
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (rig == null || rig.isOfflineVRRig || rig.isMyPlayer || rig == localRig) continue;

                    if ((rig.rightHandTransform != null && (rig.rightHandTransform.position - target).sqrMagnitude < 0.75f) ||
                        (rig.leftHandTransform != null && (rig.leftHandTransform.position - target).sqrMagnitude < 0.75f) ||
                        (rig.transform.position - target).sqrMagnitude < 1f)
                        Disconnect();
                }
            }
        }

        private static void Disconnect()
        {
            PhotonNetwork.Disconnect();
            NetworkSystem.Instance.ReturnToSinglePlayer();
            PhotonNetwork.SendAllOutgoingCommands();
            NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Anti Report disconnected you");
        }

        private static readonly List<ButtonInfo> panicSavedMods = new List<ButtonInfo>();

        public static void EnablePanic()
        {
            panicSavedMods.Clear();
            for (int i = 0; i < Buttons.buttons.Length; i++)
            {
                if (i == 14 || i == 15)
                    continue;

                foreach (ButtonInfo btn in Buttons.buttons[i])
                {
                    if (btn != null && btn.enabled && btn.buttonText != "Panic Button")
                    {
                        panicSavedMods.Add(btn);
                        btn.enabled = false;
                        btn.disableMethod?.Invoke();
                    }
                }
            }

            SlideControl(0.00425f);
            AirSwimDisable();
            JesusMonkeDisable();
            ZiplineSpeed(10f);
            ResetStickyHands();
            ReSlip();
            FixHead();
            NormalArms();
        }

        public static void DisablePanic()
        {
            foreach (ButtonInfo btn in panicSavedMods)
            {
                if (btn != null)
                {
                    btn.enabled = true;
                    btn.enableMethod?.Invoke();
                    btn.method?.Invoke();
                }
            }
            panicSavedMods.Clear();
        }
    }
}
