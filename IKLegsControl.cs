using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(IKCharacterAdapter))]
[RequireComponent(typeof(Footsteps))]
[RequireComponent(typeof(IKFeetAdjust))]

public class IKLegsControl : MonoBehaviour {

    //Komponentit
    public GameObject root;
    private CharacterControl Character;
    private IFootsteps fsteps;
    private IFootAdjustment IK_feet;
    private IFootAdjustment Dfeet = new DeAdjustFeet();

    //Ratkaisijat
    public GameObject SolverLeft;
    public GameObject SolverRight;
    public GameObject SolverChest;

    //STATES
    public enum STATES
    {
        Idle,
        Moving,
        Air
    }
    public enum HILL_LEVEL
    {
        Even,
        LeftDownHill,
        RightDownhill
    }
    public enum SIMULATE_LEGS
    {
        ONLY_LEFT,
        ONLY_RIGHT,
        BOTH_SIMULTANEOUS,
        BOTH_ALTERNATE
    }
    public enum STANCES
    {
        Standing,
        Crouching
    }
    public STATES STATE = STATES.Idle; //IDLE, MOVING, AIR
    public HILL_LEVEL HILL = HILL_LEVEL.Even; // EVEN, UPHILL, DOWNHILL
    public STANCES STANCE = STANCES.Standing; // STANDING, CROUCHING
    private bool AdjustToA = false;

    //ROOT
    private float rootX;
    private float rootY;
    private float rootForceX = 0;
    private float rootForceY = 0;
    private float rootAdjust = 0;
    [HideInInspector] public float rootAdjustOnce = 0;
    private float gaitFix = 0;
    private float rootForceDecay = 10f;
    private float standingUpSpeed = 0.99f;

    //O-Pos
    private Vector3 chestO;
    private Vector3 landingPoint;

    //Speed
    private float speed = 0;
    private float rest_speed = 0; //current state of rest
    private bool leftSupport = true;
    private bool running = false;
    private bool resting = true;

    private Vector2 anchorLeft;
    private Vector2 anchorRight;


    //Bezier Curve
    private Vector2 restA;
    private Vector2 restB;
    private Vector2 restC;
    private Vector2 MoveA;
    private Vector2 MoveB;
    private Vector2 MoveC;

    //Hahmo
    public float CharacterHeight = 0.64f;
    private float CX ;

    //STEP - Adjust
    [Range(-10.0f, 10.0f)] public float searchStepAdjust = 0;//Askeleen etsintä pituus
    [Range(-10.0f, 10.0f)] public float xStepAdjust = 0;
    [Range(0.0f, 10.0f)] public float restSpeed = 10f;
    private float stepFrequency = 10f;

    //STEP - Freq
    [Range(-100f, 1000.0f)] public float stepFreqWalk = 500;
    [Range(-100f, 1000.0f)] public float stepFreqHill = 400;
    [Range(-100f, 1000.0f)] public float stepFreqBack = 200;
    [Range(-100f, 1000.0f)] public float stepFreqHillBack = 200;

    [Range(1.0f, 20.0f)] public float hipRaise = 10f;
    [Range(0.1f, 2.0f)] public float heelLift = 0.5f;

    //STEP - Lenghts
    [Range(-2.0f, 2.0f)] public float lenStep = 0.0f;//Askeleen pituus (Mäki - takaperin)
    [Range(-100.0f, 100.0f)] public float xRestStepPos = 0;//Viimeinen lepoon askeleen etsintä pituus
    [Range(2.0f, 0.0f)] public float hillAdjustY = 0.6f;

    //LIMITS
    private float bottomDis = 0.64f;


	// Use this for initialization
	void Start () {
        Character = GetComponent<CharacterControl>();
        fsteps = GetComponent<Footsteps>();
        IK_feet = GetComponent<IKFeetAdjust>();
        CharacterHeight = Character.characterHeight;
        if (IK_feet == null) {
            IK_feet = Dfeet;    
        }

        rootX = root.transform.localPosition.x;
        rootY = root.transform.localPosition.y;
        
        anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
        anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
            

        if (leftSupport)
            MoveA = anchorLeft;
        else
            MoveA = anchorRight;

        chestO = SolverChest.transform.localPosition;

    }

