using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IKFeetAdjust : MonoBehaviour, IFootAdjustment {
    public GameObject SolverRight;
    public GameObject SolverLeft;
    public GameObject footRight;
    public GameObject footLeft;

    [Range(0, 180.0f)] public float footLeftA = 0;
    [Range(0, 180.0f)] public float footRightA = 0;

    private Quaternion footLeftO;
    private Quaternion footRightO;
    private float dir;

    // Use this for initialization
    void Start () {
        footLeftO = footLeft.transform.localRotation;
        footRightO = footRight.transform.localRotation;
    }
	
	// Update is called once per frame
	void Update () {
        dir = -1 * Mathf.Sign(transform.localScale.x);
    }

    public void AdjustBoth()
    {
        AdjustFootRight(SolverRight, footRight);
        AdjustFootLeft(SolverLeft, footLeft);
    }

    public void AdjustLeft()
    {
        AdjustFootLeft(SolverLeft, footLeft);
        //ResetRightFoot();
    }

    public void AdjustRight()
    {
        AdjustFootRight(SolverRight, footRight);
        //ResetLeftFoot();
    }

    public void ResetFeetOriantion()
    {
        footLeft.transform.localRotation = footLeftO * Quaternion.Euler(0,0,footLeftA);
        footRight.transform.localRotation = footRightO * Quaternion.Euler(0, 0, footRightA); ;
    }

    private void AdjustFootRight(GameObject _solver, GameObject _foot)
    {
        Vector2 jalkanormal;
        float a;
        Quaternion drot;

        jalkanormal = GetNormalVector(_solver);
        jalkanormal = Quaternion.Euler(0, 0, -90+ footRightA*dir) * jalkanormal;
        a = Mathf.Atan2(jalkanormal.y, jalkanormal.x) * Mathf.Rad2Deg;
        drot = Quaternion.AngleAxis(a, Vector3.forward);
        _foot.transform.rotation = drot;
    }

    private void AdjustFootLeft(GameObject _solver, GameObject _foot)
    {
        Vector2 jalkanormal;
        float a;
        Quaternion drot;

        jalkanormal = GetNormalVector(_solver);
        jalkanormal = Quaternion.Euler(0, 0, -90 + footLeftA*dir) * jalkanormal;
        a = Mathf.Atan2(jalkanormal.y, jalkanormal.x) * Mathf.Rad2Deg;
        drot = Quaternion.AngleAxis(a, Vector3.forward);
        _foot.transform.rotation = drot;
    }

    private Vector3 GetNormalVector(GameObject o){
        RaycastHit2D ray = Physics2D.Raycast(o.transform.position, Vector2.down);
        if (ray) {
            return ray.normal;
        }
        return Vector3.zero;
    }

    public void ResetRightFoot()
    {
        footRight.transform.localRotation = footRightO;
    }
    public void ResetLeftFoot()
    {
        footLeft.transform.localRotation = footLeftO;
    }

    
}
