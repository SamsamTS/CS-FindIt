// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework;
using UnityEngine;

namespace FindIt.GUI
{
    public class PreviewRenderer : MonoBehaviour
    {
        private Camera renderCamera;
        private Mesh currentMesh;
        private Bounds currentBounds;
        private float currentRotation = 120f;
        private float currentZoom = 4f;
        private Material _material;


        /// <summary>
        /// Sets material to render.
        /// </summary>
        public Material Material { set => _material = value; }


        /// <summary>
        /// Initialise the new renderer object.
        /// </summary>
        public PreviewRenderer()
        {
            // Set up camera.
            renderCamera = new GameObject("Camera").AddComponent<Camera>();
            renderCamera.transform.SetParent(transform);
            renderCamera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            renderCamera.allowHDR = true;
            renderCamera.enabled = false;

            // Basic defaults.
            renderCamera.pixelRect = new Rect(0f, 0f, 512, 512);
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            renderCamera.fieldOfView = 30f;
            renderCamera.nearClipPlane = 1f;
            renderCamera.farClipPlane = 1000f;
        }


        /// <summary>
        /// Image size.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(renderCamera.targetTexture.width, renderCamera.targetTexture.height);

            set
            {
                if (Size != value)
                {
                    // New size; set camera output sizes accordingly.
                    renderCamera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    renderCamera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }


        /// <summary>
        /// Sets mesh and material from a prefab.
        /// </summary>
        /// <param name="prefab">Prefab to render</param>
        /// <returns>True if the target was valid (valid mesh and material)</returns>
        public bool SetTarget(PrefabInfo prefab)
        {
            if (prefab is BuildingInfo buildingPrefab)
            {
                Mesh = buildingPrefab.m_mesh;
                _material = buildingPrefab.m_material;
            }
            else if (prefab is PropInfo propPrefab)
            {
                Mesh = propPrefab.m_mesh;
                _material = propPrefab.m_material;
            }
            else if (prefab is TreeInfo treePrefab)
            {
                Mesh = treePrefab.m_mesh;
                _material = treePrefab.m_material;
            }
            else
            {
                Debugging.Message("invalid prefab type for " + prefab.name);
                return false;
            }

            return Mesh != null && _material != null;
        }


        /// <summary>
        /// Currently rendered mesh.
        /// </summary>
        public Mesh Mesh
        {
            get => currentMesh;

            set
            {
                if (currentMesh != value)
                {
                    // Update currently rendered mesh if changed.
                    currentMesh = value;

                    if (value != null)
                    {
                        // Reset the bounding box to be the smallest that can encapsulate all verticies of the new mesh.
                        // That way the preview image is the largest size that fits cleanly inside the preview size.
                        currentBounds = new Bounds(Vector3.zero, Vector3.zero);

                        // Use separate verticies instance instead of accessing Mesh.vertices each time (which is slow).
                        // >10x measured performance improvement by doing things this way instead.
                        Vector3[] vertices = Mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            currentBounds.Encapsulate(vertices[i]);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Current building texture.
        /// </summary>
        public RenderTexture Texture
        {
            get => renderCamera.targetTexture;
        }


        /// <summary>
        /// Preview camera rotation (degrees).
        /// </summary>
        public float CameraRotation
        {
            get { return currentRotation; }
            set { currentRotation = value % 360f; }
        }


        /// <summary>
        /// Zoom level.
        /// </summary>
        public float Zoom
        {
            get { return currentZoom; }
            set
            {
                currentZoom = Mathf.Clamp(value, 0.5f, 5f);
            }
        }


        /// <summary>
        /// Render the current mesh.
        /// </summary>
        public void Render()
        {
            if (currentMesh == null)
            {
                return;
            }

            // Set background - plain.
            renderCamera.clearFlags = CameraClearFlags.Color;

            // Back up current game InfoManager mode.
            InfoManager infoManager = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMode = infoManager.CurrentMode;
            InfoManager.SubInfoMode currentSubMode = infoManager.CurrentSubMode; ;

            // Set current game InfoManager to default (don't want to render with an overlay mode).
            infoManager.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            infoManager.UpdateInfoMode();

            // Backup current exposure and sky tint.
            float gameExposure = DayNightProperties.instance.m_Exposure;
            Color gameSkyTint = DayNightProperties.instance.m_SkyTint;

            // Backup current game lighting.
            Light gameMainLight = RenderManager.instance.MainLight;

            // Set exposure and sky tint for render.
            DayNightProperties.instance.m_Exposure = 0.5f;
            DayNightProperties.instance.m_SkyTint = new Color(0, 0, 0);
            DayNightProperties.instance.Refresh();

            // Set zoom to encapsulate entire model.
            float magnitude = currentBounds.extents.magnitude;
            float num = magnitude + 16f;
            float num2 = magnitude * currentZoom;

            // Transforms and clip.
            renderCamera.transform.position = -Vector3.forward * num2;
            renderCamera.transform.rotation = Quaternion.identity;
            renderCamera.nearClipPlane = Mathf.Max(num2 - num * 1.5f, 0.01f);
            renderCamera.farClipPlane = num2 + num * 1.5f;

            // Set up our render lighting settings.
            Light renderLight = DayNightProperties.instance.sunLightSource;

            RenderManager.instance.MainLight = renderLight;

            // If game is currently in nighttime, enable sun and disable moon lighting.
            if (gameMainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = true;
                DayNightProperties.instance.moonLightSource.enabled = false;
            }

            // Light settings.
            renderLight.intensity = 2f;
            renderLight.color = Color.white;
            renderLight.transform.eulerAngles = new Vector3(55, 0, 0);

            // Yay!  Matrix math, my favourite!
            Quaternion quaternion = Quaternion.Euler(-20f, 0f, 0f) * Quaternion.Euler(0f, currentRotation, 0f);
            Vector3 pos = quaternion * -currentBounds.center;
            Matrix4x4 matrix = Matrix4x4.TRS(pos, quaternion, Vector3.one);

            // Render!
            Graphics.DrawMesh(currentMesh, matrix, _material, 0, renderCamera, 0, null, true, true);
            renderCamera.RenderWithShader(_material.shader, "");

            // Restore game lighting.
            RenderManager.instance.MainLight = gameMainLight;

            // Reset to moon lighting if the game is currently in nighttime.
            if (gameMainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = false;
                DayNightProperties.instance.moonLightSource.enabled = true;
            }

            // Restore game exposure and sky tint.
            DayNightProperties.instance.m_Exposure = gameExposure;
            DayNightProperties.instance.m_SkyTint = gameSkyTint;

            // Restore game InfoManager mode.
            infoManager.SetCurrentMode(currentMode, currentSubMode);
            infoManager.UpdateInfoMode();
        }
    }
}