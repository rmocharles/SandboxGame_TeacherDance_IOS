using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Battlehub.Dispatcher;
using BackEnd;
using System.Text.RegularExpressions;
using Spine.Unity;

public class LoginUI : MonoBehaviour
{
    //로그인 창
    public GameObject[] Steps;

    [Space(10f)]
    [Header("로그인 팝업창")]
    public GameObject customLoginObject; //커스텀 로그인
    public GameObject signUpObject; //회원 가입
    public GameObject nicknameObject; //닉네임 창
    [Space(10f)]
    [Header("로그인")]
    public InputField[] loginField;
    public InputField[] signField;
    public InputField nicknameField;
    [Space(10f)]
    [Header("기타")]
    public GameObject errorObject;
    public GameObject loadingObject;

    public GameObject titleObject;

    public GameObject backButton;
    public Sprite backSprite;

    public Text[] text;



    private static LoginUI instance;
    public static LoginUI GetInstance()
    {
        if (instance == null) return null;

        return instance;
    }

    void Awake()
    {
        if (!instance) instance = this;
    }

    void Start()
    {

        if (!PlayerPrefs.HasKey("LangIndex"))
        {
            SystemLanguage lang = Application.systemLanguage;
            switch (lang)
            {
                case SystemLanguage.Korean:
                    PlayerPrefs.SetInt("LangIndex", 0);
                    break;

                case SystemLanguage.English:
                    PlayerPrefs.SetInt("LangIndex", 1);
                    break;

                case SystemLanguage.Japanese:
                    break;
            }
        }

        if(PlayerPrefs.GetInt("LangIndex") == 1)
        {
            backButton.GetComponent<Image>().sprite = backSprite;
            titleObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "title_e 01", false);
        }

        loadingObject.SetActive(false);

#if UNITY_ANDROID
        Steps[1].transform.GetChild(2).gameObject.SetActive(true);
        Steps[1].transform.GetChild(1).gameObject.SetActive(false);
#elif UNITY_IOS
        Steps[1].transform.GetChild(2).gameObject.SetActive(false);
        Steps[1].transform.GetChild(1).gameObject.SetActive(true);
#endif

        SoundManager.Instance.bgmSource.mute = PlayerPrefs.GetInt("BGM_Mute") == 1 ? false : true;
        SoundManager.Instance.effectAudio.mute = PlayerPrefs.GetInt("Effect_Mute") == 1 ? false : true;
    }

#region 초기화 (ScaleCtrl -> Initialize)
    public void Initialize()
    {
        loadingObject.SetActive(false);
        errorObject.SetActive(false);
        for (int i = 1; i < Steps.Length; i++) //터치스크린 제외 모두 끄기
            Steps[i].SetActive(false);
    }
#endregion

    public void SuccessLogin(Action<bool, string> func)
    {
        Dispatcher.Current.BeginInvoke(() =>
        {
            func(true, string.Empty);
        });
        return;
    }
    //==========================================================================
#region 터치 후 자동로그인
    public void TouchStart()
    {
        // 업데이트 팝업이 떠있으면 진행 X
        //if (updateObject.activeSelf == true) return;

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().BackendTokenLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (result)
                {
                    //ChangeLobbyScene();
                    return;
                }

                loadingObject.SetActive(false);
                if (!error.Equals(string.Empty))
                {
                    errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    errorObject.SetActive(true);
                    errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Application.Quit();
                    });
                    return;
                }

                for (int i = 0; i < Steps.Length; i++)
                {
                    Steps[i].SetActive(false);
                }
                Steps[1].SetActive(true); //로그인
            });
        });
    }
#endregion
    //==========================================================================
#region 커스텀 로그인
    public void Login()
    {
        if (errorObject.activeSelf)
        {
            return;
        }
        string id = loginField[0].text;
        string pw = loginField[1].text;

        if (id.Equals(string.Empty) || pw.Equals(string.Empty))
        {
            errorObject.GetComponentInChildren<Text>().text = "ID 혹은 PW 를 먼저 입력해주세요.";
            errorObject.SetActive(true);
            return;
        }

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().CustomLogin(id, pw, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorObject.GetComponentInChildren<Text>().text = "로그인 에러\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                ChangeLobbyScene();
            });
        });
    }
#endregion
    //==========================================================================
