using GorillaLocomotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ShibaGTGenesisReborn.Libs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

namespace ShibaGTGenesisReborn.Mods.Custom
{
    internal class FatSealSpammer : MonoBehaviour
    {
        public static FatSealSpammer Me;
        public static Mesh CM;
        public static Texture2D CT;
        public static bool Down;
        public static bool Done;
        public static List<GameObject> Seals = new List<GameObject>();
        public static PhysicsMaterial BouncyMat;

        static float lastSpawn = 0f;
        const float SpawnInterval = 0.08f;
        const float BaseScale = 2f;

        static string Dir => ModsLib.GenesisDirectory;
        static string P_Obj => Path.Combine(Dir, "fatseal.obj");
        static string P_Tex => Path.Combine(Dir, "fatseal.png");

        public static void SealLoop(string u, string t)
        {
            if (!Me)
            {
                GameObject g = new GameObject("FatSealSpammer");
                Me = g.AddComponent<FatSealSpammer>();
                DontDestroyOnLoad(g);
            }

            if (!Done && !Down && CM == null)
            {
                Down = true;
                Me.StartCoroutine(Load(u, t));
            }

            if (!Done) return;

            if (XRSettings.isDeviceActive)
            {
                bool vrPressed = InputHandler.Instance != null
                    && InputHandler.Instance.RightTrigger.IsPressed;
                if (vrPressed && Time.time - lastSpawn >= SpawnInterval)
                {
                    lastSpawn = Time.time;
                    SpawnSealVR();
                }
            }
            else
            {
                bool pcPressed = UnityInput.Current.GetMouseButton(0);
                if (pcPressed && Time.time - lastSpawn >= SpawnInterval)
                {
                    lastSpawn = Time.time;
                    SpawnSealPC();
                }
            }
        }

        static void SpawnSealVR()
        {
            var player = GTPlayer.Instance;
            if (!player) return;
            Transform hand = player.RightHand.controllerTransform;
            if (!hand) return;

            Vector3 origin = hand.position + hand.forward * 0.15f;
            Vector3 velocity = hand.forward * 6f + UnityEngine.Random.insideUnitSphere * 0.5f;
            BuildSeal(origin, hand.rotation, velocity);
        }

        static void SpawnSealPC()
        {
            var player = GTPlayer.Instance;
            if (!player) return;
            Camera cam = Camera.main;
            if (!cam) return;

            Ray ray = cam.ScreenPointToRay(UnityInput.Current.mousePosition);
            Vector3 target;
            if (Physics.Raycast(ray, out RaycastHit hit, 500f)) target = hit.point;
            else target = ray.origin + ray.direction * 30f;

            Vector3 origin = cam.transform.position + cam.transform.forward * 0.5f + cam.transform.right * 0.3f;
            Vector3 dir = (target - origin).normalized;
            Vector3 velocity = dir * 18f + UnityEngine.Random.insideUnitSphere * 0.3f;
            Quaternion rot = Quaternion.LookRotation(dir);
            BuildSeal(origin, rot, velocity);
        }

        static void BuildSeal(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            var player = GTPlayer.Instance;
            if (!player) return;

            GameObject seal = new GameObject("FatSeal");
            seal.layer = 8;
            seal.transform.position = position;
            seal.transform.rotation = rotation;
            seal.transform.localScale = new Vector3(BaseScale, BaseScale, BaseScale);

            seal.AddComponent<MeshFilter>().mesh = CM;
            MeshRenderer mr = seal.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Texture2D tex = CT ?? Texture2D.whiteTexture;
            mat.mainTexture = tex;
            mat.SetTexture("_BaseMap", tex);
            mat.color = Color.white;
            mr.material = mat;

            MeshCollider col = seal.AddComponent<MeshCollider>();
            col.convex = true;
            col.sharedMesh = CM;

            if (BouncyMat == null)
            {
                BouncyMat = new PhysicsMaterial("FatSealBouncy")
                {
                    bounciness = 1f,
                    dynamicFriction = 0f,
                    staticFriction = 0f,
                    bounceCombine = PhysicsMaterialCombine.Maximum,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                };
            }
            col.material = BouncyMat;

            Rigidbody rb = seal.AddComponent<Rigidbody>();
            rb.mass = 1.5f;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = velocity;
            rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 8f;

            IgnoreCollisionRecursive(col, player.transform);
            if (GorillaTagger.Instance && GorillaTagger.Instance.offlineVRRig != null)
                IgnoreCollisionRecursive(col, GorillaTagger.Instance.offlineVRRig.transform);
            if (player.bodyCollider) Physics.IgnoreCollision(col, player.bodyCollider, true);
            if (player.headCollider) Physics.IgnoreCollision(col, player.headCollider, true);

            Seals.Add(seal);
            
            if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                seal.RegisterForNetwork();
        }

        public static void Kill()
        {
            for (int i = 0; i < Seals.Count; i++)
            {
                if (Seals[i] != null && NetworkingLibrary.Instance != null)
                    Seals[i].UnregisterFromNetwork();
                if (Seals[i]) Destroy(Seals[i]);
            }
            Seals.Clear();
        }

        void FixedUpdate()
        {
            for (int i = 0; i < Seals.Count; i++)
            {
                GameObject s = Seals[i];
                if (!s) continue;
                if (!s.TryGetComponent(out Rigidbody rb)) continue;

                if (rb.velocity.sqrMagnitude < 0.5f)
                {
                    Vector3 kick = new Vector3(
                        UnityEngine.Random.Range(-2f, 2f),
                        UnityEngine.Random.Range(5f, 8f),
                        UnityEngine.Random.Range(-2f, 2f));
                    rb.AddForce(kick, ForceMode.VelocityChange);
                    
                    if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                        s.UpdateNetworkPosition();
                }
            }
        }

        static void IgnoreCollisionRecursive(Collider myCol, Transform target)
        {
            if (!myCol || !target) return;
            foreach (Collider c in target.GetComponentsInChildren<Collider>(true))
                Physics.IgnoreCollision(myCol, c, true);
        }

        static IEnumerator Load(string u, string t)
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
                Done = true;
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