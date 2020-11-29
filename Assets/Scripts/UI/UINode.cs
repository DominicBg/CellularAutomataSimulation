using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using static InputCommand;

public class UINode : MonoBehaviour
{
    public SpriteEnum unselectedSprite;
    public SpriteEnum selectedSprite;
    public UnityEvent onEvent;
    public int2 position;

    [Header("Connections")]
    public UINode leftNode;
    public UINode rightNode;
    public UINode upNode;
    public UINode downNode;

}
