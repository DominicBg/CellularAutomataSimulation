using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputCommand;
public enum ButtonType { Jump, Exit, Action1, Action1Alt, Action2, Action2Alt, Hold}

public class InputCommand
{

    static InputCommand instance;
    Inputs inputRef;
    public enum DirectionEnum { Left, Right, Up, Down, None }

    public static void Update()
    {
        EnsureInit();
        instance.InternalUpdate();
    }
    public static bool IsButtonHeld(ButtonType keyCode) => instance.GetInput(keyCode).IsButtonHeld();

    public static bool IsButtonDown(ButtonType keyCode) => instance.GetInput(keyCode).IsButtonDown();
    public static bool IsButtonUp(ButtonType keyCode) => instance.GetInput(keyCode).IsButtonUp();

    static void EnsureInit()
    {
        if (instance == null)
        {
            instance = new InputCommand();
            instance.inputRef = new Inputs();
            instance.inputRef.Enable();
        }
    }

    public static bool HasInputDirection { get; private set; }
    public static float InputDirectionAngleDeg { get; private set; }
    public static float InputDirectionAngle8StepDeg { get; private set; }
    public static float2 Get8Direction { get; private set; }

    public static float2 Direction { get; private set; }
    public static float2 MousePosition { get; private set; }

    Dictionary<ButtonType, InputState> inputs = new Dictionary<ButtonType, InputState>();

    InputAction InputActionWithButtonType(ButtonType type)
    {
        switch (type)
        {
            case ButtonType.Jump:
                return inputRef.Player.Jump;
            case ButtonType.Exit:
                return inputRef.Player.Exit;
            case ButtonType.Action1:
                return inputRef.Player.Action1;
            case ButtonType.Action1Alt:
                return inputRef.Player.Action1Alt;
            case ButtonType.Action2:
                return inputRef.Player.Action2;
            case ButtonType.Action2Alt:
                return inputRef.Player.Action2Alt;
            case ButtonType.Hold:
                return inputRef.Player.Hold;
        }
        return null;
    }
    InputState GetInput(ButtonType keyCode)
    {
        InputState inputState;
        if (inputs.TryGetValue(keyCode, out inputState))
        {
            return inputState;
        }

        inputState = new InputState();
        inputState.Init(keyCode, InputActionWithButtonType(keyCode));
        inputs.Add(keyCode, inputState);
        return inputState;
    }

    public void InternalUpdate()
    {
        Direction = inputRef.Player.Move.ReadValue<Vector2>();
        if(math.all(Direction == 0))
        {
            InputDirectionAngleDeg = 0;
            Get8Direction = 0;
            HasInputDirection = false;
        }
        else
        {
            float angle = math.degrees(math.atan2(Direction.y, Direction.x));
            InputDirectionAngleDeg = angle;
            InputDirectionAngle8StepDeg = (math.round((angle / 360) * 8) / 8) * 360;
            if (InputDirectionAngle8StepDeg < 0)
                InputDirectionAngle8StepDeg = 360 + InputDirectionAngle8StepDeg ;

            Get8Direction = MathUtils.Rotate(new float2(1, 0), math.radians(InputDirectionAngle8StepDeg));
            //Debug.Log($"InputDirectionAngle8StepDeg {InputDirectionAngleDeg}, InputDirectionAngle8StepDeg {InputDirectionAngle8StepDeg}, Get8Direction {Get8Direction}");
            HasInputDirection = true;
        }

        MousePosition = inputRef.Player.Mouse.ReadValue<Vector2>();
        foreach (var input in inputs.Values)
            input.Update();
    }
    

    public class InputState
    {
        ButtonType buttonType;
        bool isPressed;
        bool wasPressed;
        InputAction inputAction;

        public void Init(ButtonType keyCode, InputAction inputAction)
        {
            this.buttonType = keyCode;
            this.inputAction = inputAction;
        }

        public void Update()
        {
            wasPressed = isPressed;
            isPressed = inputAction.ReadValue<float>() > 0.5f;
        }

        public bool IsButtonHeld() => isPressed && wasPressed;
        public bool IsButtonDown() => isPressed && !wasPressed;
        public bool IsButtonUp() => !isPressed && wasPressed;
    }
}
