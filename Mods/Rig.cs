using GorillaLocomotion;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using System;
using UnityEngine;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
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
    }
}
