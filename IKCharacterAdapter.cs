using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IcontrolC
{
    void AddForce(float force);
    void AddToRootOnce(float v);
    void Flip();
    void LiftSolvers();
    void ChrouchOn();
    void ChrouchOff();
}

public class IKCharacterAdapter : MonoBehaviour, IcontrolC {

    private IKLegsControl control_IK;
    private IKLegsControl[] legs;
    private CharacterState control_C;

	// Use this for initialization
	void Start () {
        control_IK = GetComponent<IKLegsControl>();
        control_C = GetComponent<CharacterControl>();
        legs = GetComponents<IKLegsControl>();
    }

    public void AddForce(float force)
    {
        //control_IK.AddRootForce(force);
        foreach(IKLegsControl i in legs) {
            i.AddRootForce(force);
        }
    }

    public void Flip()
    {
        //control_IK.Flip();
        foreach (IKLegsControl i in legs) {
            i.Flip();
            i.RestlessFeet();
        }
    }

    public void LiftSolvers()
    {
        Debug.Log("CharacterAdapter: Lift Solvers");
        foreach (IKLegsControl i in legs) {
            i.LiftSolvers();
            i.resertAnchorPositions();
        }
        //control_IK.LiftSolvers();
        //control_IK.resertAnchorPositions();
    }

    public void AddToRootOnce(float v)
    {
        foreach (IKLegsControl i in legs) {
            i.rootAdjustOnce += v;
        }
        //control_IK.rootAdjustOnce += v;
    }

    public float GetCharacterSpeed()
    {
        Vector2 m = control_C.Movement();
        return m.x;
    }

    public bool getRunning()
    {
        Vector2 m = control_C.Movement();
        if (control_C.GetMaxSpeed() == m.x) {

            return true;
        }
        else {
            return false;
        }
    }

    public void ChrouchOn()
    {
        foreach (IKLegsControl i in legs) {
            i.ChrouchOn();
        }
        //control_IK.ChrouchOn();
    }

    public void ChrouchOff()
    {
        foreach (IKLegsControl i in legs) {
            i.ChrouchOff();
        }
        //control_IK.ChrouchOff();
    }
}
