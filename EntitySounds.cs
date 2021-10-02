using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityAudio
{
    void PlaySound(string sound);
}
public class EntitySounds : MonoBehaviour, IEntityAudio
{
    public string Walking;
    public string Land;
    public string Jump;
    public string Dash;
    public string Hurt;

    private AudioSource auso;

    // Use this for initialization
    void Start()
    {
        auso = GetComponent<AudioSource>();
    }

    public void PlaySound(string sound)
    {
        AudioClip clip = Resources.Load<AudioClip>("sounds/entity/" + Dash);
        if (sound == "")
            return;
        switch (sound) {
            case "Walking":
                clip = Resources.Load<AudioClip>("sounds/entity/" + Walking);
                break;
            case "Land":
                clip = Resources.Load<AudioClip>("sounds/entity/" + Land);
                break;
            case "Jump":
                clip = Resources.Load<AudioClip>("sounds/entity/" + Jump);
                break;
            case "Dash":
                clip = Resources.Load<AudioClip>("sounds/entity/" + Dash);
                break;
            case "Hurt":
                clip = Resources.Load<AudioClip>("sounds/entity/" + Hurt);
                break;
            default:
                Debug.Log("EntitySounds Unknown playsound: " + sound);
                break;
        }
        if(clip != null)
            auso.PlayOneShot(clip);
    }
}
