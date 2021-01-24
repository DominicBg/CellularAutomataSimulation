using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPartialScene : MonoBehaviour
{
    [SerializeField] LevelElement[] subElements;

    public void OnValidate()
    {
        subElements = GetComponentsInChildren<LevelElement>();
    }

    public void SetActive(bool active)
    {
        for (int i = 0; i < subElements.Length; i++)
        {
            subElements[i].isEnable = active;
            subElements[i].isVisible = active;
        }
    }
}
