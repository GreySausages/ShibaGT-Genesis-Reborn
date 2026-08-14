using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace Plon.Libs
{
    public class GunLib : MonoBehaviour
    {
        public static VRRig LockedPlayer = null;
        public static GameObject pointer = null;
        public static bool grip_Button;
        public static bool trigger_Button;
        public static bool w_Button;
        public static bool a_Button;
        public static bool Lock = true;
        public static RaycastHit hit;

        public static int NoBarrier()
        {
            return ~((IEnumerable<string>)new string[]
            {
                "TransparentFX",
                "Ignore Raycast",
                "Zone",
                "Gorilla Trigger",
                "Gorilla Boundary",
                "GorillaCosmetics",
                "GorillaParticle"
            }).Select((Func<string, int>)LayerMask.NameToLayer).Aggregate(
                0,
                (int num, int l) => num | (1 << l)
            );
        }

        public static void Gunlib(bool LockOn)
        {
            if (InputHandler.Instance.RightGrip.IsPressed || Mouse.current.rightButton.isPressed)
            {
                RaycastHit hitInfo;

                Vector3 rayOrigin;
                Vector3 rayDirection;

                if (Mouse.current.rightButton.isPressed)
                {
                    Camera cam = GameObject.Find("Shoulder Camera")?.GetComponent<Camera>();

                    if (cam == null)
                        return;

                    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

                    rayOrigin = ray.origin;
                    rayDirection = ray.direction;
                }
                else
                {
                    rayOrigin = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
                    rayDirection = -GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.up;
                }

                bool didHit = Physics.Raycast(
                    rayOrigin,
                    rayDirection,
                    out hitInfo,
                    100f,
                    NoBarrier()
                );

                if (!didHit)
                {
                    hitInfo.point = rayOrigin + rayDirection * 100f;
                }

                if (LockOn)
                {
                    if (LockedPlayer == null && trigger_Button)
                    {
                        if (hitInfo.collider != null)
                        {
                            LockedPlayer = hitInfo.collider.GetComponentInParent<VRRig>();
                        }
                    }
                    else if (LockedPlayer != null && trigger_Button)
                    {
                        hitInfo.point = LockedPlayer.transform.position;
                    }
                    else if (LockedPlayer != null && !trigger_Button)
                    {
                        LockedPlayer = null;
                    }
                }
                else
                {
                    if (LockedPlayer == null && trigger_Button)
                    {
                        if (hitInfo.collider != null)
                        {
                            LockedPlayer = hitInfo.collider.GetComponentInParent<VRRig>();
                        }
                    }
                    else if (LockedPlayer != null)
                    {
                        if (hitInfo.collider == null ||
                            hitInfo.collider.GetComponentInParent<VRRig>() == null)
                        {
                            LockedPlayer = null;
                        }
                    }
                }

                if (pointer == null)
                {
                    pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());

                    pointer.GetComponent<Renderer>().material.color = Color.red;
                    pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

                    pointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                }

                pointer.transform.position = hitInfo.point;

                hit = hitInfo;

                bool shooting =
                    InputHandler.Instance.RightTrigger.IsPressed ||
                    Mouse.current.leftButton.isPressed;

                if (shooting)
                {
                    pointer.GetComponent<Renderer>().material.color = Color.green;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material.color = Color.red;
                }

                GameObject g = new GameObject("Line");
                LineRenderer l = g.AddComponent<LineRenderer>();

                l.startWidth = 0.01f;
                l.endWidth = 0.01f;
                l.positionCount = 2;
                l.useWorldSpace = true;

                l.SetPosition(
                    0,
                    GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position
                );

                l.SetPosition(1, hitInfo.point);

                l.material.shader = Shader.Find("GUI/Text Shader");

                if (shooting)
                {
                    l.startColor = Color.green;
                    l.endColor = Color.green;
                }
                else
                {
                    l.startColor = Color.red;
                    l.endColor = Color.red;
                }

                GameObject.Destroy(g, Time.deltaTime);
            }
            else
            {
                GameObject.Destroy(pointer);
                pointer = null;
            }
        }
    }
}