using BepInEx;
using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShibaGTGenesisReborn.Libs
{
    public static class ModsLib
    {
        #region Path Utilities
        private static string genesisDirectory;

        public static string GenesisDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(genesisDirectory))
                {
                    string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                    string assemblyDir = !string.IsNullOrEmpty(assemblyLocation) ? Path.GetDirectoryName(assemblyLocation) : null;
                    if (string.IsNullOrEmpty(assemblyDir))
                    {
                        assemblyDir = Paths.PluginPath;
                    }
                    if (string.IsNullOrEmpty(assemblyDir))
                    {
                        assemblyDir = AppDomain.CurrentDomain.BaseDirectory;
                    }

                    genesisDirectory = Path.Combine(assemblyDir, "Genesis");
                }

                if (!Directory.Exists(genesisDirectory))
                {
                    Directory.CreateDirectory(genesisDirectory);
                }

                return genesisDirectory;
            }
        }
        #endregion

        #region Ender Pearl Utilities
        private static Texture2D enderPearlTexture;
        private static Material enderPearlMaterial;

        public static Texture2D GetPearlTexture()
        {
            if (enderPearlTexture != null)
            {
                return enderPearlTexture;
            }

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();
                string pearlResource = resourceNames.FirstOrDefault(name => name.EndsWith("pearl.png", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(pearlResource))
                {
                    using Stream stream = assembly.GetManifestResourceStream(pearlResource);
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        enderPearlTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (enderPearlTexture.LoadImage(buffer))
                        {
                            return enderPearlTexture;
                        }
                    }
                }
            }
            catch { }

            string[] candidatePaths = new[]
            {
                Path.Combine(GenesisDirectory, "pearl.png"),
                Path.Combine(GenesisDirectory, "Resources", "pearl.png"),
                Path.Combine(Paths.PluginPath, "Genesis", "pearl.png"),
                Path.Combine(Paths.PluginPath, "Resources", "pearl.png"),
                Path.Combine(Paths.PluginPath, "pearl.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Genesis", "pearl.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "pearl.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", "Genesis", "pearl.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", "Resources", "pearl.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", "pearl.png"),
                "Resources/pearl.png",
                "Resources\\pearl.png",
                "pearl.png"
            };

            foreach (string path in candidatePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        byte[] fileBytes = File.ReadAllBytes(path);
                        enderPearlTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (enderPearlTexture.LoadImage(fileBytes))
                        {
                            return enderPearlTexture;
                        }
                    }
                    catch { }
                }
            }

            const int size = 64;
            enderPearlTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance > radius)
                    {
                        enderPearlTexture.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float normalizedDistance = distance / radius;
                        Color pixelColor = normalizedDistance < 0.25f
                            ? new Color(0.04f, 0.2f, 0.16f, 1f)
                            : (normalizedDistance < 0.7f ? new Color(0.12f, 0.95f, 0.76f, 1f) : new Color(0.02f, 0.4f, 0.3f, 1f));
                        enderPearlTexture.SetPixel(x, y, pixelColor);
                    }
                }
            }
            enderPearlTexture.Apply();
            return enderPearlTexture;
        }

        public static Material GetEnderPearlMaterial()
        {
            if (enderPearlMaterial == null)
            {
                Shader spriteShader = Shader.Find("Sprites/Default") ?? Shader.Find("GUI/Text Shader") ?? Shader.Find("Unlit/Transparent");
                enderPearlMaterial = new Material(spriteShader)
                {
                    mainTexture = GetPearlTexture()
                };
            }

            return enderPearlMaterial;
        }

        public static GameObject CreatePearlVisual(string objectName, Vector3 initialPosition, float scale = 0.14f)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            UnityEngine.Object.Destroy(visual.GetComponent<Collider>());
            visual.name = objectName;
            visual.transform.localScale = new Vector3(scale, scale, scale);
            visual.transform.position = initialPosition;

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = GetEnderPearlMaterial();
            }

            return visual;
        }

        public static Vector3 GetHandThrowVelocity(bool isLeftHand)
        {
            if (GunLib.IsXRDeviceActive())
            {
                var tracker = isLeftHand ? GTPlayer.Instance.LeftHand.velocityTracker : GTPlayer.Instance.RightHand.velocityTracker;
                Transform handTransform = isLeftHand ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;

                Vector3 trackedVelocity = tracker != null ? tracker.GetAverageVelocity(true, 0.05f) : Vector3.zero;

                if (trackedVelocity.sqrMagnitude > 0.08f)
                {
                    return trackedVelocity * 1.15f;
                }

                return handTransform != null ? -handTransform.up * 3f : Vector3.down * 3f;
            }

            Camera mainCamera = Camera.main != null ? Camera.main : GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
            return mainCamera != null ? (mainCamera.transform.forward * 20f + mainCamera.transform.up * 2f) : (Vector3.forward * 20f);
        }
        #endregion
        #region Zipline Utilities
        private static Material canyonsZiplineMaterial;

        public static Material GetCanyonsZiplineMaterial()
        {
            if (canyonsZiplineMaterial == null)
            {
                var existingZipline = UnityEngine.Object.FindObjectsByType<GorillaLocomotion.Gameplay.GorillaZipline>(FindObjectsSortMode.None);
                if (existingZipline != null && existingZipline.Length > 0)
                {
                    var renderer = existingZipline[0].GetComponentInChildren<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        canyonsZiplineMaterial = new Material(renderer.sharedMaterial);
                    }
                }

                if (canyonsZiplineMaterial == null)
                {
                    Shader uberShader = Shader.Find("GorillaTag/UberShader") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                    canyonsZiplineMaterial = new Material(uberShader)
                    {
                        color = new Color(0.14f, 0.14f, 0.15f, 1f)
                    };
                }
            }

            return canyonsZiplineMaterial;
        }

        public static Vector3 CalculateClosestPointOnSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point, out float normalizedDistance)
        {
            Vector3 segmentVector = segmentEnd - segmentStart;
            float segmentLengthSquared = segmentVector.sqrMagnitude;

            if (segmentLengthSquared < 0.0001f)
            {
                normalizedDistance = 0f;
                return segmentStart;
            }

            normalizedDistance = Mathf.Clamp01(Vector3.Dot(point - segmentStart, segmentVector) / segmentLengthSquared);
            return segmentStart + segmentVector * normalizedDistance;
        }

        public static void CreateZiplineVisual(
            Vector3 startPoint,
            Vector3 endPoint,
            ref GameObject cableObject,
            ref LineRenderer lineRenderer,
            ref GameObject startAnchor,
            ref GameObject endAnchor)
        {
            if (cableObject == null)
            {
                cableObject = new GameObject("CustomZiplineCable");
                lineRenderer = cableObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.026f;
                lineRenderer.endWidth = 0.026f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = GetCanyonsZiplineMaterial();
                lineRenderer.startColor = new Color(0.16f, 0.16f, 0.17f, 1f);
                lineRenderer.endColor = new Color(0.16f, 0.16f, 0.17f, 1f);
                lineRenderer.numCapVertices = 6;
                lineRenderer.numCornerVertices = 6;
            }

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);

            if (startAnchor == null)
            {
                startAnchor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                UnityEngine.Object.Destroy(startAnchor.GetComponent<Collider>());
                startAnchor.name = "ZiplineStartAnchor";
                startAnchor.transform.localScale = new Vector3(0.1f, 0.04f, 0.1f);
                Renderer renderer = startAnchor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.shader = Shader.Find("GorillaTag/UberShader") ?? Shader.Find("GUI/Text Shader");
                    renderer.material.color = new Color(0.22f, 0.22f, 0.24f, 1f);
                }
            }
            startAnchor.transform.position = startPoint;
            startAnchor.transform.rotation = Quaternion.LookRotation((endPoint - startPoint).normalized) * Quaternion.Euler(90f, 0f, 0f);

            if (endAnchor == null)
            {
                endAnchor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                UnityEngine.Object.Destroy(endAnchor.GetComponent<Collider>());
                endAnchor.name = "ZiplineEndAnchor";
                endAnchor.transform.localScale = new Vector3(0.1f, 0.04f, 0.1f);
                Renderer renderer = endAnchor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.shader = Shader.Find("GorillaTag/UberShader") ?? Shader.Find("GUI/Text Shader");
                    renderer.material.color = new Color(0.22f, 0.22f, 0.24f, 1f);
                }
            }
            endAnchor.transform.position = endPoint;
            endAnchor.transform.rotation = Quaternion.LookRotation((endPoint - startPoint).normalized) * Quaternion.Euler(90f, 0f, 0f);
        }

        public static void DestroyZiplineVisual(
            ref GameObject cableObject,
            ref LineRenderer lineRenderer,
            ref GameObject startAnchor,
            ref GameObject endAnchor)
        {
            if (cableObject != null)
            {
                UnityEngine.Object.Destroy(cableObject);
                cableObject = null;
                lineRenderer = null;
            }

            if (startAnchor != null)
            {
                UnityEngine.Object.Destroy(startAnchor);
                startAnchor = null;
            }

            if (endAnchor != null)
            {
                UnityEngine.Object.Destroy(endAnchor);
                endAnchor = null;
            }
        }
        #endregion
    }
}
