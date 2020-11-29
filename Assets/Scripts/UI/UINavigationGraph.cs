using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class UINavigationGraph : MonoBehaviour
{
    [SerializeField] UINode[] nodes;
    UINode currentNode;
    GameOverworldManager manager;

    private void OnValidate()
    {
        nodes = GetComponentsInChildren<UINode>();
    }

    public void Init(GameOverworldManager manager, int nodeIndex)
    {
        currentNode = nodes[nodeIndex];
        this.manager = manager;
    }

    public void OnUpdate()
    {
        if(InputCommand.IsButtonDown(KeyCode.Space))
        {
            Debug.Log(currentNode.name);
            currentNode.onEvent?.Invoke();
        }

        InputCommand.DirectionEnum direction = InputCommand.GetKeyDirection();
        UINode nextNode = null;
        switch (direction)
        {
            case InputCommand.DirectionEnum.Left:
                nextNode = currentNode.leftNode;
                break;
            case InputCommand.DirectionEnum.Right:
                nextNode = currentNode.rightNode;
                break;
            case InputCommand.DirectionEnum.Up:
                nextNode = currentNode.upNode;
                break;
            case InputCommand.DirectionEnum.Down:
                nextNode = currentNode.downNode;
                break;
        }

        if (nextNode != null)
            currentNode = nextNode;
    }

    public void OnRender(ref NativeArray<Color32> outputColor)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            UINode node = nodes[i];
            bool isSelected = currentNode == node;
            SpriteEnum spriteEnum = isSelected ? node.selectedSprite : node.unselectedSprite;
            NativeSprite sprite = SpriteRegistry.GetSprite(spriteEnum);
            GridRenderer.ApplySprite(ref outputColor, sprite, node.position);
        }
    }

    public void SelectLevel(int level)
    {
        manager.SelectLevel(level);
    }

    public void Rotate(int direction)
    {
        manager.RotateLevel(direction);
    }
}
