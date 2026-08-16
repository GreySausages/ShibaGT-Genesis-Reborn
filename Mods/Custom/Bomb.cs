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

namespace ShibaGTGenesisReborn.Mods
{
    internal class BombManager : MonoBehaviour
    {
        public static BombManager Instance;

        private const string ObjUrl = "https://raw.githubusercontent.com/incharilla1/assets/refs/heads/main/Bomb.obj";
        private const string TextureUrl = "https://raw.githubusercontent.com/incharilla1/assets/refs/heads/main/Bomb.png";
        private const string AudioUrl = "https://github.com/incharilla1/assets/raw/refs/heads/main/explode_bomb.mp3";

        private static string GenesisDir => ModsLib.GenesisDirectory;
        private static string LocalObjPath => Path.Combine(GenesisDir, "Bomb.obj");
        private static string LocalTexturePath => Path.Combine(GenesisDir, "Bomb.png");
        private static string LocalAudioPath => Path.Combine(GenesisDir, "explode_bomb.mp3");

        public static Mesh bombMesh;
        public static Texture2D bombTexture;
        public static Material bombMaterial;
        public static AudioClip bombAudioClip;

        public static bool isLoaded;
        public static bool isDownloading;

        private static GameObject activeBombObject;
        private static Vector3 activeBombPosition;
        private static float detonationTimestamp;
        private static bool isBombActive;
        private static bool wasGripPressedLastFrame;

        private const float BombScale = 0.35f;
        private const float FuseDurationSeconds = 3.0f;

        public static void BombLoop()
        {
            EnsureInitialized();

            if (!isLoaded && !isDownloading && bombMesh == null)
            {
                isDownloading = true;
                Instance.StartCoroutine(DownloadAndLoadAssets());
            }

            if (!isLoaded || bombMesh == null)
            {
                return;
            }

            bool isGripPressed = CheckRightGripPressed();

            if (isBombActive)
            {
                if (Time.time >= detonationTimestamp)
                {
                    DetonateBomb();
                }
                wasGripPressedLastFrame = isGripPressed;
                return;
            }

            if (isGripPressed && !wasGripPressedLastFrame)
            {
                SpawnBomb();
            }

            wasGripPressedLastFrame = isGripPressed;
        }

        private static void SpawnBomb()
        {
            Vector3 handPosition = GetRightHandPosition();
            Quaternion handRotation = GetRightHandRotation();

            activeBombObject = CreateBombGameObject(handPosition, handRotation);
            activeBombPosition = handPosition;
            detonationTimestamp = Time.time + FuseDurationSeconds;
            isBombActive = true;
        }

        private static GameObject CreateBombGameObject(Vector3 position, Quaternion rotation)
        {
            GameObject bomb = new GameObject("CustomBomb");
            bomb.transform.position = position;
            bomb.transform.rotation = rotation;
            bomb.transform.localScale = Vector3.one * BombScale;

            MeshFilter meshFilter = bomb.AddComponent<MeshFilter>();
            meshFilter.mesh = bombMesh;

            MeshRenderer meshRenderer = bomb.AddComponent<MeshRenderer>();
            meshRenderer.material = bombMaterial;

            if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
            {
                bomb.RegisterForNetwork();
            }

            return bomb;
        }

        private static void DetonateBomb()
        {
            isBombActive = false;

            Vector3 explosionPosition = activeBombPosition;

            if (activeBombObject != null)
            {
                explosionPosition = activeBombObject.transform.position;

                if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                {
                    activeBombObject.UnregisterFromNetwork();
                }

                Destroy(activeBombObject);
                activeBombObject = null;
            }

            if (bombAudioClip != null)
            {
                GameObject explosionAudioSource = new GameObject("BombExplosionAudio");
                explosionAudioSource.transform.position = explosionPosition;

                AudioSource audioSource = explosionAudioSource.AddComponent<AudioSource>();
                audioSource.clip = bombAudioClip;
                audioSource.volume = 1.0f;
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 2f;
                audioSource.maxDistance = 150f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.Play();

                Destroy(explosionAudioSource, bombAudioClip.length + 0.5f);
            }
        }

