using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MapDrawerManager : MonoBehaviour
{
    public ParticleType type;
    public CellularAutomata cellularAutomata;
    public RectTransform renderer;
    public Camera camera;
    public CanvasScaler canvas;

    public int brushSize = 2;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float2 screen = new Vector2(Screen.width, Screen.height);
            float2 mousePosition = (Vector2)Input.mousePosition;
            float2 ratio = mousePosition / screen;
            float2 resolution = canvas.referenceResolution;

            float2 result = ratio * resolution;
            float xRatio = resolution.x / resolution.y;
            float xHalfRatio = xRatio * 0.5f;
            float xExtra = cellularAutomata.sizes.x * xHalfRatio;

            int posX = (int)math.remap(0, resolution.x, -xExtra, cellularAutomata.sizes.x + xExtra, result.x);
            int posY = (int)math.remap(0, resolution.y, 0, cellularAutomata.sizes.y, result.y);

            for (int x = -brushSize; x <= brushSize; x++)
            {
                for (int y = -brushSize; y <= brushSize; y++)
                {
                    cellularAutomata.map.SetParticleType(new int2(posX + x, posY + y), type);
                }
            }
        }
    }
}
