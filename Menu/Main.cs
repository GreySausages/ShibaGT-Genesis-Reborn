using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Plon.Classes;
using Plon.Menu;
using Plon.Mods;
using Plon.Patches;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Plon.Libs.GunLib;
using static Plon.Menu.Buttons;
using static Plon.Menu.mods;
using static Plon.Settings;

namespace Plon.Menu
{
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class Main : MonoBehaviour
    {
        // Constant

        public static bool Loaded;
        public static void Prefix()
        {
            // Initialize Menu
            try
            {
                bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerPrimaryButton);
                bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);
                trigger_Button = Mouse.current.rightButton.isPressed ? Mouse.current.leftButton.isPressed : ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
                grip_Button = Mouse.current.rightButton.isPressed ? Mouse.current.leftButton.isPressed : ControllerInputPoller.instance.rightGrab;
                GameObject a = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText");
                GameObject b = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
                GameObject c = GameObject.Find("motdHeadingText");
                GameObject d = GameObject.Find("motdBodyText");

                if (a != null && d != null && b != null && c != null && d != null)
                {
                    a.GetComponent<TMP_Text>().text = $"<color=blue>ShibaGT Genesis Reborn v1</color>".ToUpper();
                    a.GetComponent<TMP_Text>().fontSize = 75f;
                    b.GetComponent<TMP_Text>().richText = true;
                    b.GetComponent<TMP_Text>().text = "\nWelcome To ShibaGT Genesis Reborn!\nThis is a Remake of the Longest Lasting Paid Mod Menu Shiba GT Genesis!".ToUpper();
                    c.GetComponent<TMP_Text>().text = "<color=blue>ShibaGT Genesis Reborn v1.0</color>".ToUpper();
                    d.GetComponent<TMP_Text>().text = "Creditz To ShibaGT/TAI for making the original menu!\nThis is just a remake!\n<color=red>We Are Not Responsible For Any Bans Using This Mod Menu!</color>".ToUpper();
                }
                if (menu == null)
                {
                    if (toOpen || keyboardOpen)
                    {
                        CreateMenu();
                        RecenterMenu(rightHanded, keyboardOpen);
                        if (reference == null)
                        {
                            CreateReference(rightHanded);
                        }
                    }
                }
                else
                {
                    if ((toOpen || keyboardOpen))
                    {
                        RecenterMenu(rightHanded, keyboardOpen);
                    }
                    else
                    {
                        GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(true);

                        Rigidbody comp = menu.AddComponent(typeof(Rigidbody)) as Rigidbody;
                        if (rightHanded)
                        {
                            comp.linearVelocity = GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
                        }
                        else
                        {
                            comp.linearVelocity = GorillaLocomotion.GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0);
                        }

                        Destroy(menu);
                        menu = null;

                        Destroy(reference);
                        reference = null;
                    }
                }
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.LogError(string.Format("{0} // Error initializing at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
            }

            // Constant
            try
            {
                // Pre-Execution
                if (fpsObject != null)
                {
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                }

                if (!Loaded)
                {
                    Load();
                    Loaded = true;
                }

                // Execute Enabled mods
                foreach (ButtonInfo[] buttonlist in buttons)
                {
                    foreach (ButtonInfo v in buttonlist)
                    {
                        if (v.enabled)
                        {
                            if (v.method != null)
                            {
                                try
                                {
                                    v.method.Invoke();
                                }
                                catch (Exception exc)
                                {
                                    UnityEngine.Debug.LogError(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, v.buttonText, exc.StackTrace, exc.Message));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.LogError(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
            }
        }

        public static bool what;
        public static Color what2 = Color.blue;
        public static bool what3;

        public static void OutlineObj(GameObject toOut, Color color1, Color color2, bool parentself = false, float thickness = 1)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.transform.parent = menu.transform;
            if (parentself)
                gameObject.transform.parent = toOut.transform.parent;

            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localPosition = toOut.transform.localPosition;
            gameObject.transform.localScale = toOut.transform.localScale + new Vector3(-0.01f / thickness, 0.01f * thickness, 0.0075f * thickness);
            GradientColorKey[] array = new GradientColorKey[3];
            array[0].color = color1;
            array[0].time = 0f;
            array[1].color = color2;
            array[1].time = 0.5f;
            array[2].color = color1;
            array[2].time = 1f;
            ColorChanger colorChanger = gameObject.AddComponent<ColorChanger>();
            colorChanger.colorInfo = new ExtGradient
            {
                colors = array
            };
            colorChanger.Start();
        }


        // Functions
        public static void CreateMenu()
        {
            // Menu Holder
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            // Menu Background
            menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menuBackground.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menuBackground.GetComponent<BoxCollider>());
            menuBackground.transform.parent = menu.transform;
            menuBackground.transform.rotation = Quaternion.identity;
            menuBackground.transform.localScale = menuSize;
            menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);
            menuBackground.GetComponent<Renderer>().material.color = Color.black;
            if (what3)
            {
                OutlineObj(menuBackground, what2, what2, false, 3);
            }



            // Canvas
            canvasObject = new GameObject();
            canvasObject.transform.parent = menu.transform;
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 1000f;

            // Title and FPS
            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = PluginInfo.Name;
            text.fontSize = 1;
            text.color = textColors[0];
            text.supportRichText = true;
            text.fontStyle = FontStyle.Normal;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.28f, 0.05f);
            component.position = new Vector3(0.06f, 0f, 0.165f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            if (fpsCounter)
            {
                fpsObject = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                fpsObject.font = currentFont;
                fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                fpsObject.color = textColors[0];
                fpsObject.fontSize = 1;
                fpsObject.supportRichText = true;
                fpsObject.fontStyle = FontStyle.Normal;
                fpsObject.alignment = TextAnchor.MiddleCenter;
                fpsObject.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                fpsObject.resizeTextForBestFit = true;
                fpsObject.resizeTextMinSize = 0;
                RectTransform component2 = fpsObject.GetComponent<RectTransform>();
                component2.localPosition = Vector3.zero;
                component2.sizeDelta = new Vector2(0.28f, 0.02f);
                component2.position = new Vector3(0.06f, 0f, 0.135f);
                component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            GameObject SearchButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                SearchButton.layer = 2;
            }
            UnityEngine.Object.Destroy(SearchButton.GetComponent<Rigidbody>());
            SearchButton.GetComponent<BoxCollider>().isTrigger = true;
            SearchButton.transform.parent = menu.transform;
            SearchButton.transform.rotation = Quaternion.identity;
            SearchButton.transform.localScale = new Vector3(0.09f, 0.8f, 0.07f);
            SearchButton.transform.localPosition = new Vector3(0.56f, 0f, 0.28f);
            SearchButton.GetComponent<Renderer>().material.color = Color.black;
            SearchButton.AddComponent<Classes.Button>().relatedText = "Search";

            if (what3)
            {
                OutlineObj(SearchButton, what2, what2, false);
            }
            else
            {
                OutlineObj(SearchButton, new Color(0.06f, 0.06f, 0.06f), new Color(0.06f, 0.06f, 0.06f), false);
            }

                Text SearchText = new GameObject
                {
                    transform =
                {
                    parent = canvasObject.transform
                }
                }.AddComponent<Text>();
            SearchText.text = "Search...";
            SearchText.font = currentFont;
            SearchText.fontSize = 1;
            SearchText.color = textColors[0];
            SearchText.alignment = TextAnchor.MiddleCenter;
            SearchText.resizeTextForBestFit = true;
            SearchText.resizeTextMinSize = 0;

            RectTransform rectt1 = SearchText.GetComponent<RectTransform>();
            rectt1.localPosition = Vector3.zero;
            rectt1.sizeDelta = new Vector2(0.2f, 0.03f);
            rectt1.localPosition = new Vector3(0.064f, 0.05f, 0.105f);
            rectt1.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            var But1 = GameObject.CreatePrimitive(PrimitiveType.Cube);

            But1.GetComponent<BoxCollider>().isTrigger = true;

            But1.transform.parent = menu.transform;
            But1.transform.rotation = Quaternion.identity;
            But1.transform.localScale = new Vector3(0.05f, 0.1f, 0.08f);
            But1.transform.localPosition = new Vector3(0.56f, -0.45f, -0.57f);

            But1.GetComponent<Renderer>().material.color = new Color(0.06f, 0.06f, 0.06f);
            if (what3)
            {
                OutlineObj(But1, what2, what2, false);
            }

            But1.AddComponent<Classes.Button>().relatedText = "home";


            Text But1text = new GameObject
            {
                transform =
    {
        parent = canvasObject.transform
    }
            }.AddComponent<Text>();

            But1text.font = currentFont;
            But1text.text = "\u2302";
            But1text.fontSize = 1;
            But1text.color = textColors[0];
            But1text.alignment = TextAnchor.MiddleCenter;
            But1text.resizeTextForBestFit = true;
            But1text.resizeTextMinSize = 0;


            RectTransform recct1 = But1text.GetComponent<RectTransform>();

            recct1.localPosition = new Vector3(0.064f, -0.134f, -0.216f);
            recct1.sizeDelta = new Vector2(0.24f, 0.034f);
            recct1.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            if (SettingsButton)
            {
                var But = GameObject.CreatePrimitive(PrimitiveType.Cube);

                But.GetComponent<BoxCollider>().isTrigger = true;

                But.transform.parent = menu.transform;
                But.transform.rotation = Quaternion.identity;
                But.transform.localScale = new Vector3(0.05f, 0.1f, 0.08f);

                But.transform.localPosition = new Vector3(0.56f, -0.29f, -0.57f);

                But.GetComponent<Renderer>().material.color = new Color(0.06f, 0.06f, 0.06f);
                if (what3)
                {
                    OutlineObj(But, what2, what2, false);
                }

                But.AddComponent<Classes.Button>().relatedText = "Settings";


                Text Buttext = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();

                Buttext.font = currentFont;
                Buttext.text = "\u2699";
                Buttext.fontSize = 1;
                Buttext.color = textColors[0];
                Buttext.alignment = TextAnchor.MiddleCenter;
                Buttext.resizeTextForBestFit = true;
                Buttext.resizeTextMinSize = 0;


                RectTransform recct = Buttext.GetComponent<RectTransform>();

                recct.localPosition = new Vector3(0.064f, -0.087f, -0.217f);
                recct.sizeDelta = new Vector2(0.14f, 0.024f);
                recct.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            if (disconnectButton)
            {
                GameObject disconnectbutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(KeyCode.Q))
                {
                    disconnectbutton.layer = 2;
                }
                UnityEngine.Object.Destroy(disconnectbutton.GetComponent<Rigidbody>());
                disconnectbutton.GetComponent<BoxCollider>().isTrigger = true;
                disconnectbutton.transform.parent = menu.transform;
                disconnectbutton.transform.rotation = Quaternion.identity;
                disconnectbutton.transform.localScale = new Vector3(0.09f, 0.4f, 0.09f);
                disconnectbutton.transform.localPosition = new Vector3(0.56f, 0f, 0.57f);
                disconnectbutton.GetComponent<Renderer>().material.color = Color.black;
                if (what3)
                {
                    OutlineObj(disconnectbutton, what2, what2, false, 3);
                }
                disconnectbutton.AddComponent<Classes.Button>().relatedText = "Disconnect";


                Text discontext = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                discontext.text = "leave";
                discontext.font = currentFont;
                discontext.fontSize = 1;
                discontext.color = textColors[0];
                discontext.alignment = TextAnchor.MiddleCenter;
                discontext.resizeTextForBestFit = true;
                discontext.resizeTextMinSize = 0;

                RectTransform rectt = discontext.GetComponent<RectTransform>();
                rectt.localPosition = Vector3.zero;
                rectt.sizeDelta = new Vector2(0.2f, 0.03f);
                rectt.localPosition = new Vector3(0.064f, 0f, 0.22f);
                rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = what ? new Vector3(0.045f, 0.25f, 0.8936298f) : new Vector3(0.06f, 0.25f, 0.06f);
            gameObject.transform.localPosition = what ? new Vector3(0.56f, 0.657f, 0.0063f) : new Vector3(0.56f, -0.37f, 0.555f);
            gameObject.GetComponent<Renderer>().material.color = Color.black;
            gameObject.AddComponent<Classes.Button>().relatedText = "PreviousPage";
            if (what3)
            {
                OutlineObj(gameObject, what2, what2, false, 3);
            }


            text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = "";
            text.fontSize = 1;
            text.color = textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0f, 0.0678f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = what ? new Vector3(0.045f, 0.25f, 0.8936298f) : new Vector3(0.06f, 0.25f, 0.06f);
            gameObject.transform.localPosition = what ? new Vector3(0.56f, -0.657f, 0.0063f) : new Vector3(0.56f, 0.37f, 0.555f);
            gameObject.GetComponent<Renderer>().material.color = Color.black;
            if (what3)
            {
                OutlineObj(gameObject, what2, what2, false, 3);
            }
            gameObject.AddComponent<Classes.Button>().relatedText = "NextPage";


            text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = "";
            text.fontSize = 1;
            text.color = textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0f, 0.105f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            // Mod Buttons
            ButtonInfo[] activeButtons = buttons[buttonsType].Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
            for (int i = 0; i < activeButtons.Length; i++)
            {
                CreateButton(i * 0.095f, activeButtons[i]);
            }
        }

        public static void CreateButton(float offset, ButtonInfo method)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.05f, 0.6f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0.1f, 0.17f - offset);
            gameObject.AddComponent<Classes.Button>().relatedText = method.buttonText;
            gameObject.GetComponent<Renderer>().material.color = new Color(0.06f, 0.06f, 0.06f);
            if (what3)
            {
                OutlineObj(gameObject, what2, what2, false, 3);
            }

            GameObject gameObject1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject1.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject1.GetComponent<Rigidbody>());
            gameObject1.GetComponent<BoxCollider>().isTrigger = true;
            gameObject1.transform.parent = menu.transform;
            gameObject1.transform.rotation = Quaternion.identity;
            gameObject1.transform.localScale = new Vector3(0.05f, 0.1f, 0.085f);
            gameObject1.transform.localPosition = new Vector3(0.56f, -0.35f, 0.17f - offset);
            gameObject1.AddComponent<Classes.Button>().relatedText = "favorite";
            gameObject1.GetComponent<Renderer>().material.color = new Color(0.06f, 0.06f, 0.06f);
            if (what3)
            {
                OutlineObj(gameObject1, what2, what2, false, 3);
            }

            ColorChanger colorChanger = gameObject.AddComponent<ColorChanger>();
            if (method.enabled)
            {
                colorChanger.colorInfo = buttonColors[1];
            }
            else
            {
                colorChanger.colorInfo = buttonColors[0];
            }
            colorChanger.Start();

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = method.buttonText;
            if (method.overlapText != null)
            {
                text.text = method.overlapText;
            }
            text.supportRichText = true;
            text.fontSize = 1;
            if (method.enabled)
            {
                text.color = textColors[1];
            }
            else
            {
                text.color = textColors[0];
            }
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Normal;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(.2f, .03f);
            component.localPosition = new Vector3(.064f, 0.03f, 0.064f - offset / 2.68f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));



