using BepInEx;
using HarmonyLib;
using Photon.Pun;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ShibaGTGenesisReborn.Mods;
using static ShibaGTGenesisReborn.Libs.GunLib;
using static ShibaGTGenesisReborn.Menu.Buttons;
using static ShibaGTGenesisReborn.Mods.mods;
using static ShibaGTGenesisReborn.Settings;

namespace ShibaGTGenesisReborn.Menu
{
    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }

        public static bool Loaded;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MenuAudio.Initialize();
        }

        private void Update()
        {
            try
            {
                bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerPrimaryButton);
                bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);
                trigger_Button = Mouse.current.rightButton.isPressed ? Mouse.current.leftButton.isPressed : ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
                InputHandler.Instance.RightGrip.IsPressed = Mouse.current.rightButton.isPressed ? Mouse.current.leftButton.isPressed : ControllerInputPoller.instance.rightGrab;

                CacheObjects();

                if (cocHeading != null && motdBody != null && cocBody != null && motdHeading != null)
                {
                    cocHeading.GetComponent<TMP_Text>().text = $"<color=blue>ShibaGT Genesis Reborn v1</color>".ToUpper();
                    cocHeading.GetComponent<TMP_Text>().fontSize = 75f;
                    cocBody.GetComponent<TMP_Text>().richText = true;
                    cocBody.GetComponent<TMP_Text>().text = "\nWelcome To ShibaGT Genesis Reborn!\nThis is a Remake of the Longest Lasting Paid Mod Menu Shiba GT Genesis!".ToUpper();
                    motdHeading.GetComponent<TMP_Text>().text = "<color=blue>ShibaGT Genesis Reborn v1.0</color>".ToUpper();
                    motdBody.GetComponent<TMP_Text>().text = "Creditz To ShibaGT/TAI for making the original menu!\nThis is just a remake!\n<color=red>We Are Not Responsible For Any Bans Using This Mod Menu!</color>".ToUpper();
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
                        if (shoulderCamera != null)
                        {
                            shoulderCamera.transform.Find("CM vcam1").gameObject.SetActive(true);
                        }

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

            try
            {
                if (fpsObject != null)
                {
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                }

                if (!Loaded)
                {
                    Load();
                    Loaded = true;
                }

                ButtonInfo[] activeButtons = GetAllButtons();
                for (int i = 0; i < activeButtons.Length; i++)
                {
                    ButtonInfo button = activeButtons[i];
                    if (button.enabled && button.method != null)
                    {
                        try
                        {
                            button.method.Invoke();
                        }
                        catch (Exception exc)
                        {
                            UnityEngine.Debug.LogError(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, button.buttonText, exc.StackTrace, exc.Message));
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.LogError(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
            }
        }

        private void OnDestroy()
        {
            if (menu != null)
            {
                Destroy(menu);
                menu = null;
            }

            if (reference != null)
            {
                Destroy(reference);
                reference = null;
            }

            if (keyboard != null)
            {
                Destroy(keyboard);
                keyboard = null;
            }

            if (lKey != null)
            {
                Destroy(lKey);
                lKey = null;
            }

            if (rKey != null)
            {
                Destroy(rKey);
                rKey = null;
            }

            if (searchDisplayText != null)
            {
                Destroy(searchDisplayText.gameObject);
                searchDisplayText = null;
            }

            if (canvasObject != null)
            {
                Destroy(canvasObject);
                canvasObject = null;
            }

            CleanupResources();

            Instance = null;
        }

        public static bool what;
        public static Color what2 = Color.blue;
        public static bool what3;
        public static bool trigger_Button = false;
        public static string searchQuery = "";
        public static bool isSearching = false;
        public static System.Collections.Generic.List<ButtonInfo> searchResults = new System.Collections.Generic.List<ButtonInfo>();
        public static Text searchDisplayText = null;
        public static GameObject keyboard = null;
        public static Transform menuSpawnPos = null;
        public static GameObject lKey = null;
        public static GameObject rKey = null;
        public static System.Collections.Generic.List<ButtonInfo> favoriteButtons = new System.Collections.Generic.List<ButtonInfo>();

        private static ButtonInfo[] allButtons;

        private static ButtonInfo[] GetAllButtons()
        {
            if (allButtons != null)
                return allButtons;

            System.Collections.Generic.List<ButtonInfo> list = new System.Collections.Generic.List<ButtonInfo>();
            for (int i = 0; i < buttons.Length; i++)
            {
                ButtonInfo[] category = buttons[i];
                if (category == null) continue;
                for (int j = 0; j < category.Length; j++)
                {
                    if (category[j] != null)
                        list.Add(category[j]);
                }
            }

            allButtons = list.ToArray();
            return allButtons;
        }

        public static void UpdateSearchDisplay()
        {
            if (searchDisplayText != null)
            {
                searchDisplayText.text = "Search: " + searchQuery + "_";
            }
        }

        public static void ExecuteSearch()
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                buttonsType = 0;
                pageNumber = 0;
                RecreateMenu();
                return;
            }

            searchResults.Clear();
            string queryLower = searchQuery.ToLower();

            foreach (ButtonInfo[] buttonList in Buttons.buttons)
            {
                foreach (ButtonInfo button in buttonList)
                {
                    if (button.buttonText.ToLower().Contains(queryLower) ||
                        (button.toolTip != null && button.toolTip.ToLower().Contains(queryLower)))
                    {
                        if (!searchResults.Contains(button))
                        {
                            searchResults.Add(button);
                        }
                    }
                }
            }

            if (searchResults.Count == 0)
            {
                NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "No mods found for: " + searchQuery);
                searchQuery = "";
                UpdateSearchDisplay();
                return;
            }

            buttonsType = 999;
            pageNumber = 0;
            RecreateMenu();
        }

        public static void ShowSearchResults()
        {
            if (menu == null) return;

            buttonsType = 999;
            pageNumber = 0;
            RecreateMenu();
        }

        public static void Search()
        {
            isSearching = !isSearching;

            if (isSearching)
            {
                searchQuery = "";

                if (keyboard == null)
                {
                    keyboard = LoadAssetBundle("keyboard");
                    if (keyboard != null)
                    {
                        keyboard.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
                        keyboard.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;

                        menuSpawnPos = keyboard.transform.Find("MenuSpawnPosition").transform;

                        foreach (Transform trans in keyboard.transform.Find("fard").GetComponentsInChildren<Transform>())
                        {
                            try
                            {
                                Renderer renderer = trans.GetComponent<Renderer>();
                                if (renderer != null && trans.name != "Canvas")
                                {
                                    renderer.material.color = Color.cyan;
                                }
                            }
                            catch { }

                            bool isExcluded = trans.name == "bg" || trans.name == "Canvas" || trans.name == "row1" ||
                                             trans.name == "row2" || trans.name == "row3" ||
                                             (trans.name == "space" && trans.parent.name == "Canvas") ||
                                             trans.name == "MenuSpawnPosition";

                            if (!isExcluded)
                            {
                                KeyboardButton btn = trans.AddComponent<KeyboardButton>();
                                btn.key = trans.name;
                            }
                        }

                        if (lKey == null)
                        {
                            lKey = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            lKey.transform.parent = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform;
                            lKey.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            lKey.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            lKey.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        if (rKey == null)
                        {
                            rKey = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            rKey.transform.parent = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform;
                            rKey.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            rKey.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            rKey.GetComponent<Renderer>().material.color = Color.blue;
                        }
                        Debug.Log("Keyboard loaded successfully");
                    }
                }

                buttonsType = 999;
                pageNumber = 0;
                searchResults.Clear();
                RecreateMenu();
            }
            else
            {
                if (lKey != null)
                {
                    UnityEngine.Object.Destroy(lKey);
                    lKey = null;
                }
                if (rKey != null)
                {
                    UnityEngine.Object.Destroy(rKey);
                    rKey = null;
                }
                if (keyboard != null)
                {
                    UnityEngine.Object.Destroy(keyboard);
                    keyboard = null;
                }

                if (searchDisplayText != null)
                {
                    UnityEngine.Object.Destroy(searchDisplayText.gameObject);
                    searchDisplayText = null;
                }

                buttonsType = 0;
                pageNumber = 0;
                searchQuery = "";
                searchResults.Clear();
                isSearching = false;

                if (menu != null)
                {
                    RecreateMenu();
                }
            }
        }

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
            Renderer r = gameObject.GetComponent<Renderer>();
            r.material.color = color1;
        }

        public static void CreateMenu()
        {
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menuBackground.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menuBackground.GetComponent<BoxCollider>());
            menuBackground.transform.parent = menu.transform;
            menuBackground.transform.rotation = Quaternion.identity;
            menuBackground.transform.localScale = menuSize;
            menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);
            menuBackground.GetComponent<Renderer>().material.color = Color.black;
            if (what3) OutlineObj(menuBackground, what2, what2, false, 3);

            canvasObject = new GameObject();
            canvasObject.transform.parent = menu.transform;
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 1000f;

            int lastPage = 0;
            if (buttonsType == 999)
            {
                lastPage = ((searchResults.Count + buttonsPerPage - 1) / buttonsPerPage) - 1;
                if (lastPage < 0) lastPage = 0;
            }
            else if (buttonsType < buttons.Length)
            {
                lastPage = ((buttons[buttonsType].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
                if (lastPage < 0) lastPage = 0;
            }

            if (pageNumber > lastPage || pageNumber < 0)
            {
                pageNumber = 0;
            }

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

            if (isSearching)
            {
                SearchText.text = "Searching...";
                SearchText.color = Color.cyan;
            }
            else
            {
                SearchText.text = "Search...";
                SearchText.color = textColors[0];
            }
            SearchText.font = currentFont;
            SearchText.fontSize = 1;
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
                discontext.text = "Leave";
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
            gameObject.AddComponent<Classes.Button>().relatedText = "NextPage";
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
            component.localPosition = what ? new Vector3(0.064f, 0.195f, 0f) : new Vector3(0.064f, -0.115f, 0.215f);
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
            gameObject.AddComponent<Classes.Button>().relatedText = "PreviousPage";

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
            component.localPosition = what ? new Vector3(0.064f, -0.195f, 0f) : new Vector3(0.064f, 0.115f, 0.215f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            ButtonInfo[] activeButtons;

            if (buttonsType == 999 && searchResults.Count > 0)
            {
                activeButtons = searchResults.Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
            }
            else if (buttonsType < buttons.Length)
            {
                activeButtons = buttons[buttonsType].Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
            }
            else
            {
                activeButtons = new ButtonInfo[0];
            }

            for (int i = 0; i < activeButtons.Length; i++)
            {
                CreateButton(i * 0.095f, activeButtons[i]);
            }

            if (isSearching)
            {
                CreateSearchDisplay();
            }
        }

        public static void CreateSearchDisplay()
        {
            if (searchDisplayText == null)
            {
                searchDisplayText = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();

                searchDisplayText.font = currentFont;
                searchDisplayText.text = "Search: " + searchQuery + "_";
                searchDisplayText.fontSize = 1;
                searchDisplayText.color = Color.cyan;
                searchDisplayText.alignment = TextAnchor.MiddleCenter;
                searchDisplayText.resizeTextForBestFit = true;
                searchDisplayText.resizeTextMinSize = 0;

                RectTransform component = searchDisplayText.GetComponent<RectTransform>();
                component.localPosition = Vector3.zero;
                component.sizeDelta = new Vector2(0.28f, 0.03f);
                component.position = new Vector3(0.06f, -0.08f, 0.065f);
                component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
            else
            {
                searchDisplayText.text = "Search: " + searchQuery + "_";
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
            Classes.Button btn = gameObject.AddComponent<Classes.Button>();
            btn.relatedText = method.buttonText;
            btn.buttonInfo = method;
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
            Classes.Button favBtn = gameObject1.AddComponent<Classes.Button>();
            favBtn.relatedText = "fav_" + method.buttonText;
            favBtn.buttonInfo = method;
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
            text1.supportRichText = true;
            text1.fontSize = 1;
            text1.color = method.isFavorite ? Color.yellow : Color.white;
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
                    if (cachedTPC == null)
                    {
                        GameObject shoulderCam = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                        if (shoulderCam != null)
                        {
                            cachedTPC = shoulderCam.GetComponent<Camera>();
                        }
                    }
                    TPC = cachedTPC;
                }
                catch { }

                if (shoulderCamera != null)
                {
                    shoulderCamera.transform.Find("CM vcam1").gameObject.SetActive(false);
                }

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
                        bool isClick = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || UnityInput.Current.GetMouseButtonDown(0);
                        if (isClick)
                        {
                            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)UnityInput.Current.mousePosition;
                            Ray ray = TPC.ScreenPointToRay(mousePos);
                            if (Physics.Raycast(ray, out RaycastHit hit, 100))
                            {
                                Classes.Button collide = hit.transform.gameObject.GetComponent<Classes.Button>();
                                if (collide != null)
                                {
                                    collide.OnTriggerEnter(buttonCollider);
                                }
                            }
                        }
                        else if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
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

        public static void ToggleFavorite(string buttonText, ButtonInfo target = null)
        {
            if (target == null)
            {
                target = GetIndex(buttonText);
            }

            if (target != null)
            {
                target.isFavorite = !target.isFavorite;

                if (target.isFavorite)
                {
                    if (!favoriteButtons.Contains(target))
                    {
                        favoriteButtons.Add(target);
                    }
                }
                else
                {
                    if (favoriteButtons.Contains(target))
                    {
                        favoriteButtons.Remove(target);
                    }
                }

                UpdateFavoritesCategory();
                RecreateMenu();
            }
        }

        public static void UpdateFavoritesCategory()
        {
            if (favoriteButtons.Count > 0)
            {
                if (buttons.Length > 15)
                {
                    buttons[15] = favoriteButtons.ToArray();
                }
            }
            else
            {
                if (buttons.Length > 15)
                {
                    buttons[15] = new ButtonInfo[0];
                }
            }
        }

        public static void Load()
        {
            UpdateFavoritesCategory();
            MenuAudio.Initialize();
        }

        public static void Toggle(string buttonText, ButtonInfo target = null)
        {
            if (buttonText.StartsWith("fav_"))
            {
                ToggleFavorite(buttonText.Substring(4), target);
                return;
            }

            int lastPage = 0;
            if (buttonsType == 999)
            {
                lastPage = ((searchResults.Count + buttonsPerPage - 1) / buttonsPerPage) - 1;
                if (lastPage < 0) lastPage = 0;
            }
            else if (buttonsType < buttons.Length)
            {
                lastPage = ((buttons[buttonsType].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
                if (lastPage < 0) lastPage = 0;
            }
            else
            {
                lastPage = 0;
            }

            if (buttonText == "PreviousPage")
            {
                pageNumber--;
                if (pageNumber < 0)
                {
                    pageNumber = lastPage;
                }
            }
            else if (buttonText == "NextPage")
            {
                pageNumber++;
                if (pageNumber > lastPage)
                {
                    pageNumber = 0;
                }
            }
            else if (buttonText.Equals("Disconnect", StringComparison.OrdinalIgnoreCase))
            {
                PhotonNetwork.Disconnect();
                NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "Disconnected from network");
            }
            else if (buttonText == "Settings")
            {
                SettingsMods.MenuSettings();
            }
            else if (buttonText == "Search")
            {
                Search();
                return;
            }
            else if (buttonText == "home")
            {
                if (isSearching)
                {
                    Search();
                    return;
                }
                buttonsType = 0;
                pageNumber = 0;
                RecreateMenu();
                return;
            }
            else
            {
                if (target == null)
                {
                    target = GetIndex(buttonText);
                }

                if (target != null)
                {
                    string displayName = string.IsNullOrEmpty(target.toolTip) ? target.buttonText : target.toolTip;

                    if (!string.IsNullOrEmpty(displayName))
                    {
                        if (target.isTogglable)
                        {
                            target.enabled = !target.enabled;
                            if (target.enabled)
                            {
                                if (target.enableMethod != null)
                                {
                                    try
                                    {
                                        target.enableMethod.Invoke();
                                        NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, displayName);
                                    }
                                    catch { }
                                }
                            }
                            else
                            {
                                if (target.disableMethod != null)
                                {
                                    try
                                    {
                                        target.disableMethod.Invoke();
                                        NotificationLib.SendNotification(NotificationLib.NotificationType.Disabled, displayName);
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            if (target.method != null)
                            {
                                try
                                {
                                    target.method.Invoke();
                                    NotificationLib.SendNotification(NotificationLib.NotificationType.Info, displayName);
                                }
                                catch { }
                            }
                        }
                    }
                    else
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
                }
                else
                {
                    UnityEngine.Debug.LogError(buttonText + " does not exist");
                }
            }

            if (buttonsType == 14)
            {
                SettingsMods.UpdateEnabledMods();
            }
            RecreateMenu();
        }

        public static GradientColorKey[] GetSolidGradient(Color color)
        {
            return new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };
        }

        public static ButtonInfo GetIndex(string buttonText)
        {
            if (string.IsNullOrEmpty(buttonText))
            {
                return null;
            }

            if (buttonsType == 999)
            {
                foreach (ButtonInfo button in searchResults)
                {
                    if (button != null && button.buttonText == buttonText)
                    {
                        return button;
                    }
                }
            }
            else if (buttonsType >= 0 && buttonsType < Buttons.buttons.Length && Buttons.buttons[buttonsType] != null)
            {
                foreach (ButtonInfo button in Buttons.buttons[buttonsType])
                {
                    if (button != null && button.buttonText == buttonText)
                    {
                        return button;
                    }
                }
            }

            foreach (ButtonInfo[] buttonList in Buttons.buttons)
            {
                if (buttonList == null)
                {
                    continue;
                }

                foreach (ButtonInfo button in buttonList)
                {
                    if (button != null && button.buttonText == buttonText)
                    {
                        return button;
                    }
                }
            }

            return null;
        }

        public static AssetBundle assetBundle = null;

        public static GameObject LoadAssetBundle(string assetName)
        {
            GameObject gameObject = null;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShibaGTGenesisReborn.AssetBundles.gen"))
            {
                if (stream != null)
                {
                    if (assetBundle == null)
                    {
                        assetBundle = AssetBundle.LoadFromStream(stream);
                    }
                    gameObject = Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>(assetName));
                }
                else
                {
                    Debug.LogError("Failed to load asset from resource: " + assetName);
                }
            }

            return gameObject;
        }

        public static GameObject LoadAssetBundle2(string fullassetName)
        {
            GameObject gameObject = null;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShibaGTGenesisReborn.AssetBundles." + fullassetName))
            {
                if (stream != null)
                {
                    if (assetBundle == null)
                    {
                        assetBundle = AssetBundle.LoadFromStream(stream);
                    }
                    gameObject = Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("IngameGUI"));
                }
                else
                {
                    Debug.LogError("Failed to load asset from resource: " + fullassetName);
                }
            }

            return gameObject;
        }

        public static GameObject LoadAssetBundle3Real(string fullassetName)
        {
            GameObject gameObject = null;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShibaGTGenesisReborn.AssetBundles." + fullassetName))
            {
                if (stream != null)
                {
                    if (assetBundle == null)
                    {
                        assetBundle = AssetBundle.LoadFromStream(stream);
                    }
                    gameObject = Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>(fullassetName));
                }
                else
                {
                    Debug.LogError("Failed to load asset from resource: " + fullassetName);
                }
            }

            return gameObject;
        }

        private static void CacheObjects()
        {
            if (cocHeading == null)
            {
                cocHeading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText");
            }
            if (cocBody == null)
            {
                cocBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
            }
            if (motdHeading == null)
            {
                motdHeading = GameObject.Find("motdHeadingText");
            }
            if (motdBody == null)
            {
                motdBody = GameObject.Find("motdBodyText");
            }
            if (shoulderCamera == null)
            {
                shoulderCamera = GameObject.Find("Shoulder Camera");
            }
        }

        public static void CleanupResources()
        {
            if (searchResults != null)
            {
                searchResults.Clear();
            }
            if (favoriteButtons != null)
            {
                favoriteButtons.Clear();
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }

            cocHeading = null;
            cocBody = null;
            motdHeading = null;
            motdBody = null;
            shoulderCamera = null;
            cachedTPC = null;
        }

        public static GameObject menu;
        public static GameObject menuBackground;
        public static GameObject reference;
        public static GameObject canvasObject;

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;

        private static GameObject cocHeading;
        private static GameObject cocBody;
        private static GameObject motdHeading;
        private static GameObject motdBody;
        private static GameObject shoulderCamera;
        private static Camera cachedTPC;

        public static int pageNumber = 0;
        public static int buttonsType = 0;
    }
}