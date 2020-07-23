using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKLegsControl : MonoBehaviour {

    //Komponentit
    public GameObject footLeft;
    public GameObject footRight;
    public GameObject root;
    private CharacterControl Character;
    private Footsteps fsteps;

    //Ratkaisijat
    public GameObject SolverLeft;
    public GameObject SolverRight;
    public GameObject SolverChest;

    //STATES
    public string STATE = "IDLE"; //IDLE, MOVING, AIR
    public string HILL = "EVEN"; // EVEN, UPHILL, DOWNHILL
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

    //O-Pos
    private Vector3 chestO;
    private Vector2 ShoulderPos;
    private Vector2 HipPos;
    private Quaternion footLeftO;
    private Quaternion footRightO;

    //Speed
    public float speed = 0;
    public float rest_speed = 0;
    public bool leftSupport = true;
    private bool running = false;

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
    [Range(-100.0f, 100.0f)] public float xStepAdjust = 0;//Askeleen etsintä pituus
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
        
        rootX = root.transform.localPosition.x;
        rootY = root.transform.localPosition.y;
        
        anchorRight = new Vector2(SolverRight.transform.position.x, SolverRight.transform.position.y);
        anchorLeft = new Vector2(SolverLeft.transform.position.x, SolverLeft.transform.position.y);
            

        if (leftSupport)
            MoveA = anchorLeft;
        else
            MoveA = anchorRight;

        chestO = SolverChest.transform.localPosition;

        footLeftO = footLeft.transform.localRotation;
        footRightO = footRight.transform.localRotation;

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
        if (Character.moveInput.x == 0)
            STATE = "IDLE";
        else
            STATE = "MOVING";

        if (Character.IsLanded()) {
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


            if (STATE == "MOVING") {

                //ASKELEEN Lasku
                if(Character.maxSpeed * CX == Character.move.x)
                    running = true;
                 else 
                    running = false;
                
                Vector2 newvec = Walking(Character.move.x * stepFrequency * CX);
                //TAAKSEPÄIN
                if (Character.move.x < 0 && CX > 0 || Character.move.x > 0 && CX < 0) {
                    if (HILL == "LEFTDOWNHILL") {
                        if(CX > 0)
                            FindBackwardStep(new Vector3(lenStep * CX + Character.move.x, 0));
                        else
                            FindBackwardStep(new Vector3(-0.5f * CX, 0));

                    }
                    else if (HILL == "RIGHTDOWNHILL") {
                        if (CX > 0)
                            FindBackwardStep(new Vector3(-0.5f * CX, 0));
                        else
                            FindBackwardStep(new Vector3(lenStep * CX + Character.move.x, 0));

                    }
                    else {
                        FindBackwardStep(new Vector3(-0.5f * CX, 0));
                    }

                }
                //ETEENPÄIN
                else {
                    if (getAngle() == 0) {
                        if (running)
                            FindFootStep(new Vector3((0.55f + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                        else
                            FindFootStep(new Vector3((0.4f + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                    }  
                    else {
                        if (HILL == "LEFTDOWNHILL" && Character.move.x < 0) {
                            FindFootStep(new Vector3((0.1f + Mathf.Abs(Character.move.x)*2) * CX , 0));
                        }
                        else if (HILL == "RIGHTDOWNHILL" && Character.move.x > 0) {
                            FindFootStep(new Vector3((0.1f + Mathf.Abs(Character.move.x)*2) * CX, 0));
                        }
                        else if (HILL == "LEFTDOWNHILL" && Character.move.x > 0) {
                            FindFootStep(new Vector3((0.55f + Mathf.Abs(Character.move.x) * 2) * CX, 0));

                        }
                        else if (HILL == "RIGHTDOWNHILL" && Character.move.x < 0) {
                            FindFootStep(new Vector3((0.55f + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                        }
                        else {
                            FindFootStep(new Vector3((0.4f + Mathf.Abs(Character.move.x) * 2) * CX, 0));
                        }
                    }
                }
                    
                if (leftSupport) {
                    SolverLeft.transform.position = Vector2.LerpUnclamped(SolverLeft.transform.position, new Vector2(newvec.x, newvec.y), 0.5f);
                    SolverRight.transform.position = new Vector2(anchorRight.x, anchorRight.y);

                    Vector2 jalkanormal = GetNormalVector(SolverRight);
                    jalkanormal = Quaternion.Euler(0, 0, -90) * jalkanormal;
                    float a = Mathf.Atan2(jalkanormal.y, jalkanormal.x) * Mathf.Rad2Deg;
                    Quaternion drot = Quaternion.AngleAxis(a, Vector3.forward);
                    footRight.transform.rotation = drot;

                    footLeft.transform.localRotation = footLeftO;
                }
                else {
                    SolverRight.transform.position = Vector2.LerpUnclamped(SolverRight.transform.position, new Vector2(newvec.x, newvec.y), 0.5f);
                    SolverLeft.transform.position = new Vector2(anchorLeft.x, anchorLeft.y);

                    Vector2 jalkanormal = GetNormalVector(SolverLeft);
                    jalkanormal = Quaternion.Euler(0, 0, -90) * jalkanormal;
                    float a = Mathf.Atan2(jalkanormal.y, jalkanormal.x) * Mathf.Rad2Deg;
                    Quaternion drot = Quaternion.AngleAxis(a, Vector3.forward);
                    footLeft.transform.rotation = drot;

                    footRight.transform.localRotation = footRightO;

                }

                //Nosto
                MoveB = new Vector2(MoveC.x - 0.2f*CX, MoveC.y + heelLift + Mathf.Abs(Character.move.x)*5);

                //FORWARDS
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
                    fsteps.playFootstep();
                }
                //BACKWARDS
                else if (speed <= 0) {
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
                    fsteps.playFootstep();
                }
            }

            else if (STATE == "IDLE") {
                //PYSÄHTYMINEN
                if (speed < 1) {
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
                    }
                }

                //Jalkapohjien asettaminen
                Vector2 jalkanormal;
                float a;
                Quaternion drot;
                //Oikea jalka
                jalkanormal = GetNormalVector(SolverRight);
                jalkanormal = Quaternion.Euler(0, 0, -90) * jalkanormal;
                a = Mathf.Atan2(jalkanormal.y, jalkanormal.x) * Mathf.Rad2Deg;
                drot = Quaternion.AngleAxis(a, Vector3.forward);
                footRight.transform.rotation = drot;
                //Vasen jalka
                jalkanormal = GetNormalVector(SolverLeft);
                jalkanormal = Quaternion.Euler(0, 0, -90) * jalkanormal;
                a = Mathf.Atan2(jalkanormal.y, jalkanormal.x) * Mathf.Rad2Deg;
                drot = Quaternion.AngleAxis(a, Vector3.forward);
                footLeft.transform.rotation = drot;
                
            }
        }
        //ILMASSA
        else {

            STATE = "AIR";
            float moveY = Character.move.y;
            if (!Character.ledgeGrabbed) {
                if (moveY > 0)
                    moveY = 0;
                if (leftSupport) {

                    Vector2 newpos1;
                    Vector2 newpos2;

                    newpos1 = new Vector2(SolverRight.transform.position.x + Character.move.x / 4, SolverRight.transform.position.y + moveY);
                    newpos2 = new Vector2(SolverLeft.transform.localPosition.x * 0.3f, (SolverLeft.transform.localPosition.y) * 0.2f - 1);

                    SolverRight.transform.position = Vector2.LerpUnclamped(SolverRight.transform.position, newpos1, 0.5f);
                    SolverLeft.transform.localPosition = Vector2.LerpUnclamped(SolverLeft.transform.localPosition, newpos2, 0.5f);
                }
                else {
                    Vector2 newpos1;
                    Vector2 newpos2;


                    newpos2 = new Vector2(SolverLeft.transform.position.x + Character.move.x / 4, SolverLeft.transform.position.y + moveY);
                    newpos1 = new Vector2(SolverRight.transform.localPosition.x * 0.3f, (SolverRight.transform.localPosition.y) * 0.2f - 1);

                    SolverLeft.transform.position = Vector2.LerpUnclamped(SolverLeft.transform.position, newpos2, 0.5f);
                    SolverRight.transform.localPosition = Vector2.LerpUnclamped(SolverRight.transform.localPosition, newpos1, 0.5f);
                }
                AdjustToA = false;
                footLeft.transform.localRotation = footLeftO;
                footRight.transform.localRotation = footRightO;
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
        Vector2 newpose;
       if (AdjustToA) {
            newpose = new Vector2(rootX + rootForceX, rootY + rootForceY + gaitFix + bottomDis + rootAdjustOnce);
            if (CX > 0 && root.transform.position.x > MoveC.x)
                root.transform.position = new Vector2(MoveC.x, root.transform.position.y);
            else if (CX < 0 && root.transform.position.x < MoveC.x)
                root.transform.position = new Vector2(MoveC.x, root.transform.position.y);
            SolverChest.transform.localPosition = new Vector2(chestO.x, chestO.y);
            if (newpose.y < -2.25f)
                newpose = new Vector3(newpose.x, -2.25f);
        }
       else {
            newpose = new Vector2(rootX + rootForceX, rootY + rootForceY + gaitFix + bottomDis + rootAdjustOnce);
            SolverChest.transform.localPosition = new Vector2(chestO.x + (Character.move.x*CX*2f), chestO.y);
            if (newpose.y < -2.25f)
                newpose = new Vector3(newpose.x, -2.25f);
       }

        root.transform.localPosition = Vector2.LerpUnclamped(root.transform.localPosition, newpose, 0.5f);

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
        groundhit = Physics2D.Raycast(transform.position + v + new Vector3(xStepAdjust, 0) * CX , Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
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
            RaycastHit2D groundhit = Physics2D.Raycast(transform.position, Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhit) {
                bottomDis = 0.6f - groundhit.distance;
            }
            RaycastHit2D groundhitleft = Physics2D.Raycast(transform.position - new Vector3(-0.4f, 0, 0), Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhitleft) {
                if (0.6f - groundhitleft.distance < bottomDis)
                    bottomDis = 0.6f - groundhitleft.distance;
            }
            RaycastHit2D groundhitright = Physics2D.Raycast(transform.position - new Vector3(0.4f, 0, 0), Vector3.down, hillAdjustY, 1 << LayerMask.NameToLayer("Ground"));
            if (groundhitright) {
                if (0.6f - groundhitright.distance < bottomDis)
                    bottomDis = 0.6f - groundhitright.distance;
            }
            Debug.DrawLine(transform.position - new Vector3(-0.4f, 0, 0), groundhitleft.point, Color.green);
            Debug.DrawLine(transform.position - new Vector3(0.4f, 0, 0), groundhitright.point, Color.green);
        }

    }

    public void CheckUpDownHill()
    {
        if (!Character.onRightGround && Character.onLeftGround) {
            HILL = "LEFTDOWNHILL";
        }
        else if (Character.onRightGround && !Character.onLeftGround) {
            HILL = "RIGHTDOWNHILL";
        }
        else {
            HILL = "EVEN";
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
       
        groundhit = Physics2D.Raycast(SolverRight.transform.position + new Vector3(0, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
        if (groundhit && !leftSupport) {
            SolverRight.transform.position = groundhit.point;
            anchorRight = groundhit.point;
        }
        groundhit = Physics2D.Raycast(SolverLeft.transform.position + new Vector3(0, 1), Vector3.down, 2f, 1 << LayerMask.NameToLayer("Ground"));
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
    public string GetState()
    {
        return STATE;
    }
}
