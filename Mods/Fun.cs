using GorillaLocomotion;
using Photon.Pun;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using UnityEngine;
using Object = UnityEngine.Object;

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
