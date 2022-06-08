using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Spine.Unity;
using Battlehub.Dispatcher;

public class GameManager : MonoBehaviour
{
    //게임 시간 관련
    [Header("< 게임 시간 설정 >")]
    public float maxTime = 15;              //게임 최대 시간
    public float nowTime = 15;              //현재 게임 시간
    public float minusTimeSpeed = 1.8f;     //시간이 떨어지는 속도
    public float plusTimeSpeed = 1.3f;      //시간이 추가되는 속도
    public float minusExpSpeed = 5;         //경험치 떨어지는 속도
    public float plusExpSpeed = 5;          //경험치 추가되는 속도

    [Header("< 아이템 설정 >")]
    [Space(25f)]
    public bool isReviveOn;
    public Toggle[] itemToggle;

    [Header("< 선생님 시간 설정 >")]
    [Space(25f)]
    public float turnMinTime = 0.5f;
    public float turnMaxTime = 0.9f;
    public float rotateMinTime = 0.8f;      //선생님이 상태가 변경되는 최소 시간
    public float rotateMaxTime = 1.2f;      //선생님이 상태가 변경되는 최대 시간
    public int teacherState = 0;            //선생님이 보고 있는지 체크

    [Header("< 경험치 설정 >")]
    [Space(25f)]
    public float currentExp = 0;            //현재 경험치
    public float maxExp = 100;              //최대 경험치
    public int currentLevel = 0;            //현재 레벨
    public int maxLevel = 3;                //최대 레벨
    public bool getRuby = false;            //루비 한판
    public GameObject getRubyObject;

    [Header("< 게임 상태 설정 >")]
    [Space(25f)]
    public bool isDance = false;
    public bool isPlaying = false;
    public GameObject timeBar;
    public GameObject actBar;
    public GameObject bubbleObject;

    public GameObject problemCountObject;
    public bool isBubble;

    [Header("< 점수 설정 >")]
    [Space(25f)]
    public Text resultText;
    public Text[] pricesText;
    public Text plusScoreText;
    public Text scoreText;
    public GameObject checkHighScore;
    public GameObject checkMultiple;

    [Header("< 번역 >")]
    [Space(50f)]
    public Text[] texts;

    public Text plusText;

    public float score;
    public int bestScore;
    public int finalPrice;

    public GameObject packageObject;

    public GameObject cashObject;
    public Sprite[] cashSprite;
    public Sprite[] backSprite;

    public Text priceText;

    bool isMent;

    [System.Serializable]
    public struct studentSpine
    {
        public SkeletonDataAsset sit;
        public SkeletonDataAsset cheating;
        public SkeletonDataAsset dance;
    }

    [Header("< 학생들 스파인 설정 >")]
    [Space(80f)]
    public List<studentSpine> studentsSpine;
    public GameObject[] studentsObject;
    private List<int> studentRandom = new List<int>(3);
    private Vector3 studentPos;

    [Header("< 선생님 스파인 설정 >")]
    [Space(25f)]
    //public GameObject Test;
    public GameObject spineTeacher;
    private Coroutine spineCoroutine = null;

    [Header("< 문제 >")]
    [Space(25f)]
    bool isFirst = true;
    public GameObject reviveObject;
    public GameObject problemObject;
    public int problemCount = 1;
    public bool isProblem = false;
    public GameObject[] problemButtons;
    public Text timeCountText;
    public Text problemText;
    private Coroutine problemCoroutine = null;
    private bool timeCheck;
    private float timeCount = 3;

    [Header("< 재화 >")]
    [Space(25f)]
    public int gold;
    public int diamond;
    public Text goldText;
    public Text diamondText;

    [Header("< 게임 결과 창>")]
    [Space(25f)]
    public GameObject resultPanel;
    public GameObject doubleObject;
    public GameObject rankPanel;
    public Text resultScoreText;

    [Header("< 상점 창 >")]
    [Space(25f)]
    public GameObject shopPanel;
    public GameObject shopPanel2;

    [Header("< 튜토리얼 창 >")]
    [Space(25f)]
    public GameObject tutorialPanel;
    public GameObject tutorialPanel2;
    public GameObject tutorialPanel3;

    [Header("< 일시정지 창 >")]
    [Space(25f)]
    public GameObject pausePanel;

    [Header("< 춤추기 버튼 >")]
    [Space(25f)]
    public GameObject danceButton;

    [Header("< 게임 오버 >")]
    public PlayableDirector[] gameOver; //0 : 걸렸을 때, 1 : 타임 오버

    [Header("Others")]
    [Space(25f)]
    public GameObject loadingObject;
    public GameObject errorObject;

    public GameObject reviveCheckObject;
    public GameObject timeUpObject;
    public GameObject timeEndObject;
    public GameObject checkAngryObject;
    public GameObject bellObject;
    public GameObject catObject;

    public GameObject[] buyButton;
    public int[] item;

    public GameObject board;
    public Sprite boardSprite;

    public GameObject retryButton;
    public GameObject retryButton2;
    public Sprite retrySprite;

    public GameObject continueButton;
    public Sprite continueButtonSprite;

    public GameObject mainButton;
    public GameObject mainButton2;
    public Sprite mainButtonSprite;

    public Sprite tutorialSprite;
    public Sprite tutorialSprite2;
    public Sprite tutorialSprite3;

    public Sprite[] packageSprite;

    public Text bubbleText;

    public GameObject packageObject2;



    public bool isCat = false;
    public float catCool = 1;

    private static GameManager instance;

