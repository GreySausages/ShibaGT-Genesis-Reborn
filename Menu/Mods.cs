using BepInEx;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using POpusCodec.Enums;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TagEffects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static ShibaGTGenesisReborn.Libs.GunLib;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Menu
{
    public class mods
    {
        public static void Disconnect()
        {
            PhotonNetwork.Disconnect();
        }

        public static void Joincodegenesis()
        {
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("GENESIS", GorillaNetworking.JoinType.Solo);
        }

        public static void Save()
        {
            List<string> list = new List<string>();
            foreach (ButtonInfo[] btn1 in Buttons.buttons)
            {
                foreach (ButtonInfo btn in btn1)
                {
                    if (btn.enabled)
                    {
                        list.Add(btn.buttonText);
                    }
                    Directory.CreateDirectory("Genesis");
                    File.WriteAllLines("Genesis\\Genesis_Saved_Prefs.txt", list);
                }
            }
            if (Main.what)
            {
                list.Add("SideMagfoar");
            }
            Directory.CreateDirectory("Genesis");
            File.WriteAllLines("Genesis\\Genesis_Saved_Prefs.txt", list);
        }
        public static void Load()
        {
            if (File.Exists("Genesis\\Genesis_Saved_Prefs.txt"))
            {
                string[] shit = File.ReadAllLines("Genesis\\Genesis_Saved_Prefs.txt");
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
        public static void GreyScreen()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            GreyZoneManager.Instance.ActivateGreyZoneAuthority();
        }
        public static void NoGreyScreen()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
        }

        private static float PullPower = 0.07f;
        private static float UpHillPower = 0.065f;

        private static bool lastLeftTouch;
        private static bool lastRightTouch;

        private static string[] pullmodes =
        {
            "Speed Boost",
            "Legit",
            "Reset"
        };

        private static int pullmodeIndex = 0;

        public static void ChangePullMode()
        {
            pullmodeIndex = (pullmodeIndex + 1) % pullmodes.Length;

            switch (pullmodeIndex)
            {
                case 0: // Speed Boost
                    PullPower = 0.025f;
                    UpHillPower = 0.02f;
                    break;

                case 1: // Legit
                    PullPower = 0.07f;
                    UpHillPower = 0.065f;
                    break;

                case 2: // Reset
                    PullPower = 0.001f;
                    UpHillPower = 0.001f;
                    break;
            }

            Main.GetIndex("pullmode").overlapText = "Pull Mode: " + pullmodes[pullmodeIndex];
        }

        public static void PullMod()
        {
            bool leftTouch = GTPlayer.Instance.IsHandTouching(true);
            bool rightTouch = GTPlayer.Instance.IsHandTouching(false);

            if ((!leftTouch && lastLeftTouch) || (!rightTouch && lastRightTouch))
            {
                Vector3 velocity = GorillaTagger.Instance.rigidbody.linearVelocity;
                GTPlayer.Instance.transform.position += new Vector3(velocity.x * PullPower, velocity.y * UpHillPower, velocity.z * PullPower);
            }

            lastLeftTouch = leftTouch;
            lastRightTouch = rightTouch;
        }

        public static void FPS(int aa)
        {
            Application.targetFrameRate = aa;
        }

        public static string _leavesName;
        public static readonly List<GameObject> leaves = new List<GameObject>();
        private static readonly Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();

        public static void removeleaves()
        {
            if (_leavesName == null)
            {
                var path = "Environment Objects/LocalObjects_Prefab/Forest";
                if (!objectPool.TryGetValue(path, out var f))
                {
                    f = GameObject.Find(path);
                    if (f != null)
                        objectPool.Add(path, f);
                }

                if (f != null)
                {
                    var counts = new Dictionary<string, (int count, int siblingIndex)>();
                    for (int i = 0; i < f.transform.childCount; i++)
                    {
                        var t = f.transform.GetChild(i);
                        if (!t.name.StartsWith("UnityTempFile"))
                            continue;
                        if (!counts.TryGetValue(t.name, out var entry))
                            counts[t.name] = (1, t.GetSiblingIndex());
                        else
                            counts[t.name] = (entry.count + 1, entry.siblingIndex);
                    }
                    _leavesName = counts.Where(kv => kv.Value.count == 3).OrderByDescending(kv => kv.Value.siblingIndex).FirstOrDefault().Key ?? "UnityTempFile";
                }
            }

            foreach (var path in new[] { "Environment Objects/LocalObjects_Prefab/Forest", "RankedMain/Ranked_Layout/Ranked_Forest_prefab" })
            {
                if (!objectPool.TryGetValue(path, out var forest))
                {
                    forest = GameObject.Find(path);
                    if (!forest && path.Contains("/"))
                    {
                        var split = path.Split('/');
                        var tr = GameObject.Find(split[0])?.transform.Find(path[(split[0].Length + 1)..]);
                        if (tr != null)
                            forest = tr.gameObject;
                    }
                    if (forest != null)
                        objectPool.Add(path, forest);
                }

                if (forest == null)
                    continue;
                for (int i = 0; i < forest.transform.childCount; i++)
                {
                    var child = forest.transform.GetChild(i).gameObject;
                    if (!child.name.Contains(_leavesName))
                        continue;
                    child.SetActive(false);
                    leaves.Add(child);
                }
            }
        }
        public static void addleaves()
        {
            foreach (var l in leaves)
                l.SetActive(true);
            leaves.Clear();
        }
        public static string lastmap;
        public static void JoinRandom()
        {
            if (PhotonNetworkController.Instance.currentJoinTrigger.networkZone != null)
            {
                lastmap = PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
            }
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(lastmap), GorillaNetworking.JoinType.Solo);
            }
        }
        public static int OutlineIndex;
        public static Color[] outlines =
        {
            Color.blue,
            Color.green,
            Color.red,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(0.06f, 0.06f, 0.06f),
        };
        public static readonly string[] outnames =
        {
            "Blue",
            "Green",
            "Red",
            "Yellow",
            "Cyan",
            "Magenta",
            "Grey",
        };
        public static void ChangeOutlineColor()
        {
            OutlineIndex = (OutlineIndex + 1) % outlines.Length;
            Main.GetIndex("COC").overlapText = "Outline: " + outnames[OutlineIndex];
            Main.what2 = outlines[OutlineIndex];
        }
        public static void Removeprefs()
        {
            if (!File.Exists("Genesis\\Genesis_Saved_Prefs.txt")) return;
            File.Delete("Genesis\\Genesis_Saved_Prefs.txt");
            File.Delete("Genesis");
        }

        public static bool G = false;
        public static bool hasTpd = false;
        public static float num = 8f;

        public static void EquipGun()
        {
            GunLib.StartGun(() =>
            {
                G = !G;
                if (!G)
                {
                    GunLib.CleanupPointer();
                }
            }, false);
        }

        private static bool teleportGunPressed;

        public static void TeleportGun()
        {
            GunLib.StartGun(() =>
            {
                if (!teleportGunPressed)
                {
                    Vector3 targetPos = GunLib.GetPointerPos();

                    Noclipistuff(true);

                    GorillaLocomotion.GTPlayer.Instance.transform.position = targetPos;
                    GorillaTagger.Instance.transform.position = targetPos;

                    GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

                    Noclipistuff(false);

                    teleportGunPressed = true;
                }

            }, false);

            if (!InputHandler.Instance.RightTrigger.IsPressed)
            {
                teleportGunPressed = false;
            }
        }

        public static GameObject checkpoint;
        private static bool teleporting;
        private static float teleportTime;

        public static void CheckPoint()
        {
            if (InputHandler.Instance.RightGrip.IsPressed)
            {
                if (checkpoint == null)
                {
                    checkpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    UnityEngine.Object.Destroy(checkpoint.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(checkpoint.GetComponent<SphereCollider>());

                    checkpoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }

                checkpoint.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }

            if (checkpoint == null)
                return;

            if (InputHandler.Instance.RightPrimary.WasPressed && !teleporting)
            {
                teleporting = true;
                teleportTime = 0.1f;

                Noclipistuff(true);

                Color color = Settings.backgroundColor.colors[0].color;
                color = Color.Lerp(color, Color.white, 0.35f);
                color.a = 0.5f;

                checkpoint.GetComponent<Renderer>().material.color = color;

                CXS.CXS.TeleportPlayer(checkpoint.transform.position); // why not

                GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }

            if (teleporting)
            {
                teleportTime -= Time.deltaTime;

                if (teleportTime <= 0)
                {
                    Noclipistuff(false);
                    teleporting = false;
                }
            }
            else
            {
                Color color = Settings.backgroundColor.colors[0].color;
                color.a = 1f;

                checkpoint.GetComponent<Renderer>().material.color = color;
            }
        }
        public static void CheckPointDisable()
        {
            if (checkpoint != null)
            {
                UnityEngine.Object.Destroy(checkpoint);
                checkpoint = null;
            }
        }

        // ShibaGTGenesisReborn is a fat little chud, chud ,chud ,chud ,chud ,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud,chud
        public static void Noclipistuff(bool b)
        {
            foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
            {
                if (b)
                {
                    collider.enabled = false;
                }
                else
                {
                    collider.enabled = true;
                }
            }
        }

        public static void TagGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null &&
                    !GunLib.LockedPlayer.mainSkin.material.name.Contains("fected") &&
                    !GunLib.LockedPlayer.isOfflineVRRig)
                {
                    if (VRRig.LocalRig.mainSkin.material.name.Contains("fected"))
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.rightHandTransform.position = GunLib.LockedPlayer.headConstraint.position;
                        VRRig.LocalRig.leftHandTransform.position = GunLib.LockedPlayer.headConstraint.position;
                        VRRig.LocalRig.transform.position = GunLib.LockedPlayer.headConstraint.position;
                        GameMode.ReportTag(GunLib.LockedPlayer.Creator);
                        VRRig.LocalRig.enabled = true;
                    }
                }
            }, true);
        }

        public static void ShibaGun()
        {
            GunLib.StartGun(() =>
            {
                Vector3 funn = (GunLib.GetPointerPos() - GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position).normalized;
                funn *= 60f;

                GameObject shiba = Main.LoadAssetBundle("shiba");
                shiba.transform.localScale /= 3f;
                shiba.transform.position = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
                shiba.transform.rotation = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.rotation;

                Object.Destroy(shiba.GetComponent<Rigidbody>());
                Object.Destroy(shiba.GetComponent<BoxCollider>());

                shiba.transform.Find("Handle1").AddComponent<BoxCollider>();
                shiba.transform.Find("Handle1").AddComponent<Rigidbody>();
                shiba.transform.Find("Handle1").GetComponent<Rigidbody>().velocity = funn;

                shiba.gameObject.layer = 8;
                shiba.transform.Find("Handle1").gameObject.layer = 8;
                shiba.transform.Find("Handle1").name = string.Concat(shiba.name, "MonoObject");

                Object.Destroy(shiba, 15f);
            }, false);
        }

        public static void LagGun(float delay, int hm)
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    if (Time.time > CDown)
                    {
                        for (int i = 0; i < hm; i++)
                        {
                            SendOPRaiseEvent202(GunLib.LockedPlayer);
                        }
                        CDown = Time.time + delay;
                    }
                }
            }, true);
        }

        public static void FlingGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    VRRig.LocalRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    VRRig.LocalRig.transform.position = GunLib.LockedPlayer.transform.position;
                    SnowballSpam1(-GunLib.LockedPlayer.transform.up * 20f, GunLib.LockedPlayer.transform.position - new Vector3(0f, -0.3f, 0f));
                }
            }, true);
        }

        public static void GunSmoothNess()
        {
            if (num == 8f)
            {
                num = 66f;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Keyboard";
            }
            else if (num == 66f)
            {
                num = 144f;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Thick";
            }
            else
            {
                num = 8;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Normal";
            }
        }

        public static void RPCProt()
        {
            if (!PhotonNetwork.InRoom) return;
            MonkeAgent.instance.rpcErrorMax = int.MaxValue;
            MonkeAgent.instance.rpcCallLimit = int.MaxValue;
            MonkeAgent.instance.logErrorMax = int.MaxValue;
            var a = typeof(MonkeAgent).GetField("userRPCCalls", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(MonkeAgent.instance);
            a?.GetType().GetMethod("Clear")?.Invoke(a, null);

            PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
            PhotonNetwork.QuickResends = int.MaxValue;

            PhotonNetwork.SendAllOutgoingCommands();
        }

        public static bool enablebracelet;
        public static void BraceletSpam()
        {
            if (Time.time > delay + 0.1f)
            {
                enablebracelet = !enablebracelet;
                GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, enablebracelet, false);
                delay = Time.time;
            }
        }

        public static void NoBracelet()
        {
            GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, false, false);
        }

        public static void SoundSpammer(int id = 18)
        {
            if (!PhotonNetwork.InRoom) return;
            if (Time.time > delay && InputHandler.Instance.RightTrigger.IsPressed)
            {
                delay = Time.time + 0.1f;
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, new object[]
                {
                    id,
                    false,
                    999f
                });
                RPCProt();
            }
        }

        public static void spamtagOthers()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                return;
            }
            if (Time.time > mods.tagTimer + 0.1f)
            {
                foreach (VRRig vrrig in VRRigCache.ActiveRigs)
                {
                    vrrig.PlayTaggedEffect();
                }
                tagTimer = Time.time;
            }
        }

        public static void FullBodyESP()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isOfflineVRRig)
                {
                    if (vrrig.mainSkin.material.name.Contains("fected") || vrrig.mainSkin.material.name.Contains("It"))
                    {
                        vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        vrrig.mainSkin.material.color = new Color32(255, 0, 0, 100);
                    }
                    else
                    {
                        vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        vrrig.mainSkin.material.color = new Color32(0, 255, 0, 100);
                    }
                }
            }
        }

        public static void CasualFullBodyESP()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isOfflineVRRig)
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = vrrig.playerColor;
                }
            }
        }

        public static void DisableFullBodyESP()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != VRRig.LocalRig && vrrig.mainSkin.material.shader == Shader.Find("GUI/Text Shader"))
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                }
            }
        }

        public static void RGB(bool strobe = false)
        {
            if (!PhotonNetwork.InRoom) return;

            Color c = strobe ? new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) : Color.HSVToRGB(Mathf.Repeat(Time.time * 0.2f, 1f), 1f, 1f);

            GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, c.r, c.g, c.b);
        }

        public static void Tracers()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isOfflineVRRig)
                {
                    GameObject g = new GameObject("Line");
                    LineRenderer l = g.AddComponent<LineRenderer>();
                    l.startWidth = 0.01f;
                    l.endWidth = 0.01f;
                    l.positionCount = 2;
                    l.useWorldSpace = true;
                    l.SetPosition(0, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position);
                    l.SetPosition(1, rig.transform.position);
                    l.material.shader = Shader.Find("GUI/Text Shader");
                    l.startColor = rig.playerColor;
                    l.endColor = rig.playerColor;
                    GameObject.Destroy(l, Time.deltaTime);
                }
            }
        }

        public static void SwitchPagePos()
        {
            if (!Main.what)
            {
                Main.what = true;
                Main.GetIndex("PPos").overlapText = "Menu Layout: Sides";
            }
            else
            {
                Main.what = false;
                Main.GetIndex("PPos").overlapText = "Menu Layout: ShibaGT";
            }
        }

        public static void NoTagOnJoin()
        {
            PlayerPrefs.SetString("didTutorial", "nope");
            PlayerPrefs.SetString("tutorial", "nope");
            Hashtable hasht = new Hashtable();
            hasht.Add("didTutorial", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hasht, null, null);
            PlayerPrefs.Save();
        }

        public static void WaterSplash()
        {
            if (!PhotonNetwork.InRoom) return;
            if (Time.time > delay)
            {
                if (InputHandler.Instance.RightTrigger.IsPressed)
                {
                    delay = Time.time + 0.3f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation, 4f, 100f, false, true });
                }
            }
            if (Time.time > delay)
            {
                if (InputHandler.Instance.LeftTrigger.IsPressed)
                {
                    delay = Time.time + 0.3f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.leftHandTransform.rotation, 4f, 100f, false, true });
                }
            }
        }

        public static void GravityManager(Gravitytypes type)
        {
            switch (type)
            {
                case Gravitytypes.Low:
                    GorillaTagger.Instance.rigidbody.AddForce(Vector3.up * 6.57f, ForceMode.Acceleration);
                    break;
                case Gravitytypes.High:
                    GorillaTagger.Instance.rigidbody.AddForce(Vector3.down * 7.67f, ForceMode.Acceleration); // omg 67
                    break;
                case Gravitytypes.Zero:
                    GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration); // trying a new zero grav since the old one was weird.
                    break;
                case Gravitytypes.Reverse:
                    GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity * 3f, ForceMode.Acceleration);
                    GTPlayer.Instance.GetControllerTransform(false).parent.rotation = Quaternion.Euler(180f, 0f, 0f); // I like the turning feature on the S menu so I added it
                    break;
            }
        }

        public static void Reset_upsidedown() => GTPlayer.Instance.GetControllerTransform(false).parent.rotation = Quaternion.identity;

        public enum Gravitytypes
        {
            Low,
            High,
            Zero,
            Reverse
        }

        public static void Noclip()
        {
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();
            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !(InputHandler.Instance.RightTrigger.IsPressed);
            }
        }

        private static int Platcolor;
        private static Color PlatColor = Color.blue;
        public static readonly Color[] PlatColors =
        {
            Color.blue,
            Color.red,
            Color.green,
            Color.cyan,
            Color.magenta,
        };
        public static readonly string[] ColorNames =
        {
            "Blue",
            "Red",
            "Green",
            "Cyan",
            "Magenta",
        };
        public static void PlatColorChange()
        {
            Platcolor = (Platcolor + 1) % PlatColors.Length;
            Main.GetIndex("Change Plat Color").overlapText = "Plat Color: " + ColorNames[Platcolor];
            PlatColor = PlatColors[Platcolor];
        }

        public static float delay;
        public static void HoverboardSpam()
        {
            if (!PhotonNetwork.InRoom) return;
            if (InputHandler.Instance.RightGrip.IsPressed)
            {
                if (Time.time > delay + 0.3f)
                {
                    delay = Time.time;
                    FreeHoverboardManager.instance.SendDropBoardRPC(GorillaTagger.Instance.rightHandTransform.position, Quaternion.identity, GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f, false), GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f, false), Color.black);
                }
            }
        }

        public static void UpAndDown()
        {
            if (InputHandler.Instance.RightTrigger.IsPressed)
            {
                GorillaTagger.Instance.rigidbody.AddForce(GTPlayer.Instance.bodyCollider.transform.up * 20f * Time.deltaTime, ForceMode.VelocityChange);

            }
            if (InputHandler.Instance.LeftTrigger.IsPressed)
            {
                GorillaTagger.Instance.rigidbody.AddForce(-GTPlayer.Instance.bodyCollider.transform.up * 20f * Time.deltaTime, ForceMode.VelocityChange);
            }
        }

        public static void BDisconnect()
        {
            if (InputHandler.Instance.RightSecondary.IsPressed)
            {
                PhotonNetwork.Disconnect();
                NetworkSystem.Instance.ReturnToSinglePlayer();
            }
        }

        private static GameObject PlatR, PlatL = null;
        private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
        public static void Platforms(bool Invis = false)
        {
            if (InputHandler.Instance.RightTrigger.IsPressed && PlatR == null)
            {
                PlatR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                PlatR.transform.localScale = scale;
                PlatR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                PlatR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                GameObject.Destroy(PlatR.GetComponent<Rigidbody>());
                PlatR.GetComponent<Renderer>().material.color = PlatColor;
                if (Invis) GameObject.Destroy(PlatR.GetComponent<Renderer>());
            }
            if (!InputHandler.Instance.LeftGrip.IsPressed && PlatL == null)
            {
                PlatL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                PlatL.transform.localScale = scale;
                PlatL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                PlatL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                GameObject.Destroy(PlatL.GetComponent<Rigidbody>());
                PlatL.GetComponent<Renderer>().material.color = PlatColor;
                if (Invis) GameObject.Destroy(PlatL.GetComponent<Renderer>());
            }
            if (!InputHandler.Instance.LeftGrip.IsPressed && PlatL != null)
            {
                GameObject.Destroy(PlatL);
                PlatL = null;
            }
        }

        public static void CarMonkeyandfly(float speed, bool fly)
        {
            if (InputHandler.Instance.RightSecondary.IsPressed)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position += GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * speed;
                if (fly) GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static float tagTimer;

        public static void SendOPRaiseEvent202(VRRig p = null)
        {
            RaiseEventOptions o;
            if (p != null)
            {
                o = new RaiseEventOptions { TargetActors = new int[] { p.Creator.ActorNumber } };
            }
            else
            {
                o = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            }
            PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new object[]
            {
                "ello"
            }, o, SendOptions.SendUnreliable);
            RPCProt();
        }

        public static float CDown;
        public static void LagAll(float delay, int hm)
        {
            if (Time.time > CDown)
            {
                for (int i = 0; i < hm; i++)
                {
                    SendOPRaiseEvent202();
                }
                CDown = Time.time + delay;
            }
        }

        private static bool Ghost_Toggled = false;
        private static bool Invis_Toggled = false;

        public static void GhostMonke()
        {
            bool isPressed = InputHandler.Instance.LeftPrimary.WasPressed;

            if (isPressed)
            {
                Ghost_Toggled = !Ghost_Toggled;
                VRRig.LocalRig.enabled = !Ghost_Toggled;
            }
        }

        public static void InvisMonke()
        {
            if (InputHandler.Instance.RightPrimary.WasPressed)
                Invis_Toggled = !Invis_Toggled;

            if (Invis_Toggled)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = new Vector3(0f, -100f, 0f);
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void placeholder()
        {

        }

        public static float notifcooldown;
        public static void AntiReport()
        {
            foreach (GorillaPlayerScoreboardLine boardline in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (boardline.linePlayer != NetworkSystem.Instance.LocalPlayer || boardline.reportButton == null)
                {
                    Transform transform = boardline.reportButton.gameObject.transform;
                    foreach (VRRig vrrig in VRRigCache.ActiveRigs)
                    {
                        if (vrrig == null || vrrig != GorillaTagger.Instance.offlineVRRig)
                        {
                            if (Vector3.Distance(vrrig.rightHandTransform.position, transform.position) < 0.4 || Vector3.Distance(vrrig.leftHandTransform.position, transform.position) < 0.4 && Time.time > notifcooldown + 1f)
                            {
                                notifcooldown = Time.time;
                                NetworkSystem.Instance.ReturnToSinglePlayer();
                                PhotonNetwork.Disconnect();
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static void LongArms()
        {
            if (InputHandler.Instance.RightTrigger.IsPressed)
            {
                GTPlayer.Instance.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
            }
            if (InputHandler.Instance.LeftTrigger.IsPressed)
            {
                GTPlayer.Instance.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
            }
        }

        public static void NormalArms()
        {
            GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private static bool dragging;
        private static float yaw, pitch, anchorX, anchorY;
        private const float sensitivity = 360f * 1.33f;
        private const float speed = 9f;

        public static void WASDFly()
        {
            Rigidbody rb = GorillaTagger.Instance.rigidbody;
            Transform cam = GorillaLocomotion.GTPlayer.Instance.GetControllerTransform(false).parent;
            rb.linearVelocity = Vector3.zero;

            if (Mouse.current.rightButton.isPressed)
            {
                float mx = Mouse.current.position.value.x / Screen.width;
                float my = Mouse.current.position.value.y / Screen.height;

                if (!dragging)
                {
                    dragging = true;
                    Vector3 e = cam.rotation.eulerAngles;
                    yaw = e.y;
                    pitch = e.x > 180f ? e.x - 360f : e.x;
                    anchorX = mx;
                    anchorY = my;
                }

                yaw += (mx - anchorX) * sensitivity;
                pitch = Mathf.Clamp(pitch - (my - anchorY) * sensitivity, -90f, 90f);
                anchorX = mx;
                anchorY = my;

                cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            else
            {
                dragging = false;
            }

            float dt = Time.deltaTime * speed * (UnityInput.Current.GetKey(KeyCode.LeftShift) ? 1.5f : 1f);
            var t = rb.transform;
            if (UnityInput.Current.GetKey(KeyCode.W)) t.position += cam.forward * dt;
            if (UnityInput.Current.GetKey(KeyCode.S)) t.position -= cam.forward * dt;
            if (UnityInput.Current.GetKey(KeyCode.A)) t.position -= cam.right * dt;
            if (UnityInput.Current.GetKey(KeyCode.D)) t.position += cam.right * dt;
            if (UnityInput.Current.GetKey(KeyCode.Space)) t.position += Vector3.up * dt;
            if (UnityInput.Current.GetKey(KeyCode.LeftControl)) t.position += Vector3.down * dt;
        }

        public static void NoFinger()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.rightControllerPrimaryButton = false;
            ControllerInputPoller.instance.rightControllerSecondaryButton = false;
            ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = false;
        }

        public static void SpazRig()
        {
            System.Random random = new System.Random();
            GorillaTagger.Instance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3(random.Next(0, 360), random.Next(0, 360), random.Next(0, 360));
            GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.eulerAngles = new Vector3(random.Next(0, 360), random.Next(0, 360), random.Next(0, 360));
            GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.eulerAngles = new Vector3(random.Next(0, 360), random.Next(0, 360), random.Next(0, 360));
        }

        public static void TagPlayer(VRRig p)
        {
            if (!p.mainSkin.material.name.Contains("fected") && VRRig.LocalRig.mainSkin.material.name.Contains("fected"))
            {
                GorillaGameModes.GameMode.ReportTag(PhotonNetwork.CurrentRoom.GetPlayer(p.Creator.ActorNumber));

                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = p.headConstraint.position;
                VRRig.LocalRig.leftHandTransform.position = p.headConstraint.position;
                VRRig.LocalRig.rightHandTransform.position = p.headConstraint.position;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void TagAll()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                if (!p.isOfflineVRRig)
                {
                    TagPlayer(p);
                }
            }
        }

        private static float delayTimer;
        public static void SpamOthers(TagEffectsLibrary.EffectType? type = null)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "You need to be master");
                return;
            }

            delayTimer += Time.deltaTime;

            if (delayTimer < (type == null ? 0.5f : 0.1f)) return;
            delayTimer = 0f;

            Quaternion rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == localRig) continue;

                if (type == null)
                {
                    rig.PlayTaggedEffect();
                    continue;
                }

                TagEffectPack pack = new TagEffectPack();
                TagEffectsLibrary.PlayEffect(rig.transform, false, 0.35f, type.Value, pack, pack, rotation);
            }
        }

        public static void SpamSelf(TagEffectsLibrary.EffectType type)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "You need to be master");
                return;
            }

            delayTimer += Time.deltaTime;

            if (delayTimer < 0.5f) return;
            delayTimer = 0f;

            TagEffectPack pack = new TagEffectPack();
            TagEffectsLibrary.PlayEffect(GorillaTagger.Instance.offlineVRRig.rightHandTransform, false, 0.35f, type, pack, pack, GorillaTagger.Instance.rightHandTransform.rotation);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GorillaLocomotion.GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GorillaLocomotion.GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        static GameObject cat = null;

        public static void sillycatholdable()
        {
            if (cat == null)
            {
                cat = Main.LoadAssetBundle("sillylilguy");
                Object.Destroy(cat.transform.Find("Cube").GetComponent<BoxCollider>());
            }

            cat.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            cat.transform.position = TrueRightHand().position;

            cat.transform.rotation = TrueRightHand().rotation;
        }

        public static void RemoveCat()
        {
            if (cat != null)
            {
                Object.Destroy(cat);
                cat = null;
            }
        }

        public class ProjectileEntry
        {
            public string Name;
            public SnowballThrowable ThrowableLeft;
            public SnowballThrowable ThrowableRight;
            public SnowballThrowable Throwable => ThrowableRight;
            public int ThrowableIndex => Throwable != null ? Throwable.throwableMakerIndex : -1;
        }

        private static ProjectileEntry _snowballEntry;
        private static bool _isInitializing;

        public static void InitializeSnowball()
        {
            if (_snowballEntry != null || _isInitializing)
                return;

            if (CosmeticsController.instance == null || CosmeticsController.instance.v2_allCosmetics == null)
                return;

            _isInitializing = true;

            try
            {
                foreach (var info in CosmeticsController.instance.v2_allCosmetics)
                {
                    if (info.isThrowable && info.displayName.Contains("Snowball", StringComparison.OrdinalIgnoreCase))
                    {
                        if (CosmeticsV2Spawner_Dirty.GetPlayfabIdFromThrowableIndex(false, info.throwableIndex, out string rightId) &&
                            CosmeticsV2Spawner_Dirty.GetPlayfabIdFromThrowableIndex(true, info.throwableIndex, out string leftId))
                        {
                            var registry = VRRig.LocalRig?.cosmeticsObjectRegistry;
                            if (registry != null)
                            {
                                registry.Cosmetic(leftId);
                                registry.Cosmetic(rightId);

                                GrowingSnowballThrowable left = null, right = null;
                                foreach (var sb in SnowballMaker.leftHandInstance?.snowballs ?? Array.Empty<SnowballThrowable>())
                                {
                                    if (sb is GrowingSnowballThrowable gsb && sb.throwableMakerIndex == info.throwableIndex)
                                        left = gsb;
                                }
                                foreach (var sb in SnowballMaker.rightHandInstance?.snowballs ?? Array.Empty<SnowballThrowable>())
                                {
                                    if (sb is GrowingSnowballThrowable gsb && sb.throwableMakerIndex == info.throwableIndex)
                                        right = gsb;
                                }

                                if (left != null && right != null)
                                {
                                    left.velocityEstimator = SnowballMaker.leftHandInstance.velocityEstimator;
                                    right.velocityEstimator = SnowballMaker.rightHandInstance.velocityEstimator;

                                    _snowballEntry = new ProjectileEntry
                                    {
                                        Name = "Growing Snowball",
                                        ThrowableLeft = left,
                                        ThrowableRight = right
                                    };
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"InitializeSnowball failed: {ex}");
            }
            finally
            {
                _isInitializing = false;
            }
        }

        public enum ThrowableHand
        {
            Left,
            Right,
            Both,
            Dynamic
        }

        public static void UpdateNetworkedProjectile(int index, ThrowableHand hand)
        {
            if (hand == ThrowableHand.Left || hand == ThrowableHand.Both)
                VRRig.LocalRig.LeftThrowableProjectileIndex = index;
            if (hand == ThrowableHand.Right || hand == ThrowableHand.Both)
                VRRig.LocalRig.RightThrowableProjectileIndex = index;
            VRRig.LocalRig.myBodyDockPositions.RefreshTransferrableItems();
        }

        public static bool biig;

        public static void SendSnowball(Vector3 position, Vector3 velocity, Color? color = null, ThrowableHand hand = ThrowableHand.Dynamic)
        {
            try
            {
                if (_snowballEntry == null)
                {
                    InitializeSnowball();
                    if (_snowballEntry == null)
                        return;
                }

                Color32 finalColor = color ?? Color.white;
                GrowingSnowballThrowable throwable = (hand == ThrowableHand.Left ? _snowballEntry.ThrowableLeft : _snowballEntry.ThrowableRight) as GrowingSnowballThrowable;
                if (throwable == null)
                    throwable = _snowballEntry.Throwable as GrowingSnowballThrowable;
                if (throwable == null)
                    return;

                UpdateNetworkedProjectile(_snowballEntry.ThrowableIndex, hand);
                VRRig.LocalRig.SetThrowableProjectileColor(true, finalColor);

                int index = GetProjectileIncrement(position, velocity, throwable.transform.lossyScale.x);
                int scale = biig ? 5 : 0;
                if (NetworkSystem.Instance.InRoom)
                {
                    var changeSizeField = typeof(GrowingSnowballThrowable).GetField("changeSizeEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                    var snowballThrowField = typeof(GrowingSnowballThrowable).GetField("snowballThrowEvent", BindingFlags.NonPublic | BindingFlags.Instance);

                    PhotonEvent changeSizeEvent = changeSizeField != null ? (PhotonEvent)changeSizeField.GetValue(throwable) : null;
                    PhotonEvent snowballThrowEvent = snowballThrowField != null ? (PhotonEvent)snowballThrowField.GetValue(throwable) : null;

                    if (changeSizeEvent == null || snowballThrowEvent == null)
                        return;

                    var eventIdField = typeof(PhotonEvent).GetField("_eventId", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (eventIdField == null)
                        return;

                    int changeSizeId = (int)eventIdField.GetValue(changeSizeEvent);
                    int snowballThrowId = (int)eventIdField.GetValue(snowballThrowEvent);

                    PhotonNetwork.RaiseEvent(PhotonEvent.PHOTON_EVENT_CODE, new object[]
                    {
                        changeSizeId,
                        scale
                    }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

                    PhotonNetwork.RaiseEvent(PhotonEvent.PHOTON_EVENT_CODE, new object[]
                    {
                        snowballThrowId,
                        position,
                        velocity,
                        index
                    }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

                    mods.RPCProt();
                }
                else
                {
                    var spawnMethod = typeof(GrowingSnowballThrowable).GetMethod("SpawnGrowingSnowball", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (spawnMethod == null)
                        return;

                    object[] spawnArgs = new object[] { velocity, throwable.snowballSizeLevels[scale].snowballScale };
                    SlingshotProjectile proj = (SlingshotProjectile)spawnMethod.Invoke(throwable, spawnArgs);
                    if (proj == null)
                        return;

                    Vector3 spawnedVel = (Vector3)spawnArgs[0];

                    proj.Launch(position, spawnedVel, VRRig.LocalRig.Creator, false, false, index, throwable.snowballSizeLevels[scale].snowballScale, true, finalColor);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SendSnowball error: {e}");
            }
        }

        public static int GetProjectileIncrement(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                GameObject container = new GameObject("SlingshotProjectileHolder");
                SlingshotProjectile projectile = container.AddComponent<SlingshotProjectile>();

                int index = Time.frameCount;
                var trackerType = typeof(GrowingSnowballThrowable).Assembly.GetType("ProjectileTracker");
                if (trackerType == null)
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        trackerType = asm.GetType("ProjectileTracker");
                        if (trackerType != null)
                            break;
                    }
                }

                if (trackerType != null)
                {
                    var addMethod = trackerType.GetMethod("AddAndIncrementLocalProjectile", BindingFlags.Public | BindingFlags.Static);
                    if (addMethod != null)
                    {
                        index = (int)addMethod.Invoke(null, new object[] { projectile, Velocity, Position, Scale });
                    }
                }

                Object.Destroy(container);
                return index;
            }
            catch
            {
                return Time.frameCount;
            }
        }

        private static float spamDihlay;
        public static void SnowballSpam(Vector3 velocity, Vector3 woah)
        {
            if (!(Time.time > spamDihlay)) return;

            bool fireLeft = ControllerInputPoller.instance.leftGrab;
            bool fireRight = ControllerInputPoller.instance.rightControllerSecondaryButton || Mouse.current.rightButton.isPressed;

            if (fireRight)
            {

                for (int i = 0; i < 2; i++)
                {
                    SendSnowball(woah, velocity, Color.white, ThrowableHand.Right);
                }
                spamDihlay = Time.time + 0.5f;
            }
        }

        public static void SnowballSpam1(Vector3 velocity, Vector3 woah)
        {
            if (!(Time.time > spamDihlay)) return;

            bool fireLeft = ControllerInputPoller.instance.leftGrab;
            bool fireRight = ControllerInputPoller.instance.rightGrab || Mouse.current.rightButton.isPressed;

            if (fireRight)
            {

                for (int i = 0; i < 2; i++)
                {
                    SendSnowball(woah, velocity, Color.white, ThrowableHand.Right);
                }
                spamDihlay = Time.time + 0.5f;
            }
        }
    }
}