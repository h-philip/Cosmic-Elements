using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipAudioController : MonoBehaviour
{
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void StartEngine()
    {
        if (_audioSource != null)
            _audioSource.Play();
    }

    public void StopEngine()
    {
        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Stop();
    }
}