    public static GameManager GetInstance()
    {
        if (instance == null)
            return null;
        return instance;
    }

    void Awake()
    {
        if (!instance) instance = this;

        
    }

    #region 해상도 초기화
    public void Initialize()
    {
        tutorialPanel.SetActive(false);
        tutorialPanel2.SetActive(false);
        tutorialPanel3.SetActive(false);
        studentPos = studentsObject[0].transform.localPosition;
        errorObject.SetActive(false);
        pausePanel.SetActive(false);
        resultPanel.SetActive(false);
        checkHighScore.SetActive(false);
        loadingObject.SetActive(false);
        problemObject.SetActive(false);
        reviveObject.SetActive(false);
        checkMultiple.SetActive(false);
        bubbleObject.SetActive(false);
        problemCountObject.SetActive(false);
        shopPanel2.SetActive(false);
        reviveCheckObject.SetActive(false);
        timeUpObject.SetActive(false);
        bellObject.SetActive(false);
        cashObject.SetActive(false);

        timeEndObject.SetActive(false);
        checkAngryObject.SetActive(false);

        //선생님이 뒤돌아 보고 있는 상태 빼고 모두 꺼줌
        spineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(true);    //선생님 보고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false);   //선생님 뒤돌아 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(false);   //선생님이 걷고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false);   //선생님이 화나 있는 상태
        TeacherMove.GetInstance().SetBreath();

        
    }


    #endregion

