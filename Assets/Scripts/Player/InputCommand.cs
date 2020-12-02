using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InputCommand
{
    static InputCommand instance;

    static readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0) /*, new int2(0, 1), new int2(0, -1)*/ };
    static readonly KeyCode[] directionInputs = new KeyCode[] { KeyCode.D, KeyCode.A /*, KeyCode.W, KeyCode.S */};
    public enum DirectionEnum { Left, Right, Up, Down, None }


    public static void Update()
    {
        EnsureInit();
        instance.InternalUpdate();
    }
    public static bool IsButtonHeld(KeyCode keyCode) => instance.GetInput(keyCode).IsButtonHeld();

    public static bool IsButtonDown(KeyCode keyCode) => instance.GetInput(keyCode).IsButtonDown();
    public static bool IsButtonUp(KeyCode keyCode) => instance.GetInput(keyCode).IsButtonUp();

    public static DirectionEnum GetKeyDirection()
    {
        if (IsButtonDown(KeyCode.W)) return DirectionEnum.Up;
        if (IsButtonDown(KeyCode.A)) return DirectionEnum.Left;
        if (IsButtonDown(KeyCode.S)) return DirectionEnum.Down;
        if (IsButtonDown(KeyCode.D)) return DirectionEnum.Right;

        return DirectionEnum.None;
    }

    static void EnsureInit()
    {
        if (instance == null)
            instance = new InputCommand();
    }


    public static int2 Direction { get; private set; }

    Dictionary<KeyCode, InputState> inputs = new Dictionary<KeyCode, InputState>();



    InputState GetInput(KeyCode keyCode)
    {
        InputState inputState;
        if (inputs.TryGetValue(keyCode, out inputState))
        {
            return inputState;
        }

        inputState = new InputState();
        inputState.Init(keyCode);
        inputs.Add(keyCode, inputState);
        return inputState;
    }

    public void InternalUpdate()
    {
        Direction = 0;
        for (int i = 0; i < directionInputs.Length; i++)
        {
            if (Input.GetKey(directionInputs[i]))
            {
                Direction = directions[i];
                break;
            }
        }

        foreach(var input in inputs.Values)
            input.Update();
    }


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
