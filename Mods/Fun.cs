using GorillaLocomotion;
using Photon.Pun;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using UnityEngine;
using Object = UnityEngine.Object;
using CXS;
using System.Collections.Generic;
using GorillaTagScripts;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static float delay;
        public static bool enablebracelet;
        private static GameObject cat = null;

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
                        if ((GorillaTagger.Instance.bodyCollider.transform.position - targetPos).sqrMagnitude >= 8.5f)
                        {
                            bypasstp(targetPos, true);
                        }

                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.All, new object[] { targetPos, Quaternion.identity, 4f, 100f, false, true });

                        if (VRRig.LocalRig != null)
                            VRRig.LocalRig.enabled = true;
                        if (GorillaTagger.Instance.offlineVRRig != null)
                            GorillaTagger.Instance.offlineVRRig.enabled = true;

                        RPCProt();
                    }
                }
            }, false);
        }

        private static readonly List<Color> braceletColorsBuffer = new List<Color>(16);
        private static float dualSpamDelay;
        private static bool dualBraceletState;

        private static void SyncBraceletColors(List<Color> colors, bool isLeftHand = false)
        {
            if (colors == null || colors.Count == 0) return;

            if (GorillaTagScripts.FriendshipGroupDetection.Instance != null)
            {
                GorillaTagScripts.FriendshipGroupDetection.Instance.myBeadColors.Clear();
                GorillaTagScripts.FriendshipGroupDetection.Instance.myBeadColors.AddRange(colors);
            }

            VRRig[] rigs = { GorillaTagger.Instance.offlineVRRig, VRRig.LocalRig };
            foreach (var rig in rigs)
            {
                if (rig == null) continue;

                rig.partyMemberStatus = VRRig.PartyMemberStatus.InLocalParty;

                if (rig.reliableState != null)
                {
                    rig.reliableState.isBraceletLeftHanded = isLeftHand;
                    rig.reliableState.braceletSelfIndex = 0;
                    rig.reliableState.braceletBeadColors.Clear();
                    rig.reliableState.braceletBeadColors.AddRange(colors);
                    rig.reliableState.SetIsDirty();
                }

                if (rig.friendshipBraceletRightHand != null && rig.friendshipBraceletRightHand.gameObject.activeInHierarchy)
                    rig.friendshipBraceletRightHand.UpdateBeads(colors, 0);

                if (rig.friendshipBraceletLeftHand != null && rig.friendshipBraceletLeftHand.gameObject.activeInHierarchy)
                    rig.friendshipBraceletLeftHand.UpdateBeads(colors, 0);
            }

            if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
                GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, true, isLeftHand);
        }

        public static void GetBracelet(bool isLeftHand = false)
        {
            if (braceletColorsBuffer.Count == 0)
            {
                Color defaultColor = VRRig.LocalRig != null ? VRRig.LocalRig.playerColor : Color.white;
                for (int i = 0; i < 8; i++)
                    braceletColorsBuffer.Add(defaultColor);
            }

            SyncBraceletColors(braceletColorsBuffer, isLeftHand);

            VRRig rig = GorillaTagger.Instance.offlineVRRig ?? VRRig.LocalRig;
            if (rig != null)
            {
                if (isLeftHand && rig.nonCosmeticLeftHandItem != null)
                    rig.nonCosmeticLeftHandItem.EnableItem(true);
                else if (!isLeftHand && rig.nonCosmeticRightHandItem != null)
                    rig.nonCosmeticRightHandItem.EnableItem(true);
            }
        }

        public static void GetLeftBracelet() => GetBracelet(true);

        public static void GetDualBracelets()
        {
            GetBracelet(false);
            GetBracelet(true);
        }

        public static void RemoveBracelet()
        {
            if (GorillaTagScripts.FriendshipGroupDetection.Instance != null)
                GorillaTagScripts.FriendshipGroupDetection.Instance.myBeadColors.Clear();

            VRRig[] rigs = { GorillaTagger.Instance.offlineVRRig, VRRig.LocalRig };
            foreach (var rig in rigs)
            {
                if (rig == null) continue;

                rig.partyMemberStatus = VRRig.PartyMemberStatus.NotInLocalParty;

                if (rig.reliableState != null)
                {
                    rig.reliableState.braceletBeadColors.Clear();
                    rig.reliableState.SetIsDirty();
                }

                if (rig.nonCosmeticRightHandItem != null)
                    rig.nonCosmeticRightHandItem.EnableItem(false);
                if (rig.nonCosmeticLeftHandItem != null)
                    rig.nonCosmeticLeftHandItem.EnableItem(false);
            }

            if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, false, false);
                GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, false, true);
            }
        }

        public static void NoBracelet() => RemoveBracelet();

        public static void BraceletSpam()
        {
            if (Time.time > delay + 0.1f)
            {
                enablebracelet = !enablebracelet;
                if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
                    GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, enablebracelet, false);

                VRRig rig = GorillaTagger.Instance.offlineVRRig ?? VRRig.LocalRig;
                if (rig != null && rig.nonCosmeticRightHandItem != null)
                    rig.nonCosmeticRightHandItem.EnableItem(enablebracelet);

                delay = Time.time;
            }
        }

        public static void DualBraceletSpam()
        {
            if (Time.time > dualSpamDelay + 0.1f)
            {
                dualSpamDelay = Time.time;
                dualBraceletState = !dualBraceletState;

                if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, dualBraceletState, false);
                    GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, dualBraceletState, true);
                }

                VRRig rig = GorillaTagger.Instance.offlineVRRig ?? VRRig.LocalRig;
                if (rig != null)
                {
                    if (rig.nonCosmeticRightHandItem != null) rig.nonCosmeticRightHandItem.EnableItem(dualBraceletState);
                    if (rig.nonCosmeticLeftHandItem != null) rig.nonCosmeticLeftHandItem.EnableItem(dualBraceletState);
                }
            }
        }

        public static void RainbowBracelet()
        {
            braceletColorsBuffer.Clear();
            float time = Time.time * 2f;
            for (int i = 0; i < 8; i++)
            {
                float hue = (time + i * 0.125f) % 1f;
                braceletColorsBuffer.Add(Color.HSVToRGB(hue, 1f, 1f));
            }

            SyncBraceletColors(braceletColorsBuffer, false);
        }

        public static void CustomColorBracelet()
        {
            VRRig rig = GorillaTagger.Instance.offlineVRRig ?? VRRig.LocalRig;
            Color bodyColor = rig != null ? rig.playerColor : Color.white;

            braceletColorsBuffer.Clear();
            for (int i = 0; i < 8; i++)
                braceletColorsBuffer.Add(bodyColor);

            SyncBraceletColors(braceletColorsBuffer, false);
        }

        public static void GoldBracelet()
        {
            Color gold = new Color(1f, 0.84f, 0f);
            Color darkGold = new Color(0.85f, 0.65f, 0.12f);

            braceletColorsBuffer.Clear();
            for (int i = 0; i < 7; i++)
                braceletColorsBuffer.Add(i % 2 == 0 ? gold : darkGold);
            braceletColorsBuffer.Add(gold);

            SyncBraceletColors(braceletColorsBuffer, false);
        }

        public static void PartyWithRoom()
        {
            braceletColorsBuffer.Clear();
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig != null && !rig.isOfflineVRRig)
                    braceletColorsBuffer.Add(rig.playerColor);
            }

            if (braceletColorsBuffer.Count == 0)
                braceletColorsBuffer.Add(VRRig.LocalRig != null ? VRRig.LocalRig.playerColor : Color.white);

            SyncBraceletColors(braceletColorsBuffer, false);
        }

        public static void SoundSpammer(int id)
        {
            if (!NetworkSystem.Instance.InRoom) VRRig.LocalRig.PlayHandTapLocal(id, false, 999999f);
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
    }
}
