using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKCharacterAdapter : MonoBehaviour {

    private IKLegsControl control_IK;
    private CharacterControl control_C;

	// Use this for initialization
	void Start () {
        control_IK = GetComponent<IKLegsControl>();
        control_C = GetComponent<CharacterControl>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddForce(float force)
    {
        control_IK.AddRootForce(force);
    }

    public void Flip()
    {
        control_IK.Flip();
    }

    public void LiftSolvers()
    {
        Debug.Log("CharacterAdapter: Lift Solvers");
        control_IK.LiftSolvers();
        control_IK.resertAnchorPositions();
    }

    public void AddToRootOnce(float v)
    {
        control_IK.rootAdjustOnce += v;
    }
    public void AddToChestOnce(float v)
    {

    }

    public float GetCharacterSpeed()
    {
        return control_C.move.x;
    }

    public bool getRunning()
    {
        if(control_C.maxSpeed == control_C.move.x) {

            Debug.Log("Juostaan");
            return true;
        }
        else {
            return false;
        }
    }

    public void StartMoving()
    {

    }
    public void StopMoving()
    {

    }
}
