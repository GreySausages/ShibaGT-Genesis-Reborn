using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using Plon.Menu;
using static Plon.Libs.GunLib;

namespace Plon.Mods
{
    public static class Projectiles
    {
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
        public static void FlingGun()
        {
            Gunlib(Lock);
            if (trigger_Button && grip_Button && LockedPlayer != null)
            {
                if (!(Time.time > spamDihlay)) return;
                VRRig.LocalRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                VRRig.LocalRig.transform.position = LockedPlayer.transform.position;
                SnowballSpam1(-LockedPlayer.transform.up * 20f, LockedPlayer.transform.position - new Vector3(0f, -0.3f, 0f));
            }
            else if (!trigger_Button && grip_Button && LockedPlayer == null)
            {
                VRRig.LocalRig.enabled = true;
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }
    }
}