using BepInEx;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag.Audio;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Menu
{
    public class mods
    {
        #region Menu Settings
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
            List<string> list = new List<string>();
            foreach (ButtonInfo[] btn1 in Buttons.buttons)
            {
                foreach (ButtonInfo btn in btn1)
                {
                    if (btn.enabled)
                    {
                        list.Add(btn.buttonText);
                    }
                }
            }
            if (Main.what)
            {
                list.Add("SideMagfoar");
            }
            string prefsPath = Path.Combine(ModsLib.GenesisDirectory, "Genesis_Saved_Prefs.txt");
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

        public static void ChangeOutlineColor()
        {
            OutlineIndex = (OutlineIndex + 1) % outlines.Length;
            Main.GetIndex("COC").overlapText = "Outline: " + outnames[OutlineIndex];
            Main.what2 = outlines[OutlineIndex];
        }

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
        #endregion

        #region Advantages
        public static string _leavesName;
        public static readonly List<GameObject> leaves = new List<GameObject>();
        private static readonly Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();

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

        public static void NoTagOnJoin()
        {
            PlayerPrefs.SetString("didTutorial", "nope");
            PlayerPrefs.SetString("tutorial", "nope");
            Hashtable hasht = new Hashtable();
            hasht.Add("didTutorial", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hasht, null, null);
            PlayerPrefs.Save();
        }

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

        public static void FPS(int aa) => Application.targetFrameRate = aa;

        public static void NoTagFreeze()
        {
            GorillaTagger.Instance.statusEndTime = 0f;
            GorillaTagger.Instance.currentStatus = GorillaTagger.StatusEffect.None;
            GTPlayer.Instance.disableMovement = false;
        }

        private static float tagAuraCooldown;
        public static void TagAura(float radius = 3.5f)
        {
            if (!NetworkSystem.Instance.InRoom || VRRig.LocalRig == null) return;
            if (!VRRig.LocalRig.mainSkin.material.name.Contains("fected")) return;
            if (Time.time < tagAuraCooldown) return;

            Vector3 localHead = GorillaTagger.Instance.headCollider.transform.position;
            foreach (VRRig targetRig in VRRigCache.ActiveRigs)
            {
                if (targetRig == null || targetRig.isOfflineVRRig || targetRig == VRRig.LocalRig) continue;

                if (!targetRig.mainSkin.material.name.Contains("fected") && targetRig.Creator != null)
                {
                    Vector3 targetHead = targetRig.headConstraint != null ? targetRig.headConstraint.position : targetRig.transform.position;
                    if (Vector3.Distance(localHead, targetHead) <= radius)
                    {
                        tagAuraCooldown = Time.time + 0.35f;
                        TagPlayer(targetRig);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Movement
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

        private static GameObject PlatR, PlatL = null;
        private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);

        private static bool teleportGunPressed;

        public static GameObject checkpoint;
        private static bool teleporting;
        private static float teleportTime;

        private static bool dragging;
        private static float yaw, pitch, anchorX, anchorY;
        private const float sensitivity = 360f * 1.33f;
        private const float speed = 9f;

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

        public static void PlatColorChange()
        {
            Platcolor = (Platcolor + 1) % PlatColors.Length;
            Main.GetIndex("Change Plat Color").overlapText = "Plat Color: " + ColorNames[Platcolor];
            PlatColor = PlatColors[Platcolor];
        }

        public static void Noclip()
        {
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();
            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !(InputHandler.Instance.RightTrigger.IsPressed);
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

        public static void ChangePullMode()
        {
            pullmodeIndex = (pullmodeIndex + 1) % pullmodes.Length;

            switch (pullmodeIndex)
            {
                case 0:
                    PullPower = 0.025f;
                    UpHillPower = 0.02f;
                    break;

                case 1:
                    PullPower = 0.07f;
                    UpHillPower = 0.065f;
                    break;

                case 2:
                    PullPower = 0.001f;
                    UpHillPower = 0.001f;
                    break;
            }

            Main.GetIndex("pullmode").overlapText = "Pull Mode: " + pullmodes[pullmodeIndex];
        }

        public static void GravityManager(Gravitytypes type)
        {
            switch (type)
            {
                case Gravitytypes.Low:
                    GorillaTagger.Instance.rigidbody.AddForce(Vector3.up * 6.57f, ForceMode.Acceleration);
                    break;
                case Gravitytypes.High:
                    GorillaTagger.Instance.rigidbody.AddForce(Vector3.down * 7.67f, ForceMode.Acceleration);
                    break;
                case Gravitytypes.Zero:
                    GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);
                    break;
                case Gravitytypes.Reverse:
                    GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity * 3f, ForceMode.Acceleration);
                    GTPlayer.Instance.GetControllerTransform(false).parent.rotation = Quaternion.Euler(180f, 0f, 0f);
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

        public static void CheckPoint()
        {
            if (InputHandler.Instance.RightGrip.IsPressed)
            {
                if (checkpoint == null)
                {
                    checkpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    Object.Destroy(checkpoint.GetComponent<Rigidbody>());
                    Object.Destroy(checkpoint.GetComponent<SphereCollider>());

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

                CXS.CXS.TeleportPlayer(checkpoint.transform.position);

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
                Object.Destroy(checkpoint);
                checkpoint = null;
            }
        }

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

        public static void SlideControl(float control) => GTPlayer.Instance.slideControl = control;

        private static GameObject hookRightObj, hookLeftObj;
        private static LineRenderer hookRightLine, hookLeftLine;
        private static Vector3 rightHookPoint, leftHookPoint;
        private static bool isRightHooked, isLeftHooked;

        public static void GrapplingHook()
        {
            HandleHookHand(true);
            HandleHookHand(false);
        }

        private static void HandleHookHand(bool isRight)
        {
            bool vr = GunLib.IsXRDeviceActive();
            bool pull = vr
                ? (isRight ? InputHandler.Instance.RightTrigger.IsPressed : InputHandler.Instance.LeftTrigger.IsPressed)
                : (isRight ? (Mouse.current?.rightButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.E) : (Mouse.current?.leftButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.Q));

            Transform hand = isRight ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
            ref GameObject hookObj = ref isRight ? ref hookRightObj : ref hookLeftObj;
            ref LineRenderer hookLine = ref isRight ? ref hookRightLine : ref hookLeftLine;
            ref Vector3 hookPoint = ref isRight ? ref rightHookPoint : ref leftHookPoint;
            ref bool isHooked = ref isRight ? ref isRightHooked : ref isLeftHooked;

            if (pull)
            {
                if (!isHooked)
                {
                    Ray ray = vr
                        ? new Ray(hand.position, -hand.up)
                        : (Camera.main != null ? Camera.main.ScreenPointToRay(Mouse.current?.position.ReadValue() ?? Vector2.zero) : new Ray(hand.position, hand.forward));

                    if (Physics.Raycast(ray, out RaycastHit hit, 100f, GunLib.BypassLayers))
                    {
                        hookPoint = hit.point;
                        isHooked = true;
                    }
                }

                if (isHooked)
                {
                    Vector3 handPos = hand.position;
                    Vector3 pullDir = (hookPoint - handPos).normalized;
                    float dist = Vector3.Distance(handPos, hookPoint);

                    if (dist > 1.2f)
                    {
                        float force = Mathf.Clamp(dist * 2.5f, 18f, 45f);
                        GorillaTagger.Instance.rigidbody.AddForce(pullDir * force, ForceMode.Acceleration);
                    }

                    if (hookObj == null)
                    {
                        hookObj = new GameObject(isRight ? "HookRight" : "HookLeft");
                        hookLine = hookObj.AddComponent<LineRenderer>();
                        hookLine.startWidth = 0.015f;
                        hookLine.endWidth = 0.015f;
                        hookLine.positionCount = 2;
                        hookLine.useWorldSpace = true;
                        hookLine.material = new Material(Shader.Find("Sprites/Default"));
                        hookLine.startColor = Color.white;
                        hookLine.endColor = Color.white;
                    }

                    hookLine.SetPosition(0, handPos);
                    hookLine.SetPosition(1, hookPoint);
                }
            }
            else
            {
                isHooked = false;
                if (hookObj != null)
                {
                    Object.Destroy(hookObj);
                    hookObj = null;
                    hookLine = null;
                }
            }
        }

        public static void GrapplingHookDisable()
        {
            isRightHooked = false;
            isLeftHooked = false;
            if (hookRightObj != null)
            {
                Object.Destroy(hookRightObj);
                hookRightObj = null;
                hookRightLine = null;
            }
            if (hookLeftObj != null)
            {
                Object.Destroy(hookLeftObj);
                hookLeftObj = null;
                hookLeftLine = null;
            }
        }

        private static GameObject asVolume;

        public static void AirSwim()
        {
            if (asVolume == null)
            {
                var template = Object.FindFirstObjectByType<GorillaLocomotion.Swimming.WaterVolume>();
                if (template != null)
                {
                    asVolume = Object.Instantiate(template.gameObject);
                }
                else
                {
                    GameObject prefab = GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/ForestToBeach_Geo/CaveWaterVolume") ?? GameObject.Find("CaveWaterVolume");
                    if (prefab != null)
                    {
                        asVolume = Object.Instantiate(prefab);
                    }
                }

                if (asVolume != null)
                {
                    asVolume.name = "AirSwimWaterVolume";
                    asVolume.transform.localScale = new Vector3(6f, 6f, 6f);
                    foreach (var rend in asVolume.GetComponentsInChildren<Renderer>())
                    {
                        rend.enabled = false;
                    }
                }
            }

            if (asVolume != null)
            {
                asVolume.transform.position = GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 2.5f, 0f);
                if (GTPlayer.Instance.audioManager != null)
                {
                    GTPlayer.Instance.audioManager.UnsetMixerSnapshot();
                }
            }
        }

        public static void AirSwimDisable()
        {
            if (asVolume != null)
            {
                Object.Destroy(asVolume);
                asVolume = null;
            }
        }

        public static void ZiplineSpeed(float speed)
        {
            foreach (GorillaLocomotion.Gameplay.GorillaZipline zip in Object.FindObjectsByType<GorillaLocomotion.Gameplay.GorillaZipline>(FindObjectsSortMode.None))
            {
                if (zip.settings != null)
                {
                    zip.settings.maxSpeed = speed;
                    zip.settings.gravityMulti = speed > 10f ? 3f : 1.1f;
                    zip.settings.friction = speed > 10f ? 0.05f : 0.25f;
                }
            }
        }

        public static void Catapult(float power = 40f)
        {
            GunLib.StartGun(() =>
            {
                Vector3 target = GunLib.GetPointerPos();
                if (target != Vector3.zero)
                {
                    Vector3 handPos = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 launchDir = (target - handPos).normalized;
                    GorillaTagger.Instance.rigidbody.linearVelocity = launchDir * power;
                }
            }, false);
        }

        public static void StickyHands()
        {
            bool leftGrip = InputHandler.Instance.LeftGrip.IsPressed;
            bool rightGrip = InputHandler.Instance.RightGrip.IsPressed;

            if (leftGrip || rightGrip)
            {
                bool leftTouching = Physics.Raycast(GorillaTagger.Instance.leftHandTransform.position, -GorillaTagger.Instance.leftHandTransform.up, 0.25f, GunLib.BypassLayers);
                bool rightTouching = Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, 0.25f, GunLib.BypassLayers);

                if ((leftGrip && leftTouching) || (rightGrip && rightTouching))
                {
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                    GorillaTagger.Instance.rigidbody.useGravity = false;
                    return;
                }
            }
            GorillaTagger.Instance.rigidbody.useGravity = true;
        }

        public static void ResetStickyHands() => GorillaTagger.Instance.rigidbody.useGravity = true;

        private static readonly List<GameObject> modifiedWaterVolumes = new List<GameObject>();
        public static void JesusMonke()
        {
            int defaultLayer = LayerMask.NameToLayer("Default");
            var volumes = Object.FindObjectsByType<GorillaLocomotion.Swimming.WaterVolume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                var volume = volumes[i];
                if (volume != null && volume.gameObject.layer != defaultLayer)
                {
                    volume.gameObject.layer = defaultLayer;
                    if (!modifiedWaterVolumes.Contains(volume.gameObject))
                    {
                        modifiedWaterVolumes.Add(volume.gameObject);
                    }
                }
            }
        }

        public static void JesusMonkeDisable()
        {
            int waterLayer = LayerMask.NameToLayer("Water");
            for (int i = 0; i < modifiedWaterVolumes.Count; i++)
            {
                var obj = modifiedWaterVolumes[i];
                if (obj != null)
                {
                    obj.layer = waterLayer;
                }
            }
            modifiedWaterVolumes.Clear();
        }

        private static VRRig piggybackTarget;

        public static void PiggyBack()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig)
                {
                    piggybackTarget = GunLib.LockedPlayer;
                }
            }, true);

            if (piggybackTarget == null || piggybackTarget.isOfflineVRRig || !piggybackTarget.gameObject.activeInHierarchy)
            {
                piggybackTarget = RigManager.GetClosestVRRig();
            }

            if (piggybackTarget != null && !piggybackTarget.isOfflineVRRig)
            {
                Vector3 ridePosition = piggybackTarget.transform.position + Vector3.up * 0.65f - piggybackTarget.transform.forward * 0.25f;
                GorillaLocomotion.GTPlayer.Instance.transform.position = ridePosition;
                GorillaTagger.Instance.transform.position = ridePosition;
                GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void PiggyBackDisable()
        {
            piggybackTarget = null;
            GunLib.CleanupPointer();
        }

        private static VRRig followPlayerTarget;

        public static void FollowPlayer()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig)
                {
                    followPlayerTarget = GunLib.LockedPlayer;
                }
            }, true);

            if (followPlayerTarget == null || followPlayerTarget.isOfflineVRRig || !followPlayerTarget.gameObject.activeInHierarchy)
            {
                followPlayerTarget = RigManager.GetClosestVRRig();
            }

            if (followPlayerTarget != null && !followPlayerTarget.isOfflineVRRig)
            {
                Vector3 behindOffset = -followPlayerTarget.transform.forward * 1.5f + Vector3.up * 0.1f;
                Vector3 targetPosition = followPlayerTarget.transform.position + behindOffset;

                float distance = Vector3.Distance(GorillaLocomotion.GTPlayer.Instance.transform.position, targetPosition);
                float followSpeed = Mathf.Max(12f, distance * 5f);

                GorillaLocomotion.GTPlayer.Instance.transform.position = Vector3.MoveTowards(
                    GorillaLocomotion.GTPlayer.Instance.transform.position,
                    targetPosition,
                    followSpeed * Time.deltaTime
                );
                GorillaTagger.Instance.transform.position = GorillaLocomotion.GTPlayer.Instance.transform.position;
                GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void FollowPlayerDisable()
        {
            followPlayerTarget = null;
            GunLib.CleanupPointer();
        }

        private sealed class ThrownEnderPearl
        {
            public GameObject VisualObject;
            public Vector3 Position;
            public Vector3 Velocity;
            public float ElapsedTime;
        }

        private static readonly List<ThrownEnderPearl> activeEnderPearls = new List<ThrownEnderPearl>();
        private static GameObject leftHeldPearlVisual;
        private static GameObject rightHeldPearlVisual;
        private static bool isHoldingLeftPearl;
        private static bool isHoldingRightPearl;

        public static void EnderPearl()
        {
            Camera mainCamera = Camera.main != null ? Camera.main : GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
            Quaternion cameraRotation = mainCamera != null ? mainCamera.transform.rotation : Quaternion.identity;

            bool isVr = GunLib.IsXRDeviceActive();

            bool leftGripPressed = isVr
                ? InputHandler.Instance.LeftGrip.IsPressed
                : UnityInput.Current.GetKey(KeyCode.Q);

            bool rightGripPressed = isVr
                ? InputHandler.Instance.RightGrip.IsPressed
                : (Mouse.current?.rightButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.E);

            if (leftGripPressed)
            {
                Vector3 leftHandPosition = GorillaTagger.Instance.leftHandTransform.position;
                if (leftHeldPearlVisual == null)
                {
                    leftHeldPearlVisual = ModsLib.CreatePearlVisual("LeftHeldPearl", leftHandPosition);
                }

                leftHeldPearlVisual.transform.position = leftHandPosition;
                leftHeldPearlVisual.transform.rotation = cameraRotation;
                isHoldingLeftPearl = true;
            }
            else if (isHoldingLeftPearl)
            {
                isHoldingLeftPearl = false;
                if (leftHeldPearlVisual != null)
                {
                    Object.Destroy(leftHeldPearlVisual);
                    leftHeldPearlVisual = null;
                }

                Vector3 leftHandPosition = GorillaTagger.Instance.leftHandTransform.position;
                Vector3 throwVelocity = ModsLib.GetHandThrowVelocity(true);

                GameObject pearlObject = ModsLib.CreatePearlVisual("ThrownEnderPearl", leftHandPosition);
                activeEnderPearls.Add(new ThrownEnderPearl
                {
                    VisualObject = pearlObject,
                    Position = leftHandPosition,
                    Velocity = throwVelocity,
                    ElapsedTime = 0f
                });
            }

            if (rightGripPressed)
            {
                Vector3 rightHandPosition = GorillaTagger.Instance.rightHandTransform.position;
                if (rightHeldPearlVisual == null)
                {
                    rightHeldPearlVisual = ModsLib.CreatePearlVisual("RightHeldPearl", rightHandPosition);
                }

                rightHeldPearlVisual.transform.position = rightHandPosition;
                rightHeldPearlVisual.transform.rotation = cameraRotation;
                isHoldingRightPearl = true;
            }
            else if (isHoldingRightPearl)
            {
                isHoldingRightPearl = false;
                if (rightHeldPearlVisual != null)
                {
                    Object.Destroy(rightHeldPearlVisual);
                    rightHeldPearlVisual = null;
                }

                Vector3 rightHandPosition = GorillaTagger.Instance.rightHandTransform.position;
                Vector3 throwVelocity = ModsLib.GetHandThrowVelocity(false);

                GameObject pearlObject = ModsLib.CreatePearlVisual("ThrownEnderPearl", rightHandPosition);
                activeEnderPearls.Add(new ThrownEnderPearl
                {
                    VisualObject = pearlObject,
                    Position = rightHandPosition,
                    Velocity = throwVelocity,
                    ElapsedTime = 0f
                });
            }

            for (int i = activeEnderPearls.Count - 1; i >= 0; i--)
            {
                ThrownEnderPearl pearl = activeEnderPearls[i];
                pearl.Velocity += Physics.gravity * Time.deltaTime;
                Vector3 displacement = pearl.Velocity * Time.deltaTime;
                float stepDistance = displacement.magnitude;

                if (stepDistance > 0.0001f && Physics.Raycast(pearl.Position, pearl.Velocity.normalized, out RaycastHit hit, stepDistance, GunLib.BypassLayers))
                {
                    Vector3 teleportTarget = hit.point + hit.normal * 0.25f;
                    GTPlayer.Instance.transform.position = teleportTarget;
                    GorillaTagger.Instance.transform.position = teleportTarget;
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;

                    if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { teleportTarget, Quaternion.identity, 3f, 80f, false, true });
                        RPCProt();
                    }

                    if (pearl.VisualObject != null)
                    {
                        Object.Destroy(pearl.VisualObject);
                    }

                    activeEnderPearls.RemoveAt(i);
                }
                else
                {
                    pearl.Position += displacement;
                    pearl.ElapsedTime += Time.deltaTime;

                    if (pearl.VisualObject != null)
                    {
                        pearl.VisualObject.transform.position = pearl.Position;
                        pearl.VisualObject.transform.rotation = cameraRotation;
                    }

                    if (pearl.ElapsedTime > 7f)
                    {
                        if (pearl.VisualObject != null)
                        {
                            Object.Destroy(pearl.VisualObject);
                        }

                        activeEnderPearls.RemoveAt(i);
                    }
                }
            }
        }

        public static void EnderPearlDisable()
        {
            isHoldingLeftPearl = false;
            isHoldingRightPearl = false;

            if (leftHeldPearlVisual != null)
            {
                Object.Destroy(leftHeldPearlVisual);
                leftHeldPearlVisual = null;
            }

            if (rightHeldPearlVisual != null)
            {
                Object.Destroy(rightHeldPearlVisual);
                rightHeldPearlVisual = null;
            }

            for (int i = 0; i < activeEnderPearls.Count; i++)
            {
                if (activeEnderPearls[i].VisualObject != null)
                {
                    Object.Destroy(activeEnderPearls[i].VisualObject);
                }
            }

            activeEnderPearls.Clear();
        }

        private static GameObject ziplineCableObject;
        private static LineRenderer ziplineLineRenderer;
        private static GameObject ziplineStartAnchor;
        private static GameObject ziplineEndAnchor;
        private static Vector3 ziplineStartPosition;
        private static Vector3 ziplineEndPosition;
        private static bool hasActiveZipline;
        private static bool isRidingZipline;
        private static bool wasZiplineShootPressed;
        private static float ziplineCooldown;

        public static void ZiplineGun()
        {
            bool isVr = GunLib.IsXRDeviceActive();
            bool isAimingGun = isVr
                ? InputHandler.Instance.RightGrip.IsPressed
                : (Mouse.current?.rightButton.isPressed ?? false);

            bool shootPressed = isVr
                ? InputHandler.Instance.RightTrigger.IsPressed
                : (Mouse.current?.leftButton.isPressed ?? false);

            GunLib.StartGun(() =>
            {
                if (shootPressed && !wasZiplineShootPressed)
                {
                    Vector3 pointerPosition = GunLib.GetPointerPos();
                    if (pointerPosition != Vector3.zero)
                    {
                        ziplineStartPosition = GorillaTagger.Instance.rightHandTransform.position;
                        ziplineEndPosition = pointerPosition;
                        hasActiveZipline = true;
                        ziplineCooldown = Time.time + 0.35f;

                        ModsLib.CreateZiplineVisual(
                            ziplineStartPosition,
                            ziplineEndPosition,
                            ref ziplineCableObject,
                            ref ziplineLineRenderer,
                            ref ziplineStartAnchor,
                            ref ziplineEndAnchor
                        );
                    }
                }
            }, false);

            wasZiplineShootPressed = shootPressed;

            if (!hasActiveZipline || Time.time < ziplineCooldown)
            {
                return;
            }

            Vector3 leftHandPosition = GorillaTagger.Instance.leftHandTransform.position;
            Vector3 rightHandPosition = GorillaTagger.Instance.rightHandTransform.position;

            Vector3 closestToLeft = ModsLib.CalculateClosestPointOnSegment(ziplineStartPosition, ziplineEndPosition, leftHandPosition, out _);
            Vector3 closestToRight = ModsLib.CalculateClosestPointOnSegment(ziplineStartPosition, ziplineEndPosition, rightHandPosition, out _);

            float distanceToLeft = Vector3.Distance(leftHandPosition, closestToLeft);
            float distanceToRight = Vector3.Distance(rightHandPosition, closestToRight);

            bool leftGrabbing = (isVr ? InputHandler.Instance.LeftGrip.IsPressed : UnityInput.Current.GetKey(KeyCode.Q)) && distanceToLeft <= 0.45f;
            bool rightGrabbing = !isAimingGun && (isVr ? InputHandler.Instance.RightGrip.IsPressed : UnityInput.Current.GetKey(KeyCode.E)) && distanceToRight <= 0.45f;

            Vector3 ziplineDirection = (ziplineEndPosition - ziplineStartPosition).normalized;
            const float ziplineSpeed = 26f;

            if (leftGrabbing || rightGrabbing)
            {
                isRidingZipline = true;
                Vector3 playerBodyPosition = GTPlayer.Instance.transform.position;
                Vector3 segmentPoint = ModsLib.CalculateClosestPointOnSegment(ziplineStartPosition, ziplineEndPosition, playerBodyPosition, out float progress);

                if (progress >= 0.97f)
                {
                    isRidingZipline = false;
                    GorillaTagger.Instance.rigidbody.linearVelocity = ziplineDirection * ziplineSpeed;
                }
                else
                {
                    Vector3 advancedPosition = segmentPoint + ziplineDirection * (ziplineSpeed * Time.deltaTime);
                    GTPlayer.Instance.transform.position = advancedPosition;
                    GorillaTagger.Instance.transform.position = advancedPosition;
                    GorillaTagger.Instance.rigidbody.linearVelocity = ziplineDirection * ziplineSpeed;
                }
            }
            else if (isRidingZipline)
            {
                isRidingZipline = false;
                GorillaTagger.Instance.rigidbody.linearVelocity = ziplineDirection * ziplineSpeed;
            }
        }

        public static void ZiplineGunDisable()
        {
            hasActiveZipline = false;
            isRidingZipline = false;
            wasZiplineShootPressed = false;

            ModsLib.DestroyZiplineVisual(
                ref ziplineCableObject,
                ref ziplineLineRenderer,
                ref ziplineStartAnchor,
                ref ziplineEndAnchor
            );

            GunLib.CleanupPointer();
        }
        #endregion

        #region Visuals
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
                    Object.Destroy(l, Time.deltaTime);
                }
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
            if (!NetworkSystem.Instance.InRoom) return;

            Color c = strobe ? new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) : Color.HSVToRGB(Mathf.Repeat(Time.time * 0.2f, 1f), 1f, 1f);

            GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, c.r, c.g, c.b);
        }

        public static void SkeletonESP()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isOfflineVRRig)
                    continue;

                Color col = rig.playerColor;
                Vector3 head = rig.headConstraint != null ? rig.headConstraint.position : rig.transform.position + Vector3.up * 0.5f;
                Vector3 spine = rig.transform.position + Vector3.up * 0.1f;
                Vector3 leftHand = rig.leftHandTransform != null ? rig.leftHandTransform.position : spine;
                Vector3 rightHand = rig.rightHandTransform != null ? rig.rightHandTransform.position : spine;
                Vector3 basePos = rig.transform.position - Vector3.up * 0.2f;

                DrawLine(head, spine, col);
                DrawLine(spine, leftHand, col);
                DrawLine(spine, rightHand, col);
                DrawLine(spine, basePos, col);
            }
        }

        public static void BoxESP()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isOfflineVRRig)
                    continue;

                Color col = rig.playerColor;
                Vector3 center = rig.transform.position;
                Vector3 extents = new Vector3(0.35f, 0.45f, 0.35f);

                Vector3 c0 = center + new Vector3(-extents.x, -extents.y, -extents.z);
                Vector3 c1 = center + new Vector3(extents.x, -extents.y, -extents.z);
                Vector3 c2 = center + new Vector3(extents.x, -extents.y, extents.z);
                Vector3 c3 = center + new Vector3(-extents.x, -extents.y, extents.z);

                Vector3 c4 = center + new Vector3(-extents.x, extents.y, -extents.z);
                Vector3 c5 = center + new Vector3(extents.x, extents.y, -extents.z);
                Vector3 c6 = center + new Vector3(extents.x, extents.y, extents.z);
                Vector3 c7 = center + new Vector3(-extents.x, extents.y, extents.z);

                DrawLine(c0, c1, col);
                DrawLine(c1, c2, col);
                DrawLine(c2, c3, col);
                DrawLine(c3, c0, col);

                DrawLine(c4, c5, col);
                DrawLine(c5, c6, col);
                DrawLine(c6, c7, col);
                DrawLine(c7, c4, col);

                DrawLine(c0, c4, col);
                DrawLine(c1, c5, col);
                DrawLine(c2, c6, col);
                DrawLine(c3, c7, col);
            }
        }

        public static void TwoDBoxESP()
        {
            Camera cam = Camera.main != null ? Camera.main : GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isOfflineVRRig)
                    continue;

                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.Destroy(quad.GetComponent<Collider>());
                quad.name = "2DBoxESP";
                quad.transform.position = rig.transform.position;
                quad.transform.localScale = new Vector3(0.65f, 0.85f, 1f);
                if (cam != null)
                    quad.transform.rotation = cam.transform.rotation;

                Renderer rend = quad.GetComponent<Renderer>();
                rend.material.shader = Shader.Find("GUI/Text Shader");
                rend.material.color = new Color(rig.playerColor.r, rig.playerColor.g, rig.playerColor.b, 0.45f);
                Object.Destroy(quad, Time.deltaTime);
            }
        }

        public static string PlayerPlatform(Player p)
        {
            p.CustomProperties.TryGetValue("platform", out object v);
            if (v == null) { v = "Quest"; }
            return v.ToString();
        }

        public static void NameAndDistanceTags()
        {
            Camera cam = Camera.main != null ? Camera.main : GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isOfflineVRRig)
                    continue;

                string name = rig.Creator != null ? rig.Creator.NickName : rig.playerNameVisible;
                float dist = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, rig.transform.position);
                int fps = rig.fps;
                string platform = PlayerPlatform(RigManager.GetPlayerFromVRRig(rig));

                GameObject tagObj = new GameObject("NameTagESP");
                Vector3 headPos = (rig.headConstraint != null ? rig.headConstraint.position : rig.transform.position) + Vector3.up * 0.35f;
                tagObj.transform.position = headPos;
                if (cam != null)
                    tagObj.transform.LookAt(tagObj.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

                TextMesh tm = tagObj.AddComponent<TextMesh>();
                tm.text = $"{name} [{dist:F1}m] (FPS: {fps}) [Platform: {platform}]";
                tm.fontSize = 24;
                tm.characterSize = 0.02f;
                tm.alignment = TextAlignment.Center;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.color = rig.playerColor;
                Object.Destroy(tagObj, Time.deltaTime);
            }
        }

        private static void DrawLine(Vector3 start, Vector3 end, Color col)
        {
            GameObject obj = new GameObject("ESPLine");
            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.012f;
            lr.endWidth = 0.012f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.material.shader = Shader.Find("GUI/Text Shader");
            lr.startColor = col;
            lr.endColor = col;
            Object.Destroy(obj, Time.deltaTime);
        }

        private static int cursedIndex;
        private static readonly string[] cursedNames = { "1", "2", "3", "4", "Off" };

        public static void CursedGTAG()
        {
            cursedIndex = (cursedIndex + 1) % cursedNames.Length;
            Main.GetIndex("cursedgtag").overlapText = "Cursed Index: " + cursedNames[cursedIndex];

            if (BetterDayNightManager.instance == null)
                return;

            if (cursedIndex == 4)
            {
                BetterDayNightManager.instance.UnsetTimeIndexOverrideFunction();
            }
            else
            {
                int target = cursedIndex;
                BetterDayNightManager.instance.SetTimeIndexOverrideFunction(_ => target);
            }
            BetterDayNightManager.instance.UpdateTimeOfDay(true);
        }

        private static int timeOfDayIndex;
        private static readonly string[] timeOfDayNames = { "Morning", "Day", "Evening", "Night", "Default" };

        public static void TimeSwitcher()
        {
            timeOfDayIndex = (timeOfDayIndex + 1) % timeOfDayNames.Length;

            ButtonInfo timeBtn = Main.GetIndex("Time Switcher") ?? Main.GetIndex("Weather Switcher");
            if (timeBtn != null)
            {
                timeBtn.overlapText = "Time: " + timeOfDayNames[timeOfDayIndex];
            }

            if (BetterDayNightManager.instance == null)
            {
                return;
            }

            switch (timeOfDayIndex)
            {
                case 0:
                    BetterDayNightManager.instance.SetTimeOfDay(1, true);
                    break;
                case 1:
                    BetterDayNightManager.instance.SetTimeOfDay(3, true);
                    break;
                case 2:
                    BetterDayNightManager.instance.SetTimeOfDay(7, true);
                    break;
                case 3:
                    BetterDayNightManager.instance.SetTimeOfDay(0, true);
                    break;
                case 4:
                    BetterDayNightManager.instance.ClearTimeOfDay(true);
                    break;
            }

            BetterDayNightManager.instance.UpdateTimeOfDay(true);
        }

        public static void WeatherSwitcher() => TimeSwitcher();

        private static int weatherIndex;
        private static readonly string[] weatherNames = { "Rain", "Clear", "Default" };

        public static void CycleWeather()
        {
            weatherIndex = (weatherIndex + 1) % weatherNames.Length;
            Main.GetIndex("Weather Switcher").overlapText = "Weather: " + weatherNames[weatherIndex];

            if (BetterDayNightManager.instance == null)
                return;

            switch (weatherIndex)
            {
                case 0:
                    BetterDayNightManager.instance.SetFixedWeather(BetterDayNightManager.WeatherType.Raining, true);
                    break;
                case 1:
                    BetterDayNightManager.instance.SetFixedWeather(BetterDayNightManager.WeatherType.None, true);
                    break;
                case 2:
                    BetterDayNightManager.instance.ClearFixedWeather(true);
                    break;
            }
        }
        #endregion

        #region Overpowered
        public static float tagTimer;
        public static float CDown;

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

        public static void SendOPRaiseEvent202(VRRig p = null)
        {
            RaiseEventOptions o;
            if (p != null)
                o = new RaiseEventOptions { TargetActors = new int[] { p.Creator.ActorNumber } };
            else
                o = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new object[]
            {
                "ello"
            }, o, SendOptions.SendUnreliable);
            RPCProt();
        }
        #endregion

        #region Fun
        public static float delay;
        public static bool enablebracelet;
        static GameObject cat = null;

        public static void HoverboardSpam()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            if (InputHandler.Instance.RightGrip.IsPressed)
            {
                if (Time.time > delay + 0.3f)
                {
                    delay = Time.time;
                    FreeHoverboardManager.instance.SendDropBoardRPC(GorillaTagger.Instance.rightHandTransform.position, Quaternion.identity, GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f, false), GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f, false), Color.black);
                }
            }
        }

        public static void WaterSplash()
        {
            if (!NetworkSystem.Instance.InRoom) return;
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

        private static float splashGunDelay;

        public static void SplashGun()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            GunLib.StartGun(() =>
            {
                if (Time.time > splashGunDelay)
                {
                    splashGunDelay = Time.time + 0.15f;
                    Vector3 targetPos = GunLib.GetPointerPos();
                    if (targetPos != Vector3.zero)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { targetPos, Quaternion.identity, 4f, 100f, false, true });
                        RPCProt();
                    }
                }
            }, false);
        }

        private static float splashRightDelay;
        private static float splashLeftDelay;

        public static void SplashHands()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (InputHandler.Instance.RightGrip.IsPressed && Time.time > splashRightDelay)
            {
                splashRightDelay = Time.time + 0.15f;
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation, 4f, 100f, false, true });
                RPCProt();
            }

            if (InputHandler.Instance.LeftGrip.IsPressed && Time.time > splashLeftDelay)
            {
                splashLeftDelay = Time.time + 0.15f;
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.leftHandTransform.rotation, 4f, 100f, false, true });
                RPCProt();
            }
        }

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
            if (!NetworkSystem.Instance.InRoom) return;
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
                shiba.transform.Find("Handle1").GetComponent<Rigidbody>().linearVelocity = funn;

                shiba.gameObject.layer = 8;
                shiba.transform.Find("Handle1").gameObject.layer = 8;
                shiba.transform.Find("Handle1").name = string.Concat(shiba.name, "MonoObject");

                Object.Destroy(shiba, 15f);
            }, false);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GorillaLocomotion.GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GorillaLocomotion.GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }
        #endregion

        #region Gun Settings
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
                num = 8f;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Normal";
            }
        }
        #endregion

        #region Projectiles
        public class ProjectileEntry
        {
            public string Name;
            public SnowballThrowable ThrowableLeft;
            public SnowballThrowable ThrowableRight;
            public SnowballThrowable Throwable => ThrowableRight;
            public int ThrowableIndex => Throwable != null ? Throwable.throwableMakerIndex : -1;
        }

        public enum ThrowableHand
        {
            Left,
            Right,
            Both,
            Dynamic
        }

        public static bool biig;
        private static ProjectileEntry _snowballEntry;
        private static bool _isInitializing;
        private static float spamDihlay;

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

        public static void UpdateNetworkedProjectile(int index, ThrowableHand hand)
        {
            if (hand == ThrowableHand.Left || hand == ThrowableHand.Both)
                VRRig.LocalRig.LeftThrowableProjectileIndex = index;
            if (hand == ThrowableHand.Right || hand == ThrowableHand.Both)
                VRRig.LocalRig.RightThrowableProjectileIndex = index;
            VRRig.LocalRig.myBodyDockPositions.RefreshTransferrableItems();
        }

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

        public static void SnowballSpam(Vector3 velocity, Vector3 woah)
        {
            if (!(Time.time > spamDihlay)) return;

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
        #endregion

        #region Room
        public static string lastmap;
        private static float actionDelay;

        public static void BDisconnect()
        {
            if (InputHandler.Instance.RightSecondary.IsPressed)
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
                PhotonNetwork.Disconnect();
            }
        }

        public static void Joincodegenesis()
        {
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("GENESIS", GorillaNetworking.JoinType.Solo);
        }

        public static void JoinRandom()
        {
            if (PhotonNetworkController.Instance.currentJoinTrigger.networkZone != null)
            {
                lastmap = PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
            }
            if (!NetworkSystem.Instance.InRoom)
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(lastmap), GorillaNetworking.JoinType.Solo);
            }
        }

        public static void ConnectToRegion(string region)
        {
            if (PhotonNetwork.CloudRegion != region) PhotonNetwork.ConnectToRegion(region);
            NetworkSystem.Instance.currentRegionIndex = Array.IndexOf(NetworkSystem.Instance.regionNames, region);
        }

        public static void RPCProt()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            try
            {
                if (MonkeAgent.instance != null)
                {
                    MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                    MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                    MonkeAgent.instance.logErrorMax = int.MaxValue;
                    MonkeAgent.instance.userDecayTime = 0f;
                    MonkeAgent.instance.reportedPlayers.Clear();
                    MonkeAgent.instance.userRPCCalls.Clear();

                    Application.logMessageReceived -= MonkeAgent.instance.LogErrorCount;
                    GorillaSlicerSimpleManager.UnregisterSliceable(MonkeAgent.instance, GorillaSlicerSimpleManager.UpdateStep.Update);
                }

                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;
            }
            catch { }
        }

        public static void lbaction(GorillaPlayerLineButton.ButtonType type, NetPlayer player = null, bool? state = null)
        {
            if (type == GorillaPlayerLineButton.ButtonType.Mute)
            {
                foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (player == null ? (state == true ? !line.muteButton.isAutoOn : line.muteButton.isAutoOn) : line.linePlayer == player)
                    {
                        bool on = state ?? !line.muteButton.isOn;
                        line.muteButton.isOn = on;
                        line.PressButton(on, GorillaPlayerLineButton.ButtonType.Mute);
                        if (player != null) break;
                    }
                }
            }
            else
            {
                if (player != null)
                    GorillaPlayerScoreboardLine.ReportPlayer(player.UserId, type, player.NickName);
                else
                    foreach (var p in NetworkSystem.Instance.PlayerListOthers)
                        GorillaPlayerScoreboardLine.ReportPlayer(p.UserId, type, p.NickName);
            }
        }

        public static void MuteGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig && Time.time > actionDelay)
                {
                    lbaction(GorillaPlayerLineButton.ButtonType.Mute, GunLib.LockedPlayer.Creator);
                    actionDelay = Time.time + 0.5f;
                }
            }, true);
        }

        public static void MuteAll() => lbaction(GorillaPlayerLineButton.ButtonType.Mute, state: true);
        public static void UnmuteAll() => lbaction(GorillaPlayerLineButton.ButtonType.Mute, state: false);

        private static Recorder GetActiveRecorder()
        {
            if (NetworkSystem.Instance?.LocalRecorder != null)
            {
                return NetworkSystem.Instance.LocalRecorder;
            }

            if (NetworkSystem.Instance?.VoiceConnection?.PrimaryRecorder != null)
            {
                return NetworkSystem.Instance.VoiceConnection.PrimaryRecorder;
            }

            if (GorillaTagger.Instance?.myRecorder != null)
            {
                return GorillaTagger.Instance.myRecorder;
            }

            return Object.FindFirstObjectByType<GTRecorder>() ?? (Recorder)Object.FindFirstObjectByType<Recorder>();
        }

        public static void LoudMicrophone(float volumeMultiplier = 15f)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null)
            {
                return;
            }

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = true;
                gtRecorder.VolumeAdjustment = volumeMultiplier;
            }

            recorder.VoiceDetection = false;
            recorder.TransmitEnabled = true;
        }

        public static void ResetMicrophoneVolume()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null)
            {
                return;
            }

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = false;
                gtRecorder.VolumeAdjustment = 1f;
            }

            recorder.VoiceDetection = true;
            recorder.VoiceDetectionThreshold = 0.07f;
        }

        public static void MuteMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null)
            {
                return;
            }

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = true;
                gtRecorder.VolumeAdjustment = 0f;
            }

            recorder.TransmitEnabled = false;
            recorder.VoiceDetectionThreshold = 1f;

            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.shouldSendSpeakingLoudness = false;
            }
        }

        public static void UnmuteMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null)
            {
                return;
            }

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = false;
                gtRecorder.VolumeAdjustment = 1f;
            }

            recorder.TransmitEnabled = true;
            recorder.VoiceDetectionThreshold = 0.07f;

            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.shouldSendSpeakingLoudness = true;
            }
        }

        public static bool microphoneEchoForOthers;
        public static float echoDelaySeconds = 0.25f;
        public static float echoDecayFactor = 0.55f;

        public static void MicrophoneEcho(bool enableEcho = true)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            microphoneEchoForOthers = enableEcho;
        }

        public static void HearSelf(bool enable = true)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null)
            {
                return;
            }

            recorder.DebugEchoMode = enable;

            if (enable && !recorder.TransmitEnabled)
            {
                recorder.TransmitEnabled = true;
            }
        }

        public static void SetMicrophonePitch(float pitch)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowPitchAdjustment = true;
                gtRecorder.PitchAdjustment = pitch;
            }
        }

        public static void ResetMicrophonePitch()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowPitchAdjustment = false;
                gtRecorder.PitchAdjustment = 1f;
            }
        }

        public static void FixMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            microphoneEchoForOthers = false;

            Recorder recorder = GetActiveRecorder();
            if (recorder != null)
            {
                recorder.SourceType = Recorder.InputSourceType.Microphone;
                recorder.AudioClip = null;
                recorder.DebugEchoMode = false;
                recorder.TransmitEnabled = true;
                recorder.VoiceDetection = true;
                recorder.VoiceDetectionThreshold = 0.07f;
                recorder.VoiceDetectionDelayMs = 500;
                recorder.RecordOnlyWhenJoined = true;
                recorder.StopRecordingWhenPaused = false;

                if (recorder is GTRecorder gtRecorder)
                {
                    gtRecorder.AllowVolumeAdjustment = false;
                    gtRecorder.VolumeAdjustment = 1f;
                    gtRecorder.AllowPitchAdjustment = false;
                    gtRecorder.PitchAdjustment = 1f;
                }

                recorder.RestartRecording(true);
            }

            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.remoteUseReplacementVoice = false;
                GorillaTagger.Instance.offlineVRRig.localUseReplacementVoice = false;
                GorillaTagger.Instance.offlineVRRig.shouldSendSpeakingLoudness = true;
            }

            if (GorillaComputer.instance != null)
            {
                GorillaComputer.instance.voiceChatOn = "TRUE";
            }
        }

        public static void ReportGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig && Time.time > actionDelay)
                {
                    lbaction(GorillaPlayerLineButton.ButtonType.Cheating, GunLib.LockedPlayer.Creator);
                    actionDelay = Time.time + 0.3f;
                }
            }, true);
        }

        public static void ReportAll() => lbaction(GorillaPlayerLineButton.ButtonType.Cheating);

        public static void CopyPlayerIdentity()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig)
                {
                    string targetName = GunLib.LockedPlayer.Creator != null ? GunLib.LockedPlayer.Creator.NickName : GunLib.LockedPlayer.playerNameVisible;
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        NetworkSystem.Instance.SetMyNickName(targetName);
                        GorillaComputer.instance.currentName = targetName;
                        GorillaComputer.instance.savedName = targetName;
                        GorillaTagger.Instance.offlineVRRig.SetNameTagText(targetName);
                        PhotonNetwork.LocalPlayer.NickName = targetName;
                        PlayerPrefs.SetString("playerName", targetName);
                    }

                    Color targetColor = GunLib.LockedPlayer.playerColor;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, targetColor.r, targetColor.g, targetColor.b);
                    PlayerPrefs.SetFloat("redValue", targetColor.r);
                    PlayerPrefs.SetFloat("greenValue", targetColor.g);
                    PlayerPrefs.SetFloat("blueValue", targetColor.b);
                    PlayerPrefs.Save();
                    VRRig.LocalRig.SetColor(targetColor);
                    GorillaTagger.Instance.offlineVRRig.SetColor(targetColor);
                }
            }, true);
        }

        public static void LobbyHop()
        {
            if (PhotonNetworkController.Instance.currentJoinTrigger?.networkZone != null)
                lastmap = PhotonNetworkController.Instance.currentJoinTrigger.networkZone;

            PhotonNetwork.Disconnect();
            NetworkSystem.Instance.ReturnToSinglePlayer();
            GorillaTagger.Instance.StartCoroutine(LobbyHopRoutine());
        }

        private static IEnumerator LobbyHopRoutine()
        {
            while (NetworkSystem.Instance.InRoom || NetworkSystem.Instance.InRoom)
                yield return null;

            yield return new WaitForSeconds(0.2f);
            PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(lastmap ?? "forest"), GorillaNetworking.JoinType.Solo);
        }
        #endregion

        #region Rig
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

        public static void FixHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
        }

        public static void HeadSpinner(float speed = 360f)
        {
            VRRig.LocalRig.head.trackingRotationOffset.y += Time.deltaTime * speed;
        }

        public static void HelicopterMonkey(float speed = 720f)
        {
            VRRig.LocalRig.head.trackingRotationOffset.y += Time.deltaTime * speed;
            GorillaTagger.Instance.offlineVRRig.transform.Rotate(0f, Time.deltaTime * speed, 0f);
        }

        private static int faceExpressionIndex;
        private static readonly string[] faceExpressionNames = { "Default", "Surprised", "Closed", "Derp", "Wink" };
        private static readonly Vector4[] faceExpressionUVs =
        {
            new Vector4(0.5f, 1f, 0f, 0f),
            new Vector4(0.5f, 1f, 0.8f, 0f),
            new Vector4(0.5f, 1f, 0.6f, 0f),
            new Vector4(0.5f, 1f, 0.4f, 0f),
            new Vector4(0.5f, 1f, 0.2f, 0f)
        };

        public static void CycleFaceExpression()
        {
            faceExpressionIndex = (faceExpressionIndex + 1) % faceExpressionNames.Length;
            Main.GetIndex("Face Expression").overlapText = "Face: " + faceExpressionNames[faceExpressionIndex];

            VRRig rig = VRRig.LocalRig ?? GorillaTagger.Instance.offlineVRRig;
            if (rig == null) return;

            GorillaEyeExpressions eyes = rig.GetComponent<GorillaEyeExpressions>();
            if (eyes != null && eyes.targetFace != null)
            {
                Renderer renderer = eyes.targetFace.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetVector("_BaseMap_ST", faceExpressionUVs[faceExpressionIndex]);
                }
            }
        }

        public static void TPose()
        {
            VRRig rig = GorillaTagger.Instance.offlineVRRig;
            if (rig == null)
            {
                return;
            }

            Transform headTransform = rig.head != null && rig.head.rigTarget != null ? rig.head.rigTarget : rig.transform;
            if (rig.leftHand != null && rig.leftHand.rigTarget != null)
            {
                rig.leftHand.rigTarget.position = headTransform.position - headTransform.right * 0.65f;
                rig.leftHand.rigTarget.rotation = Quaternion.LookRotation(headTransform.forward, -headTransform.right);
            }

            if (rig.rightHand != null && rig.rightHand.rigTarget != null)
            {
                rig.rightHand.rigTarget.position = headTransform.position + headTransform.right * 0.65f;
                rig.rightHand.rigTarget.rotation = Quaternion.LookRotation(headTransform.forward, headTransform.right);
            }
        }
        #endregion

        #region Misc
        public static void placeholder()
        {
        }
        #endregion
    }
}