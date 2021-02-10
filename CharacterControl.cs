﻿using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public interface CharacterInputsOutPuts
{
    void MovementInput(Vector2 mInput);
    void InputJump();
    Vector2 GetPosition();
    bool GetLookDir();
    void Flip();
    void ForceJumpDir(Vector2 jumpDir);
    void SetJumpForce(float force = 0.2f);
    void InputChrouch();
    void InputDash();
    void InputLevitate();
    void InputAddForce(Vector2 f);
}

public interface CharacterState
{
    bool IsTouchingLeftWall();
    bool IsTouchingRightWall();
    bool IsOnLedge();
    bool IsOnGround();
    bool IsBumpingCeiling();
    bool IsDashing();
    bool IsChrouching();
    bool IsLookingLeft();
    Vector2 Movement();
    float GetMaxSpeed();
    float GetLocalScaleX();
}

public interface CharacterProperties
{
    void AddJump();
    void SubJump();
    void AddDash();
    void SubDash();
}

public class CharacterControl : MonoBehaviour, CharacterInputsOutPuts, CharacterState, CharacterProperties
{
    public float speedAcceleration = 0.1f;
    public float maxSpeed = 0.1f;
    public float gravity = 0.5f;
    public float jumpMax = 2f;
    public float jumpPower = 0.1f;
    public float jumpCounter = 0;
    public bool crouch = false;
    public Vector3 crouchVector = new Vector3(0, 0);

    //Hit Detection - Level
    public bool onGround = false;
    public bool onLeftGround = false;
    public bool LeftGround = false;
    public bool onRightGround = false;
    public bool RightGround = false;

    //Ledge grab
    public GameObject grabPoint;
    public bool ledgeRight = false;
    public bool ledgeLeft = false;
    public bool ledgeGrabbed = false;

    [HideInInspector] public bool wallLeft = false;
    [HideInInspector] public bool wallRight = false;
    [HideInInspector] public bool wallUpLeft = false;
    [HideInInspector] public bool wallUpRight = false;

    [HideInInspector] public bool hitCeilingLeft = false;
    [HideInInspector] public bool hitCeilingRight = false;

    //Gun
    [HideInInspector] public bool lookLeft = false;


    //Move
    [HideInInspector] public Vector3 moveInput = new Vector3();
    [HideInInspector] public Vector3 move = new Vector3();
    [HideInInspector] public Timer jumpTimer;
    [HideInInspector] public float jumpCounterTimer = 1.5f;
    private bool jumped = true;
    private float dashStopTimer = 0f;
    private bool dash = false;
    public int dashCounterMax = 2;
    public int dashCounter = 2;
    private float dashChargeSpeed = 0;
    private float dashChargeSpeedMax = 1.0f;

    public float characterWidth = 0.5f;
    public float characterHeight = 0.65f;
    public float slopeTolerance = 0.15f;
    public float wallTolerance = 0.1f;

    public bool STEEPCLIFF = false;
    public Vector3 STEEPVECTOR;

    public float yVelocity = 0f;
    public float xVelocity = 0f;
    public float maxYVelocity = -0.5f;

    private CharacterController cc;
    private Transform gchck;
    private IcontrolC IKCharacterOutput;
    private Vector2 ICinput = Vector2.zero;

    private AudioSource p_as;
    private IEntityAudio IAudio;
    //After
    public bool landed = false;


    public float ScaleX;
    public float ScaleY;

    // Use this for initialization
    void Start()
    {
        cc = GetComponent<CharacterController>();
        p_as = GetComponent<AudioSource>();
        IAudio = GetComponent<EntitySounds>();
        IKCharacterOutput = GetComponent<IKCharacterAdapter>();
        if (IKCharacterOutput == null)
            IKCharacterOutput = new NullCharacterAdapater();


        jumpTimer = new Timer(100);
        jumpTimer.Elapsed += OnJumpTimer;


        ScaleX = transform.localScale.x;
        ScaleY = transform.localScale.y;
    }

    private void OnJumpTimer(object source, ElapsedEventArgs e)
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //For afterupdate
        landed = IsLanded();

        int GroundLayer = LayerMask.NameToLayer("Ground");
        int GroundHelpLayer = LayerMask.NameToLayer("HelpGeom");
        int Mask1 = 1 << GroundLayer;
        int Mask2 = 1 << GroundHelpLayer;
        int finalMask = Mask1 | Mask2;
        /*
        float ChrouchZ = Input.GetAxis("CrouchZ");
        if (ChrouchZ > 0)
            ChrouchZ = 0;
        IKCharacterOutput.AddToRootOnce(ChrouchZ);
        */
        //moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        //onGround = Physics2D.Raycast(transform.position, Vector3.down, characterHeight, finalMask);
        //onLeftGround = Physics2D.Raycast(transform.position - new Vector3(-(characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight, finalMask);
        //onRightGround = Physics2D.Raycast(transform.position - new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight, finalMask);

