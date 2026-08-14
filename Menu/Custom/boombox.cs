using GorillaLocomotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Photon.Realtime;
using ShibaGTGenesisReborn.Libs;
using UnityEngine;
using UnityEngine.Networking;

namespace ShibaGTGenesisReborn.Mods
{
    internal class BoomboxManager : MonoBehaviour
    {
        public static float Volume = 0.4f;
        public static float MaxDistance = 15f;
        public static float SpatialBlend3D = 1.0f;
        public static float SpatialBlendShoulder = 0.0f;
        public static float PitchAndSpeed = 1.0f;
        public static bool UseVisualizer = true;
        public static float VisualizerIntensity = 0.3f;
        public static float BaseScale = 0.8f;
        public static float BodySide = 0.0f;
        public static float BodyHeight = -0.1f;
        public static float BodyDepth = -0.15f;
        public static float BodyRoll = -15f;
        public static Vector3 BackOffset => new Vector3(BodySide, BodyHeight, BodyDepth);

        public static BoomboxManager Me;
        public static GameObject Obj;
        public static AudioSource Aud;
        public static Mesh CM;
        public static Texture2D CT;
        public static bool Done;
        public static bool Down;
        public static bool Held = false;
        public static bool OnBack = false;
        public static Transform Hand;
        private static bool isRightHand = true;
        private static Vector3 OffP;
        private static Quaternion OffR;
        private static float ignoreTimer = 0f;
        private static bool pickerOpen = false;
        private static readonly Queue<Action> ThreadQueue = new Queue<Action>();
        private static float[] samples = new float[256];

        static string Dir => Application.persistentDataPath;
        static string P_Obj => Path.Combine(Dir, "boombox.obj");
        static string P_Tex => Path.Combine(Dir, "boombox.png");
        public static string P_Aud => Path.Combine(Dir, "boombox_audio.wav");

        public static void AdjustVolume(float delta) { Volume = Mathf.Clamp(Volume + delta, 0f, 1f); if (Aud) Aud.volume = Volume; }
        public static void AdjustPitchSpeed(float delta) { PitchAndSpeed = Mathf.Clamp(PitchAndSpeed + delta, 0.5f, 2.0f); if (Aud) Aud.pitch = PitchAndSpeed; }
        public static void ToggleVisualizer() => UseVisualizer = !UseVisualizer;

        public static void BoomboxLoop(string modelUrl, string texUrl)
        {
            if (!Me)
            {
                GameObject g = new GameObject("BoomboxProcessor");
                Me = g.AddComponent<BoomboxManager>();
                DontDestroyOnLoad(g);
            }
            lock (ThreadQueue) { while (ThreadQueue.Count > 0) ThreadQueue.Dequeue().Invoke(); }
            if (!Done && !Down && CM == null) { Down = true; Me.StartCoroutine(DoResources(modelUrl, texUrl)); }
            else if (!Done && CM != null) Spawn();
            if (Done && Obj)
            {
                HandleInteraction();
                HandleVisualizer();
            }
        }

        static void HandleVisualizer()
        {
            if (!UseVisualizer || !Aud || !Aud.isPlaying || !Obj)
            {
                if (Obj)
                    Obj.transform.localScale = Vector3.one * BaseScale;
                return;
            }

            Aud.GetOutputData(samples, 0);
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i] * samples[i];
            float rms = Mathf.Sqrt(sum / samples.Length);
            float scale = BaseScale + (rms * VisualizerIntensity);
            Obj.transform.localScale = Vector3.one * scale;
        }

        public static void Kill()
        {
            if (Obj != null && NetworkingLibrary.Instance != null)
                Obj.UnregisterFromNetwork();
            
            if (Me) Me.StopAllCoroutines();
            if (Obj) Destroy(Obj);
            Obj = null; Aud = null; CM = null; CT = null;
            Done = false; Down = false; Held = false; OnBack = false;
        }

