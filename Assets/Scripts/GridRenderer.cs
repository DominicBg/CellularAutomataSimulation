using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GridRenderer : MonoBehaviour
{
    Texture2D texture;

    //todo add enumerated array

    public Color noneColor;
    public Color waterColor;
    public Color sandColor;
    public Color mudColor;

    public RawImage renderer;

    public void OnUpdate(NativeArray<Particle> particles, Map map, int2 sizes, PlayerCellularAutomata player)
    {
        GetTextureFromMap(particles, texture, map, sizes);
        AddPixelSprite(texture, player);
        texture.Apply();

        renderer.texture = texture;
    }

    public void Init(int2 sizes)
    {
        texture = new Texture2D(sizes.x, sizes.y, TextureFormat.RGBA32, false, true);
        texture.filterMode = FilterMode.Point;
    }

    void GetTextureFromMap(NativeArray<Particle> particles, Texture2D texture, Map map, int2 sizes)
    {
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int i = map.PosToIndex(x, y);
                texture.SetPixel(x, y, GetColorForType(particles[i].type));
            }
        }
    }

    //TODO convert player cellulara automata to a more generic sprite methode
    void AddPixelSprite(Texture2D texture, PlayerCellularAutomata player)
    {
        PixelSprite sprite = player.sprite;

        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int2 texturePos = new int2(x, y) + player.topLeftPosition;
                if(sprite.collisions[x,y])
                {
                    texture.SetPixel(texturePos.x, texturePos.y, sprite.pixels[x,y]);
                }
            }
        }
    }

    Color GetColorForType(ParticleType type)
    {
        switch (type)
        {
            case ParticleType.None:
                return noneColor;
            case ParticleType.Water:
                return waterColor;
            case ParticleType.Sand:
                return sandColor;
            case ParticleType.Mud:
                return mudColor;
            case ParticleType.Player:
                //Gets overriden when trying the sprite
                return Color.clear;
            default:
                return Color.black;
        }
    }
}