        RaycastHit2D groundhit = Physics2D.Raycast(transform.position, Vector3.down, characterHeight, finalMask);
        RaycastHit2D groundhitleft;
        RaycastHit2D groundhitright;

        //hitCeilingLeft = Physics2D.Raycast(transform.position - new Vector3(-characterWidth / 2, 0, 0), Vector3.up, 0.70f, finalMask);
        //hitCeilingRight = Physics2D.Raycast(transform.position - new Vector3(characterWidth / 2, 0, 0), Vector3.up, 0.70f, finalMask);

        wallLeft = Physics2D.Raycast(transform.position, Vector3.left, characterWidth + 0.01f, finalMask);
        wallRight = Physics2D.Raycast(transform.position, Vector3.right, characterWidth + 0.01f, finalMask);
        wallUpLeft = Physics2D.Raycast(transform.position + new Vector3(0, 0.4f, 0) + crouchVector, Vector3.left, characterWidth + 0.01f, finalMask);
        wallUpRight = Physics2D.Raycast(transform.position + new Vector3(0, 0.4f, 0) + crouchVector, Vector3.right, characterWidth + 0.01f, finalMask);

        RaycastHit2D wallLeftfix;
        RaycastHit2D wallRightfix;



        /*
        move += moveInput * Time.deltaTime * speedAcceleration;
        if (STEEPCLIFF)
            move = new Vector3(0, 0, 0)
        else
            move += moveInput * Time.deltaTime * speedAcceleration;
        if (moveInput.x == 0) {
            move.x = move.x / 4;
        }
        else if (moveInput.x < 0 && move.x > 0) {
            move.x = move.x / 4;
        }
        else if (moveInput.x > 0 && move.x < 0) {
            move.x = move.x / 4;
        }
        */



        if (jumpCounterTimer > 0.1f && !jumped) {
            yVelocity = jumpPower;
            onGround = false;
            onLeftGround = false;
            onRightGround = false;
            jumped = true;
            OnJump();
        }
        else if (!jumped) {
            jumpCounterTimer += Time.deltaTime;
            IKCharacterOutput.AddToRootOnce(-0.75f * (jumpCounterTimer * 10));
        }

        //Ceiling
        hitCeilingLeft = Physics2D.Raycast(transform.position - new Vector3(-characterWidth / 2, 0, 0), Vector3.up, 0.70f, finalMask);
        hitCeilingRight = Physics2D.Raycast(transform.position - new Vector3(characterWidth / 2, 0, 0), Vector3.up, 0.70f, finalMask);
        if ((hitCeilingLeft || hitCeilingRight) && yVelocity > 0) {
            yVelocity = 0;
        }

        if (yVelocity < -1)
            yVelocity = -1;
        //maxSpeed * moveInput.x
        if (((moveInput.x < 0 && !IsTouchingLeftWall()) || (moveInput.x > 0 && !IsTouchingRightWall())) && !STEEPCLIFF && !ledgeGrabbed) {
            
            move = new Vector3(move.x + speedAcceleration * moveInput.x * Time.deltaTime, move.y);
            if (move.x > maxSpeed * Mathf.Abs(moveInput.x))
                move.x = maxSpeed * Mathf.Abs(moveInput.x);
            if (move.x < -maxSpeed * Mathf.Abs(moveInput.x))
                move.x = -maxSpeed * Mathf.Abs(moveInput.x);
        }
        else if(moveInput.x == 0 && IsOnGround()) {
            move.x = move.x * 0.8f;
            if (Mathf.Abs(move.x) < 0.01f)
                move.x = 0;

        }
        /*
        else if (STEEPCLIFF) {
            if (onLeftGround)
                STEEPVECTOR = Quaternion.AngleAxis(90, Vector3.forward) * (STEEPVECTOR);
            else
                STEEPVECTOR = Quaternion.AngleAxis(-90, Vector3.forward) * (STEEPVECTOR);
            move = STEEPVECTOR * Time.deltaTime * 10;
        }
        */

