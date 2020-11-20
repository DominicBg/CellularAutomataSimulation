using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct InputCommand
{
    static readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0) /*, new int2(0, 1), new int2(0, -1)*/ };
    static readonly KeyCode[] inputs = new KeyCode[] { KeyCode.D, KeyCode.A /*, KeyCode.W, KeyCode.S */};

    public int2 direction;
    public InputState spaceInput;

    public void Update()
    {
        direction = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            if (Input.GetKey(inputs[i]))
            {
                direction = directions[i];
                break;
            }
        }

        spaceInput.SetCurrentPress(Input.GetKey(KeyCode.Space));
    }

    public struct InputState
    {
        bool isPressed;
        bool wasPressed;

        public void SetCurrentPress(bool isPressed)
        {
            wasPressed = this.isPressed;
            this.isPressed = isPressed;
        }

        public bool IsButtonHeld() => isPressed && wasPressed;
        public bool IsButtonDown() => isPressed && !wasPressed;
        public bool IsButtonUp() => !isPressed && wasPressed;
    }
}
