using BepInEx;
using GorillaLocomotion;
using ShibaGTGenesisReborn.Libs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Mods.Custom
{
    internal class StunGrenadeManager : MonoBehaviour
    {
        public static StunGrenadeManager Instance;

        private const string ObjUrl = "https://raw.githubusercontent.com/incharilla1/assets/refs/heads/main/stun_grenade.obj";
        private const string MtlUrl = "https://raw.githubusercontent.com/incharilla1/assets/refs/heads/main/stun_grenade.mtl";
        private const string AudioUrl = "https://github.com/incharilla1/assets/raw/refs/heads/main/loud.mp3";

        private static string GenesisDir => ModsLib.GenesisDirectory;
        private static string LocalObjPath => Path.Combine(GenesisDir, "stun_grenade.obj");
        private static string LocalMtlPath => Path.Combine(GenesisDir, "stun_grenade.mtl");
        private static string LocalAudioPath => Path.Combine(GenesisDir, "loud.mp3");

        public static Mesh grenadeMesh;
        public static Material grenadeMaterial;
        public static AudioClip stunAudioClip;

        public static bool isLoaded;
        public static bool isDownloading;

        private static GameObject heldGrenadeObject;
        private static GameObject thrownGrenadeObject;
        private static Vector3 thrownGrenadePosition;
        private static Vector3 thrownGrenadeVelocity;

        private static GameObject screenOverlayObject;
        private static Material screenOverlayMaterial;
        private static MeshRenderer screenOverlayRenderer;

        private static bool isHoldingGrenade;
        private static bool isCooldownActive;
        private static bool isGrounded;
        private static float cooldownEndTime;
        private const float GrenadeRadius = 0.06f;
        private const float GrenadeScale = 1.6f;

        private static bool isBlindActive;
        private static float blindStartTime;
        private static float blindTotalDuration;
        private static float blindMaxIntensity;

        private void Update()
        {
            UpdateBlindEffect();
        }

        private static void UpdateBlindEffect()
        {
            if (!isBlindActive)
            {
                if (screenOverlayRenderer != null && screenOverlayRenderer.enabled)
                {
                    screenOverlayRenderer.enabled = false;
                }
                return;
            }

            float elapsed = Time.time - blindStartTime;
            if (elapsed >= blindTotalDuration)
            {
                isBlindActive = false;
                if (screenOverlayRenderer != null)
                {
                    screenOverlayRenderer.enabled = false;
                }
                return;
            }

            float fadeInDuration = Mathf.Min(0.25f, blindTotalDuration * 0.08f);
            float fadeOutDuration = Mathf.Min(2.5f, blindTotalDuration * 0.45f);

            float currentAlpha;
            if (elapsed < fadeInDuration)
            {
                float progress = elapsed / fadeInDuration;
                currentAlpha = blindMaxIntensity * Mathf.SmoothStep(0f, 1f, progress);
            }
            else if (elapsed < (blindTotalDuration - fadeOutDuration))
            {
                currentAlpha = blindMaxIntensity;
            }
            else
            {
                float fadeElapsed = elapsed - (blindTotalDuration - fadeOutDuration);
                float progress = Mathf.Clamp01(fadeElapsed / fadeOutDuration);
                currentAlpha = blindMaxIntensity * Mathf.SmoothStep(1f, 0f, progress);
            }

            EnsureOverlayExists();

            if (screenOverlayRenderer != null && screenOverlayMaterial != null)
            {
                screenOverlayRenderer.enabled = currentAlpha > 0.001f;
                screenOverlayMaterial.color = new Color(1f, 1f, 1f, currentAlpha);
            }
        }

        private static void EnsureOverlayExists()
        {
            Transform cameraTransform = GetCameraTransform();
            if (cameraTransform == null)
            {
                return;
            }

            if (screenOverlayObject == null)
            {
                screenOverlayObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                screenOverlayObject.name = "StunGrenade_ScreenOverlay";

                Collider quadCollider = screenOverlayObject.GetComponent<Collider>();
                if (quadCollider != null)
                {
                    Destroy(quadCollider);
                }

                Shader overlayShader = Shader.Find("GUI/Text Shader")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Transparent");

                screenOverlayMaterial = new Material(overlayShader)
                {
                    renderQueue = 4000
                };
                screenOverlayMaterial.mainTexture = Texture2D.whiteTexture;
                screenOverlayMaterial.color = new Color(1f, 1f, 1f, 0f);

                screenOverlayRenderer = screenOverlayObject.GetComponent<MeshRenderer>();
                screenOverlayRenderer.material = screenOverlayMaterial;
                screenOverlayRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                screenOverlayRenderer.receiveShadows = false;
                screenOverlayRenderer.enabled = false;
            }

            if (screenOverlayObject.transform.parent != cameraTransform)
            {
                screenOverlayObject.transform.SetParent(cameraTransform, false);
                screenOverlayObject.transform.localPosition = new Vector3(0f, 0f, 0.12f);
                screenOverlayObject.transform.localRotation = Quaternion.identity;
                screenOverlayObject.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            }
        }

        public static void TriggerStunBlind(float intensity, float duration)
        {
            EnsureInitialized();
            isBlindActive = true;
            blindStartTime = Time.time;
            blindTotalDuration = Mathf.Max(duration, 0.5f);
            blindMaxIntensity = Mathf.Clamp01(intensity);
            EnsureOverlayExists();
        }

        public static void StunLoop()
        {
            EnsureInitialized();

            if (!isLoaded && !isDownloading && grenadeMesh == null)
            {
                isDownloading = true;
                Instance.StartCoroutine(DownloadAndLoadAssets());
            }

            if (!isLoaded || grenadeMesh == null)
            {
                return;
            }

            bool isVr = GunLib.IsXRDeviceActive();
            bool rightGripPressed = isVr
                ? InputHandler.Instance.RightGrip.IsPressed
                : (Mouse.current?.rightButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.E);

            Vector3 handPosition = GetHandPalmPosition();
            Quaternion handRotation = GetHandPalmRotation();

            if (isCooldownActive)
            {
                float remainingTime = cooldownEndTime - Time.time;
                if (remainingTime > 0f)
                {
                    UpdateThrownGrenadePhysics();
                }
                else
                {
                    DetonateGrenade();
                }
                return;
            }

            if (rightGripPressed)
            {
                if (heldGrenadeObject == null)
                {
                    heldGrenadeObject = CreateGrenadeGameObject(handPosition, handRotation);
                }

                heldGrenadeObject.transform.position = handPosition;
                heldGrenadeObject.transform.rotation = handRotation * Quaternion.Euler(0f, 90f, 0f);
                isHoldingGrenade = true;
            }
            else if (isHoldingGrenade)
            {
                isHoldingGrenade = false;
                if (heldGrenadeObject != null)
                {
                    Destroy(heldGrenadeObject);
                    heldGrenadeObject = null;
                }

                Vector3 throwVelocity = ModsLib.GetHandThrowVelocity(false);
                thrownGrenadePosition = handPosition;
                thrownGrenadeVelocity = throwVelocity;
                isGrounded = false;
                thrownGrenadeObject = CreateGrenadeGameObject(thrownGrenadePosition, handRotation);

                cooldownEndTime = Time.time + 3.0f;
                isCooldownActive = true;
            }
        }

        private static void UpdateThrownGrenadePhysics()
        {
            if (isGrounded)
            {
                if (Physics.Raycast(thrownGrenadePosition + Vector3.up * 0.15f, Vector3.down, out RaycastHit groundHit, 0.35f, GunLib.BypassLayers))
                {
                    thrownGrenadePosition = groundHit.point + groundHit.normal * GrenadeRadius;
                }

                thrownGrenadeVelocity.x *= 0.85f;
                thrownGrenadeVelocity.z *= 0.85f;
                thrownGrenadeVelocity.y = 0f;
            }
            else
            {
                thrownGrenadeVelocity += Physics.gravity * Time.deltaTime;
                Vector3 displacement = thrownGrenadeVelocity * Time.deltaTime;
                float distance = displacement.magnitude;

                if (distance > 0.0001f)
                {
                    Vector3 direction = displacement / distance;

                    if (Physics.SphereCast(thrownGrenadePosition, GrenadeRadius, direction, out RaycastHit hit, distance, GunLib.BypassLayers) ||
                        Physics.Raycast(thrownGrenadePosition, direction, out hit, distance + GrenadeRadius, GunLib.BypassLayers))
                    {
                        thrownGrenadePosition = hit.point + hit.normal * GrenadeRadius;

                        if (hit.normal.y > 0.5f && thrownGrenadeVelocity.magnitude < 2.5f)
                        {
                            isGrounded = true;
                            thrownGrenadeVelocity = Vector3.zero;
                        }
                        else
                        {
                            thrownGrenadeVelocity = Vector3.Reflect(thrownGrenadeVelocity, hit.normal) * 0.4f;
                        }
                    }
                    else
                    {
                        thrownGrenadePosition += displacement;
                    }
                }

                if (thrownGrenadeObject != null && !isGrounded)
                {
                    thrownGrenadeObject.transform.Rotate(Vector3.right, 360f * Time.deltaTime, Space.Self);
                    thrownGrenadeObject.transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);
                }
            }

            if (thrownGrenadeObject != null)
            {
                thrownGrenadeObject.transform.position = thrownGrenadePosition;
            }
        }

        private static void DetonateGrenade()
        {
            isCooldownActive = false;
            isGrounded = false;

            Vector3 explosionPosition = thrownGrenadePosition;

            if (thrownGrenadeObject != null)
            {
                explosionPosition = thrownGrenadeObject.transform.position;
                Destroy(thrownGrenadeObject);
                thrownGrenadeObject = null;
            }

            float audioDuration = 5.0f;
            if (stunAudioClip != null)
            {
                audioDuration = stunAudioClip.length;

                GameObject audioObj = new GameObject("StunGrenadeAudio");
                audioObj.transform.position = explosionPosition;
                AudioSource audioSource = audioObj.AddComponent<AudioSource>();
                audioSource.clip = stunAudioClip;
                audioSource.volume = 1.0f;
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 2f;
                audioSource.maxDistance = 150f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.Play();
                Destroy(audioObj, stunAudioClip.length + 0.5f);
            }

            Transform cameraTransform = GetCameraTransform();
            if (cameraTransform != null)
            {
                Vector3 cameraPosition = cameraTransform.position;
                Vector3 vectorToExplosion = explosionPosition - cameraPosition;
                float distance = vectorToExplosion.magnitude;

                if (distance > 0.001f)
                {
                    Vector3 directionToExplosion = vectorToExplosion / distance;
                    float viewAngle = Vector3.Angle(cameraTransform.forward, directionToExplosion);

                    bool hasDirectLineOfSight = !Physics.Raycast(
                        cameraPosition,
                        directionToExplosion,
                        out RaycastHit _,
                        Mathf.Max(0.01f, distance - 0.15f),
                        GunLib.BypassLayers);

                    const float maxBlindingDistance = 35f;
                    if (hasDirectLineOfSight && distance <= maxBlindingDistance)
                    {
                        const float maxViewAngle = 70f;
                        if (viewAngle <= maxViewAngle)
                        {
                            float angleFactor = Mathf.Clamp01((maxViewAngle - viewAngle) / 40f);
                            float distanceFactor = Mathf.Clamp01(1f - (distance / maxBlindingDistance));

                            float intensity = angleFactor * (0.6f + 0.4f * distanceFactor);
                            if (viewAngle <= 40f)
                            {
                                intensity = 1.0f;
                            }

                            if (intensity > 0.1f)
                            {
                                TriggerStunBlind(intensity, audioDuration);
                            }
                        }
                    }
                }
            }
        }

        private static GameObject CreateGrenadeGameObject(Vector3 position, Quaternion rotation)
        {
            GameObject grenade = new GameObject("StunGrenade");
            grenade.transform.position = position;
            grenade.transform.rotation = rotation;
            grenade.transform.localScale = Vector3.one * GrenadeScale;

            MeshFilter mf = grenade.AddComponent<MeshFilter>();
            mf.mesh = grenadeMesh;

            MeshRenderer mr = grenade.AddComponent<MeshRenderer>();
            mr.material = grenadeMaterial;

            return grenade;
        }

        public static void Kill()
        {
            isHoldingGrenade = false;
            isCooldownActive = false;
            isGrounded = false;
            isBlindActive = false;

            if (screenOverlayRenderer != null)
            {
                screenOverlayRenderer.enabled = false;
            }

            if (heldGrenadeObject != null)
            {
                Destroy(heldGrenadeObject);
                heldGrenadeObject = null;
            }

            if (thrownGrenadeObject != null)
            {
                Destroy(thrownGrenadeObject);
                thrownGrenadeObject = null;
            }
        }

        private static Transform GetCameraTransform()
        {
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.mainCamera != null)
            {
                return GorillaTagger.Instance.mainCamera.transform;
            }

            if (Camera.main != null)
            {
                return Camera.main.transform;
            }

            return null;
        }

        private void OnDestroy()
        {
            if (screenOverlayObject != null)
            {
                Destroy(screenOverlayObject);
                screenOverlayObject = null;
            }

            if (screenOverlayMaterial != null)
            {
                Destroy(screenOverlayMaterial);
                screenOverlayMaterial = null;
            }
        }

        private static Vector3 GetHandPalmPosition()
        {
            if (GTPlayer.Instance != null)
            {
                Transform rightController = GTPlayer.Instance.RightHand.controllerTransform;
                if (rightController != null)
                {
                    return rightController.position + rightController.rotation * GTPlayer.Instance.RightHand.handOffset;
                }
            }

            if (GorillaTagger.Instance != null && GorillaTagger.Instance.rightHandTransform != null)
            {
                return GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.forward * 0.08f;
            }

            return Vector3.zero;
        }

        private static Quaternion GetHandPalmRotation()
        {
            if (GTPlayer.Instance != null)
            {
                Transform rightController = GTPlayer.Instance.RightHand.controllerTransform;
                if (rightController != null)
                {
                    return rightController.rotation * GTPlayer.Instance.RightHand.handRotOffset;
                }
            }

            if (GorillaTagger.Instance != null && GorillaTagger.Instance.rightHandTransform != null)
            {
                return GorillaTagger.Instance.rightHandTransform.rotation;
            }

            return Quaternion.identity;
        }

        private static void EnsureInitialized()
        {
            if (!Instance)
            {
                GameObject managerObject = new GameObject("StunGrenadeManager");
                Instance = managerObject.AddComponent<StunGrenadeManager>();
                DontDestroyOnLoad(managerObject);
            }
        }

        private static IEnumerator DownloadAndLoadAssets()
        {
            if (!Directory.Exists(GenesisDir))
            {
                Directory.CreateDirectory(GenesisDir);
            }

            if (!File.Exists(LocalAudioPath) || new FileInfo(LocalAudioPath).Length < 100)
            {
                using UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(AudioUrl, AudioType.MPEG);
                yield return audioRequest.SendWebRequest();
                if (audioRequest.result == UnityWebRequest.Result.Success)
                {
                    stunAudioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                    File.WriteAllBytes(LocalAudioPath, audioRequest.downloadHandler.data);
                }
            }
            else
            {
                using UnityWebRequest localAudioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + LocalAudioPath, AudioType.MPEG);
                yield return localAudioRequest.SendWebRequest();
                if (localAudioRequest.result == UnityWebRequest.Result.Success)
                {
                    stunAudioClip = DownloadHandlerAudioClip.GetContent(localAudioRequest);
                }
            }

            string mtlContent = "";
            if (File.Exists(LocalMtlPath) && new FileInfo(LocalMtlPath).Length > 10)
            {
                mtlContent = File.ReadAllText(LocalMtlPath);
            }
            else
            {
                using UnityWebRequest mtlRequest = UnityWebRequest.Get(MtlUrl);
                yield return mtlRequest.SendWebRequest();
                if (mtlRequest.result == UnityWebRequest.Result.Success)
                {
                    mtlContent = mtlRequest.downloadHandler.text;
                    File.WriteAllText(LocalMtlPath, mtlContent);
                }
            }

            string objContent = "";
            if (File.Exists(LocalObjPath) && new FileInfo(LocalObjPath).Length > 100)
            {
                objContent = File.ReadAllText(LocalObjPath);
            }
            else
            {
                using UnityWebRequest objRequest = UnityWebRequest.Get(ObjUrl);
                yield return objRequest.SendWebRequest();
                if (objRequest.result == UnityWebRequest.Result.Success)
                {
                    objContent = objRequest.downloadHandler.text;
                    File.WriteAllText(LocalObjPath, objContent);
                }
            }

            if (!string.IsNullOrEmpty(objContent) && !objContent.StartsWith("<"))
            {
                grenadeMesh = ParseObj(objContent);
                Color grenadeColor = ParseMtlColor(mtlContent);
                grenadeMaterial = CreateGrenadeMaterial(grenadeColor);
                isLoaded = true;
            }

            isDownloading = false;
        }

        private static Mesh ParseObj(string objText)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> outVertices = new List<Vector3>();
            List<Vector2> outUvs = new List<Vector2>();
            List<Vector3> outNormals = new List<Vector3>();
            List<int> triangles = new List<int>();

            using (StringReader reader = new StringReader(objText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length < 2 || line.StartsWith("#")) continue;

                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    if (parts[0] == "v" && parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            vertices.Add(new Vector3(-x, y, z));
                        }
                    }
                    else if (parts[0] == "vt" && parts.Length >= 3)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float u) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        {
                            uvs.Add(new Vector2(u, v));
                        }
                    }
                    else if (parts[0] == "vn" && parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            normals.Add(new Vector3(-x, y, z));
                        }
                    }
                    else if (parts[0] == "f" && parts.Length >= 4)
                    {
                        int count = parts.Length - 1;
                        for (int i = 1; i <= count - 2; i++)
                        {
                            AddVertex(parts[1], vertices, uvs, normals, outVertices, outUvs, outNormals, triangles);
                            AddVertex(parts[i + 2], vertices, uvs, normals, outVertices, outUvs, outNormals, triangles);
                            AddVertex(parts[i + 1], vertices, uvs, normals, outVertices, outUvs, outNormals, triangles);
                        }
                    }
                }
            }

            Mesh mesh = new Mesh();
            if (outVertices.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.vertices = outVertices.ToArray();
            if (outUvs.Count == outVertices.Count) mesh.uv = outUvs.ToArray();
            if (outNormals.Count == outVertices.Count) mesh.normals = outNormals.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void AddVertex(string vertexString, List<Vector3> v, List<Vector2> vt, List<Vector3> vn, List<Vector3> outV, List<Vector2> outVt, List<Vector3> outVn, List<int> outTris)
        {
            string[] tokens = vertexString.Split('/');
            if (tokens.Length > 0 && int.TryParse(tokens[0], out int vIndex) && vIndex > 0 && vIndex <= v.Count)
            {
                outV.Add(v[vIndex - 1]);
            }
            else
            {
                outV.Add(Vector3.zero);
            }

            if (tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1]) && int.TryParse(tokens[1], out int vtIndex) && vtIndex > 0 && vtIndex <= vt.Count)
            {
                outVt.Add(vt[vtIndex - 1]);
            }
            else
            {
                outVt.Add(Vector2.zero);
            }

            if (tokens.Length > 2 && !string.IsNullOrEmpty(tokens[2]) && int.TryParse(tokens[2], out int vnIndex) && vnIndex > 0 && vnIndex <= vn.Count)
            {
                outVn.Add(vn[vnIndex - 1]);
            }
            else
            {
                outVn.Add(Vector3.up);
            }

            outTris.Add(outV.Count - 1);
        }

        private static Color ParseMtlColor(string mtlText)
        {
            Color fallbackColor = new Color(0.24f, 0.32f, 0.20f, 1f);
            if (string.IsNullOrEmpty(mtlText)) return fallbackColor;

            using StringReader reader = new StringReader(mtlText);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("Kd "))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float r) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float g) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                    {
                        float maxChannel = Mathf.Max(r, g, b);
                        if (maxChannel < 0.12f)
                        {
                            return new Color(0.24f, 0.32f, 0.20f, 1f);
                        }
                        return new Color(r, g, b, 1f);
                    }
                }
            }

            return fallbackColor;
        }

        private static Material CreateGrenadeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("GorillaTag/UberShader") ?? Shader.Find("Sprites/Default");
            Material material = new Material(shader)
            {
                color = color
            };

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            material.mainTexture = texture;

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.6f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.5f);

            return material;
        }
    }
}