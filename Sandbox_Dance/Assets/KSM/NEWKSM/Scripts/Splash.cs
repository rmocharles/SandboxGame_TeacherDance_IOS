using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    public Image SplashImage;
    public AudioSource SplashAudio;
    public Camera cam;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(RunSplash());
    }

    public IEnumerator RunSplash()
    {
        //SplashImage.transform.localScale = Vector2.one * 0.3888888888888889f;
        SplashImage.transform.localScale = Vector2.one * 0.2f;

        List<Sprite> sprites = new List<Sprite>();
        for(int i = 0; i < 42; i++)
        {
            sprites.Add(Resources.Load<Sprite>("Splash/" + i.ToString("00")));
        }

        yield return new WaitForSeconds(0.1f);
        SplashAudio.Play();

        int seq = 0;
        while(seq < sprites.Count)
        {
            SplashImage.sprite = sprites[seq++];
            yield return new WaitForSeconds(0.05f);
        }
        OnSplashDone();


        yield return new WaitForSeconds(0.5f);

        Color bgcolor = cam.backgroundColor;
        Color logocolor = SplashImage.color;
        

        float time = 1.0f;
        while (time > 0)
        {
            float delta = Time.deltaTime;
            time -= delta;

            logocolor.a -= delta;
            logocolor.r -= delta;
            logocolor.g -= delta;
            logocolor.b -= delta;
            SplashImage.color = logocolor;

            bgcolor.r -= delta;
            bgcolor.g -= delta;
            bgcolor.b -= delta;
            cam.backgroundColor = bgcolor;

            yield return new WaitForEndOfFrame();
        }

        
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    public void OnSplashDone()
    {
        //todo Load Scene
        SceneManager.LoadScene("2. Lobby");
    }
}