        public static void Kill()
        {
            isBombActive = false;
            wasGripPressedLastFrame = false;

            if (activeBombObject != null)
            {
                if (NetworkingLibrary.Instance != null && NetworkingLibrary.Instance.NetworkEnabled)
                {
                    activeBombObject.UnregisterFromNetwork();
                }

                Destroy(activeBombObject);
                activeBombObject = null;
            }
        }

        private static bool CheckRightGripPressed()
        {
            if (GunLib.IsXRDeviceActive())
            {
                return InputHandler.Instance != null && InputHandler.Instance.RightGrip.IsPressed;
            }

            return (Mouse.current?.rightButton.isPressed ?? false) || UnityInput.Current.GetKey(KeyCode.E) || UnityInput.Current.GetMouseButton(1);
        }

        private static Vector3 GetRightHandPosition()
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
                return GorillaTagger.Instance.rightHandTransform.position;
            }

            return Vector3.zero;
        }

        private static Quaternion GetRightHandRotation()
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
                GameObject managerObject = new GameObject("BombManager");
                Instance = managerObject.AddComponent<BombManager>();
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
                    bombAudioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                    File.WriteAllBytes(LocalAudioPath, audioRequest.downloadHandler.data);
                }
            }
            else
            {
                using UnityWebRequest localAudioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + LocalAudioPath, AudioType.MPEG);
                yield return localAudioRequest.SendWebRequest();
                if (localAudioRequest.result == UnityWebRequest.Result.Success)
                {
                    bombAudioClip = DownloadHandlerAudioClip.GetContent(localAudioRequest);
                }
            }

            if (File.Exists(LocalTexturePath) && new FileInfo(LocalTexturePath).Length > 100)
            {
                bombTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bombTexture.LoadImage(File.ReadAllBytes(LocalTexturePath));
            }
            else
            {
                using UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(TextureUrl);
                yield return textureRequest.SendWebRequest();
                if (textureRequest.result == UnityWebRequest.Result.Success)
                {
                    bombTexture = DownloadHandlerTexture.GetContent(textureRequest);
                    File.WriteAllBytes(LocalTexturePath, textureRequest.downloadHandler.data);
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
                bombMesh = ParseObj(objContent);
                bombMaterial = CreateBombMaterial(bombTexture);
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
                    if (line.Length < 2 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        continue;
                    }

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
            if (outUvs.Count == outVertices.Count)
            {
                mesh.uv = outUvs.ToArray();
            }
            if (outNormals.Count == outVertices.Count)
            {
                mesh.normals = outNormals.ToArray();
            }
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void AddVertex(
            string vertexString,
            List<Vector3> sourceVertices,
            List<Vector2> sourceUvs,
            List<Vector3> sourceNormals,
            List<Vector3> outputVertices,
            List<Vector2> outputUvs,
            List<Vector3> outputNormals,
            List<int> outputTriangles)
        {
            string[] tokens = vertexString.Split('/');
            if (tokens.Length > 0 && int.TryParse(tokens[0], out int vertexIndex) && vertexIndex > 0 && vertexIndex <= sourceVertices.Count)
            {
                outputVertices.Add(sourceVertices[vertexIndex - 1]);
            }
            else
            {
                outputVertices.Add(Vector3.zero);
            }

            if (tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1]) && int.TryParse(tokens[1], out int uvIndex) && uvIndex > 0 && uvIndex <= sourceUvs.Count)
            {
                outputUvs.Add(sourceUvs[uvIndex - 1]);
            }
            else
            {
                outputUvs.Add(Vector2.zero);
            }

            if (tokens.Length > 2 && !string.IsNullOrEmpty(tokens[2]) && int.TryParse(tokens[2], out int normalIndex) && normalIndex > 0 && normalIndex <= sourceNormals.Count)
            {
                outputNormals.Add(sourceNormals[normalIndex - 1]);
            }
            else
            {
                outputNormals.Add(Vector3.up);
            }

            outputTriangles.Add(outputVertices.Count - 1);
        }

        private static Material CreateBombMaterial(Texture2D texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("GorillaTag/UberShader")
                ?? Shader.Find("Sprites/Default");

            Material material = new Material(shader)
            {
                mainTexture = texture != null ? texture : Texture2D.whiteTexture,
                color = Color.white
            };

            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", Color.white);
                }
            }

            return material;
        }
    }
}
