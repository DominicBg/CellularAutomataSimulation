using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RaymarchingManager : MonoBehaviour
{

    public static RaymarchingManager Instance;

    enum LightClass
    {
        DIRECTIONAL,
        POINT,
        SPOT
    }

    struct ShapeDataStruct
    {
        public int shapeType;
        public int blendType;
        public float blendStrength;
        public Vector2 radii;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector4 color;

        public static int GetBytes() { return sizeof(int) * 2 + sizeof(float) * (1 + 2 + 3 * 3 + 4); }
    }

    struct LightStruct
    {
        public float range;
        public float angle;
        public float intensity; // Common value
        public Vector3 dir;       // Common value
        public Vector3 pos;
        public Vector4 color;     // Common value

        public static int GetBytes() { return sizeof(float) * (3 + 3 * 2 + 4); }
    }
    [Range(0f, 1f)]
    [SerializeField] private float m_ambientIntensity = .2f;
    [SerializeField] private float m_softShadowCoef = 8f;
    [SerializeField] private Color m_ambientColor = Color.white;
    [SerializeField] private bool m_paintNormals = false;
    [SerializeField] private ComputeShader m_raymarchingShader = default;

    private Camera m_camera;
    private ComputeBuffer m_shapesBuffer;
    private ComputeBuffer m_lightsBuffer;
    //private ComputeBuffer m_inputBuffer;
    //private ComputeBuffer m_outputBuffer;
    private LightStruct[] m_lights;
    private ShapeDataStruct[] m_shapesData;
    private List<RayMarchingShape> m_shapes;

    private Texture2D texture;

    private void Start()
    {
        Instance = this;
        m_shapes = new List<RayMarchingShape>(FindObjectsOfType<RayMarchingShape>());
    }

    public unsafe void RenderImage(ref NativeArray<Color32> outputColor)
    {
        //m_camera = Camera.current;
        m_camera = Camera.main;
        CleanOrCreateRenderTexture();

        ProcessLights();

        if (m_shapes.Count > 0)
        {
            // Convert shapes into structs and then to a compute buffer
            m_shapesData = new ShapeDataStruct[m_shapes.Count];
            m_shapesBuffer = new ComputeBuffer(m_shapesData.Length, ShapeDataStruct.GetBytes());
            ProcessShapes(ref m_shapes, ref m_shapesData, ref m_shapesBuffer);

            //m_outputBuffer = new ComputeBuffer(outputColor.Length, sizeof(Color32));
            //m_outputBuffer = new ComputeBuffer(outputColor.Length, sizeof(Color32));
            //m_inputBuffer = new ComputeBuffer(outputColor.Length, sizeof(Color));
            // m_inputBuffer.SetData(outputColor);
            //m_outputBuffer.SetData(outputColor);

            SetupComputeShader();
    
            // Launch kernel
            // Get the proper grid size
            //int gridSizeX = Mathf.CeilToInt(m_camera.pixelWidth / 8.0f);
            //int gridSizeY = Mathf.CeilToInt(m_camera.pixelHeight / 8.0f);
            // "Run" the compute shader
            m_raymarchingShader.Dispatch(0, GameManager.GridSizes.x, GameManager.GridSizes.y, 1);

            var array = new uint[outputColor.Length];
            //m_outputBuffer.GetData(array);

            Debug.Log(array[0]);
            var pixels = texture.GetPixelData<Color32>(0);
            for (int i = 0; i < outputColor.Length; i++)
            {
                //float4 value = array[i];
                outputColor[i] = pixels[i];
            }
            Debug.Log(outputColor[0]);
            pixels.Dispose();

            //var array = outputColor.ToArray();
            //m_outputBuffer.GetData(array);
            // = new NativeArray<Color32>(array, Allocator.Persistent);

            // Copy the processed texture onto the output
            //Graphics.Blit(m_tmpRenderTex, outRenderTex);

            // Clean buffers
            m_shapesBuffer.Dispose();
            m_lightsBuffer.Dispose();
            //m_outputBuffer.Dispose();
            //m_inputBuffer.Dispose();
        }
    }

    public Color32 UnpackColor(uint col)
    {
        byte r = (byte)((col) & 0xFF); ;
        byte g = (byte)((col >> 8) & 0xFF);
        byte b = (byte)((col >> 16) & 0xFF);
        byte a = (byte)((col >> 24) & 0xFF);
        Color32 c = new Color32(r, g, b, a);
        //c.b = (byte)((col) & 0xFF);
        //c.g = (byte)((col >> 8) & 0xFF);
        //c.r = (byte)((col >> 16) & 0xFF);
        //c.a = (byte)((col >> 24) & 0xFF);
        return c;
    }


    // Gets all the lights in the scene and pass them to the buffer
    void ProcessLights()
    {
        Light[] lights = FindObjectsOfType<Light>();
        m_lights = new LightStruct[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            m_lights[i].dir = lights[i].transform.forward;
            m_lights[i].color = lights[i].color;
            m_lights[i].intensity = lights[i].intensity;

            switch (lights[i].type)
            {
                case LightType.Point:
                    m_lights[i].angle = 360f;
                    m_lights[i].range = lights[i].range;
                    m_lights[i].pos = lights[i].transform.position;
                    break;

                case LightType.Spot:
                    m_lights[i].angle = lights[i].spotAngle;
                    m_lights[i].range = lights[i].range;
                    m_lights[i].pos = lights[i].transform.position;
                    break;

                default: // Directional lights
                         // NOTE: For now, area lights are not supported, so they are treated as directional ones
                    m_lights[i].angle = 360f;
                    m_lights[i].range = float.PositiveInfinity;
                    m_lights[i].pos = Vector3.positiveInfinity;
                    break;
            }
        }

        m_lightsBuffer = new ComputeBuffer(m_lights.Length, LightStruct.GetBytes());
        m_lightsBuffer.SetData(m_lights);
    }

    // Cleans the render texture or creates a new one if it doesn't exist
    void CleanOrCreateRenderTexture()
    {
        if (texture == null || texture.width != GameManager.GridSizes.x || texture.height != GameManager.GridSizes.y)
        {
            // if (renderTexture != null) renderTexture.Release();

            texture = new Texture2D(GameManager.GridSizes.x, GameManager.GridSizes.x, TextureFormat.ARGB32, false);
            texture.Apply();
            // RenderTextureFormat.ARGBFloat,
            //RenderTextureReadWrite.Linear);
            //texture = true;
            //texture.Create();
        }
    }

    // Sorts and transforms the shapes into structs to being able to pass them as a buffer to the shader
    void ProcessShapes(ref List<RayMarchingShape> shapes, ref ShapeDataStruct[] data, ref ComputeBuffer buffer)
    {
        // Sort the shapes by blending operation (This way we should minimize the creation order bug)
        shapes.Sort((a, b) => a.GetBlendType().CompareTo(b.GetBlendType()));

        for (int i = 0; i < shapes.Count; i++)
        {
            data[i].shapeType = shapes[i].GetShapeType();
            data[i].blendType = shapes[i].GetBlendType();
            data[i].blendStrength = shapes[i].GetBlendStrength();
            data[i].radii = shapes[i].GetTorusR1R2();
            data[i].position = shapes[i].GetPos();
            data[i].rotation = shapes[i].GetRot();
            data[i].scale = shapes[i].GetScale();
            data[i].color = shapes[i].GetColor();
        }

        buffer.SetData(data);
    }

    // Passes all the needed uniforms to the shader
    // TODO: Add more light types and the possibility of multiple lights
    void SetupComputeShader()
    {
        // Pass the shapes buffer
        m_raymarchingShader.SetBuffer(0, "_shapes", m_shapesBuffer);
        m_raymarchingShader.SetInt("_numShapes", m_shapesData.Length);
        // Pass the lights buffer
        m_raymarchingShader.SetBuffer(0, "_lights", m_lightsBuffer);
        m_raymarchingShader.SetInt("_numLights", m_lights.Length);
        m_raymarchingShader.SetFloats("_numLights", m_lights.Length);

        // Pass the needed matrices
        m_raymarchingShader.SetMatrix("_Camera2WorldMatrix", m_camera.cameraToWorldMatrix);
        m_raymarchingShader.SetMatrix("_InverseProjectionMatrix", m_camera.projectionMatrix.inverse);
        
        m_raymarchingShader.SetVector("_sizes", new Vector4(GameManager.GridSizes.x, GameManager.GridSizes.y, 0, 0));


        // Pass the textures
        //m_raymarchingShader.SetBuffer(0, "_inputTex", m_inputBuffer);
        //m_raymarchingShader.SetBuffer(0, "_outTex", m_outputBuffer);

        m_raymarchingShader.SetTexture(0, "_outputTex", texture);


        // Pass the ambient light information
        m_raymarchingShader.SetFloat("_Ka", m_ambientIntensity);
        m_raymarchingShader.SetVector("_ambientColor", m_ambientColor);

        m_raymarchingShader.SetFloat("_Ksh", m_softShadowCoef);

        m_raymarchingShader.SetInt("_paintNormals", (m_paintNormals) ? 1 : 0);
    }
}