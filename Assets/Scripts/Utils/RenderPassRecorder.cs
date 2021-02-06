using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RenderPassRecorder
{
    const string path = "Assets/RenderPass/";
    int imageCount = 0;
    Texture2D output;
    byte[] previousBytes;

    public void RecordRenderPass(PixelScene pixelScene, ref TickBlock tickBlock, int2 resolution)
    {
        output = new Texture2D(resolution.x, resolution.y);
        imageCount = 0;
        var result = pixelScene.pixelCamera.Render(pixelScene, ref tickBlock, false, RecordRenderPass);
        RecordRenderPass(result);
        result.Dispose();
    }

    void RecordRenderPass(NativeArray<Color32> pixels)
    {
        output.SetPixelData(pixels, 0);
        byte[] bytes = output.EncodeToPNG();

        bool isDifferent = false;
        if(previousBytes == null || bytes.Length != previousBytes.Length)
        {
            previousBytes = bytes;
            isDifferent = true;
        }
        else
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (previousBytes[i] != bytes[i])
                    isDifferent = true;
            }
        }

        if (!isDifferent)
            return;

        File.WriteAllBytes(path+imageCount+".png", bytes);
        imageCount++;
        previousBytes = bytes;
    }
}
