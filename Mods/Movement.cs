using System;
using BepInEx;
using GorillaLocomotion;
using Photon.Pun;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
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
            if (InputHandler.Instance.RightGrip.IsPressed && PlatR == null)
            {
                PlatR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                PlatR.transform.localScale = scale;
                PlatR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                PlatR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                GameObject.Destroy(PlatR.GetComponent<Rigidbody>());
                PlatR.GetComponent<Renderer>().material.color = PlatColor;
                if (Invis) GameObject.Destroy(PlatR.GetComponent<Renderer>());
            }
            if (!InputHandler.Instance.RightGrip.IsPressed && PlatR != null)
            {
                GameObject.Destroy(PlatR);
                PlatR = null;
            }

            if (InputHandler.Instance.LeftGrip.IsPressed && PlatL == null)
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

        public static void Noclip() => Noclipistuff(!InputHandler.Instance.RightTrigger.IsPressed);

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

        public static void bypasstp(Vector3 position, bool tprig = false)
        {
            if (tprig)
            {
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null && VRRig.LocalRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = position;
                    if (GorillaTagger.Instance.offlineVRRig.rightHandTransform != null)
                        GorillaTagger.Instance.offlineVRRig.rightHandTransform.position = position;
                    if (GorillaTagger.Instance.offlineVRRig.leftHandTransform != null)
                        GorillaTagger.Instance.offlineVRRig.leftHandTransform.position = position;

                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = position;
                    if (VRRig.LocalRig.rightHandTransform != null)
                        VRRig.LocalRig.rightHandTransform.position = position;
                    if (VRRig.LocalRig.leftHandTransform != null)
                        VRRig.LocalRig.leftHandTransform.position = position;
                }
                return;
            }

            Noclipistuff(true);

            Vector3 headOffset = GorillaTagger.Instance.headCollider.transform.position - GTPlayer.Instance.transform.position;
            Vector3 targetPlayerPos = position - headOffset;

            GTPlayer.Instance.transform.position = targetPlayerPos;
            GorillaTagger.Instance.transform.position = targetPlayerPos;

            if (GTPlayer.Instance.playerRigidBody != null)
            {
                GTPlayer.Instance.playerRigidBody.position = targetPlayerPos;
                GTPlayer.Instance.playerRigidBody.linearVelocity = Vector3.zero;
            }

            if (GorillaTagger.Instance.rigidbody != null)
            {
                GorillaTagger.Instance.rigidbody.position = targetPlayerPos;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }

            GTPlayer.Instance.lastPosition = targetPlayerPos;
            GTPlayer.Instance.lastHeadPosition = position;
            GTPlayer.Instance.lastOpenHeadPosition = position;

            GTPlayer.Instance.ClearHandHolds();
            GTPlayer.Instance.leftHand.OnTeleport();
            GTPlayer.Instance.rightHand.OnTeleport();

            if (GorillaTagger.Instance.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.transform.position = targetPlayerPos;
                GorillaTagger.Instance.offlineVRRig.leftHandLink?.BreakLink();
                GorillaTagger.Instance.offlineVRRig.rightHandLink?.BreakLink();
            }

            if (VRRig.LocalRig != null)
            {
                VRRig.LocalRig.transform.position = targetPlayerPos;
            }

            Physics.SyncTransforms();
            GTPlayer.Instance.ForceRigidBodySync();

            Noclipistuff(false);
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
                collider.enabled = !b;
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
            bool pull = vr ? (isRight ? InputHandler.Instance.RightTrigger.IsPressed : InputHandler.Instance.LeftTrigger.IsPressed) : (isRight ? (Mouse.current?.rightButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.E) : (Mouse.current?.leftButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.Q));

            Transform hand = isRight ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
            ref GameObject hookObj = ref isRight ? ref hookRightObj : ref hookLeftObj;
            ref LineRenderer hookLine = ref isRight ? ref hookRightLine : ref hookLeftLine;
            ref Vector3 hookPoint = ref isRight ? ref rightHookPoint : ref leftHookPoint;
            ref bool isHooked = ref isRight ? ref isRightHooked : ref isLeftHooked;

            if (pull)
            {
                if (!isHooked)
                {
                    Ray ray = vr ? new Ray(hand.position, -hand.up) : (Camera.main != null ? Camera.main.ScreenPointToRay(Mouse.current?.position.ReadValue() ?? Vector2.zero) : new Ray(hand.position, hand.forward));

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
                //if ()
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

                GorillaLocomotion.GTPlayer.Instance.transform.position = Vector3.MoveTowards(GorillaLocomotion.GTPlayer.Instance.transform.position, targetPosition, followSpeed * Time.deltaTime);
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

            bool leftGripPressed = isVr ? InputHandler.Instance.LeftGrip.IsPressed : UnityInput.Current.GetKey(KeyCode.Q);
            bool rightGripPressed = isVr ? InputHandler.Instance.RightGrip.IsPressed : (Mouse.current?.rightButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.E);

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
            bool isAimingGun = isVr ? InputHandler.Instance.RightGrip.IsPressed : (Mouse.current?.rightButton.isPressed ?? false);
            bool shootPressed = isVr ? InputHandler.Instance.RightTrigger.IsPressed : (Mouse.current?.leftButton.isPressed ?? false);

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

                        ModsLib.CreateZiplineVisual(ziplineStartPosition, ziplineEndPosition, ref ziplineCableObject, ref ziplineLineRenderer, ref ziplineStartAnchor, ref ziplineEndAnchor);
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

            ModsLib.DestroyZiplineVisual(ref ziplineCableObject, ref ziplineLineRenderer, ref ziplineStartAnchor, ref ziplineEndAnchor);

            GunLib.CleanupPointer();
        }

        private static GorillaSurfaceOverride[] _cachedg;
        private static float _cexpiry = -1f;
        private static readonly List<(GorillaSurfaceOverride surface, float multiplier)> _saved = new List<(GorillaSurfaceOverride surface, float multiplier)>();

        public static void SlipSlap()
        {
            if (Time.time > _cexpiry)
            {
                _cachedg = Object.FindObjectsByType<GorillaSurfaceOverride>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                _cexpiry = Time.time + 5f;
            }

            foreach (GorillaSurfaceOverride s in _cachedg)
            {
                float slide = s.slidePercentageOverride > 0f ? s.slidePercentageOverride : GTPlayer.Instance.materialData[s.overrideIndex].slidePercent;
                if (slide <= 0f)
                    continue;

                _saved.Add((s, s.extraVelMultiplier));
                s.extraVelMultiplier += slide;
            }
        }

        public static void UnSlipSlap()
        {
            foreach (var (surface, multiplier) in _saved)
                surface.extraVelMultiplier = multiplier;

            _saved.Clear();
        }
    }
}
