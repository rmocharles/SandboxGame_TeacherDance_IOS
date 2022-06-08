using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spine.Unity;

public class ScaleCtrl : MonoBehaviour
{
    /*
     * 배경 = 위치, 크기
     * 팝업창 = 위치, 정비율
     */

    public Camera letterBox;

    void Start()
    {

        //SetAspect(1080, 1920);
        Debug.LogError(Fixed.GetInstance().value);
        GameObject[] temp = GameObject.FindObjectsOfType<GameObject>();

        if(Fixed.GetInstance().value > 1)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                switch (temp[i].tag)
                {
                    case "Background":
                        temp[i].GetComponent<RectTransform>().anchoredPosition *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().y);
                        temp[i].GetComponent<RectTransform>().sizeDelta *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().y);
                        //temp[i].GetComponent<RectTransform>().localScale /= new Vector2(Fixed.GetInstance().value, Fixed.GetInstance().value);
                        break;
                    case "Element":
                        temp[i].GetComponent<RectTransform>().anchoredPosition *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().x);

                        if (temp[i].GetComponent<VerticalLayoutGroup>() != null || temp[i].GetComponent<HorizontalLayoutGroup>() != null || temp[i].GetComponent<GridLayoutGroup>() != null) temp[i].GetComponent<RectTransform>().localScale *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().x);
                        else temp[i].GetComponent<RectTransform>().sizeDelta *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().x);


                        if (temp[i].GetComponent<HorizontalLayoutGroup>() != null)
                            temp[i].GetComponent<HorizontalLayoutGroup>().spacing /= Fixed.GetInstance().x;

                        if (temp[i].GetComponent<SkeletonGraphic>() != null) temp[i].GetComponent<RectTransform>().localScale *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().x);

                        //텍스트일 경우 폰트 크기 조절
                        if (temp[i].GetComponent<Text>() != null)
                            temp[i].GetComponent<Text>().fontSize = Mathf.FloorToInt(temp[i].GetComponent<Text>().fontSize / Fixed.GetInstance().value);
                        break;

                    case "OnlyScale":
                        if (temp[i].GetComponent<VerticalLayoutGroup>() != null || temp[i].GetComponent<HorizontalLayoutGroup>() != null || temp[i].GetComponent<GridLayoutGroup>() != null) temp[i].GetComponent<RectTransform>().localScale *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().x);
                        else temp[i].GetComponent<RectTransform>().sizeDelta *= new Vector2(Fixed.GetInstance().x, Fixed.GetInstance().x);

                        if (temp[i].GetComponent<Text>() != null)
                            temp[i].GetComponent<Text>().fontSize = Mathf.FloorToInt(temp[i].GetComponent<Text>().fontSize / Fixed.GetInstance().value);
                        break;
                }
            }
        }
        

        switch (SceneManager.GetActiveScene().name)
        {
            case "1. Login":
                LoginUI.GetInstance().Initialize();
                break;
            case "2. Lobby":
                LobbyUI.GetInstance().Initialize();
                break;
            case "3. Game":
                GameManager.GetInstance().Initialize();
                break;
        }
    }

    void SetAspect(float wRatio, float hRatio)
    {
        //메인 카메라의 비율 변경을 위해 받아옵니다.
        Camera mainCam = Camera.main;

        //새로운 화면 크기 0f~1f의 값을 가집니다.
        float newScreenWidth;
        float newScreenHeight;
        //레터박스의 크기 0f~1f의 값을 가집니다.
        float letterWidth;
        float letterHeight;

        
        //레터박스. 레터박스는 화면을 렌더하지않는 카메라 프리팹입니다.
        Camera letterBox1 = Instantiate(letterBox);
        Camera letterBox2 = Instantiate(letterBox);

        //가로가 더 긴 비율을 원하는 경우. 상하로 레터박스가 생깁니다.
        if (wRatio > hRatio)
        {
            //새로운 화면의 가로 크기는 화면의 최대치
            newScreenWidth = 1f;
            //세로 크기는 가로를 기준으로 비율을 맞춰줍니다(newScreenWidth : newScreenHeight = wRatio : hRatio)
            newScreenHeight = newScreenWidth / wRatio * hRatio;

            //레터박스의 가로 크기는 새로운 화면의 크기와 같습니다.
            letterWidth = newScreenWidth;
            //세로 크기는 원래 화면 크기(1f)에서 새로운 화면 크기를 뺀 크기입니다.
            //위, 아래 두곳에서 생기므로 2로 나누어줍니다.
            letterHeight = (1f - newScreenHeight) * 0.5f;

            //camera.rect는 왼쪽 아래부분이 0f,0f입니다.
            //새로운 크기의 화면을 할당해줍니다. 화면의 시작점x는 0, y는 아래 레터박스의 위부터입니다.
            mainCam.rect = new Rect(0f, letterHeight, newScreenWidth, newScreenHeight);

            letterBox1.rect = new Rect(0f, 0f, letterWidth, letterHeight);//아래 레터박스
            letterBox2.rect = new Rect(0f, 1f - letterHeight, letterWidth, letterHeight);//위 레터박스
        }
        //세로가 더 긴 비율을 원하는 경우. 좌우로 레터박스가 생깁니다. 나머지는 비슷합니다.
        else
        {
            newScreenHeight = 1f;
            newScreenWidth = newScreenHeight / hRatio * wRatio;

            letterHeight = newScreenHeight;
            letterWidth = (1f - newScreenWidth) * 0.5f;


            mainCam.rect = new Rect(letterWidth, 0f, newScreenWidth, newScreenHeight);

            letterBox1.rect = new Rect(0f, 0f, letterWidth, letterHeight);//왼쪽 레터박스
            letterBox2.rect = new Rect(1f - letterWidth, 0f, letterWidth, letterHeight);//오른쪽 레터박스
        }
        //레터박스를 예쁘게 카메라의 자식으로 설정해줍니다
        letterBox1.transform.parent = mainCam.transform;
        letterBox2.transform.parent = mainCam.transform;
    }
}