            Text text1 = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text1.font = currentFont;
            text1.text = "\u2764";
            if (method.overlapText != null)
            {
                text1.text = "\u2764";
            }
            text1.supportRichText = true;
            text1.fontSize = 1;
            if (method.enabled)
            {
                text1.color = Color.white;
            }
            else
            {
                text1.color = Color.white;
            }
            text1.alignment = TextAnchor.MiddleCenter;
            text1.fontStyle = FontStyle.Normal;
            text1.resizeTextForBestFit = true;
            text1.resizeTextMinSize = 0;
            RectTransform component1 = text1.GetComponent<RectTransform>();

            component1.localPosition = Vector3.zero;
            component1.sizeDelta = new Vector2(.2f, .03f);
            component1.localPosition = new Vector3(.064f, -0.105f, 0.064f - offset / 2.68f);
            component1.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void RecreateMenu()
        {
            if (menu != null)
            {
                UnityEngine.Object.Destroy(menu);
                menu = null;

                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try
                {
                    TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                }
                catch { }

                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;
                    GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bg.transform.localScale = new Vector3(10f, 10f, 0.01f);
                    bg.transform.transform.position = TPC.transform.position + TPC.transform.forward;
                    bg.GetComponent<Renderer>().material.color = new Color32((byte)(backgroundColor.colors[0].color.r * 100), (byte)(backgroundColor.colors[0].color.g * 50), (byte)(backgroundColor.colors[0].color.b * 50), 255);
                    GameObject.Destroy(bg, Time.deltaTime);
                    menu.transform.parent = TPC.transform;
                    menu.transform.position = (TPC.transform.position + (Vector3.Scale(TPC.transform.forward, new Vector3(0.5f, 0.5f, 0.5f)))) + (Vector3.Scale(TPC.transform.up, new Vector3(-0.02f, -0.02f, -0.02f)));
                    Vector3 rot = TPC.transform.rotation.eulerAngles;
                    rot = new Vector3(rot.x - 90, rot.y + 90, rot.z);
                    menu.transform.rotation = Quaternion.Euler(rot);

                    if (reference != null)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                            RaycastHit hit;
                            bool worked = Physics.Raycast(ray, out hit, 100);
                            if (worked)
                            {
                                Classes.Button collide = hit.transform.gameObject.GetComponent<Classes.Button>();
                                if (collide != null)
                                {
                                    collide.OnTriggerEnter(buttonCollider);
                                }
                            }
                        }
                        else
                        {
                            reference.transform.position = new Vector3(999f, -999f, -999f);
                        }
                    }
                }
            }
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (isRightHanded)
            {
                reference.transform.parent = GorillaTagger.Instance.leftHandTransform;
            }
            else
            {
                reference.transform.parent = GorillaTagger.Instance.rightHandTransform;
            }
            reference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            buttonCollider = reference.GetComponent<SphereCollider>();

            ColorChanger colorChanger = reference.AddComponent<ColorChanger>();
            colorChanger.colorInfo = backgroundColor;
            colorChanger.Start();
        }

        public static void Toggle(string buttonText)
        {
            int lastPage = ((buttons[buttonsType].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
            if (buttonText == "PreviousPage")
            {
                pageNumber--;
                if (pageNumber < 0)
                {
                    pageNumber = lastPage;
                }
            }
            else
            {
                if (buttonText == "NextPage")
                {
                    pageNumber++;
                    if (pageNumber > lastPage)
                    {
                        pageNumber = 0;
                    }
                }
                if (buttonText == "disconnect")
                {
                    PhotonNetwork.Disconnect();
                }
                if (buttonText == "Settings")
                {
                    SettingsMods.MenuSettings();
                }
                else
                {
                    ButtonInfo target = GetIndex(buttonText);
                    if (target != null)
                    {
                        if (target.isTogglable)
                        {
                            target.enabled = !target.enabled;
                            if (target.enabled)
                            {
                                if (target.enableMethod != null)
                                {
                                    try { target.enableMethod.Invoke(); } catch { }
                                }
                            }
                            else
                            {
                                if (target.disableMethod != null)
                                {
                                    try { target.disableMethod.Invoke(); } catch { }
                                }
                            }
                        }
                        else
                        {
                            if (target.method != null)
                            {
                                try { target.method.Invoke(); } catch { }
                            }
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(buttonText + " does not exist");
                    }
                }
            }
            RecreateMenu();
        }

        public static GradientColorKey[] GetSolidGradient(Color color)
        {
            return new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };
        }

        public static ButtonInfo GetIndex(string buttonText)
        {
            foreach (ButtonInfo[] buttons in Menu.Buttons.buttons)
            {
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        return button;
                    }
                }
            }

            return null;
        }

        // Variables
        // Important
        // Objects
        public static GameObject menu;
        public static GameObject menuBackground;
        public static GameObject reference;
        public static GameObject canvasObject;

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;

        // Data
        public static int pageNumber = 0;
        public static int buttonsType = 0;
    }
}