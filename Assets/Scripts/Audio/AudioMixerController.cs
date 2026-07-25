using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerController : MonoBehaviour
{
    //public static AudioController singleton;
    public AudioMixer audioMixer;

    public Slider masterAudioSlider;
    public Slider sfxAudioSlider;
    public Slider musicAudioSlider;
    /* void Awake() {
        if (singleton == null) {
            singleton=this;
        } else {
            Debug.LogWarning("Temos 2 audioCOntrollers na cena");
        }
    } */
    void Start() {
        float vol = PlayerPrefs.GetFloat("MasterVolume",0.3f);
        masterAudioSlider.value=vol;
        MasterVolChange(vol);
        vol = PlayerPrefs.GetFloat("SfxVolume",0.3f);
        sfxAudioSlider.value=vol;
        SfxVolChange(vol);
        vol = PlayerPrefs.GetFloat("MusicVolume",0.3f);
        musicAudioSlider.value=vol;
        MusicVolChange(vol);
    }
    public void MasterVolChange(float value) {
        float dB;
        if(value==0)dB=-80;
        else dB= 40*value-20;
        audioMixer.SetFloat("MasterVolume",dB);
        PlayerPrefs.SetFloat("MasterVolume",value);
    }
    public void SfxVolChange(float value) {
        float dB;
        if(value==0)dB=-80;
        else dB= 40*value-20;
        audioMixer.SetFloat("SfxVolume",dB);
        PlayerPrefs.SetFloat("SfxVolume",value);
    }
    public void MusicVolChange(float value) {
        float dB;
        if(value==0)dB=-80;
        else dB= 40*value-20;
        audioMixer.SetFloat("MusicVolume",dB);
        PlayerPrefs.SetFloat("MusicVolume",value);
    }
}