    // Update is called once per frame
    void Update() {

        //Debug
        /*
        if (Input.GetButtonDown("Up")) {
            SolverChest.transform.position = new Vector2(SolverChest.transform.position.x, SolverChest.transform.position.y + 0.05f);
            rootY += 0.05f;
            rootAdjust += 1;
        }
        if (Input.GetButtonDown("Down")) {
            root.transform.position = new Vector2(SolverChest.transform.position.x, SolverChest.transform.position.y - 0.05f);
            rootY -= 0.05f;
            rootAdjust -= 1;
        }*/


        //CX
        CX = (1 / Character.GetLocalScaleX()) * Character.ScaleX;

        // ON GROUND Walking
        if (Character.move.x == 0)
            STATE = STATES.Idle;
        else {
            resting = false;
            STATE = STATES.Moving;
        }
            

        if (Character.IsOnGround()) {
            //ASKELTIHEYS
            if (Character.move.x < 0 && CX > 0 || Character.move.x > 0 && CX < 0) {
                if (getAngle() == 0)
                    stepFrequency = 100 - Mathf.Abs(Character.move.x) * stepFreqBack;
                else {
                    stepFrequency = 100 - Mathf.Abs(Character.move.x) * stepFreqHillBack;
                }
            }
            else if (getAngle() == 0)
                stepFrequency = 100 - Mathf.Abs(Character.move.x) * stepFreqWalk;
            else
                stepFrequency = 100 - Mathf.Abs(Character.move.x) * stepFreqHill;
            //

            if (STATE == STATES.Moving) {
                //ASKELEEN LASKEUTUMISPISTEEN ENNAKOINTI
                //TAKAPERIN KÄVELY
                if (Character.move.x < 0 && CX > 0 || Character.move.x > 0 && CX < 0) {
                    if (HILL == HILL_LEVEL.LeftDownHill) {
                        if(CX > 0)
                            FindBackwardStep(new Vector3(lenStep * CX + Character.move.x, 0));
                        else
                            FindBackwardStep(new Vector3(-0.5f * CX, 0));

                    }
                    else if (HILL == HILL_LEVEL.RightDownhill) {
                        if (CX > 0)
                            FindBackwardStep(new Vector3(-0.5f * CX, 0));
                        else
                            FindBackwardStep(new Vector3(lenStep * CX + Character.move.x, 0));

                    }
                    else {
                        FindBackwardStep(new Vector3(-0.5f * CX, 0));
                    }

                }
                //ETUPERIN KÄVELY
                else {
                    if (getAngle() == 0) {//Tasainen maasto
                        if (Character.maxSpeed * CX == Character.move.x)//Juokseeko
                            FindFootStep(new Vector3(((0 + 0.55f) + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                        else
                            FindFootStep(new Vector3(((0 + 0.4f) + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                    }  
                    else {//Kaalteva taso   
                        if (HILL == HILL_LEVEL.LeftDownHill && Character.move.x < 0 || HILL == HILL_LEVEL.RightDownhill && Character.move.x > 0) {//Ylämäki
                            FindFootStep(new Vector3(((0 + 0.1f) + Mathf.Abs(Character.move.x)*2) * CX , 0));
                        }
                        else if (HILL == HILL_LEVEL.LeftDownHill && Character.move.x > 0 || HILL == HILL_LEVEL.RightDownhill && Character.move.x < 0) {//Alamäki
                            FindFootStep(new Vector3(((0 + 0.55f) + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                        }
                        else {
                            FindFootStep(new Vector3(((0 + 0.4f) + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                        }
                    }
                }

                //KÄVELYN SIMULOINTI
                Vector2 newvec = Walking(Character.move.x * stepFrequency * CX);

                if (leftSupport) {
                    SolverLeft.transform.position = Vector2.LerpUnclamped(SolverLeft.transform.position, new Vector2(newvec.x, newvec.y), 0.5f);
                    SolverRight.transform.position = new Vector2(anchorRight.x, anchorRight.y);
                    IK_feet.ResetLeftFoot();
                    IK_feet.AdjustRight();
                }
                else {
                    SolverRight.transform.position = Vector2.LerpUnclamped(SolverRight.transform.position, new Vector2(newvec.x, newvec.y), 0.5f);
                    SolverLeft.transform.position = new Vector2(anchorLeft.x, anchorLeft.y);
                    IK_feet.ResetRightFoot();
                    IK_feet.AdjustLeft();

                }

                //Nosto
                if(STANCE == STANCES.Crouching) {
                    MoveB = new Vector2(MoveC.x - 0.2f * CX, MoveC.y + Mathf.Abs(Character.move.x) * 5);
                }else
                    MoveB = new Vector2(MoveC.x - 0.2f*CX, MoveC.y + heelLift + Mathf.Abs(Character.move.x)*5);

                //FORWARDS - ASKELVALMIS JA TUKIJALAN VAIHTO
                if (speed >= 1) {
                    speed = 0;
                    if (leftSupport) {
                        anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
                        anchorRight.x -= transform.position.x;
                        anchorRight.x *= CX;
                        anchorRight.x += transform.position.x;
                        MoveA = anchorRight;
                    }
                    else {
                        anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
                        anchorLeft.x -= transform.position.x;
                        anchorLeft.x *= CX;
                        anchorLeft.x += transform.position.x;
                        MoveA = anchorLeft;
                    }
                    MoveA.x -= transform.position.x;
                    MoveA.x *= CX;
                    MoveA.x += transform.position.x;
                    leftSupport = !leftSupport;
                    fsteps.PlayFootStep();
                }
                //BACKWARDS
                else if (speed < 0) {
                    speed = 1;
                    if (leftSupport) {
                        anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
                        MoveC = anchorRight;
                    }
                    else {
                        anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
                        MoveC = anchorLeft;
                    }
                    leftSupport = !leftSupport;

                    fsteps.PlayFootStep();
                }
            }

            else if (STATE == STATES.Idle) {
                //PYSÄHTYMINEN
                if (speed < 1 && !resting) {
                    FindFootStep(Vector3.zero); //Viedään jo liikellä oleva askel kehon eteen
                    Vector2 newvec = Walking(restSpeed);
                    if (leftSupport) {
                        SolverLeft.transform.position = new Vector2(newvec.x, newvec.y);
                        SolverRight.transform.position = new Vector2(anchorRight.x, anchorRight.y);
                        restA = new Vector2(anchorRight.x, anchorRight.y);
                    }
                    else {
                        SolverRight.transform.position = new Vector2(newvec.x, newvec.y);
                        SolverLeft.transform.position = new Vector2(anchorLeft.x, anchorLeft.y);
                        restA = new Vector2(anchorLeft.x, anchorLeft.y);
                    }
                    restC = FindFootstepRest(new Vector2(xRestStepPos * CX, 0));
                    restB = new Vector2(restC.x - 0.1f, restC.y + 0.2f);
                    rest_speed = 0;

                }
                //LEPOON
                else {
                    //Debug.DrawLine(restA, restB, Color.white);
                    //Debug.DrawLine(restB, restC, Color.yellow);
                   
                    if (rest_speed < 1) {
                        Vector2 newvec = WalkingToRest(restSpeed, restA, restB, restC + new Vector2(xRestStepPos*CX, 0));
                        //Debug.Log("V:" + newvec.x + "," + newvec.y);
                        if (!leftSupport) {;
                            SolverLeft.transform.position = new Vector2(newvec.x, newvec.y);
                            anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
                            anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
                            MoveA = anchorLeft;
                        }
                        else {
                            SolverRight.transform.position = new Vector2(newvec.x, newvec.y);
                            anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
                            anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
                            MoveA = anchorRight;
                        }
                        speed = 0;
                        resting = true;
                    }
                }

                //Jalkapohjien asettaminen
                LiftSolversFeet();
                IK_feet.AdjustBoth();
                
            }
        }
        //ILMASSA
        else {
            Vector2 newpos1;
            Vector2 newpos2;
            STATE = STATES.Air;
            float moveY = Character.move.y;

            //Vapaa pudotus
            if (!Character.ledgeGrabbed) {
                if (moveY > 0)
                    moveY = 0;
                if (leftSupport) {
                    newpos1 = new Vector2(SolverRight.transform.position.x + Character.move.x / 4, SolverRight.transform.position.y + moveY);
                    newpos2 = new Vector2(SolverLeft.transform.localPosition.x * 0.3f, (SolverLeft.transform.localPosition.y) * 0.2f - 1);

                    SolverRight.transform.position = Vector2.LerpUnclamped(SolverRight.transform.position, newpos1, 0.5f);
                    SolverLeft.transform.localPosition = Vector2.LerpUnclamped(SolverLeft.transform.localPosition, newpos2, 0.5f);
                }
                else {
                    newpos2 = new Vector2(SolverLeft.transform.position.x + Character.move.x / 4, SolverLeft.transform.position.y + moveY);
                    newpos1 = new Vector2(SolverRight.transform.localPosition.x * 0.3f, (SolverRight.transform.localPosition.y) * 0.2f - 1);

                    SolverLeft.transform.position = Vector2.LerpUnclamped(SolverLeft.transform.position, newpos2, 0.5f);
                    SolverRight.transform.localPosition = Vector2.LerpUnclamped(SolverRight.transform.localPosition, newpos1, 0.5f);
                }
                AdjustToA = false;
                IK_feet.ResetFeetOriantion();
            }
            else {
                //REUNAAN TARTUMINEN
                RaycastHit2D ledge = Physics2D.Raycast(transform.position - new Vector3(0, 0.3f), Vector2.right * CX, 1 << LayerMask.NameToLayer("Ground"));
                if (ledge) {
                    SolverLeft.transform.position = Vector2.LerpUnclamped(SolverLeft.transform.position, ledge.point, 0.5f);
                    SolverRight.transform.position = Vector2.LerpUnclamped(SolverRight.transform.position, ledge.point, 0.5f);
                }
            }
            
        }

        //ROOT ADJUST
        if(rootForceX > 0) {
            rootForceX -= Time.deltaTime * rootForceDecay;
            if (rootForceX < 0)
                rootForceX = 0;
        }else if (rootForceX < 0) {
            rootForceX += Time.deltaTime * rootForceDecay;
            if (rootForceX > 0)
                rootForceX = 0;
        }
        if (rootForceY > 0) {
            rootForceY -= Time.deltaTime * rootForceDecay;
            if (rootForceY < 0)
                rootForceY = 0;
        }
        else if (rootForceY < 0) {
            rootForceY += Time.deltaTime * rootForceDecay;
            if (rootForceY > 0)
                rootForceY = 0;
        }



        //CHEST ADJUST
        //-2.25f MAX DOWN FORCE

        Vector2 newpose;
        Vector2 chestpose;
        //Tasapainottele reunalla, jos askeletta ei löydy edestä
        AdjustToA = false;
       if (AdjustToA) {
            newpose = new Vector2(rootX + rootForceX, rootY + rootForceY + gaitFix + bottomDis + rootAdjustOnce);
            if (leftSupport) {
                newpose = new Vector2(SolverLeft.transform.localPosition.x, rootY + rootForceY + gaitFix + bottomDis + rootAdjustOnce);
                chestpose = new Vector2(SolverLeft.transform.localPosition.x, chestO.y);
            }
            else {
                newpose = new Vector2(SolverRight.transform.localPosition.x, rootY + rootForceY + gaitFix + bottomDis + rootAdjustOnce);
                chestpose = new Vector2(SolverRight.transform.localPosition.x, chestO.y);
            }
            //root.transform.position = new Vector2(MoveC.x, root.transform.position.y);
            //float c = SolverChest.transform.position.x - MoveC.x;
            //SolverChest.transform.localPosition = new Vector2(chestO.x - c*CX, chestO.y);
            //SolverChest.transform.localPosition = new Vector2(chestO.x, chestO.y);
            //if (newpose.y < -2.25f)
            // newpose = new Vector3(newpose.x, -2.25f);
        }
       else {
            //nojaa eteenpäin kun on liikeessä
            newpose = new Vector2(rootX + rootForceX, rootY + rootForceY + gaitFix + bottomDis + rootAdjustOnce);
            chestpose = new Vector2(chestO.x + (Character.move.x*CX*1.5f), chestO.y);

            //  if (newpose.y < -2.25f)
            // newpose = new Vector3(newpose.x, -2.25f);
        }


        if (STANCE == STANCES.Crouching) {
            newpose = new Vector3(newpose.x, -2.25f);
            chestpose = new Vector2(chestO.x + Character.characterWidth/2, chestO.y - CharacterHeight*1.5f);
            standingUpSpeed = 0.2f;
        }
        else if(standingUpSpeed < 0.99f) {
            standingUpSpeed += Time.deltaTime;
            if (standingUpSpeed > 0.99f)
                standingUpSpeed = 0.99f;
        }

        SolverChest.transform.localPosition = Vector2.LerpUnclamped(SolverChest.transform.localPosition, chestpose, standingUpSpeed);
        root.transform.localPosition = Vector2.LerpUnclamped(root.transform.localPosition, newpose, standingUpSpeed);


        rootAdjustOnce = 0;
        HillAdjust();
        CheckUpDownHill();
        //LiftSolversFeet();

    }

    public Vector2 Walking (float t)
    {
        speed += t * Time.deltaTime;
        if (speed > 1)
            speed = 1;
        gaitFix = (4 * (-((speed - 0.5f) * (speed - 0.5f)) + 0.25f))/ hipRaise;
        return CalculateBezierPoint(speed);
    }
    public Vector2 WalkingToRest(float t, Vector2 _A, Vector2 _B, Vector2 _C)
    {
        rest_speed += t * Time.deltaTime;
        if (rest_speed > 1)
            rest_speed = 1;

        return CalculateBezierPoint_Rest(rest_speed, _A, _B, _C);
    }

    public Vector2 FindFootStep(Vector3 v)
    {
        RaycastHit2D groundhit;
        AdjustToA = true;
        groundhit = Physics2D.Raycast(transform.position + v + new Vector3(searchStepAdjust, 0) * CX , Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit) {
            AdjustToA = false;
            MoveC = groundhit.point;
            Debug.DrawLine(transform.position + new Vector3(0, 0), groundhit.point, Color.green);
            return groundhit.point;
        }

        return new Vector2(transform.position.x, transform.position.y - CharacterHeight);
    }

    public Vector2 FindBackwardStep(Vector3 v)
    {
        RaycastHit2D groundhit;
        AdjustToA = true;
        groundhit = Physics2D.Raycast(transform.position + v, Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit) {
            AdjustToA = false;
            MoveA = groundhit.point;
            Debug.DrawLine(transform.position + new Vector3(0, 0),groundhit.point, Color.green);
            return groundhit.point;
        }

        return new Vector2(transform.position.x, transform.position.y - CharacterHeight);
    }

    public Vector2 FindFootstepRest(Vector3 v)
    {
        RaycastHit2D groundhit;
        AdjustToA = true;
        groundhit = Physics2D.Raycast(transform.position + v, Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit) {
            AdjustToA = false;
            MoveA = groundhit.point;
            Debug.DrawLine(transform.position + new Vector3(0, 0), groundhit.point, Color.green);
            return groundhit.point;
        }
        else {
            return MoveC;
        }
        
    }

    public Vector2 CalculateBezierPoint(float t)
    {
        float x = 0;
        float y = 0;

        x = (1 - t) * (1 - t) * MoveA.x + 2 * (1 - t) * t * MoveB.x + t * t * MoveC.x;
        y = (1 - t) * (1 - t) * MoveA.y + 2 * (1 - t) * t * MoveB.y + t * t * MoveC.y;

        return new Vector2(x, y);
    }

    public Vector2 CalculateBezierPoint_Rest(float t, Vector2 _A, Vector2 _B, Vector2 _C)
    {

        float x = 0;
        float y = 0;

        x = (1 - t) * (1 - t) * _A.x + 2 * (1 - t) * t * _B.x + t * t * _C.x;
        y = (1 - t) * (1 - t) * _A.y + 2 * (1 - t) * t * _B.y + t * t * _C.y;

        return new Vector2(x, y);
    }

    public void resertAnchorPositions()
    {
        anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
        anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
        MoveC = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
        MoveA = MoveC;

    }
    public void resetSolverPositions()
    {
        SolverRight.transform.position = anchorRight;
        SolverLeft.transform.position = anchorLeft;
    }

    public void AddRootForce(float force)
    {
        rootForceY = force;
    }

    public void Flip()
    {
        CX = (1 / Character.GetLocalScaleX()) * Character.ScaleX;
        speed = 0;
        //leftSupport = !leftSupport;
    }

    public void HillAdjust()
    {
        bottomDis = 0f;
        if (!Character.IsLanded()) {
            return;
        }
        else {
            //0.6f = Hahmon pituus jalasta lantioon
            RaycastHit2D groundhit = Physics2D.Raycast(transform.position, Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhit) {
                bottomDis = CharacterHeight - groundhit.distance;
            }
            RaycastHit2D groundhitleft = Physics2D.Raycast(transform.position - new Vector3(-0.4f, 0, 0), Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhitleft) {
                if (CharacterHeight - groundhitleft.distance < bottomDis)
                    bottomDis = CharacterHeight - groundhitleft.distance;
            }
            RaycastHit2D groundhitright = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhitright) {
                if (CharacterHeight - groundhitright.distance < bottomDis)
                    bottomDis = CharacterHeight - groundhitright.distance;
            }
            Debug.DrawLine(transform.position - new Vector3(-0.4f, 0, 0), groundhitleft.point, Color.green);
            Debug.DrawLine(transform.position - new Vector3(0.4f, 0, 0), groundhitright.point, Color.green);
        }

    }

    public void CheckUpDownHill()
    {
        if (!Character.onRightGround && Character.onLeftGround) {
            HILL = HILL_LEVEL.LeftDownHill;
        }
        else if (Character.onRightGround && !Character.onLeftGround) {
            HILL = HILL_LEVEL.RightDownhill;
        }
        else {
            HILL = HILL_LEVEL.Even;
        }

    }

    public float getAngle()
    {
        RaycastHit2D groundhit = Physics2D.Raycast(transform.position, Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
        Vector2 n = groundhit.normal;
        //Debug.Log(Mathf.Atan2(n.y, n.x) * (180 / Mathf.PI));
        //Debug.Log("getAngle: " + Vector2.Angle(new Vector2(0, 1), n));
        return Vector2.Angle(new Vector2(0, 1), n);
    }

    public void LiftSolvers()
    {
        RaycastHit2D groundhit;
        //Jos maata on hahmon keskellä
        groundhit = Physics2D.Raycast(transform.position + new Vector3(0, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit) {

            //SolverRight.transform.position = groundhit.point;
            //SolverLeft.transform.position = groundhit.point;
            anchorRight = groundhit.point;
            anchorLeft = groundhit.point;
            MoveC = groundhit.point;
            MoveB = groundhit.point;
            MoveA = groundhit.point;
        }
        else {
            //jos maata on hahmon vasemmalla puolella
            groundhit = Physics2D.Raycast(transform.position + new Vector3(-0.4f, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhit) {

                //SolverRight.transform.position = groundhit.point;
                //SolverLeft.transform.position = groundhit.point;
                anchorRight = groundhit.point;
                anchorLeft = groundhit.point;
                MoveC = groundhit.point;
                MoveB = groundhit.point;
                MoveA = groundhit.point;
            }
            else {
                //jos maata on hahmon oikealla puolella
                groundhit = Physics2D.Raycast(transform.position + new Vector3(0.4f, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
                if (groundhit) {

                    //SolverRight.transform.position = groundhit.point;
                    //SolverLeft.transform.position = groundhit.point;
                    anchorRight = groundhit.point;
                    anchorLeft = groundhit.point;
                    MoveC = groundhit.point;
                    MoveB = groundhit.point;
                    MoveA = groundhit.point;
                }
                else {
                    MoveC = transform.position;
                    MoveB = transform.position;
                    MoveA = transform.position;
                    anchorRight = transform.position;
                    anchorLeft = transform.position;
                }
            }
        }
        /*groundhit = Physics2D.Raycast(SolverRight.transform.position + new Vector3(0, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit) {
            SolverRight.transform.position = groundhit.point;
            anchorRight = groundhit.point;
        }
        groundhit = Physics2D.Raycast(SolverLeft.transform.position + new Vector3(0, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit) {
            SolverLeft.transform.position = groundhit.point;
            anchorLeft = groundhit.point;
        }*/
        //leftSupport = !leftSupport;

        restA = MoveC;
        restB = MoveC;
        restC = MoveC;
        speed = 0.9f;
        rest_speed = 0.8f;

    }

    public void LiftSolversFeet()
    {
        RaycastHit2D groundhit;
       
        groundhit = Physics2D.Raycast(SolverRight.transform.position + new Vector3(0, 0.4f), Vector3.down, 1f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit && !leftSupport) {
            SolverRight.transform.position = groundhit.point;
            anchorRight = groundhit.point;
        }
        groundhit = Physics2D.Raycast(SolverLeft.transform.position + new Vector3(0, 0.4f), Vector3.down, 1f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit && leftSupport) {
            SolverLeft.transform.position = groundhit.point;
            anchorLeft = groundhit.point;
        }
    }


    public float GetNormal(GameObject o)
    {
        RaycastHit2D ray = Physics2D.Raycast(o.transform.position, Vector2.down);
        if (ray) {
            float a = Vector2.Angle(new Vector2(0, 1), CX * ray.normal);
            return a;
        }
        return 0;
    }

    public Vector3 GetNormalVector(GameObject o)
    {
        RaycastHit2D ray = Physics2D.Raycast(o.transform.position, Vector2.down);
        if (ray) {
            return ray.normal;
        }
        return Vector3.zero;
    }

    //OUTPUT
    public void OutputWalkingStarts()
    {

    }

    public void OutputWalkingStops()
    {

    }

    public void OutputStep()
    {

    }

    public void RestlessFeet()
    {
        rest_speed = 0;
        resting = false;
        //resertAnchorPositions();
        LiftSolvers();
    }

    public void ChrouchOn()
    {
        STANCE = STANCES.Crouching;
    }

    public void ChrouchOff()
    {
        STANCE = STANCES.Standing;
    }

    public float GetBottomDis()
    {
        return bottomDis;
    }

    public bool GetLeftSupport()
    {
        return leftSupport;
    }
    public float GetLegSpeed()
    {
        return speed;
    }
    public float GetTorsoSpeed()
    {
        return Character.move.x;
    }
    public float GetFallSpeed()
    {
        return Character.move.y;
    }
    public STATES GetState()
    {
        return STATE;
    }

    public bool LegsAreInAir()
    {
        return STATE == STATES.Air;
    }
}