    void Start()
    {
        priceText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "5,900 원" : "$ 4.99";
        bubbleText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[3].kor : BackendServerManager.GetInstance().langaugeSheet[3].eng;
        plusText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[19].kor : BackendServerManager.GetInstance().langaugeSheet[19].eng;

        if (PlayerPrefs.GetInt("LangIndex") == 1)
        {
            board.GetComponent<Image>().sprite = boardSprite;

            retryButton.GetComponent<Image>().sprite = retrySprite;
            retryButton2.GetComponent<Image>().sprite = retrySprite;

            continueButton.GetComponent<Image>().sprite = continueButtonSprite;

            mainButton.GetComponent<Image>().sprite = mainButtonSprite;
            mainButton2.GetComponent<Image>().sprite = mainButtonSprite;

            tutorialPanel.transform.GetChild(1).GetComponent<Image>().sprite = tutorialSprite;
            tutorialPanel2.transform.GetChild(1).GetComponent<Image>().sprite = tutorialSprite2;
            tutorialPanel3.transform.GetChild(1).GetComponent<Image>().sprite = tutorialSprite3;

        }

        isFirst = true;

        #region 퀴즈 정보 불러오기

        //한국어
        if (PlayerPrefs.GetInt("LangIndex") == 0)
        {
            loadingObject.SetActive(true);
            BackendServerManager.GetInstance().GetQuizSheet((bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    loadingObject.SetActive(false);
                    if (result)
                    {
                        print("한국어 퀴즈 불러오기 성공");
                        return;
                    }
                    errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    errorObject.SetActive(true);
                    errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Application.Quit();
                    });
                });
            });
        }
        else
        {
            loadingObject.SetActive(true);
            BackendServerManager.GetInstance().GetQuizSheet2((bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    loadingObject.SetActive(false);
                    if (result)
                    {
                        print("영어 퀴즈 불러오기 성공");
                        return;
                    }
                    errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    errorObject.SetActive(true);
                    errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Application.Quit();
                    });
                });
            });
        }
        #endregion

        #region 밸런스 정보 불러오기
        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().GetBalanceSheet((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                loadingObject.SetActive(false);
                if (result)
                {
                    print("밸런스 불러오기 성공");
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                errorObject.SetActive(true);
                errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Application.Quit();
                });
            });
        });
        #endregion

        #region 새로고침
        BackendServerManager.GetInstance().RefreshInfo();
        #endregion

        



        for (int i = 0; i < studentsObject.Length; i++)
        {
            MakeStudentRandom();
        }
        SetStudentAnimation();






        if (PlayerPrefs.GetInt("PACKAGE") == 1)
        {
        }
        else
        {
            if (!PlayerPrefs.HasKey("NEW"))
            {
                PlayerPrefs.SetInt("NEW", 1);
            }
            else
            {
                ADManager.GetInstance().ShowAd(() =>
                {
                });
            }
        }
    }

    void Update()
    {
        Debug.LogError("PACKAGE : " + PlayerPrefs.GetInt("PACKAGE"));

        if (PlayerPrefs.GetInt("LangIndex") == 1)
        {
            packageObject.GetComponent<Image>().sprite = packageSprite[1];
        }

        if (PlayerPrefs.GetInt("PACKAGE") == 1)
        {
            Debug.LogError("PACAKGE HAD");
            packageObject.SetActive(false);
            packageObject2.SetActive(false);
        }


        if (isPlaying && score > 50)
        {
            if(catCool > 0)
                catCool -= Time.deltaTime;

            if (!isCat && catCool <= 0)
            {
                int ran = Random.Range(0, 1000);

                if (ran == 1)
                {
                    TutorialCheck3();

                    Debug.LogError("발동!");
                    SoundManager.Instance.PlayEffect(9);
                    catObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "02", false);
                    isCat = true;
                    catCool = 4f;
                }
            }

            else if (isCat)
            {
                

                if (catCool <= 2f)
                {
                    Debug.LogError("튀어나오기 전!");
                    if (!SoundManager.Instance.effectAudio.isPlaying)
                    {
                        SoundManager.Instance.Vibrate();

                        catObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "03", false);
                        SoundManager.Instance.PlayEffect(10);

                    }
                    SoundManager.Instance.Vibrate();


                    if (catCool <= 0)
                    {
                        Debug.LogError("튀어나옴!");

                        SoundManager.Instance.effectAudio.Stop();
                        catCool = 9999;
                        isCat = false;
                        isPlaying = false;
                        GameOver(false, true);
                    }
                    
                }
            }
        }

        
        if (isPlaying)
        {
            if (isReviveOn)
            {
                reviveCheckObject.SetActive(true);
            }
            else
                reviveCheckObject.SetActive(false);
        }
        

        //번역
        if (PlayerPrefs.HasKey("LangIndex"))
        {
            //print(BackendServerManager.GetInstance().langaugeSheet[5].kor);
            switch (PlayerPrefs.GetInt("LangIndex"))
            {
                case 0: //한국어
                    for (int i = 0; i < texts.Length; i++)
                    {
                        texts[i].text = BackendServerManager.GetInstance().langaugeSheet[i + 5].kor;
                    }
                    break;
                case 1: //영어
                    for (int i = 0; i < texts.Length; i++)
                    {
                        texts[i].text = BackendServerManager.GetInstance().langaugeSheet[i + 5].eng;
                    }
                    break;
            }
        }

        if(Mathf.FloorToInt(score % 250) == 0 && isPlaying && Mathf.FloorToInt(score) > 0 && !isMent)
        {
            StartCoroutine(Bubble());
        }

        foreach(var balance in BackendServerManager.GetInstance().balanceSheet)
        {
            if(score > balance.minScore && score <= balance.maxScore)
            {
                rotateMinTime = balance.rotateMinTime;
                rotateMaxTime = balance.rotateMaxTime;
                turnMinTime = balance.turnMinTime;
                turnMaxTime = balance.turnMaxTime;
            }
        }

        if (isPlaying && isProblem)
        {
            if (problemCount > 0)
            {
                problemCountObject.SetActive(true);
                problemCountObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Text>().text = problemCount.ToString();
            }
            else
                problemCountObject.SetActive(false);
        }
        

        if (nowTime < 5)
        {
            timeBar.GetComponent<Animator>().SetBool("isBlind", true);
            SoundManager.Instance.bgmSource.pitch = 1.25f;
        }
        else
        {
            timeBar.GetComponent<Animator>().SetBool("isBlind", false);
            SoundManager.Instance.bgmSource.pitch = 1f;
        }

        if (isPlaying)
        {
            if (minusTimeSpeed < 3.5f)
            {
                minusTimeSpeed += 0.0005f;
            }
            else print("멈춰!");
        }
        

        if (rotateMaxTime > rotateMinTime)
        {
            rotateMaxTime -= 0.005f;
        }

        if (timeCheck)
        {
            timeCount -= Time.deltaTime;
            timeCountText.text = timeCount.ToString("F1");
        }

        goldText.text = gold.ToString();
        //diamondText.text = diamond.ToString();

        TimeManage();

        if (Input.GetKey(KeyCode.Escape) && !isPlaying)
        {
            SetPause(true);
        }

        Color color;
        switch (currentLevel)
        {
            case 0:
                getRubyObject.SetActive(false);
                ColorUtility.TryParseHtmlString("#5889FF", out color);
                actBar.GetComponent<Image>().color = color;
                minusExpSpeed = 25f;
                plusExpSpeed = 40f;
                plusScoreText.text = "Score X 100%";
                break;
            case 1:
                getRubyObject.SetActive(false);
                ColorUtility.TryParseHtmlString("#73FF58", out color);
                actBar.GetComponent<Image>().color = color;
                minusExpSpeed = 28f;
                plusExpSpeed = 36f;
                plusScoreText.text = "Score X 150%";
                break;
            case 2:
                getRubyObject.SetActive(false);
                ColorUtility.TryParseHtmlString("#FEFF00", out color);
                actBar.GetComponent<Image>().color = color;
                minusExpSpeed = 31f;
                plusExpSpeed = 32f;
                plusScoreText.text = "Score X 200%";
                break;
            case 3:
                getRubyObject.SetActive(!getRuby);
                ColorUtility.TryParseHtmlString("#FF00AC", out color);
                actBar.GetComponent<Image>().color = color;
                minusExpSpeed = 34f;
                plusExpSpeed = 28f;
                plusScoreText.text = "Score X 300%";
                break;

        }
    }

    public void BuyCash(int num)
    {
        Debug.LogError("??");
        if (!string.IsNullOrEmpty(BackendServerManager.GetInstance().isGuest()))
        {
            Debug.LogError("게스트로그인 맞음");

            cashObject.GetComponentsInChildren<Image>()[1].sprite = PlayerPrefs.GetInt("LangIndex") == 0 ? cashSprite[0] : cashSprite[1];
            cashObject.SetActive(true);
            cashObject.GetComponentsInChildren<Image>()[2].sprite = PlayerPrefs.GetInt("LangIndex") == 0 ? backSprite[0] : backSprite[1];
            cashObject.GetComponentsInChildren<Button>()[1].onClick.AddListener(() =>
            {
                InAppPurchaser.instance.BuyItem(num);
                cashObject.GetComponentsInChildren<Button>()[1].onClick.RemoveAllListeners();
            });
        }
        else
        {
            Debug.LogError("게스트로그인 아님");
            InAppPurchaser.instance.BuyItem(num);
        }
    }

    public void OnApplicationFocus(bool focus)
    {
        //if (!focus && isPlaying)
        //{
        //    SetPause(true);
        //}
    }

    #region 일시정지
    public void SetPause(bool isPause)
    {
        pausePanel.SetActive(isPause);
        if (spineTeacher.GetComponent<PlayableDirector>().state != PlayState.Playing)
        {
            isPlaying = !isPause;
        }

        if (isPause)
        {
            Time.timeScale = 0;
            SoundManager.Instance.bgmSource.Pause();
            SoundManager.Instance.gameBgm_2.Pause();
        }
        else
        {
            Time.timeScale = 1;
            SoundManager.Instance.bgmSource.Play();
        }
    }
    #endregion

    #region 학생 및 선생님 애니메이션 설정
    private void MakeStudentRandom()
    {
        int num = Random.Range(0, studentsSpine.Count);
        if (studentRandom.Contains(num))
            MakeStudentRandom();
        else
            studentRandom.Add(num);
    }

    private void SetStudentAnimation()
    {
        for(int i = 0; i < studentRandom.Count; i++)
        {
            studentsObject[i].GetComponentsInChildren<RectTransform>()[1].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentsSpine[studentRandom[i]].sit;
            studentsObject[i].GetComponentsInChildren<RectTransform>()[2].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentsSpine[studentRandom[i]].cheating;
            studentsObject[i].GetComponentsInChildren<RectTransform>()[3].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentsSpine[studentRandom[i]].dance;

            studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[0].AnimationState.SetAnimation(0, "sit_b", true);
            studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
            studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance", true);

            studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
            studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
        }
    }
    #endregion

    #region 게임 시작 버튼 누른 후
    public void PreviousGameStart()
    {
        Time.timeScale = 1;
        nowTime = maxTime;      //게임 시작시 현재 시간을 최대 시간에 맞춰줌

        //선생님이 걸어가고 있는 모션 제외 스파인 꺼줌
        spineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(false);   //선생님 보고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false);   //선생님 뒤를 보고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(true);    //선생님이 걷고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false);   //선생님이 화난 상태


        int needMoney = 0;
        for(int i = 0; i < itemToggle.Length; i++)
        {
            if (itemToggle[i].isOn)
            {
                if (item[i] > 0) break;
                needMoney += (i + 1) * 100;
            }
        }

        if (needMoney > gold)
        {
            errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "골드가 부족합니다." : "Not enough gold.";
            errorObject.SetActive(true);

            itemToggle[0].isOn = false;
            itemToggle[1].isOn = false;
            itemToggle[2].isOn = false;
            return;
        }
        else
        {
            ActiveItem();
            BackendServerManager.GetInstance().SaveGold(gold - needMoney);

            //게임 시작
            SoundManager.Instance.bgmSource.Stop();
            shopPanel.SetActive(false);
            danceButton.SetActive(false);

            StartCoroutine(GameStart());
            TeacherMove.GetInstance().SetBreath();
            SoundManager.Instance.PlayEffect(2);
            //spineTeacher.GetComponent<PlayableDirector>().Play();
        }

        
    }
    #endregion

    #region 아이템

    public void ActiveItem()
    {
        if (itemToggle[0].isOn) //부활
        {
            if(item[0] > 0)
            {
                BackendServerManager.GetInstance().SaveItem(item[0]-1, item[1], item[2]);
            }
            isReviveOn = true;
        }
        //시간 증가
        if (itemToggle[1].isOn)
        {
            minusTimeSpeed -= 0.3f;
            timeUpObject.SetActive(true);
            BackendServerManager.GetInstance().SaveItem(item[0], item[1]-1, item[2]);
        }

        if (itemToggle[2].isOn)
        {
            //우등생
            problemCount = 3;
            isProblem = true;
            BackendServerManager.GetInstance().SaveItem(item[0], item[1], item[2]-1);
        }  
    }

   
    #endregion

    #region 게임 시작
    public IEnumerator GameStart()
    {
        bellObject.SetActive(true);
        bellObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "bell2", true);
        yield return new WaitForSeconds(2f);

        bellObject.SetActive(false);
        //선생님이 뒤돌아 보고 있는 상태 빼고 모두 꺼줌
        spineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(true);    //선생님 보고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false);   //선생님 뒤돌아 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(false);   //선생님이 걷고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false);   //선생님이 화나 있는 상태

        //춤추기 버튼 활성화
        danceButton.SetActive(true);

        //게임 브금 시작
        SoundManager.Instance.PlayBGM(2);

        //게임 초기화
        ResetGame();

        //게임을 처음 시작했을 경우 튜토리얼 창 띄움
        TutorialCheck();
    }
    #endregion

    #region 튜토리얼 확인
    public void TutorialCheck()
    {
        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            isPlaying = false;
            Time.timeScale = 0;
            PlayerPrefs.SetInt("Tutorial", 1);
            tutorialPanel.SetActive(true);
        }
    }

    public void TutorialCheck2()
    {
        if (!PlayerPrefs.HasKey("Tutorial2"))
        {
            Time.timeScale = 0;
            PlayerPrefs.SetInt("Tutorial2", 1);
            tutorialPanel2.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            ChangeStudentAct(false);
            GoProblem();
            isFirst = false;
            problemCount--;
        }
    }

    public void TutorialCheck3()
    {
        if (!PlayerPrefs.HasKey("Tutorial3"))
        {
            isPlaying = false;
            isDance = false;
            Time.timeScale = 0;
            PlayerPrefs.SetInt("Tutorial3", 1);
            tutorialPanel3.SetActive(true);
        }
    }
    #endregion

    #region 게임 초기화
    public void ResetGame()
    {
        //선생님이 행동 시간을 랜덤으로 얻어서 적용해줌
        if(spineCoroutine != null)
        {
            StopCoroutine(spineCoroutine);
        }
        spineCoroutine = StartCoroutine(TeacherStateChange());

        Time.timeScale = 1;
        isPlaying = true;                                                                       //현재 게임을 하고 있는 상태로 변경
        isDance = false;                                                                        //현재 춤을 비활성화
        teacherState = 0;                                                                       //선생님은 뒤를 돌아있는 상태로 변경
        spineTeacher.GetComponent<TeacherMove>().Move((byte)teacherState);                      //선생님의 애니메이션 상태 변경
        //게임 결과창 꺼지는 UI
        ChangeStudentAct(false);
        spineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false);   //선생님 뒤돌아 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(false);   //선생님이 걷고 있는 상태
        spineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false);   //선생님이 화나 있는 상태
        danceButton.SetActive(true);                                                            //춤추기 버튼 활성화
        
    }
    #endregion

    #region 시간 체크 (게임 시작)
    public void TimeManage()
    {
        if (isPlaying)
        {
            timeBar.GetComponent<RectTransform>().localScale = new Vector2(nowTime / maxTime, 1);   //남은 시간
            actBar.GetComponent<RectTransform>().localScale = new Vector2(currentExp / maxExp, 1);  //경험치
            

            float num = 0;
            switch (currentLevel)
            {
                case 0:
                    num = 1;
                    break;
                case 1:
                    num = 1.5f;
                    break;
                case 2:
                    num = 2f;
                    break;
                case 3:
                    num = 3f;
                    break;
            }

            score += (num) * 0.05f; //현래 레벨에 따라 점수 속도 변경
            scoreText.text = Mathf.FloorToInt(score).ToString();                                    //점수

            DanceCheck();
        }
    }
    #endregion

    #region 댄스 체크
    public void DanceCheck()
    {
        if (isDance)
        {
            //눌렀을 때 시간이 오르는 것과 딴짓 게이지가 오름
            nowTime += plusTimeSpeed * Time.deltaTime;
            currentExp += plusExpSpeed * Time.deltaTime;
            
            if(currentExp > maxExp)
            {
                if(currentLevel < maxLevel)
                {
                    currentLevel++;
                    currentExp = 0;

                    ChangeStudentAct(true);
                }
                else
                {
                    //if (!getRuby)
                    //{
                    //    SoundManager.Instance.PlayEffect(8);
                    //    getRuby = true;
                    //}

                    score += 1f;
                    currentExp = maxExp;
                }
            }

            //선생님이 보고 있는 중일 경우
            if(teacherState == 5)
            {
                print("Watching");
                SoundManager.Instance.Vibrate();
                isPlaying = false;

                //문제 부분 처리
                if (isReviveOn)
                {
                    isReviveOn = false;

                    Revive();
                }
                else
                {
                    if(problemCount > 0)
                    {
                        TutorialCheck2();
                        //SoundManager.Instance.PlayEffect(3);
                        
                    }
                    else
                    {


                        StopCoroutine(spineCoroutine);

                        ChangeStudentAct(false);
                        GameOver(false);
                    }
                }

            }

            if (nowTime >= maxTime)
                nowTime = maxTime;

        }
        else
        {
            ChangeStudentAct(false);

            //남은 시간과 경험치 수치가 점점 내려감
            nowTime -= minusTimeSpeed * Time.deltaTime;
            currentExp -= minusExpSpeed * Time.deltaTime;

            if(currentExp <= 0)
            {
                if (currentLevel != 0)
                {
                    currentExp = maxExp;
                    currentLevel--;
                }
                else
                    currentExp = 0;
            }

            //남은 시간이 0보다 작아졌다면 게임 오버 처리
            if(nowTime <= 0 && isPlaying)
            {
                isPlaying = false;
                SoundManager.Instance.Vibrate();

                //부활 아이템 처리

                if (isReviveOn)
                {
                    isReviveOn = false;
                    nowTime = 7;
                    Revive();
                }
                else
                {
                    StopCoroutine(spineCoroutine);
                    GameOver(true);
                }

            }
        }
    }
    #endregion

    #region 문제
    public void GoProblem()
    {
        SoundManager.Instance.gameBgm_2.Pause();
        problemObject.SetActive(true);
        bubbleObject.SetActive(true);
        bubbleObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[4].kor : BackendServerManager.GetInstance().langaugeSheet[4].eng;

        if (isFirst)
        {
            //problemObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            //SoundManager.Instance.PlayEffect(7);
            problemObject.transform.GetChild(0).gameObject.SetActive(false);

            problemObject.transform.GetChild(1).gameObject.SetActive(false);
            problemObject.transform.GetChild(2).gameObject.SetActive(false);
            problemObject.transform.GetChild(3).gameObject.SetActive(false);

            Invoke("GoProblem", 1.5f);
        }
        else
        {
            
            //problemObject.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            SoundManager.Instance.PlayEffect(6);
            problemObject.transform.GetChild(0).gameObject.SetActive(true);
            problemObject.transform.GetChild(1).gameObject.SetActive(true);
            problemObject.transform.GetChild(2).gameObject.SetActive(true);
            problemObject.transform.GetChild(3).gameObject.SetActive(true);

            if (problemCoroutine != null)
            {
                StopCoroutine(problemCoroutine);
            }
            problemCoroutine = StartCoroutine(CountTime());

            int randomNum = Random.Range(0, BackendServerManager.GetInstance().quizSheet.Count);


            problemText.text = BackendServerManager.GetInstance().quizSheet[randomNum].quiz;        //문제 출력

            if (string.Equals(BackendServerManager.GetInstance().quizSheet[randomNum].error2, string.Empty))
            {
                if (Random.Range(0, 2) == 0)
                {
                    problemButtons[0].GetComponentInChildren<Text>().text = BackendServerManager.GetInstance().quizSheet[randomNum].answer;
                    problemButtons[0].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        print("정답");
                        bubbleObject.SetActive(false);
                        SoundManager.Instance.bgmSource.Play();
                        SoundManager.Instance.effectAudio.Stop();
                        SoundManager.Instance.PlayEffect(4);
                        Revive();
                        problemObject.SetActive(false);
                        StopCoroutine(problemCoroutine);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                    problemButtons[1].GetComponentInChildren<Text>().text = BackendServerManager.GetInstance().quizSheet[randomNum].error1;
                    problemButtons[1].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        print("오답");
                        bubbleObject.GetComponentInChildren<Text>().text = "누가 딴짓 중이야!";
                        bubbleObject.SetActive(false);
                        SoundManager.Instance.effectAudio.Stop();
                        //SoundManager.Instance.PlayEffect(5);
                        StopCoroutine(problemCoroutine);
                        problemObject.SetActive(false);
                        GameOver(false);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                }
                else
                {
                    problemButtons[1].GetComponentInChildren<Text>().text = BackendServerManager.GetInstance().quizSheet[randomNum].answer;
                    problemButtons[1].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        print("정답");
                        bubbleObject.SetActive(false);
                        SoundManager.Instance.bgmSource.Play();
                        SoundManager.Instance.effectAudio.Stop();
                        SoundManager.Instance.PlayEffect(4);
                        Revive();
                        problemObject.SetActive(false);
                        StopCoroutine(problemCoroutine);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                    problemButtons[0].GetComponentInChildren<Text>().text = BackendServerManager.GetInstance().quizSheet[randomNum].error1;
                    problemButtons[0].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        print("오답");
                        bubbleObject.GetComponentInChildren<Text>().text = "누가 딴짓 중이야!";
                        bubbleObject.SetActive(false);
                        SoundManager.Instance.effectAudio.Stop();
                        //SoundManager.Instance.PlayEffect(5);

                        StopCoroutine(problemCoroutine);
                        problemObject.SetActive(false);
                        GameOver(false);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                }
            }

            else
            {
                int errorNum = Random.Range(0, 3);
                string errorText = "";
                switch (errorNum)
                {
                    case 0:
                        errorText = BackendServerManager.GetInstance().quizSheet[randomNum].error1;
                        break;
                    case 1:
                        errorText = BackendServerManager.GetInstance().quizSheet[randomNum].error2;
                        break;
                    case 2:
                        errorText = BackendServerManager.GetInstance().quizSheet[randomNum].error3;
                        break;
                }

                if (Random.Range(0, 2) == 0)
                {
                    problemButtons[0].GetComponentInChildren<Text>().text = BackendServerManager.GetInstance().quizSheet[randomNum].answer;
                    problemButtons[0].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        print("정답");
                        bubbleObject.SetActive(false);
                        SoundManager.Instance.bgmSource.Play();
                        SoundManager.Instance.effectAudio.Stop();
                        SoundManager.Instance.PlayEffect(4);
                        Revive();
                        problemObject.SetActive(false);
                        StopCoroutine(problemCoroutine);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                    problemButtons[1].GetComponentInChildren<Text>().text = errorText;
                    problemButtons[1].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        bubbleObject.GetComponentInChildren<Text>().text = "누가 딴짓 중이야!";
                        bubbleObject.SetActive(false);
                        print("오답");
                        SoundManager.Instance.effectAudio.Stop();
                        //SoundManager.Instance.PlayEffect(5);
                        StopCoroutine(problemCoroutine);
                        problemObject.SetActive(false);
                        GameOver(false);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                }
                else
                {
                    problemButtons[1].GetComponentInChildren<Text>().text = BackendServerManager.GetInstance().quizSheet[randomNum].answer;
                    problemButtons[1].GetComponent<Button>().onClick.AddListener(() =>
                    {

                        print("정답");
                        bubbleObject.SetActive(false);
                        SoundManager.Instance.bgmSource.Play();
                        SoundManager.Instance.effectAudio.Stop();
                        SoundManager.Instance.PlayEffect(4);
                        Revive();
                        problemObject.SetActive(false);
                        StopCoroutine(problemCoroutine);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                    problemButtons[0].GetComponentInChildren<Text>().text = errorText;
                    problemButtons[0].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        bubbleObject.GetComponentInChildren<Text>().text = "누가 딴짓 중이야!";
                        bubbleObject.SetActive(false);
                        print("오답");
                        SoundManager.Instance.effectAudio.Stop();
                        //SoundManager.Instance.PlayEffect(5);
                        StopCoroutine(problemCoroutine);
                        problemObject.SetActive(false);
                        GameOver(false);
                        problemButtons[1].GetComponent<Button>().onClick.RemoveAllListeners();
                        problemButtons[0].GetComponent<Button>().onClick.RemoveAllListeners();
                    });
                }
            }
        }
        
    }

    IEnumerator CountTime()
    {
        timeCheck = true;
        timeCount = 5;
        yield return new WaitForSeconds(timeCount);
        timeCheck = false;
        problemObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        print("설마 나 실행되니?");
        bubbleObject.GetComponent<RectTransform>().localPosition += new Vector3(0, 200 + Fixed.GetInstance().x * 400, 0);

        GameOver(false);
        bubbleObject.GetComponentInChildren<Text>().text = "누가 딴짓 중이야!";

        bubbleObject.SetActive(false);
    }

    public void Revive()
    {
        if (problemCoroutine != null)
        {
            StopCoroutine(problemCoroutine);
        }

        ResetGame();


        reviveObject.SetActive(true); // 부활 아이템 이펙트를 켜줌
        reviveObject.GetComponent<PlayableDirector>().Play(); // 부활 아이템 타임라인 켜줌
        SoundManager.Instance.bgmSource.Play(); // 게임 음악을 다시켜줌
        SoundManager.Instance.gameBgm_2.Pause(); // 게임을 잠시 멈춰줌
    }
    #endregion

    #region 댄스 버튼
    public void DanceButton(bool isDancing)
    {
        if (isPlaying && isDancing)
        {
            SoundManager.Instance.bgmSource.Pause();
            SoundManager.Instance.gameBgm_2.Play();
            ChangeStudentAct(true);
            isDance = true;
        }
        else if(isPlaying && !isDancing)
        {
            SoundManager.Instance.bgmSource.Play();
            SoundManager.Instance.gameBgm_2.Pause();
            ChangeStudentAct(false);
            isDance = false;
        }
    }
    #endregion

    #region 선생님 말풍선
    IEnumerator Bubble()
    {
        isMent = true;

        bubbleObject.SetActive(true);
        //bubbleObject.GetComponentInChildren<Text>().text = "";
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                bubbleObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[0].kor : BackendServerManager.GetInstance().langaugeSheet[0].eng;
                break;
            case 1:
                bubbleObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[1].kor : BackendServerManager.GetInstance().langaugeSheet[1].eng;
                break;
            case 2:
                bubbleObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[2].kor : BackendServerManager.GetInstance().langaugeSheet[2].eng;
                break;
        }

        yield return new WaitForSeconds(1.5f);
        isMent = false;
        bubbleObject.SetActive(false);
    }
    #endregion

    #region 선생님 상태 변화
    IEnumerator TeacherStateChange()
    {
        if (spineCoroutine != null)
            StopCoroutine(spineCoroutine);

        //선생님이 행동 시간을 랜덤으로 얻어서 적용해줌
        float turnTime = Random.Range(rotateMinTime, rotateMaxTime);




        //선생님이 뒤를 돌아본 상태라면 보기 전 상태로 변경 시켜줌
        if(teacherState == 0)
        {
            teacherState = 1;
        }
        else
        {
            //뒤돌기 전 상태라면 시간에 따라 다른 애니메이션 적용
            if(teacherState == 1)
            {
                int num = Random.Range(0, 2);
                switch (num)
                {
                    case 0:
                        teacherState += 2;
                        turnTime = Random.Range(turnMinTime, turnMaxTime);
                        break;
                    case 1:
                        teacherState += 3;
                        turnTime = Random.Range(turnMinTime, turnMaxTime);
                        break;
                }
            }

            else if(teacherState >= 3 && teacherState <= 4)
            {
                int num = Random.Range(0, 5);
                if (num <= 3)
                {
                    teacherState = 5;
                    turnTime = Random.Range(turnMinTime, turnMaxTime);
                }
                else
                {
                    teacherState = 0;
                    turnTime = Random.Range(turnMinTime, turnMaxTime);
                }
            }

            else if(teacherState == 5)
            {
                teacherState = 0;
                turnTime = Random.Range(turnMinTime, turnMaxTime);
            }
        }

        spineTeacher.GetComponent<TeacherMove>().Move((byte)teacherState);

        yield return new WaitForSeconds(turnTime);

        if (isPlaying)
            StartCoroutine(TeacherStateChange());
    }
    #endregion

    #region 학생들 상태 변화
    public void ChangeStudentAct(bool isDance)
    {
        if (isDance)
        {
            //레벨이 최대치라면 춤추고 있는 애니메이션으로 변경
            if(currentLevel == maxLevel)
            {
                for(int i = 0; i < studentsObject.Length; i++)
                {
                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = false;
                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = true;

                    int ranNum = Random.Range(0, 2);
                    if (ranNum == 0)
                        studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance", true);
                    else
                        studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance2", true);

                    studentsObject[i].transform.SetSiblingIndex(2);
                    studentsObject[i].transform.localPosition = new Vector2(0, -300);
                }
            }

            else
            {
                for (int i = 0; i < studentsObject.Length; i++)
                {
                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = false;
                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = true;
                    switch (currentLevel)
                    {
                        
                        case 0:
                            int ranNum1 = Random.Range(0, 2);
                            if (ranNum1 == 0)
                            {
                                studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 1", true);
                            }

                            else
                            {
                                studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 1-1", true);
                            }
                            break;
                        case 1:
                            int ranNum2 = Random.Range(0, 2);
                            if (ranNum2 == 0)
                            {
                                studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2", true);
                            }

                            else
                            {
                                if (studentRandom[i] == 2)
                                {
                                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2-2", true);
                                }

                                else
                                {
                                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2-1", true);
                                }
                            }
                            break;
                        case 2:
                            studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
                            break;
                    }
                    studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
                    studentsObject[i].transform.SetSiblingIndex(2);
                    studentsObject[i].transform.localPosition = studentPos;
                }
            }
        }

        else
        {
            for (int i = 0; i < studentsObject.Length; i++)
            {
                studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = true;
                studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
                studentsObject[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
                studentsObject[i].transform.SetSiblingIndex(1);
                studentsObject[i].transform.localPosition = studentPos;
            }
        }
    }
    #endregion

    #region 게임 오버
    public void GameOver(bool isTimeOver, bool isCat = false)
    {
        print(bubbleObject.GetComponent<RectTransform>().localPosition);
        print(bubbleObject.GetComponent<RectTransform>().position);


        //선생님 해상도
        //Test.GetComponent<RectTransform>().position = new Vector3(Test.GetComponent<RectTransform>().position.x, Test.GetComponent<RectTransform>().position.y - (1000 + (Fixed.GetInstance().x * 200)), Test.GetComponent<RectTransform>().position.z);
        //bubbleObject.GetComponent<RectTransform>().localPosition = new Vector3(bubbleObject.GetComponent<RectTransform>().position.x, bubbleObject.GetComponent<RectTransform>().position.y + 200 + Fixed.GetInstance().x * 400, bubbleObject.GetComponent<RectTransform>().position.z);

        SoundManager.Instance.StopBGM();
        SoundManager.Instance.gameBgm_2.Pause();


        //bubbleObject.GetComponentInChildren<Text>().text = isCat ? "누가 고양이 데리고 왔어?!" : "누가 딴짓중이야!";

        if (spineCoroutine != null)
            StopCoroutine(spineCoroutine);

        finalPrice = Mathf.FloorToInt(int.Parse(scoreText.text) * 0.1f);

        danceButton.SetActive(false);
        ChangeStudentAct(false);

        BackendServerManager.GetInstance().SaveGold(gold + finalPrice);

        BackendServerManager.GetInstance().UpdateScore2(int.Parse(scoreText.text));

        if (isTimeOver)
        {
            gameOver[1].Play();
        }
        else
        {
            
            gameOver[0].Play();
        }
    }

    public void EndGame()
    {
        bubbleObject.GetComponent<RectTransform>().localPosition += new Vector3(0, 200 + Fixed.GetInstance().x * 400, 0);

        if (spineCoroutine != null)
            StopCoroutine(spineCoroutine);

        Time.timeScale = 0;
        ChangeStudentAct(false);

        SoundManager.Instance.PlayEffect(11);

        resultPanel.SetActive(true);
        if (Mathf.FloorToInt(int.Parse(scoreText.text)) >= 10)
            doubleObject.SetActive(true);
        else
            doubleObject.SetActive(false);

        if (getRuby)
        {
            BackendServerManager.GetInstance().SaveDiamond(diamond + 1);
            pricesText[1].text = 1.ToString();
        }
            
        rankPanel.SetActive(false);

        finalPrice = Mathf.FloorToInt(int.Parse(scoreText.text) * 0.1f); // 얻을 골드 체크
        resultScoreText.text = Mathf.FloorToInt(int.Parse(scoreText.text)).ToString(); //최종 점수를 텍스트에 적어줌

        pricesText[0].text = finalPrice.ToString(); // 최종적으로 얻은 돈을 텍스트에 적어줌

        

    }

    public void EndGameStatus()
    {
        doubleObject.SetActive(false);
        getRankInfo();
    }

    public void getRankInfo()
    {
        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().getRank((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                loadingObject.SetActive(false);
                if (result)
                {
                    rankPanel.SetActive(true);
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                errorObject.SetActive(true);
                errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Application.Quit();
                });
            });
        });
    }

    public void EndMusic() // 게임이 끝날때 음악을 꺼줌
    {
        SoundManager.Instance.bgmSource.Pause();
        SoundManager.Instance.gameBgm_2.Pause();
        danceButton.SetActive(false);
    }
    #endregion

    public void ButtonEffect()
    {
        SoundManager.Instance.effectAudio.clip = SoundManager.Instance.effectSounds[4];
        SoundManager.Instance.effectAudio.Play();
    }

    #region 보상 골드 2배
    public void GetDoublePrice()
    {
        loadingObject.SetActive(true);
        if(PlayerPrefs.GetInt("PACKAGE") == 1)
        {
            BackendServerManager.GetInstance().SaveGold(gold + finalPrice);
            pricesText[0].text = (finalPrice * 2).ToString();
            checkMultiple.SetActive(true);
            EndGameStatus();
            loadingObject.SetActive(false);
        }
        else
        {
            ADManager.GetInstance().ShowRewardedAd(() =>
            {
                BackendServerManager.GetInstance().SaveGold(gold + finalPrice);
                pricesText[0].text = (finalPrice * 2).ToString();
                checkMultiple.SetActive(true);
                EndGameStatus();
                loadingObject.SetActive(false);
            },
        () =>
        {
            EndGameStatus();
            loadingObject.SetActive(false);
        },
        () =>
        {
            errorObject.GetComponentInChildren<Text>().text = "네트워크가 불안정하여 취소되었습니다.";
            errorObject.SetActive(true);
            EndGameStatus();
            loadingObject.SetActive(false);
        }
        );
        }
        
    }
    #endregion

    #region 고양이
    public void CatClick()
    {
        isCat = false;
        catCool = Random.Range(1f, 5f);
        catObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "01", false);
    }
    #endregion

    #region 씬 이동
    public void SceneLobby(string name)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(name);

    }
    #endregion
}