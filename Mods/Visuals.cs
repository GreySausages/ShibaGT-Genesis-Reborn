using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Menu;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
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
    }
}