        static void HandleInteraction()
        {
            var player = GorillaLocomotion.GTPlayer.Instance;
            if (!player) return;

            float rGrip = ControllerInputPoller.instance.rightControllerGripFloat;
            float lGrip = ControllerInputPoller.instance.leftControllerGripFloat;

            if (Time.time > ignoreTimer)
            {
                ignoreTimer = Time.time + 1.0f;
                if (Obj.TryGetComponent(out Collider c))
                {
                    IgnoreCollisionRecursive(c, GorillaTagger.Instance.transform);
                    if (GorillaTagger.Instance.offlineVRRig != null)
                        IgnoreCollisionRecursive(c, GorillaTagger.Instance.offlineVRRig.transform);
                }
            }

            if (Held)
            {
                if (!Hand) { Held = false; return; }
                Obj.transform.position = Hand.TransformPoint(OffP);
                Obj.transform.rotation = Hand.rotation * OffR;
                
                if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                    Obj.UpdateNetworkPosition();

                float grip = isRightHand ? rGrip : lGrip;
                if (isRightHand ? ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f : ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f)
                    if (Aud.clip != null && !Aud.isPlaying) 
                    {
                        Aud.Play();
                        if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                            Obj.SyncBoomboxAudio();
                    }

                if (grip < 0.1f)
                {
                    Held = false;
                    Transform body = GorillaTagger.Instance.offlineVRRig.transform;
                    if (Vector3.Distance(Obj.transform.position, body.position) < 0.2f)
                    {
                        OnBack = true;
                        Obj.transform.SetParent(body);
                        Obj.transform.localPosition = BackOffset;
                        Obj.transform.localRotation = Quaternion.Euler(0, 0, BodyRoll);
                        if (Obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
                        Aud.spatialBlend = SpatialBlendShoulder;
                    }
                    else
                    {
                        OnBack = false;
                        if (Obj.TryGetComponent(out Rigidbody rel))
                        {
                            rel.isKinematic = false;
                            rel.velocity = isRightHand ? player.GetHandVelocityTracker(false).GetAverageVelocity(true, 0.05f) : player.GetHandVelocityTracker(true).GetAverageVelocity(true, 0.05f);
                        }
                    }
                }
            }
            else if (!OnBack)
            {
                if (Obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
            }

            if (!Held)
            {
                if (rGrip > 0.5f && Vector3.Distance(player.RightHand.controllerTransform.position, Obj.transform.position) < 0.15f) Grab(player.RightHand.controllerTransform, true);
                else if (lGrip > 0.5f && Vector3.Distance(player.LeftHand.controllerTransform.position, Obj.transform.position) < 0.15f) Grab(player.LeftHand.controllerTransform, false);
            }
        }

        static void Grab(Transform h, bool right)
        {
            Held = true; OnBack = false; Hand = h; isRightHand = right;
            Obj.transform.SetParent(null);
            if (Obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            Aud.spatialBlend = SpatialBlend3D;
            OffP = Hand.InverseTransformPoint(Obj.transform.position);
            OffR = Quaternion.Inverse(Hand.rotation) * Obj.transform.rotation;
        }

        static IEnumerator DoResources(string u, string t)
        {
            if (File.Exists(P_Tex)) { CT = new Texture2D(2, 2); CT.LoadImage(File.ReadAllBytes(P_Tex)); }
            else { UnityWebRequest tr = UnityWebRequestTexture.GetTexture(t); yield return tr.SendWebRequest(); if (tr.result == UnityWebRequest.Result.Success) { CT = DownloadHandlerTexture.GetContent(tr); File.WriteAllBytes(P_Tex, tr.downloadHandler.data); } }
            string objData = "";
            if (File.Exists(P_Obj)) objData = File.ReadAllText(P_Obj);
            else { UnityWebRequest r = UnityWebRequest.Get(u); yield return r.SendWebRequest(); if (r.result == UnityWebRequest.Result.Success) { objData = r.downloadHandler.text; File.WriteAllText(P_Obj, objData); } }
            if (!string.IsNullOrEmpty(objData)) { CM = Pars(objData); Spawn(); } else Down = false;
        }

        static void Spawn()
        {
            if (Obj) return;
            var player = GorillaLocomotion.GTPlayer.Instance;
            Obj = new GameObject("BoomboxItem");
            Obj.transform.position = player.RightHand.controllerTransform.position;
            Obj.layer = 8;
            Obj.AddComponent<MeshFilter>().mesh = CM;
            var mr = Obj.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { mainTexture = CT ?? Texture2D.whiteTexture };
            BoxCollider col = Obj.AddComponent<BoxCollider>();
            var rb = Obj.AddComponent<Rigidbody>();
            rb.mass = 1f; rb.drag = 0.5f; rb.angularDrag = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Aud = Obj.AddComponent<AudioSource>();
            Aud.spatialBlend = SpatialBlend3D;
            Aud.loop = true;
            Aud.maxDistance = MaxDistance;
            Aud.volume = Volume;
            Aud.pitch = PitchAndSpeed;
            Obj.transform.localScale = Vector3.one * BaseScale;
            IgnoreCollisionRecursive(col, player.transform);
            if (GorillaTagger.Instance.offlineVRRig != null) IgnoreCollisionRecursive(col, GorillaTagger.Instance.offlineVRRig.transform);
            Done = true;
            
            if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                Obj.RegisterForNetwork();
        }

        public static void OpenNativePicker()
        {
            pickerOpen = true;
            Thread t = new Thread(() => {
                OpenFileName ofn = new OpenFileName { lStructSize = Marshal.SizeOf(typeof(OpenFileName)), lpstrFilter = "Audio Files\0*.mp3;*.wav;*.ogg\0\0", lpstrFile = new string(new char[256]), lpstrTitle = "Select Music", Flags = 0x00080000 | 0x00001000 | 0x00000800 };
                ofn.nMaxFile = ofn.lpstrFile.Length;
                if (GetOpenFileName(ofn)) { string path = ofn.lpstrFile; lock (ThreadQueue) ThreadQueue.Enqueue(() => { Me.StartCoroutine(SetAudio(path)); pickerOpen = false; }); }
                else lock (ThreadQueue) ThreadQueue.Enqueue(() => { pickerOpen = false; });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        static IEnumerator SetAudio(string p)
        {
            string url = p.Contains("://") ? p : "file://" + p;
            using (UnityWebRequest u = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
            {
                yield return u.SendWebRequest();
                if (u.result == UnityWebRequest.Result.Success) 
                { 
                    if (Aud.clip != null) AudioClip.Destroy(Aud.clip); 
                    Aud.clip = DownloadHandlerAudioClip.GetContent(u); 
                    Aud.Play();
    
                    if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                    {
                        NetworkingLibrary.Instance.SendEvent("audioclip", ReceiverGroup.Others, 
                            NetworkingLibrary.Instance.FindObjectId(Obj), p);
                        Obj.SyncBoomboxAudio();
                    }
                }
            }
        }

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)] private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] private class OpenFileName { public int lStructSize; public IntPtr hwndOwner; public IntPtr hInstance; public string lpstrFilter; public string lpstrCustomFilter; public int nMaxCustFilter; public int nFilterIndex; public string lpstrFile; public int nMaxFile; public string lpstrFileTitle; public int nMaxFileTitle; public string lpstrInitialDir; public string lpstrTitle; public int Flags; public short nFileOffset; public short nFileExtension; public string lpstrDefExt; public IntPtr lCustData; public IntPtr lpfnHook; public string lpTemplateName; public IntPtr pvReserved; public int dwReserved; public int FlagsEx; }
        static void IgnoreCollisionRecursive(Collider col, Transform target) { if (!col || !target) return; foreach (Collider c in target.GetComponentsInChildren<Collider>(true)) Physics.IgnoreCollision(col, c, true); }
        static Mesh Pars(string s)
        {
            List<Vector3> v = new List<Vector3>(); List<Vector2> u = new List<Vector2>(); List<Vector3> n = new List<Vector3>(); List<int> t = new List<int>(); List<Vector3> nv = new List<Vector3>(); List<Vector2> nu = new List<Vector2>(); List<Vector3> nn = new List<Vector3>();
            using (StringReader r = new StringReader(s)) { string l; while ((l = r.ReadLine()) != null) { if (l.Length < 2 || l[0] == '#') continue; string[] p = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); if (p[0] == "v") v.Add(new Vector3(-float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]))); else if (p[0] == "vt") u.Add(new Vector2(float.Parse(p[1]), float.Parse(p[2]))); else if (p[0] == "vn") n.Add(new Vector3(-float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]))); else if (p[0] == "f") { for (int i = 3; i >= 1; i--) Fix(p[i], v, u, n, nv, nu, nn, t); if (p.Length == 5) { Fix(p[4], v, u, n, nv, nu, nn, t); Fix(p[3], v, u, n, nv, nu, nn, t); Fix(p[1], v, u, n, nv, nu, nn, t); } } } }
            Mesh m = new Mesh { vertices = nv.ToArray(), uv = nu.ToArray(), normals = nn.ToArray(), triangles = t.ToArray() }; m.RecalculateBounds(); m.RecalculateNormals(); return m;
        }
        static void Fix(string s, List<Vector3> v, List<Vector2> u, List<Vector3> n, List<Vector3> nv, List<Vector2> nu, List<Vector3> nn, List<int> t) { string[] c = s.Split('/'); nv.Add(v[int.Parse(c[0]) - 1]); if (c.Length > 1 && c[1] != "") nu.Add(u[int.Parse(c[1]) - 1]); else nu.Add(Vector2.zero); if (c.Length > 2 && c[2] != "") nn.Add(n[int.Parse(c[2]) - 1]); else nn.Add(Vector3.up); t.Add(nv.Count - 1); }
    }
}