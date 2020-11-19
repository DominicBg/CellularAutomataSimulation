﻿using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public interface IRenderable
{
    void Render(ref NativeArray<Color32> colorArray);
}
