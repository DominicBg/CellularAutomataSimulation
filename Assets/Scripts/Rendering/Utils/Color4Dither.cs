using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct Color4Dither
{
    public Color lightColor;
    public Color mediumColor;
    public Color darkColor;
    public Color veryDarkColor;

    public float lightThreshold;
    public float mediumThreshold;
    public float darkThreshold;
    public float ditherRange;

    public Color32 GetColorWitLightValue(float lightValue, int2 position)
    {
        bool checker = (position.x + position.y) % 2 == 0;
        if (lightValue > lightThreshold)
        {
            return lightColor;
        }
        else if(lightValue > lightThreshold - ditherRange)
        {
            return checker ? lightColor : mediumColor;
        }
        else if (lightValue > mediumThreshold)
        {
            return mediumColor;
        }
        else if (lightValue > mediumThreshold - ditherRange)
        {
            return checker ? mediumColor : darkColor;
        }
        else if (lightValue > darkThreshold)
        {
            return darkColor;
        }
        else if (lightValue > darkThreshold - ditherRange)
        {
            return checker ? darkColor : veryDarkColor;
        }
        else
        {
            return veryDarkColor;
        }
    }

}
