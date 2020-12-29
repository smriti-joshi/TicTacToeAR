using UnityEngine;

public class PlayButtonSound : MonoBehaviour
{
    AudioSource buttonAudio;

    public void PlaySound ()
    {
        buttonAudio = GetComponent<AudioSource> ();
        buttonAudio.Play (0);
    }
}
