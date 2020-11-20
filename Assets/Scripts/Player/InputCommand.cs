using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InputCommand
{
    static readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0) /*, new int2(0, 1), new int2(0, -1)*/ };
    static readonly KeyCode[] directionInputs = new KeyCode[] { KeyCode.D, KeyCode.A /*, KeyCode.W, KeyCode.S */};

    public int2 direction;

    Dictionary<KeyCode, InputState> inputs = new Dictionary<KeyCode, InputState>();

    public void CreateInput(KeyCode keyCode)
    {
        if (inputs.ContainsKey(keyCode))
            return;

        InputState inputState = new InputState();
        inputState.Init(keyCode);
        inputs.Add(keyCode, inputState);
    }

    public void Update()
    {
        direction = 0;
        for (int i = 0; i < directionInputs.Length; i++)
        {
            if (Input.GetKey(directionInputs[i]))
            {
                direction = directions[i];
                break;
            }
        }

        foreach(var input in inputs.Values)
            input.Update();
    }

    public bool IsButtonHeld(KeyCode keyCode) => inputs[keyCode].IsButtonHeld();
    public bool IsButtonDown(KeyCode keyCode) => inputs[keyCode].IsButtonDown();
    public bool IsButtonUp(KeyCode keyCode) => inputs[keyCode].IsButtonUp();

    public class InputState
    {
        KeyCode keyCode;
        bool isPressed;
        bool wasPressed;

        public void Init(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }

        public void Update()
        {
            wasPressed = isPressed;
            isPressed = Input.GetKey(keyCode);
        }

        public bool IsButtonHeld() => isPressed && wasPressed;
        public bool IsButtonDown() => isPressed && !wasPressed;
        public bool IsButtonUp() => !isPressed && wasPressed;
    }
}
