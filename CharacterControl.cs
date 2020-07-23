using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class CharacterControl : MonoBehaviour {
    public float speedAcceleration = 5.0f;
    public float maxSpeed = 2f;
    public float gravity = 0.5f;
    public float jumpPower = 10.0f;
    public float jumpCounter = 0;
    public bool crouch = false;


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

    //RaycastPoints
    public GameObject bottomRight;
    public GameObject bottomLeft;
    public GameObject sideRight;
    public GameObject sideLeft;
    public GameObject HeadRight;
    public GameObject HeadLeft;

    //Gun
    [HideInInspector] public bool lookLeft = false;
    [HideInInspector] public bool fireButtonDown = false;
    [HideInInspector] public bool fireRate = true;
    [HideInInspector] public Timer FireRateCoolDown;
    public bool enableShoot = true;

    //Move
    [HideInInspector] public Vector3 moveInput = new Vector3();
    [HideInInspector] public Vector3 move = new Vector3();
    [HideInInspector] public Timer jumpTimer;
    public float jumpCounterTimer = 1f;
    private bool jumped = true;

    public Transform gun;
    public AudioClip shootsound;
    public GameObject Missile;

    public bool STEEPCLIFF = false;
    public Vector3 STEEPVECTOR;

    private float yVelocity = 0f;

    private CharacterController cc;
    private Transform gchck;
    private IKCharacterAdapter IKCharacterOutput;

    private AudioSource p_as;

    //After
    public bool landed = false;


    public float ScaleX;
    public float ScaleY;

    // Use this for initialization
    void Start() {
        cc = GetComponent<CharacterController>();
        p_as = GetComponent<AudioSource>();
        IKCharacterOutput = GetComponent<IKCharacterAdapter>();

        fireButtonDown = false;
        fireRate = true;
        FireRateCoolDown = new Timer(200);
        FireRateCoolDown.Elapsed += OnFireRateCoolDown;

        jumpTimer = new Timer(100);
        jumpTimer.Elapsed += OnJumpTimer;


        ScaleX = transform.localScale.x;
        ScaleY = transform.localScale.y;
    }

    private void OnFireRateCoolDown(object source, ElapsedEventArgs e)
    {
        fireRate = true;
    }

    private void OnJumpTimer(object source, ElapsedEventArgs e)
    {

    }

    // Update is called once per frame
    void FixedUpdate() {

        landed = IsLanded();

        int GroundLayer = LayerMask.NameToLayer("Ground");
        int GroundHelpLayer = LayerMask.NameToLayer("HelpGeom");
        int Mask1 = 1 << GroundLayer;
        int Mask2 = 1 << GroundHelpLayer;
        int finalMask = Mask1 | Mask2;

        float ChrouchZ = Input.GetAxis("CrouchZ");
        if (ChrouchZ > 0)
            ChrouchZ = 0;
        IKCharacterOutput.AddToRootOnce(ChrouchZ);
        moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        onGround = Physics2D.Raycast(transform.position, Vector3.down, 0.65f, finalMask);
        onLeftGround = Physics2D.Raycast(transform.position - new Vector3(-0.4f, 0, 0), Vector3.down, 0.65f, finalMask);
        onRightGround = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.down, 0.65f, finalMask);

        RaycastHit2D groundhit = Physics2D.Raycast(transform.position, Vector3.down, 0.65f, finalMask);
        RaycastHit2D groundhitleft = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.down, 0.65f, finalMask);
        RaycastHit2D groundhitright = Physics2D.Raycast(transform.position - new Vector3(-0.4f, 0, 0), Vector3.down, 0.65f, finalMask);

        hitCeilingLeft = Physics2D.Raycast(transform.position - new Vector3(-0.4f, 0, 0), Vector3.up, 0.70f, finalMask);
        hitCeilingRight = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.up, 0.70f, finalMask);

        wallLeft = Physics2D.Raycast(transform.position, Vector3.left, 0.42f, finalMask);
        wallRight = Physics2D.Raycast(transform.position, Vector3.right, 0.42f, finalMask);
        wallUpLeft = Physics2D.Raycast(transform.position + new Vector3(0, 0.4f, 0), Vector3.left, 0.42f, finalMask);
        wallUpRight = Physics2D.Raycast(transform.position + new Vector3(0, 0.4f, 0), Vector3.right, 0.42f, finalMask);

        RaycastHit2D wallLeftfix = Physics2D.Raycast(transform.position, Vector3.left, 0.40f, finalMask);
        RaycastHit2D wallRightfix = Physics2D.Raycast(transform.position, Vector3.right, 0.40f, finalMask);



        if (!onGround && !onLeftGround && !onRightGround) {
            yVelocity -= gravity * Time.deltaTime;
        }
        else {
            if (landed != IsLanded())
                OnLand();
            yVelocity = 0;
            jumpCounter = 0;
            
        }
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

        if (Input.GetButtonDown("Jump") && (onGround || onLeftGround || onRightGround)) {
            jumpCounterTimer = 0;   
            jumped = false;
            //OnJump();
        }else if (Input.GetButtonDown("Jump") && jumpCounter == 0) {
            yVelocity = (jumpPower) * Time.deltaTime;
            jumpCounter++;
        }

        if(jumpCounterTimer > 0.1f && !jumped) {
            yVelocity = jumpPower * Time.deltaTime;
            onGround = false;
            onLeftGround = false;
            onRightGround = false;
            jumped = true;
            OnJump();
        }
        else if(!jumped) {
            jumpCounterTimer += Time.deltaTime;
            IKCharacterOutput.AddToRootOnce(-0.75f * (jumpCounterTimer*10));
        }

        //Ceiling
        if ((hitCeilingLeft || hitCeilingRight) && yVelocity > 0) {
            yVelocity = 0;
        }

        if (yVelocity < -1)
            yVelocity = -1;
   
        if ((move.x > maxSpeed * moveInput.x || move.x < maxSpeed * moveInput.x) && !STEEPCLIFF) {
            move = new Vector3(maxSpeed * moveInput.x, move.y);
        }else if (STEEPCLIFF) {
            if(onLeftGround)
                STEEPVECTOR = Quaternion.AngleAxis(90, Vector3.forward) * (STEEPVECTOR);
            else
                STEEPVECTOR = Quaternion.AngleAxis(-90, Vector3.forward) * (STEEPVECTOR);
            move = STEEPVECTOR * Time.deltaTime * 10;
        }

        //GROUNDCHECK
        
        /*if (groundhitleft && onGround) {
            if (groundhitleft.distance < 0.64f) {
                Vector3 currentPosition = transform.position;
                float yfix = 0.64f - groundhitleft.distance;
                currentPosition.y += yfix;
                transform.position = currentPosition;
            }
        }
        if (groundhitright && onGround) {
            if (groundhitright.distance < 0.64f) {
                Vector3 currentPosition = transform.position;
                float yfix = 0.64f - groundhitright.distance;
                currentPosition.y += yfix;
                transform.position = currentPosition;
            }
        }*/

        //WALLCHECK
        if (wallLeft && move.x < 0) {
            move.x = 0;
        }else if(wallRight && move.x > 0) {
            move.x = 0;
        }
        if (wallUpLeft && move.x < 0) {
            move.x = 0;
        }
        else if (wallUpRight && move.x > 0) {
            move.x = 0;
        }


        //LEDGECHECK
        RaycastHit2D downCheck = Physics2D.Raycast(grabPoint.transform.position, -Vector2.up, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        RaycastHit2D leftCheck = Physics2D.Raycast(grabPoint.transform.position, Vector2.left * ScaleX, 1.0f, 1 << LayerMask.NameToLayer("Ground"));
        if (downCheck && !leftCheck && yVelocity < 0) {
             ledgeGrabbed = true;
             yVelocity = 0;

        }
        else {
            ledgeGrabbed = false;
        }
        



        move.y = yVelocity;

        
        //Move
        cc.Move(move);


        //GROUNDFIX
        /*groundhit = Physics2D.Raycast(transform.position, Vector3.down, 0.80f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit && onGround) {
            if (groundhit.distance < 0.80f) {
                Vector3 currentPosition = transform.position;
                float yfix = groundhit.distance - 0.64f;
                currentPosition.y -= yfix;
                transform.position = currentPosition;
            }
        }*/

        groundhitleft = Physics2D.Raycast(transform.position + new Vector3(0.4f, 0, 0), Vector3.down, 0.80f, finalMask);
        groundhitright = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.down, 0.80f, finalMask);
        LeftGround = groundhitleft;
        RightGround = groundhitright;
        Debug.DrawRay(transform.position + new Vector3(0.4f, 0, 0), new Vector3(0, -groundhitleft.distance, 0), Color.red);
        Debug.DrawRay(transform.position - new Vector3(0.4f, 0, 0), new Vector3(0, -groundhitright.distance, 0), Color.cyan);


        if (groundhit && onGround && !onLeftGround && !onRightGround) {
            if (groundhit.distance < 0.64f) {
                Vector3 currentPosition = transform.position;
                float yfix = groundhit.distance - 0.64f;
                currentPosition.y -= yfix;
                transform.position = currentPosition;
            }
        }
        else {
            if (groundhitleft && onLeftGround) {
                if (groundhitleft.distance < 0.80f) {
                    Vector3 currentPosition = transform.position;
                    float yfix = groundhitleft.distance - 0.64f;
                    currentPosition.y -= yfix;
                    transform.position = currentPosition;
                }
            }
            if (groundhitright && onRightGround) {
                if (groundhitright.distance < 0.80f) {
                    Vector3 currentPosition = transform.position;
                    float yfix = groundhitright.distance - 0.64f;
                    currentPosition.y -= yfix;
                    transform.position = currentPosition;
                }
            }
        }

        


        STEEPCLIFF = false;
        if (Vector2.Angle(groundhitleft.normal, new Vector2(0, 1)) > 45){
            STEEPCLIFF = true;
            STEEPVECTOR = groundhitleft.normal;
        }
        if (Vector2.Angle(groundhitright.normal, new Vector2(0, 1)) > 45) {
            STEEPCLIFF = true;
            STEEPVECTOR = groundhitright.normal;
        }

        //WALLFIX
        wallLeftfix = Physics2D.Raycast(transform.position, Vector3.left, 0.45f, 1 << LayerMask.NameToLayer("Ground"));
        wallRightfix = Physics2D.Raycast(transform.position, Vector3.right, 0.45f, 1 << LayerMask.NameToLayer("Ground"));
        if (wallLeft && wallLeftfix) {
            if (wallLeftfix.distance < 0.41f) {
                Vector3 currentPosition = transform.position;
                float xfix = wallLeftfix.distance - 0.41f;
                currentPosition.x -= xfix;
                transform.position = currentPosition;
                move.x = 0;
            }
        }
        if (wallRight && wallRightfix) {
            if (wallRightfix.distance < 0.41f) {
                Vector3 currentPosition = transform.position;
                float xfix = wallRightfix.distance - 0.41f;
                currentPosition.x += xfix;
                transform.position = currentPosition;
                move.x = 0;
            }
        }
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
        Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gun.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //gun.rotation = Quaternion.Slerp(gun.rotation, rotation, 5f * Time.deltaTime);
        //gun.rotation = Quaternion.Lerp(gun.rotation, rotation, 1);

        //FLIP
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

        //FIRE
        if (Input.GetMouseButtonDown(0)) {
            fireButtonDown = true;
        }
        if (Input.GetMouseButtonUp(0)) {
            fireButtonDown = false;
        }
        if (fireRate && fireButtonDown && enableShoot) {
            fireRate = false;
            Vector2 GunAimDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gun.transform.position;
            angle = Mathf.Atan2(GunAimDir.y, GunAimDir.x) * Mathf.Rad2Deg;

            GameObject bullet = Instantiate(Missile, gun.transform.position, rotation);


            float h = Mathf.Sqrt(GunAimDir.x * GunAimDir.x + GunAimDir.y * GunAimDir.y);
            float nX = GunAimDir.x / h;
            float nY = GunAimDir.y / h;
            float bulletVelocity = 25f;

            
            bullet.SendMessage("SetVeloctiyVector", new Vector2(nX * bulletVelocity, nY * bulletVelocity));

            //bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(nX * bulletVelocity, nY * bulletVelocity);
            //rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            //bullet.transform.rotation = Quaternion.Lerp(gun.rotation, rotation, 1f);

            p_as.PlayOneShot(shootsound);

            
            FireRateCoolDown.Start();
        }
    }

    void FixPosition()
    {

    }

    public void FlipX(float s)
    {

        transform.localScale = new Vector3(ScaleX * s, ScaleY, 0);
    }

    public float GetLocalScaleX()
    {
        return transform.localScale.x;
    }

    public bool IsLanded()
    {
        if (onGround || (onLeftGround || onRightGround))
            return true;
        return false;
    }
    
    //OUTPUTS
    public void OnJump()
    {
        IKCharacterOutput.AddForce(-1f);
    }

    public void OnLand()
    {
        IKCharacterOutput.AddForce(yVelocity*5);
        IKCharacterOutput.LiftSolvers();
    }

    public void OnFlip()
    {
        IKCharacterOutput.Flip();
    }
}
