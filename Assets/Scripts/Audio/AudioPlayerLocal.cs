using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayerLocal : MonoBehaviour
{
    [SerializeField][Tooltip("Vai ser pego no awake se n for setado")]AudioSource audioSource;
    [SerializeField]AudioClip[] audioClips;
    [SerializeField]float pitchVariance;
    void Awake() {
        if(audioSource==null)audioSource=GetComponent<AudioSource>();
    }

    public void PlayAudioSource() {
        audioSource.pitch=1f;
        audioSource.Play();
    }
    public void PlayRandomAudioClip() {
        if(audioClips==null){Debug.LogWarning($"Você tentou chamar um audioClipAleatorio num objeto sem audioclips, chamado {gameObject.name}");return;}
        if(audioClips.Length==0){Debug.LogWarning($"Você tentou chamar um audioClipAleatorio num objeto sem audioclips, chamado {gameObject.name}");return;}
        AudioClip audioClip = audioClips[UnityEngine.Random.Range(0,audioClips.Length)];
        audioSource.PlayOneShot(audioClip);
    }
    public void PlayAudioClip(AudioClip audioClip,float volume=1f) {
        float randomPitch = UnityEngine.Random.Range(1f-pitchVariance,1f+pitchVariance);
        audioSource.pitch=randomPitch;
        audioSource.PlayOneShot(audioClip,volume);
    }
}
