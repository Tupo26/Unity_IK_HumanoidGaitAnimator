using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IFootsteps
{
    void PlayFootStep();
}

[RequireComponent(typeof(AudioSource))]


public class Footsteps : MonoBehaviour, IFootsteps {

    public AudioClip footstep;
    public bool enable = true;
    private AudioSource auso;

	// Use this for initialization
	void Start () {
        auso = GetComponent<AudioSource>();
	}

    public void PlayFootStep()
    {
        if(enable)
            auso.PlayOneShot(footstep);
    }
}