        if (dash && dashStopTimer < 0.2f) {
            dashStopTimer += Time.deltaTime;
            if (move.x != 0)
                move = new Vector3(maxSpeed * 3 * Mathf.Sign(move.x), 0);
            else
                move = new Vector3(maxSpeed * 3 * Mathf.Sign(transform.localScale.x), 0);
            if (IsTouchingLeftWall() && move.x < 0) {
                move.x = 0;
            }
            else if (IsTouchingRightWall() && move.x > 0) {
                move.x = 0;
            }
            yVelocity = 0;
        }
        else {
            dash = false;
        }

        onGround = Physics2D.Raycast(transform.position, Vector3.down, characterHeight, finalMask);
        onLeftGround = Physics2D.Raycast(transform.position - new Vector3(-(characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
        onRightGround = Physics2D.Raycast(transform.position - new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
        if ((!onGround && !onLeftGround && !onRightGround) || yVelocity > 0) {
            yVelocity -= gravity * Time.deltaTime;
        }
        if (yVelocity < maxYVelocity)
            yVelocity = maxYVelocity;

        if (ledgeGrabbed)
            yVelocity = 0;

        move.y = yVelocity;

        //FIXIT
        onGround = Physics2D.Raycast(transform.position, Vector3.down, characterHeight, finalMask);
        onLeftGround = Physics2D.Raycast(transform.position - new Vector3(-(characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
        onRightGround = Physics2D.Raycast(transform.position - new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
        if ((onGround || onLeftGround || onRightGround) && yVelocity <= 0) {
            if (landed != IsLanded()) {
                OnLand();
            }
            yVelocity = 0;
            jumpCounter = 0;
            //dashCounter = 0;
        }

        //LEDGECHECK
        RaycastHit2D downCheck = Physics2D.Raycast(grabPoint.transform.position, -Vector2.up, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        RaycastHit2D upCheck = Physics2D.Raycast(grabPoint.transform.position, Vector2.up, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        RaycastHit2D leftCheck = Physics2D.Raycast(grabPoint.transform.position, Vector2.left * ScaleX, 1.0f, 1 << LayerMask.NameToLayer("Ground"));

        if ((downCheck || upCheck) && !leftCheck && yVelocity < 0) {
            //ledgeGrabbed = true;
            //yVelocity = 0;

        }
        else if(ledgeGrabbed){
            //ledgeGrabbed = false;
            jumpCounter = jumpMax - 1;
        }


        //Move
        cc.Move(move);

        //WALLCHECK
        wallLeft = Physics2D.Raycast(transform.position, Vector3.left, characterWidth + 0.01f, finalMask);
        wallRight = Physics2D.Raycast(transform.position, Vector3.right, characterWidth + 0.01f, finalMask);
        wallUpLeft = Physics2D.Raycast(transform.position + new Vector3(0, 0.4f, 0) + crouchVector, Vector3.left, characterWidth + 0.01f, finalMask);
        wallUpRight = Physics2D.Raycast(transform.position + new Vector3(0, 0.4f, 0) + crouchVector, Vector3.right, characterWidth + 0.01f, finalMask);

        if ((wallLeft || wallUpLeft) && move.x < 0) {
            move.x = 0;
        }
        else if ((wallRight || wallUpRight) && move.x > 0) {
            move.x = 0;
        }




        //WALLFIX
        //.45 hahmon leveys
        float cW = characterWidth - 0.01f;
        wallLeftfix = Physics2D.Raycast(transform.position, Vector3.left, cW, 1 << LayerMask.NameToLayer("Ground"));
        if (wallLeft && wallLeftfix) {
            if (wallLeftfix.distance < cW + 0.01) {
                Vector3 currentPosition = transform.position;
                float xfix = wallLeftfix.distance - (cW + 0.01f);
                currentPosition.x -= xfix;
                transform.position = currentPosition;
                move.x = 0;
            }
        }

        wallRightfix = Physics2D.Raycast(transform.position, Vector3.right, cW, 1 << LayerMask.NameToLayer("Ground"));
        if (wallRight && wallRightfix) {
            if (wallRightfix.distance < cW + 0.01) {
                Vector3 currentPosition = transform.position;
                float xfix = wallRightfix.distance - (cW + 0.01f);
                currentPosition.x += xfix;
                transform.position = currentPosition;
                move.x = 0;
            }
        }
        Debug.DrawLine(transform.position + new Vector3(0, 0, 0), transform.position + new Vector3(cW, 0, 0), Color.red);
        Debug.DrawLine(transform.position + new Vector3(0, 0, 0), transform.position + new Vector3(-cW, 0, 0), Color.red);
        Debug.DrawLine(transform.position + new Vector3(0, 0.4f, 0) + crouchVector, transform.position + new Vector3(cW, 0.4f, 0) + crouchVector, Color.red);
        Debug.DrawLine(transform.position + new Vector3(0, 0.4f, 0) + crouchVector, transform.position + new Vector3(-cW, 0.4f, 0) + crouchVector, Color.red);

        groundhitleft = Physics2D.Raycast(transform.position + new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
        groundhitright = Physics2D.Raycast(transform.position - new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
        LeftGround = groundhitleft;
        RightGround = groundhitright;
        Debug.DrawRay(transform.position + new Vector3((characterWidth - wallTolerance), 0, 0), new Vector3(0, -groundhitleft.distance, 0), Color.red);
        Debug.DrawRay(transform.position - new Vector3((characterWidth - wallTolerance), 0, 0), new Vector3(0, -groundhitright.distance, 0), Color.cyan);

        //.64 on puolet hahmonpituudesta


        groundhit = Physics2D.Raycast(transform.position, Vector3.down, characterHeight + slopeTolerance, finalMask);
        if (groundhit && onGround && yVelocity <= 0) {
            if (groundhit.distance < (characterHeight - 0.01f + slopeTolerance)) {
                Vector3 currentPosition = transform.position;
                float yfix = groundhit.distance - (characterHeight - 0.01f);
                currentPosition.y -= yfix;
                transform.position = currentPosition;
            }
        }
        else {

            groundhitleft = Physics2D.Raycast(transform.position + new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
            if (groundhitleft && onLeftGround && yVelocity <= 0) {
                if (groundhitleft.distance < characterHeight + slopeTolerance) {
                    Vector3 currentPosition = transform.position;
                    float yfix = groundhitleft.distance - (characterHeight - 0.01f);
                    currentPosition.y -= yfix;
                    transform.position = currentPosition;
                }
            }

            groundhitright = Physics2D.Raycast(transform.position - new Vector3((characterWidth - wallTolerance), 0, 0), Vector3.down, characterHeight + slopeTolerance, finalMask);
            if (groundhitright && onRightGround && yVelocity <= 0) {
                if (groundhitright.distance < characterHeight + slopeTolerance) {
                    Vector3 currentPosition = transform.position;
                    float yfix = groundhitright.distance - (characterHeight - 0.01f);
                    currentPosition.y -= yfix;
                    transform.position = currentPosition;
                }
            }
        }

        //DASHCOUNTER
        if(dashChargeSpeed > 1.0f) {
            dashChargeSpeed = 0;
            dashCounter++;

        }else if(dashCounter < dashCounterMax) {
            dashChargeSpeed += dashChargeSpeedMax * Time.deltaTime;
        }



        /*
        STEEPCLIFF = false;
        if (Vector2.Angle(groundhitleft.normal, new Vector2(0, 1)) > 45){
            STEEPCLIFF = true;
            STEEPVECTOR = groundhitleft.normal;
        }
        if (Vector2.Angle(groundhitright.normal, new Vector2(0, 1)) > 45) {
            STEEPCLIFF = true;
            STEEPVECTOR = groundhitright.normal;
        }
        */

        /*
        Debug.DrawRay(transform.position, new Vector3(0, -0.65f, 0), Color.red);

        Debug.DrawRay(transform.position - new Vector3(-0.4f, 0, 0), new Vector3(0, -0.65f, 0), Color.red);
        Debug.DrawRay(transform.position - new Vector3(0.4f, 0, 0), new Vector3(0, -0.65f, 0), Color.red);

        Debug.DrawRay(transform.position, new Vector3(0, 0.65f, 0), Color.red);

        Debug.DrawRay(transform.position, new Vector3(0.4f, 0.4f, 0), Color.red);
        Debug.DrawRay(transform.position, new Vector3(-0.4f, 0.4f, 0), Color.red);

        Debug.DrawRay(transform.position, new Vector3(0.4f, -0.4f, 0), Color.red);
        Debug.DrawRay(transform.position, new Vector3(-0.4f, -0.4f, 0), Color.red);
        */

        //AIM
        //Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gun.position;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //gun.rotation = Quaternion.Slerp(gun.rotation, rotation, 5f * Time.deltaTime);
        //gun.rotation = Quaternion.Lerp(gun.rotation, rotation, 1);

        //FLIP
        /*
        Vector2 aimDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        
        if (aimDir.x > 0f && lookLeft) {
            //GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
            //gun.GetComponent<SpriteRenderer>().flipY = !gun.GetComponent<SpriteRenderer>().flipY;
            //gun.GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
            transform.localScale = new Vector3(ScaleX, ScaleY, 0);
            lookLeft = false;
            OnFlip();
        }
        else if (aimDir.x < 0f && !lookLeft) {
            //GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
            //gun.GetComponent<SpriteRenderer>().flipY = !gun.GetComponent<SpriteRenderer>().flipY;
            //gun.GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
            transform.localScale = new Vector3(-ScaleX, ScaleY, 0);
            lookLeft = true;
            OnFlip();
        }
        */
    }

    public void Flip()
    {
        if (ledgeGrabbed)
            return;
        transform.localScale = new Vector3(transform.localScale.x * -1, ScaleY, 0);
        lookLeft = !lookLeft;
        OnFlip();
    }

    //OUTPUTS
    public void OnJump()
    {
        IKCharacterOutput.AddForce(-1f);

        IAudio.PlaySound("Jump");
    }

    public void OnLand()
    {
        IKCharacterOutput.AddForce(yVelocity * 5);
        IKCharacterOutput.LiftSolvers();
        IAudio.PlaySound("Land");
        move.x = 0;
    }

    public void OnFlip()
    {
        IKCharacterOutput.Flip();
    }

    public void OnDash()
    {
        IAudio.PlaySound("Dash");
    }

    //STATES
    public bool IsTouchingLeftWall()
    {
        return (wallLeft || wallUpLeft);
    }
    public bool IsTouchingRightWall()
    {
        return (wallRight || wallUpRight);
    }

    public bool IsOnLedge()
    {
        RaycastHit2D checkLeft = Physics2D.Raycast(transform.position - new Vector3(-0.4f, 0, 0), Vector3.down, 1.7f, 1 << LayerMask.NameToLayer("Ground"));
        RaycastHit2D checkRight = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.down, 1.7f, 1 << LayerMask.NameToLayer("Ground"));
        return !((checkLeft && !lookLeft) || (checkRight && lookLeft));
    }
    public bool IsBumpingCeiling()
    {
        return (hitCeilingLeft || hitCeilingRight);
    }
    public bool IsDashing()
    {
        return dash;
    }

    public float GetLocalScaleX()
    {
        return transform.localScale.x;
    }

    public bool IsLanded()
    {
        if ((onGround || (onLeftGround || onRightGround)) && yVelocity <= 0)
            return true;
        return false;
    }

    public bool IsOnGround()
    {
        return landed;
    }

    public bool IsLookingLeft()
    {
        return lookLeft;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }
    public bool GetLookDir()
    {
        return lookLeft;
    }


    public bool IsChrouching()
    {
        return crouch;
    }

    //INPUTS
    public void InputChrouch()
    {
        if (ledgeGrabbed)
            ledgeGrabbed = false;
        if (crouch) {
            crouch = false;
            ChrouchOff();
        }
        else {
            crouch = true;
            ChrouchOn();
        }
    }

    public void InputJump()
    {
        if (ledgeGrabbed)
            ledgeGrabbed = false;
        if ((onGround || onLeftGround || onRightGround)) {
            jumpCounterTimer = 0;
            jumped = false;
        }
        else if (jumpCounter < jumpMax) {
            yVelocity = (jumpPower);
            jumpCounter++;
            
            OnJump();
        }
    }

    public void InputDash()
    {
        if (dashCounter > 0) {
            dashCounter--;
            //Debug.Log("DASH!");
            dash = true;
            dashStopTimer = 0;

            OnDash();
        }
    }

    public void InputLevitate()
    {
        yVelocity = (jumpPower);
    }

    public void InputAddForce(Vector2 f)
    {
        move = f;
        yVelocity = 0;
    }
    public void ChrouchOn()
    {
        crouchVector = new Vector3(0, -0.4f, 0);
        IKCharacterOutput.ChrouchOn();
    }
    public void ChrouchOff()
    {
        crouchVector = new Vector3(0, 0, 0);
        IKCharacterOutput.ChrouchOff();
    }
    public void ForceJumpDir(Vector2 jumpDir)
    {

    }
    public void MovementInput(Vector2 mInput)
    {
        moveInput = mInput;
    }



    public void SetJumpForce(float force = 0.2f)
    {
        jumpPower = force;
    }

    public Vector2 Movement()
    {
        return move;
    }
    

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    // Properties
    public void AddJump()
    {
        jumpMax++;
    }

    public void SubJump()
    {
        jumpMax--;
    }

    public void AddDash()
    {
        dashCounterMax++;
        dashCounter = dashCounterMax;
    }

    public void SubDash()
    {
        dashCounterMax--;
    }
}
