using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static class Vibration
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        public static AndroidJavaClass AndroidPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        public static AndroidJavaObject AndroidcurrentActivity = AndroidPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        public static AndroidJavaObject AndroidVibrator = AndroidcurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif
        public static void Vibrate()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrator.Call("vibrate");
#else
            Handheld.Vibrate();
#endif
        }

        public static void Vibrate(long milliseconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrator.Call("vibrate", milliseconds);
#else
            Handheld.Vibrate();
#endif
        }

        public static void Vibrate(long[] pattern, int repeat)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrator.Call("vibrate", pattern, repeat);
#else
            Handheld.Vibrate();
#endif
        }

        public static void Cancel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrator.Call("cancel");
#endif
        }
    }
    public AudioSource bgmSource;
    public AudioSource effectAudio;
    public AudioSource gameBgm_2;

    public AudioClip[] effectSounds;
    public AudioClip[] bgmClips;

    bool viveOn;

    private static SoundManager instance = null;

    public static SoundManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {

    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this.GetComponent<SoundManager>();
        }

        if (!PlayerPrefs.HasKey("BGM_Mute")) 
        {
            PlayerPrefs.SetInt("BGM_Mute", 0);
            PlayerPrefs.SetInt("Effect_Mute", 0);
            PlayerPrefs.SetInt("Vibrate_Mute", 0);
        }
    }

    void Update()
    {
        bgmSource.mute = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false;
        if(SceneManager.GetActiveScene().name == "3. Game")
        {
            gameBgm_2.mute = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false;
        }
        effectAudio.mute = PlayerPrefs.GetInt("Effect_Mute") == 1 ? true : false;
        viveOn = PlayerPrefs.GetInt("Vibrate_Mute") == 1 ? true : false;
    }

    public void Vibrate()
    {
        if (!viveOn)
        {
            Vibration.Vibrate((long)100);
        }
    }

    public void SetBGM(bool check)
    {
        PlayerPrefs.SetInt("BGM_Mute", (check) ? 1 : 0);
    }

    public void PlayBGM(int num)
    {
        bgmSource.clip = bgmClips[num];
        bgmSource.Play();
    }
    
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetEffect(bool check)
    {
        PlayerPrefs.SetInt("Effect_Mute", (check) ? 1 : 0);
    }

    public void SetVibrate(bool check)
    {
        PlayerPrefs.SetInt("Vibrate_Mute", (check) ? 1 : 0);
    }

    public void PlayEffect(int num)
    {
        //effectAudio.clip = effectSounds[num];
        //effectAudio.Play();
        effectAudio.PlayOneShot(effectSounds[num]);
    }

    public void StopEffect()
    {
        effectAudio.Stop();
    }
}
