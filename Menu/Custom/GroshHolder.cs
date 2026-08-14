using GorillaLocomotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Plon.Libs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

namespace Plon.Mods
{
    internal class GroshHolder : MonoBehaviour
    {
        public static GroshHolder Me;
        public static GameObject Obj;
        public static AudioSource Aud;
        public static Mesh CM;
        public static Texture2D CT;
        public static bool Down;
        public static bool Done;
        public static bool Held = true;
        public static Transform Hand;
        public static Vector3 OffP;
        public static Quaternion OffR;

        private static bool isRightHand = true;
        private static float ignoreTimer = 0f;

        static string Dir => Application.persistentDataPath;
        static string P_Obj => Path.Combine(Dir, "grosh.obj");
        static string P_Tex => Path.Combine(Dir, "grosh.png");

        public static void GroshLoop(string u, string t)
        {
            if (!Me)
            {
                GameObject g = new GameObject("GroshHoldable");
                Me = g.AddComponent<GroshHolder>();
                DontDestroyOnLoad(g);
            }

            if (!Done && !Down && CM == null)
            {
                Down = true;
                Me.StartCoroutine(Do(u, t));
            }
            else if (!Done && CM != null) Spawn();

            if (Done && Obj)
            {
                var player = GorillaLocomotion.GTPlayer.Instance;
                if (!player) return;

                if (!Hand)
                {
                    Hand = player.RightHand.controllerTransform;
                    isRightHand = true;
                }

                float rGrip = ControllerInputPoller.instance.rightControllerGripFloat;
                float rTrig = ControllerInputPoller.instance.rightControllerIndexFloat;
                float lGrip = ControllerInputPoller.instance.leftControllerGripFloat;
                float lTrig = ControllerInputPoller.instance.leftControllerIndexFloat;

                if (Time.time > ignoreTimer)
                {
                    ignoreTimer = Time.time + 1.0f;
                    if (Obj.TryGetComponent(out Collider myCol))
                    {
                        IgnoreCollisionRecursive(myCol, player.transform);
                        if (GorillaTagger.Instance.offlineVRRig != null)
                            IgnoreCollisionRecursive(myCol, GorillaTagger.Instance.offlineVRRig.transform);
                        if (player.bodyCollider) Physics.IgnoreCollision(myCol, player.bodyCollider, true);
                        if (player.headCollider) Physics.IgnoreCollision(myCol, player.headCollider, true);
                    }
                }

                if (Held)
                {
                    if (Hand == null) { Held = false; return; }

                    Obj.transform.position = Hand.TransformPoint(OffP);
                    Obj.transform.rotation = Hand.rotation * OffR;
                    
                    if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                        Obj.UpdateNetworkPosition();

                    if (Obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

                    float currentTrig = isRightHand ? rTrig : lTrig;
                    float currentGrip = isRightHand ? rGrip : lGrip;

                    float sq = 0.6f * (1f - (currentTrig * 0.4f));
                    Obj.transform.localScale = new Vector3(sq, 0.3f, sq);

                    if (currentTrig > 0.8f && !Aud.isPlaying) Aud.Play();

                    if (currentGrip < 0.1f)
                    {
                        Held = false;
                        if (Obj.TryGetComponent(out Rigidbody releaseRb))
                        {
                            releaseRb.isKinematic = false;
                            Vector3 throwVel = isRightHand
                                ? player.GetHandVelocityTracker(false).GetAverageVelocity(true, 0.05f)
                                : player.GetHandVelocityTracker(true).GetAverageVelocity(true, 0.05f);

                            releaseRb.velocity = throwVel;
                            releaseRb.angularVelocity = UnityEngine.Random.insideUnitSphere * 5f;
                        }
                    }
                }
                else
                {
                    if (Obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
                    Obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                    if (rGrip > 0.5f && Vector3.Distance(player.RightHand.controllerTransform.position, Obj.transform.position) < 0.15f)
                    {
                        Held = true;
                        Hand = player.RightHand.controllerTransform;
                        isRightHand = true;
                        OffP = Hand.InverseTransformPoint(Obj.transform.position);
                        OffR = Quaternion.Inverse(Hand.rotation) * Obj.transform.rotation;
                    }
                    else if (lGrip > 0.5f && Vector3.Distance(player.LeftHand.controllerTransform.position, Obj.transform.position) < 0.15f)
                    {
                        Held = true;
                        Hand = player.LeftHand.controllerTransform;
                        isRightHand = false;
                        OffP = Hand.InverseTransformPoint(Obj.transform.position);
                        OffR = Quaternion.Inverse(Hand.rotation) * Obj.transform.rotation;
                    }
                }
            }
        }

        static void IgnoreCollisionRecursive(Collider myCol, Transform target)
        {
            if (!myCol || !target) return;
            foreach (Collider c in target.GetComponentsInChildren<Collider>(true))
                Physics.IgnoreCollision(myCol, c, true);
        }

        public static void Kill()
        {
            if (Obj != null && NetworkingLibrary.Instance != null)
                Obj.UnregisterFromNetwork();
            
            if (Obj) Destroy(Obj);
            Done = false; Down = false; Held = true;
            OffP = Vector3.zero; OffR = Quaternion.identity;
        }

        static void Spawn()
        {
            if (Obj) return;
            Obj = new GameObject("GroshItem");
            Obj.layer = 8;

            MeshFilter mf = Obj.AddComponent<MeshFilter>();
            MeshRenderer mr = Obj.AddComponent<MeshRenderer>();
            mf.mesh = CM;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            if (!CT) CT = Texture2D.whiteTexture;

            mat.mainTexture = CT;
            mat.SetTexture("_BaseMap", CT);
            mat.color = Color.white;
            mr.material = mat;

            BoxCollider col = Obj.AddComponent<BoxCollider>();
            col.isTrigger = false;

            Rigidbody rb = Obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Aud = Obj.AddComponent<AudioSource>();
            Aud.spatialBlend = 1f;
            Aud.volume = 0.4f;

            Obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            if (GorillaLocomotion.GTPlayer.Instance)
                IgnoreCollisionRecursive(col, GorillaLocomotion.GTPlayer.Instance.transform);

            Done = true;
            
            if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                Obj.RegisterForNetwork();
        }

        static IEnumerator Do(string u, string t)
        {
            if (File.Exists(P_Tex) && new FileInfo(P_Tex).Length > 100)
            {
                CT = new Texture2D(2, 2);
                CT.LoadImage(File.ReadAllBytes(P_Tex));
            }
            else
            {
                UnityWebRequest tr = UnityWebRequestTexture.GetTexture(t);
                yield return tr.SendWebRequest();
                if (tr.result == UnityWebRequest.Result.Success)
                {
                    CT = DownloadHandlerTexture.GetContent(tr);
                    File.WriteAllBytes(P_Tex, tr.downloadHandler.data);
                }
            }

            string objData = "";
            if (File.Exists(P_Obj) && new FileInfo(P_Obj).Length > 100) objData = File.ReadAllText(P_Obj);
            else
            {
                UnityWebRequest r = UnityWebRequest.Get(u);
                yield return r.SendWebRequest();
                if (r.result == UnityWebRequest.Result.Success)
                {
                    objData = r.downloadHandler.text;
                    File.WriteAllText(P_Obj, objData);
                }
            }

            if (!string.IsNullOrEmpty(objData) && !objData.StartsWith("<"))
            {
                CM = Pars(objData);
                Spawn();
            }
            else Down = false;
        }

        static Mesh Pars(string s)
        {
            List<Vector3> v = new List<Vector3>(); List<Vector2> u = new List<Vector2>();
            List<Vector3> n = new List<Vector3>(); List<int> t = new List<int>();
            List<Vector3> nv = new List<Vector3>(); List<Vector2> nu = new List<Vector2>();
            List<Vector3> nn = new List<Vector3>();
            using (StringReader r = new StringReader(s))
            {
                string l;
                while ((l = r.ReadLine()) != null)
                {
                    if (l.Length < 2 || l[0] == '#') continue;
                    string[] p = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p[0] == "v") v.Add(new Vector3(-float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));
                    else if (p[0] == "vt") u.Add(new Vector2(float.Parse(p[1]), float.Parse(p[2])));
                    else if (p[0] == "vn") n.Add(new Vector3(-float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));
                    else if (p[0] == "f")
                    {
                        for (int i = 3; i >= 1; i--) Fix(p[i], v, u, n, nv, nu, nn, t);
                        if (p.Length == 5) { Fix(p[4], v, u, n, nv, nu, nn, t); Fix(p[3], v, u, n, nv, nu, nn, t); Fix(p[1], v, u, n, nv, nu, nn, t); }
                    }
                }
            }
            Mesh m = new Mesh();
            if (nv.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = nv.ToArray(); m.uv = nu.ToArray(); m.normals = nn.ToArray(); m.triangles = t.ToArray();
            m.RecalculateBounds(); m.RecalculateNormals(); return m;
        }

        static void Fix(string s, List<Vector3> v, List<Vector2> u, List<Vector3> n, List<Vector3> nv, List<Vector2> nu, List<Vector3> nn, List<int> t)
        {
            string[] c = s.Split('/'); nv.Add(v[int.Parse(c[0]) - 1]);
            if (c.Length > 1 && c[1] != "") nu.Add(u[int.Parse(c[1]) - 1]); else nu.Add(Vector2.zero);
            if (c.Length > 2 && c[2] != "") nn.Add(n[int.Parse(c[2]) - 1]); else nn.Add(Vector3.up);
            t.Add(nv.Count - 1);
        }
    }
}