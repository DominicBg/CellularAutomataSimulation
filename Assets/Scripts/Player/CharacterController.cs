using Unity.Mathematics;
using UnityEngine;

public abstract class CharacterController : PhysicObject
{
    [SerializeField] CharacterControlSettings characterSettings;


    [HideInInspector] public bool lookLeft;
    [HideInInspector] public bool isDirectionLocked;
    int lookDirection;
    bool canJump;
    float inAirDuration;
    float pressJumpBufferDuration;

    protected SpriteAnimator spriteAnimator;
    protected bool allowsInput = true;

    public int2 AnimOffset => spriteAnimator.GetCurrentAnimOffset();

    public override void OnInit()
    {
        base.OnInit();
        spriteAnimator = new SpriteAnimator(characterSettings.spriteSheet);
        InitPhysicData(characterSettings.collisionTexture);
    }

    public override Bound GetBound()
    {
        return physicData.physicBound.GetCollisionBound(position);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        bool isGrounded = IsGrounded();

        if(allowsInput)
        {
            float2 direction = new float2(InputCommand.Direction.x, 0);
            const float turnAroundThreshold = 0.2f;
            if (!isDirectionLocked)
            {
                if (math.abs(direction.x) > turnAroundThreshold)
                    lookDirection = (int)math.sign(direction.x);
                else
                    lookDirection = 0;
            }


            if (lookDirection != 0)
            {
                lookLeft = lookDirection == -1;
            }

            if (InputCommand.IsButtonHeld(ButtonType.Hold))
            {
                physicData.velocity.x = 0;
            }
            else
            {
                UpdateMovement(direction);

            }

            UpdateJump(isGrounded);

            if (isGrounded && InputCommand.IsButtonHeld(ButtonType.Jump))
            {
                PhysiXVII.MoveUpFromPile(ref physicData, map, GameManager.PhysiXVIISetings);
                position = physicData.gridPosition;
            }
        }

  
        UpdateAnimation(lookDirection, isGrounded);
        HandlePhysic();
    }

    public void SetIsControlled(bool isControlled)
    {
        allowsInput = isControlled;
        physicData.applyFriction = !isControlled;

        if (!isControlled)
        {
            spriteAnimator.SetAnimation(0);
            if(physicData.isGrounded)
                physicData.velocity.x = 0;
        }
    }

    private void UpdateAnimation(float direction, bool isGrounded)
    {
        if (!isGrounded)
        {
            spriteAnimator.SetAnimation(2);
        }
        else if (direction == 0)
        {
            spriteAnimator.SetAnimation(0);
        }
        else
        {
            spriteAnimator.SetAnimation(1);
        }
        spriteAnimator.Update(lookLeft);
    }


    private void UpdateMovement(float2 direction)
    {
        physicData.velocity.x += direction.x * characterSettings.acceleration * GameManager.DeltaTime;
        float damping;
        if (math.abs(direction.x) < 0.01f)
            damping = characterSettings.stopMovingDamping;
        else if (math.sign(direction.x) != math.sign(physicData.velocity.x))
            damping = characterSettings.turiningDamping;
        else
            damping = characterSettings.movingDamping;

        physicData.velocity.x *= math.pow(1 - damping, GameManager.DeltaTime * characterSettings.dampingForce);
    }

    private void UpdateJump(bool isGrounded)
    {
        if (isGrounded)
        {
            canJump = true;
            inAirDuration = 0;
        }
        else
        {
            inAirDuration += GameManager.DeltaTime;

            //Control jump height
            if (physicData.velocity.y > 0 && InputCommand.IsButtonUp(ButtonType.Jump))
            {
                physicData.velocity.y *= characterSettings.releaseJumpButtonCutoff;
            }

        }

        pressJumpBufferDuration -= GameManager.DeltaTime;
        if (InputCommand.IsButtonDown(ButtonType.Jump))
        {
            pressJumpBufferDuration = characterSettings.pressJumpBuffer;
        }

        if (canJump && inAirDuration < characterSettings.inAirJumpThreshold && pressJumpBufferDuration > 0)
        {
            physicData.velocity = new float2(physicData.velocity.x, characterSettings.jumpForce);
            canJump = false;
            pressJumpBufferDuration = 0;
        }
    }

    public override void Dispose()
    {
        spriteAnimator.Dispose();
    }
}
