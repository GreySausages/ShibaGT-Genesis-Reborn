using BepInEx;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Plon.Classes;
using POpusCodec.Enums;
using ShibaGTGenesisReborn.Libs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Plon.Libs;
using TagEffects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Plon.Libs.GunLib;
using Object = UnityEngine.Object;

namespace Plon.Menu
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

        public static void EquipGun()
        {
            if (!G)
            {
                Gunlib(Lock);
            }
            else
            {
                GunTemplate();
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
        // Un used rn btw
        public static void SoundSpammer(int id = 18)
        {
            if (!PhotonNetwork.InRoom) return;
            if (Time.time > delay && ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
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
        public static void RGB()
        {
            Color c = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.2f, 1f), 1f, 1f);
            if (!PhotonNetwork.InRoom) return;
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
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
                {
                    delay = Time.time + 0.3f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation, 4f, 100f, false, true });
                }
            }
            if (Time.time > delay)
            {
                if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.1f)
                {
                    delay = Time.time + 0.3f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.leftHandTransform.rotation, 4f, 100f, false, true });
                }
            }
        }

        public static void LowGravity() => GorillaTagger.Instance.rigidbody.AddForce(Vector3.up * 6.5f, ForceMode.Acceleration);

        public static void HighGravity() => GorillaTagger.Instance.rigidbody.AddForce(Vector3.down * 10f, ForceMode.Acceleration);

        public static void Noclip()
        {
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();
            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !(ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f);
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
            if (trigger_Button)
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
            if (trigger_Button)
            {
                GorillaTagger.Instance.rigidbody.AddForce(GTPlayer.Instance.bodyCollider.transform.up * 20f * Time.deltaTime, ForceMode.VelocityChange);

            }
            if (grip_Button)
            {
                GorillaTagger.Instance.rigidbody.AddForce(-GTPlayer.Instance.bodyCollider.transform.up * 20f * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
        public static void BDisconnect()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                PhotonNetwork.Disconnect();
                NetworkSystem.Instance.ReturnToSinglePlayer();
            }
        }

        private static GameObject PlatR, PlatL = null;
        private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
        public static void Platforms(bool Invis = false)
        {
            if (ControllerInputPoller.instance.rightGrab && PlatR == null)
            {
                PlatR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                PlatR.transform.localScale = scale;
                PlatR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                PlatR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                GameObject.Destroy(PlatR.GetComponent<Rigidbody>());
                PlatR.GetComponent<Renderer>().material.color = PlatColor;
                if (Invis) GameObject.Destroy(PlatR.GetComponent<Renderer>());
            }
            if (!ControllerInputPoller.instance.rightGrab && PlatR != null)
            {
                GameObject.Destroy(PlatR);
                PlatR = null;
            }
            if (ControllerInputPoller.instance.leftGrab && PlatL == null)
            {
                PlatL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                PlatL.transform.localScale = scale;
                PlatL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                PlatL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                GameObject.Destroy(PlatL.GetComponent<Rigidbody>());
                PlatL.GetComponent<Renderer>().material.color = PlatColor;
                if (Invis) GameObject.Destroy(PlatL.GetComponent<Renderer>());
            }
            if (!ControllerInputPoller.instance.leftGrab && PlatL != null)
            {
                GameObject.Destroy(PlatL);
                PlatL = null;
            }
        }

        public static void CarMonkeyandfly(float speed, bool fly)
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
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
        public static void LagGun(float delay, int hm)
        {
            if (!G)
            {
                Gunlib(Lock);
            }
            else
            {
                GunTemplate();
            }
            if (trigger_Button && grip_Button)
            {
                if (Time.time > CDown)
                {
                    for (int i = 0; i < hm; i++)
                    {
                        SendOPRaiseEvent202(LockedPlayer);
                    }
                    CDown = Time.time + delay;
                }
            }
        }

        public static bool Ghost;
        public static bool Button;
        public static void GhostMonkey()
        {
            VRRig.LocalRig.enabled = !Ghost;
            if (ControllerInputPoller.instance.rightControllerPrimaryButton && !Button)
                Ghost = !Ghost;
            Button = ControllerInputPoller.instance.rightControllerPrimaryButton;
            if (Ghost) NoFinger();
        }
        public static void InvisMonkey()
        {
            VRRig.LocalRig.enabled = !Button;
            VRRig.LocalRig.headBodyOffset.x = Ghost ? 180f : 0f;
            if (ControllerInputPoller.instance.rightControllerPrimaryButton && !Button)
                Ghost = !Ghost;
            Button = ControllerInputPoller.instance.rightControllerPrimaryButton;
        }

        public static void placeholder()
        {
            
        }



        public static float num = 8f;

        public static void GunSmoothNess()
        {
            if (num == 8f)
            {
                num = 66f;  // Normal
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Keyboard";
            }
            else if (num == 66f)
            {
                num = 144f; // Keyboard
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Thick";
            }
            else
            {
                num = 8; // Creamy
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Normal";
            }
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


        private static GameObject GunSphere;
        private static LineRenderer lineRenderer;
        private static RaycastHit woah;
        private static float timeCounter = 0f;
        private static Vector3[] linePositions;
        private static Vector3 previousControllerPosition;

        public static int NoBarrier()
        {
            return ~((IEnumerable<String>)new String[] { "TransparentFX", "Ignore Raycast", "Zone", "Gorilla Trigger", "Gorilla Boundary", "GorillaCosmetics", "GorillaParticle" }).Select((Func<String, int>)LayerMask.NameToLayer).Aggregate(0, (int num, int l) => num | (1 << l));
        }

        public static void GunTemplate()
        {
            if (ControllerInputPoller.instance.rightControllerGripFloat > 0.1f || UnityInput.Current.GetMouseButton(1))
            {
                if (Physics.Raycast(GTPlayer.Instance.RightHand.controllerTransform.position, -GTPlayer.Instance.RightHand.controllerTransform.up, out var hitinfo, 100f, NoBarrier()))
                {
                    if (Mouse.current.rightButton.isPressed)
                    {
                        Camera cam = GameObject.Find("Shoulder Camera").GetComponent<Camera>();
                        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                        Physics.Raycast(ray, out hitinfo, 100f, NoBarrier());
                    }
                    if (Lock)
                    {
                        if (LockedPlayer == null && ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
                        {
                            LockedPlayer = hitinfo.collider?.GetComponentInParent<VRRig>();
                        }
                        else if (LockedPlayer != null && ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
                        {
                            hitinfo.point = LockedPlayer.transform.position;
                        }
                        else if (LockedPlayer != null && ControllerInputPoller.instance.rightControllerIndexFloat < 0.1f)
                        {
                            LockedPlayer = null;
                        }
                    }
                    else
                    {
                        if (LockedPlayer == null && ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
                        {
                            LockedPlayer = hitinfo.collider?.GetComponentInParent<VRRig>();
                        }
                        else if (LockedPlayer != null && hitinfo.collider?.GetComponentInParent<VRRig>() == null)
                        {
                            LockedPlayer = null;
                        }
                    }
                    if (GunSphere == null)
                    {
                        GunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        GunSphere.transform.localScale = new Vector3(0f, 0f, 0f);
                        GunSphere.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                        GunSphere.GetComponent<Renderer>().material.color = Color.white;
                        GameObject.Destroy(GunSphere.GetComponent<BoxCollider>());
                        GameObject.Destroy(GunSphere.GetComponent<Rigidbody>());
                        GameObject.Destroy(GunSphere.GetComponent<Collider>());

                        lineRenderer = GunSphere.AddComponent<LineRenderer>();
                        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                        lineRenderer.startWidth = 0.01f;
                        lineRenderer.endWidth = 0.01f;
                        lineRenderer.startColor = Color.white;
                        lineRenderer.endColor = Color.white;

                        linePositions = new Vector3[50];
                        for (int i = 0; i < linePositions.Length; i++)
                        {
                            linePositions[i] = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
                        }
                    }

                    GunSphere.transform.position = hitinfo.point;

                    timeCounter += Time.deltaTime;

                    Vector3 pos1 = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
                    Vector3 direction = (hitinfo.point - pos1).normalized;
                    float distance = Vector3.Distance(pos1, hitinfo.point);

                    Vector3 controller = pos1 - previousControllerPosition;
                    previousControllerPosition = pos1;
                    woah = hitinfo;

                    if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f || Mouse.current.leftButton.isPressed)
                    {
                        
                    }

                    for (int i = 0; i < linePositions.Length; i++)
                    {
                        float t = i / (float)(linePositions.Length - 1);
                        Vector3 nigmax = Vector3.Lerp(pos1, hitinfo.point, t);

                        linePositions[i] += controller * 1f;
                        linePositions[i] += UnityEngine.Random.insideUnitSphere * 0.01f;
                        linePositions[i] = Vector3.Lerp(linePositions[i], nigmax, Time.deltaTime * 10f);
                    }

                    lineRenderer.positionCount = linePositions.Length;
                    lineRenderer.SetPositions(linePositions);

                    float gayanalsex = Mathf.PingPong(timeCounter, 1f);
                    Color fuckingcolor = Color.Lerp(Color.white, Color.cyan, gayanalsex);
                    lineRenderer.startColor = fuckingcolor;
                    lineRenderer.endColor = fuckingcolor;
                }
            }

            if (GunSphere != null && (ControllerInputPoller.instance.rightControllerGripFloat <= 0.1f && !UnityInput.Current.GetMouseButton(1)))
            {
                GameObject.Destroy(GunSphere);
                GameObject.Destroy(lineRenderer);
                timeCounter = 0f;
                linePositions = null;
            }
        }
        public static void LongArms()
        {
            if (trigger_Button)
            {
                GTPlayer.Instance.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
            }
            if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.1f)
            {
                GTPlayer.Instance.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
            }
        }
        public static void NormalArms()
        {
            GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        public static bool hasTpd = false;
        public static bool G = false;
        public static void TPGun()
        {
            if (!G)
            {
                Gunlib(Lock);
            }
            else
            {
                GunTemplate();
            }
            if (!hasTpd && grip_Button && trigger_Button)
            {
                GTPlayer.Instance.TeleportTo(!G ? hit.point : woah.point, Quaternion.Euler(Vector3.zero));
                hasTpd = true;
            }
            else if (hasTpd && grip_Button && !trigger_Button)
            {
                hasTpd = false;
            }
            else if (hasTpd && !grip_Button && !trigger_Button)
            {
                hasTpd = false;
            }
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
        public static void TagGun()
        {
            if (!G)
            {
                Gunlib(Lock);
            }
            else
            {
                GunTemplate();
            }
            if (grip_Button && trigger_Button)
            {
                if (LockedPlayer != null && !LockedPlayer.mainSkin.material.name.Contains("fected"))
                {
                    if (!LockedPlayer.isOfflineVRRig && VRRig.LocalRig.mainSkin.material.name.Contains("fected"))
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.rightHandTransform.position = LockedPlayer.headConstraint.position;
                        VRRig.LocalRig.leftHandTransform.position = LockedPlayer.headConstraint.position;
                        VRRig.LocalRig.transform.position = LockedPlayer.headConstraint.position;
                        GameMode.ReportTag(LockedPlayer.Creator);
                        VRRig.LocalRig.enabled = true;
                    }
                }
            }
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

            if (delayTimer < (type == null ? 0.5f : 0.1f)) return; // null means we do lava monkey tag effect
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
        
        // fun custom shit
        
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

        
        public static void ShibaGun()
        {
            GunLib.Gunlib(false);

            if (!GunLib.grip_Button || !GunLib.trigger_Button)
                return;

            Vector3 funn = (GunLib.hit.point - GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position).normalized;
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
        }
    }
}