#region 커스텀 회원가입
    public void SignUp()
    {
        if (errorObject.activeSelf)
        {
            return;
        }
        string id = signField[0].text;
        string pw = signField[1].text;

        if (id.Equals(string.Empty) || pw.Equals(string.Empty))
        {
            errorObject.GetComponentInChildren<Text>().text = "ID 혹은 PW 를 먼저 입력해주세요.";
            errorObject.SetActive(true);
            return;
        }

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().CustomSignIn(id, pw, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorObject.GetComponentInChildren<Text>().text = "회원가입 에러\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                ChangeLobbyScene();
            });
        });
    }
#endregion
    //==========================================================================
#region 닉네임 활성
    public void ActiveNickNameObject()
    {
        Dispatcher.Current.BeginInvoke(() =>
        {
            for(int i = 0; i < Steps.Length; i++)
            {
                Steps[i].SetActive(false);
            }
            errorObject.SetActive(false);
            loadingObject.SetActive(false);
            Steps[Steps.Length - 1].SetActive(true);
        });
    }
#endregion
    //==========================================================================
#region 닉네임 설정
    public void UpdateNickName()
    {

        if (errorObject.activeSelf)
        {
            return;
        }
        string nickname = nicknameField.text;
        if (nickname.Equals(string.Empty))
        {
            //errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[13].kor : BackendServerManager.GetInstance().langaugeSheet[13].eng;
            errorObject.SetActive(true);
            return;
        }

        string idChecker = Regex.Replace(nickname, @"[^a-zA-Z0-9가-힣\.*,]", "", RegexOptions.Singleline);
        if (!nickname.Equals(idChecker))
        {
            //errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[14].kor : BackendServerManager.GetInstance().langaugeSheet[14].eng;
            errorObject.SetActive(true);
            return;
        }

        if (nickname.IndexOf("시발") != -1 || nickname.IndexOf("씨발") != -1 || nickname.IndexOf("병신") != -1 || nickname.IndexOf("새끼") != -1 || nickname.IndexOf("애미") != -1 || nickname.IndexOf("애비") != -1 || nickname.IndexOf("fuck") != -1)
        {
            //errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[15].kor : BackendServerManager.GetInstance().langaugeSheet[15].eng;
            errorObject.SetActive(true);
            return;
        }
        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().UpdateNickname(nickname, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorObject.GetComponentInChildren<Text>().text = "닉네임 생성 오류\n\n" + error + "\n";
                    errorObject.SetActive(true);
                    return;
                }
                //ChangeLobbyScene();
                TouchStart();
                //SceneManager.LoadScene("1. Login");
            });
        });
    }
#endregion
    //==========================================================================
#region 구글 로그인
    public void GoogleFederation()
    {
        if (errorObject.activeSelf)
        {
            return;
        }

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().GoogleAuthorizeFederation((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorObject.GetComponentInChildren<Text>().text = "Login ERROR\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                errorObject.SetActive(true);
                errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Application.Quit();
                });
                //ChangeLobbyScene();
            });
        });
    }
#endregion
    //==========================================================================
#region 애플 로그인
    public void AppleFederation()
    {
        if (errorObject.activeSelf)
        {
            return;
        }

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().AppleLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {

                    loadingObject.SetActive(false);
                    errorObject.GetComponentInChildren<Text>().text = "Login ERROR\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                errorObject.SetActive(true);
                errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Application.Quit();
                });
                //ChangeLobbyScene();
            });
        });
    }
#endregion
    //==========================================================================
#region 게스트 로그인
    public void GuestLogin()
    {
        if (errorObject.activeSelf)
        {
            return;
        }

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().GuestLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    errorObject.GetComponentInChildren<Text>().text = "Login ERROR\n\n" + error;
                    errorObject.SetActive(true);
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                errorObject.SetActive(true);
                errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Application.Quit();
                });
                //ChangeLobbyScene();
            });
        });
    }
#endregion
    //==========================================================================
    public void ChangeLobbyScene()
    {
        SceneManager.LoadScene("2. Lobby");
    }
    //==========================================================================

    public void CancelLogin()
    {
        for (int i = 0; i < Steps.Length; i++)
        {
            Steps[i].SetActive(false);
        }
        Steps[1].SetActive(true); //로그인
    }
}
