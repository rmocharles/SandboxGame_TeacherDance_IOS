using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Battlehub.Dispatcher;
using System;
using Spine.Unity;
using System.Text.RegularExpressions;

public class LobbyUI : MonoBehaviour
{
    [Header("< 로그인 >")]
    public GameObject loginObject;
    public GameObject copyObject;
    public InputField nicknameField;
    public GameObject[] offObject;
    public bool isLogin = false;


    [Space(50f)]
    [Header("< 재화 요소 >")]
    public int gold;
    public int diamond;

    [Header("< 재화 요소 >")]
    [Space(25f)]
    public int adCount;
    public int adReset;

    [Header("< 설정 창 >")]
    [Space(80f)]
    public GameObject settingPanel;
    public Text nickNameText;

    [Header("< 상점 창 >")]
    [Space(25f)]
    public GameObject storePanel;
    public Text goldText2;
    public Text diamondText2;
    public Text adCountText;

    [Header("< 랭크 창 >")]
    [Space(25f)]
    public GameObject rankPanel;

    [Header("<카드 모음 창 >")]
    [Space(25f)]
    public GameObject collectionPanel;
    public ToggleGroup collectionGroup;
    public Text gameNameText;

    public GameObject[] Cards;
    public Sprite[] CardImage;

    // 내가 추가한거(이태호)
    public GameObject cardInfo;
    public GameObject getRubyInfo;
    public GameObject popUpUI;
    public GameObject drawCardUI;
    public GameObject detailPopUpUI;
    public GameObject closeAppUI;
    public Sprite[] cardOutLine;
    //public Text adCount;
    public int testBOmb = 40; // 임시 확률
    public bool isCanWatchAd = false;

    public GameObject[] cardBookObjs;

    [Header("DrawCard Panel")]
    [Space(25f)]
    public GameObject drawCardPanel;
    public Text goldText;
    public Text diamondText;

    [Header("Others")]
    [Space(25f)]
    public GameObject loadingObject;
    public GameObject errorObject;

    public GameObject packagePanel;
    public Text adviceRemainText;

    public GameObject titleObject;
    public GameObject agreeButton;
    public Sprite agreeSprite;
    public Sprite agreeSprite2;
    public Text agreeText;
    public GameObject cardObject;
    public Sprite[] cardSprite;
    public Text getText;
    public Text EarnText;
    public GameObject packageObject;
    public GameObject packageObject2;
    public Sprite[] packageSprite;


    public GameObject skipButton;
    public Sprite[] skipSprite;

    public GameObject[] cardBackObject;

    public GameObject quitObject;
    public Sprite[] backSprite;
    public Sprite[] exitSprite;

    public GameObject cashObject;
    public Sprite[] cashSprite;

    public Text cancelText;

    public Text dollarText1;
    public Text dollarText2;
    public Text dollarText3;

    public GameObject offPackageObject;
    public GameObject offPackageObject2;

    private static LobbyUI instance;

    public int currentDay = 0;


    #region 초기화 (ScaleCtrl -> Initialize)
    public void Initialize()
    {
        loadingObject.SetActive(false);
        errorObject.SetActive(false);

        settingPanel.SetActive(false);
        storePanel.SetActive(false);
        rankPanel.SetActive(false);
        collectionPanel.SetActive(false);
        drawCardPanel.SetActive(false);
        cardInfo.SetActive(false);
        getRubyInfo.SetActive(false);
        popUpUI.SetActive(false);
        drawCardUI.SetActive(false);
        detailPopUpUI.SetActive(false);
        closeAppUI.SetActive(false);
        packagePanel.SetActive(false);
        loginObject.SetActive(false);
        copyObject.SetActive(true);
        cashObject.SetActive(false);

        CheckBuyItems();

        for (int i = 0; i < offObject.Length; i++)
            offObject[i].SetActive(false);
    }
    #endregion

    public void SuccessLogin(Action<bool, string> func)
    {
        Dispatcher.Current.BeginInvoke(() =>
        {
            for(int i = 0; i < offObject.Length; i++)
            {
                offObject[i].SetActive(true);
            }

            Debug.LogError("로그인 성공");
            loadingObject.SetActive(false);
            copyObject.SetActive(false);
            loginObject.SetActive(false);

            
            isLogin = true;
            func(true, string.Empty);

            if (BackendServerManager.GetInstance() != null) setNickName(); //로비 들어오면 닉네임 설정

            BackendServerManager.GetInstance().RefreshInfo();
        });
        return;
    }

    public void CancelLogin()
    {
        loginObject.transform.GetChild(0).gameObject.SetActive(true);
        loginObject.transform.GetChild(1).gameObject.SetActive(false);
    }
    //==========================================================================
    #region 터치 후 자동로그인
    public void TouchStart()
    {

        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().BackendTokenLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (result)
                {
                    return;
                }

                loadingObject.SetActive(false);
                if (!error.Equals(string.Empty))
                {
                    Debug.LogError("ERROR!!!!");
                    errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    errorObject.SetActive(true);
                    errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Application.Quit();
                    });
                    return;
                }

                loginObject.SetActive(true);
                copyObject.SetActive(true);
                loginObject.transform.GetChild(0).gameObject.SetActive(true);
                loginObject.transform.GetChild(1).gameObject.SetActive(false);

                
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
            for (int i = 0; i < offObject.Length; i++)
            {
                offObject[i].SetActive(false);
            }
            errorObject.SetActive(false);
            loadingObject.SetActive(false);

            loginObject.SetActive(true);
            loginObject.transform.GetChild(0).gameObject.SetActive(false);
            loginObject.transform.GetChild(1).gameObject.SetActive(true);
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
                    errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    errorObject.SetActive(true);
                    errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log("애플 로그인");

                        Application.Quit();
                    });
                    return;
                }
                Debug.LogError("애플 로그인");
                
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
                    errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    errorObject.SetActive(true);
                    errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log("게스트 로그인");

                        Application.Quit();
                    });
                    return;
                }
                Debug.Log("게스트 로그인");
                
                //ChangeLobbyScene();
            });
        });
    }
    #endregion
    //==========================================================================

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

#if UNITY_ANDROID
        loginObject.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
        loginObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
#elif UNITY_IOS
        loginObject.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        loginObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
#endif


        titleObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, PlayerPrefs.GetInt("LangIndex") == 0 ? "title 02" : "title_e 02", true);

        

        settingPanel.GetComponentsInChildren<Toggle>()[0].isOn = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false; //소리 설정
        settingPanel.GetComponentsInChildren<Toggle>()[1].isOn = PlayerPrefs.GetInt("Effect_Mute") == 1 ? true : false; // 효과음
        settingPanel.GetComponentsInChildren<Toggle>()[2].isOn = PlayerPrefs.GetInt("Vibrate_Mute") == 1 ? true : false; // 진동
        settingPanel.transform.GetChild(8).GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "한국어" : "English";

        

        #region 패키지 구매 확인
        if (!PlayerPrefs.HasKey("PACKAGE"))
        {
            PlayerPrefs.SetInt("PACKAGE", 0);
        }
        #endregion
    }

    void Update()
    {
        Debug.LogError("PACKAGE : " + PlayerPrefs.GetInt("PACKAGE"));
        if (PlayerPrefs.GetInt("PACKAGE") == 1)
        {
            Debug.LogError("패키지 구매했음");
            offPackageObject.SetActive(false);
            offPackageObject2.SetActive(false);
            offPackageObject2.GetComponent<SkeletonGraphic>().color = new Color(0, 0, 0, 0);
        }
        else
        {
            offPackageObject.SetActive(true);
            offPackageObject2.SetActive(true);
        }

        dollarText1.text = PlayerPrefs.GetInt("LangIndex") == 1 ? "$ 4.99" : "5,900 원";
        dollarText2.text = PlayerPrefs.GetInt("LangIndex") == 1 ? "$ 1.99" : "2,500 원";
        dollarText3.text = PlayerPrefs.GetInt("LangIndex") == 1 ? "$ 3.99" : "4,900 원";

        if (PlayerPrefs.GetInt("PACKAGE") == 1)
        {
            //offObject[7].SetActive(false);
        }

            if (!isLogin) return;

        Time.timeScale = 1;

        if (PlayerPrefs.GetInt("LangIndex") == 1)
        {
            for (int i = 0; i < cardBackObject.Length; i++)
            {
                cardBackObject[i].GetComponent<Image>().sprite = cardSprite[1];
            }
            cancelText.text = "It is possible to withdraw the subscription for payment products within 7 days after purchase (However, it cannot be withdrawn when used)\nPurchasing can be canceled by minors if the legal representative does not agree.";

            agreeButton.GetComponent<Image>().sprite = agreeSprite;
            cardObject.GetComponent<Image>().sprite = cardSprite[1];
            getText.text = BackendServerManager.GetInstance().langaugeSheet[17].eng;
            EarnText.text = BackendServerManager.GetInstance().langaugeSheet[18].eng;
            packageObject.GetComponent<Image>().sprite = packageSprite[1];
            packageObject2.GetComponent<Image>().sprite = packageSprite[1];
            skipButton.GetComponent<Image>().sprite = skipSprite[1];

            quitObject.transform.GetChild(2).GetComponent<Image>().sprite = exitSprite[1];
            quitObject.transform.GetChild(3).GetComponent<Image>().sprite = backSprite[1];
            quitObject.transform.GetChild(4).GetComponent<Text>().text = BackendServerManager.GetInstance().langaugeSheet[21].eng;

            if (drawCardUI.activeSelf)
            {
                Debug.LogError("AA");
                //cardEffectObject[0].GetComponent<SkeletonGraphic>().skeletonDataAsset = cardEffect[2];
                //cardEffectObject[1].GetComponent<SkeletonGraphic>().skeletonDataAsset = cardEffect[3];
                //cardEffectObject[0].GetComponent<SkeletonGraphic>().Initialize(true);
                //cardEffectObject[1].GetComponent<SkeletonGraphic>().Initialize(true);
            }
            
            //for (int i = 0; i < cardEffectObject.Length; i++)
            //{
            //    cardEffectObject[i].GetComponent<SkeletonGraphic>().skeletonDataAsset = cardEffect[i * 2];

            //}
            agreeText.text = "Article 22 (Withdrawal of subscription, etc.)\n① Users who have concluded a contract for the purchase of paid content with the company, verbally or in writing (including electronic documents), within 7 days from the date of the purchase contract or the date of content availability, whichever is later You can withdraw your subscription.\nUnless otherwise stipulated by the operating policy, etc., withdrawal of subscription or refund related to service use must be directly applied to the company through the customer center.\n② The user cannot withdraw from the subscription pursuant to Paragraph 1 against the will of the company in any of the following cases. However, in the case of a purchase contract consisting of separable content, this is not the case for the rest of the separable content that does not fall under any of the following subparagraphs.\n1. Paid content to be used or applied immediately upon purchase\n2. If additional benefits are provided, the content in which the additional benefits are used\n3. If the opening act can be viewed as use or there is an act of opening content whose utility is determined upon opening\n4. In case of using some of the additional content (goods, points, mileage, items, etc.) paid at the time of purchase\n5. In the event that part of the content sold in bundles is used and cannot be retrieved\n6. Content not purchased by users, such as receiving gifts from others\n7. Content that has been partially or completely lost or damaged due to reasons attributable to the user\n8. Other content whose subscription withdrawal is restricted in accordance with related laws such as the 「Act on Consumer Protection in Electronic Commerce, Etc.」 (hereinafter referred to as the 「E-Commerce Act」)\n③ In the case of content that cannot be withdrawn in accordance with the provisions of each subparagraph of Paragraph 2, the company clearly indicates the fact in a place where users can easily understand it, and provides trial use products of the content (allowing temporary use, for experience use) etc.) or if it is difficult to provide it, we take measures so that the user's right to withdraw subscription is not hindered by providing information about the content. If the company does not take such measures, the user may withdraw the subscription notwithstanding the reasons for the restriction on withdrawal of subscription in each subparagraph of Paragraph 2.\n④ Notwithstanding Paragraphs 1 and 2, if the content of the purchased paid content is different from that of the display/advertisement or the content of the purchase contract is different from the content of the purchase contract, within 3 months from the date the content becomes available , You can withdraw your subscription within 30 days from the day you knew or could have known that fact.\n⑤ When the user withdraws the subscription, the company checks the purchase history through the platform operator or open market operator. In addition, the company may contact the user through the information provided by the user to confirm the user's justifiable reason for withdrawal, and may request additional evidence.\n⑥ If the subscription is withdrawn in accordance with the provisions of Paragraphs 1 to 4, the Company will collect the paid content from the user without delay and refund the payment within 3 business days. In this case, if the company delays the refund, it pays the delayed interest calculated by multiplying the delay period by the interest rate prescribed in the 「Electronic Commerce Act」 and Article 21-3 of the Enforcement Decree of the same Act.\n⑦ When the company refunds the price in accordance with Paragraph 6, if the consumer has paid the payment using the payment method specified in Article 18 Paragraph 3 of the 「Electronic Commerce Act」, it requests the payment service provider to suspend or cancel the billing. However, if the company has already received the payment for the purchase subject to withdrawal of subscription from the payment company, it will refund the payment to the payment company and notify the user of the fact.\n⑧ When a minor concludes a content purchase contract on a mobile device, the company notifies that the minor or his/her legal representative may cancel the contract without the consent of the legal representative, and the minor enters into the purchase contract without the consent of the legal representative When the contract is signed, the minor or his/her legal representative may cancel the contract with the company. However, it cannot be canceled if the minor purchases the content with the property that the legal representative has set the scope and allowed the disposition, or if the minor has deceived the minor into believing that he or she is an adult or has the consent of the legal representative.\n⑨ Whether the party to the content purchase contract is a minor is judged based on the mobile device where the payment was made, information on the person performing the payment, and the name of the payment method. In addition, the company may request the submission of documents proving that it is a minor or a legal representative in order to confirm whether the cancellation is justified.\n⑩ Communication charges (call charges, data call charges, etc.) incurred by downloading applications or using network services may not be eligible for refund.\n⑪ The user may withdraw the subscription verbally or in writing (including electronic documents).\nArticle 23 (Refund of Overpayment, etc.)① In the event of an overpayment, the company refunds the overpayment to the user in the same way as the payment.\nHowever, if a refund is not possible in the same way, the user is notified, and if the overpayment is due to the user's negligence without the intention or negligence of the company, the actual cost for the refund shall be borne by the user within a reasonable range.\n② Payment through the application follows the payment method provided by the open market operator, and if an overpayment occurs during the payment process, the user must request a refund from the open market operator. However, if it is possible for the company to substitute or support the processing of the refund procedure due to the policy and system of the open market operator, the company may substitute or support the refund.\n③ In order to process the refund of overpayment, the company may contact the user through the information provided by the user and request the provision of necessary information.\n④ Refund of overpayment is processed according to the refund policy of each open market operator or company depending on the type of operating system of the mobile device using the service.\n⑤ Even if it does not fall under the reason for refund due to withdrawal of subscription or refund of overpayment, the company may proceed with refund according to a separate refund policy.\n⑥ Communication charges (call charges, data call charges, etc.) incurred by downloading applications or using network services may not be eligible for refund in Article 23.\nArticle 24 (Termination of Contract, etc.)\n① If the user does not want to use the service at any time, he/she may terminate the service contract by withdrawing from the service. Except for cases where the company retains information in accordance with related laws, all game use information held by the user within the game service will be deleted due to withdrawal from the service and recovery will not be possible.\n② Withdrawal from the service in Paragraph 1 can be done through the customer center or the withdrawal procedure within the service. In the case of application for withdrawal from the service, the company can check whether the user is the person or not, and if the user is identified as the person, the company will take action according to the user's application.\n③ In the event that there is a serious reason that the user cannot maintain this contract, such as the user commits an act prohibited by these terms and conditions, the operation policy and the service policy, the company gives a notice before a reasonable period and sets a period to suspend the use of the service or terminate the use contract You can cancel. However, if there is an urgent reason, the contract of use may be terminated immediately without prior notice or notice.\n④ In the case of the proviso to Paragraph 3, the user loses the right to use the paid service and paid content, and cannot claim refund or compensation for this.";
        }
        else
        {
            cancelText.text = "구매 후 7일 이내 결제 상품 청약철회 가능 (단, 사용 시 철회 불가)\n법정대리인 미동의 시 미성년자의 구매 취소 가능";


            quitObject.transform.GetChild(2).GetComponent<Image>().sprite = exitSprite[0];
            quitObject.transform.GetChild(3).GetComponent<Image>().sprite = backSprite[0];
            quitObject.transform.GetChild(4).GetComponent<Text>().text = BackendServerManager.GetInstance().langaugeSheet[21].kor;

            skipButton.GetComponent<Image>().sprite = skipSprite[0];
            for (int i = 0; i < cardBackObject.Length; i++)
            {
                cardBackObject[i].GetComponent<Image>().sprite = cardSprite[0];
            }

            if (drawCardUI.activeSelf)
            {
                //cardEffectObject[0].GetComponent<SkeletonGraphic>().skeletonDataAsset = cardEffect[0];
                //cardEffectObject[1].GetComponent<SkeletonGraphic>().skeletonDataAsset = cardEffect[1];
                //cardEffectObject[0].GetComponent<SkeletonGraphic>().Initialize(true);
                //cardEffectObject[1].GetComponent<SkeletonGraphic>().Initialize(true);
            }


            agreeButton.GetComponent<Image>().sprite = agreeSprite2;
            cardObject.GetComponent<Image>().sprite = cardSprite[0];
            getText.text = BackendServerManager.GetInstance().langaugeSheet[17].kor;
            EarnText.text = BackendServerManager.GetInstance().langaugeSheet[18].kor;
            packageObject.GetComponent<Image>().sprite = packageSprite[0];
            packageObject2.GetComponent<Image>().sprite = packageSprite[0];

            agreeText.text = "구매 후 7일 이내 결제 상품 청약철회 가능 (단, 사용시 철회 불가)\n법정대리인 미동의시 미성년자의 구매 취소 가능\n예시)\n자세히 보기 선택하면 아래 내용 안내\n(청약철회 등)\n① 회사와 유료 콘텐츠의 구매에 관한 계약을 체결한 이용자는 구매계약일과 콘텐츠 이용 가능일 중 늦은 날부터 7일 이내에 별도의 수수료‧위약금‧손해배상 등의 부담 없이 구두 또는 서면(전자문서를 포함합니다)으로 청약철회를 할 수 있습니다.\n운영정책 등으로 달리 정하지 않는 한, 서비스 이용과 관련된 청약철회나 환급은 고객센터를 통해 회사에 직접 신청해야 합니다.\n② 이용자는 다음 각 호에 해당하는 경우에는 회사의 의사에 반하여 제1항에 따른 청약철회를 할 수 없습니다.다만, 가분적 콘텐츠로 구성된 구매계약의 경우에는 가분적 콘텐츠 중 다음 각 호에 해당하지 아니하는 나머지 부분에 대하여는 그러하지 아니합니다.\n1.구매 즉시 사용되거나 적용되는 유료 콘텐츠\n2.추가혜택이 제공되는 경우에 그 추가 혜택이 사용된 콘텐츠\n3.개봉행위를 사용으로 볼 수 있거나 개봉 시 효용이 결정되는 콘텐츠의 개봉행위가 있는 경우\n4.구매 시 지급되는 부가 콘텐츠(재화, 포인트, 마일리지, 아이템 등)의 일부를 사용한 경우\n5.묶음형으로 판매된 콘텐츠의 일부가 사용되어 회수가 불가능한 경우\n6.타인으로부터 선물받는 등 이용자가 구매하지 않은 콘텐츠\n7.이용자에게 책임이 있는 사유로 일부 또는 전부가 멸실되거나 훼손된 콘텐츠\n8.그 밖에 「전자상거래 등에서의 소비자보호에 관한 법률」(이하 「전자상거래법」이라 합니다) 등 관계법령에 따라 청약철회가 제한되는 콘텐츠\n③ 회사는 제2항 각 호의 규정에 따라 청약철회가 불가능한 콘텐츠의 경우에는 그 사실을 이용자가 쉽게 알 수 있는 곳에 명확하게 표시하고, 해당 콘텐츠의 시험사용 상품을 제공(한시적 이용의 허용, 체험용 제공 등)하거나 이에 대한 제공이 곤란한 경우에는 콘텐츠에 관한 정보를 제공함으로써 이용자의 청약철회의 권리행사가 방해 받지 아니하도록 조치합니다.만약 회사가 이러한 조치를 하지 아니한 경우에는 제2항 각 호의 청약철회 제한사유에도 불구하고 이용자는 청약철회를 할 수 있습니다.\n④ 이용자는 제1항 및 제2항에도 불구하고 구매한 유료 콘텐츠의 내용이 표시∙광고의 내용과 다르거나 구매계약의 내용과 다르게 이행된 경우에 해당 콘텐츠가 이용 가능하게 된 날부터 3개월 이내, 그 사실을 안 날 또는 알 수 있었던 날부터 30일 이내에 청약철회를 할 수 있습니다.\n⑤ 이용자가 청약철회를 할 경우 회사는 플랫폼사업자 또는 오픈마켓 사업자를 통해 구매내역을 확인합니다. 또한 회사는 이용자의 정당한 철회 사유를 확인하기 위해 이용자에게서 제공받은 정보를 통해 이용자에게 연락할 수 있으며, 추가적인 증빙을 요구할 수 있습니다.\n⑥ 제1항부터 제4항까지의 규정에 따라 청약철회가 이루어질 경우 회사는 지체 없이 이용자의 유료 콘텐츠를 회수하고 3영업일 이내에 대금을 환급합니다. 이 경우 회사가 환급을 지연한 때에는 그 지연기간에 대하여 「전자상거래법」 및 같은 법 시행령 제21조의3에서 정하는 이율을 곱하여 산정한 지연이자를 지급합니다.\n⑦ 회사는 제6항에 따라 대금을 환급할 때 소비자가 「전자상거래법」 제18조 제3항의 결제수단으로 대금을 지급한 경우에는 결제업자에게 대금 청구를 정지하거나 취소하도록 요청합니다. 다만 회사가 결제업자로부터 청약철회 대상 구매의 대금을 이미 받은 때에는 그 대금을 결제업자에게 환급하고, 그 사실을 이용자에게 알립니다.\n⑧ 미성년자가 모바일 기기에서 콘텐츠 구매계약을 체결하는 경우, 회사는 법정대리인의 동의가 없으면 미성년자 본인 또는 법정대리인이 그 계약을 취소할 수 있다는 내용을 고지하며, 미성년자가 법정대리인의 동의 없이 구매계약을 체결한 때에는 미성년자 본인 또는 법정대리인은 회사에 그 계약을 취소할 수 있습니다.다만, 미성년자가 법정대리인이 범위를 정하여 처분을 허락한 재산으로 콘텐츠를 구매한 경우 또는 미성년자가 속임수로써 자기를 성년자로 믿게 하거나 법정대리인의 동의가 있는 것으로 믿게 한 경우에는 취소할 수 없습니다.\n⑨ 콘텐츠 구매계약의 당사자가 미성년자인지 여부는 결제가 진행된 모바일 기기, 결제 실행자 정보, 결제 수단 명의자 등을 근거로 판단합니다. 또한 회사는 정당한 취소인지를 확인하기 위해 미성년자 및 법정대리인임을 증명할 수 있는 서류의 제출을 요청할 수 있습니다.\n⑩ 애플리케이션의 다운로드 또는 네트워크 서비스의 이용으로 인해 발생한 통신요금(통화료, 데이터 통화료 등)은 환급 대상에서 제외될 수 있습니다.\n⑪ 이용자는 구두 또는 서면(전자문서 포함)으로 청약철회를 할 수 있습니다.\n\n(과오납금 등의 환급)\n① 회사는 과오납금이 발생하는 경우 과오납금을 이용자에게 대금결제와 동일한 방법으로 전액 환급합니다.\n다만 동일한 방법으로 환급이 불가능한 경우 이용자에게 고지하며, 과오납금이 회사의 고의 또는 과실 없이 이용자의 과실로 인하여 발생한 경우에는 그 환급에 소요되는 실제 비용은 합리적인 범위 내에서 이용자가 부담합니다.\n② 애플리케이션을 통한 결제는 오픈마켓 사업자가 제공하는 결제방식에 따르며, 결제 과정에서 과오납금이 발생하는 경우 이용자는 오픈마켓 사업자에게 환급을 요청하여야 합니다. 다만 오픈마켓 사업자의 정책 및 시스템상 회사가 환급절차의 처리를 대신하거나 지원하는 것이 가능한 경우, 회사가 환급을 대신하거나 지원할 수 있습니다.\n③ 회사는 과오납금의 환급을 처리하기 위해 이용자에게서 제공받은 정보를 통해 이용자에게 연락할 수 있으며, 필요한 정보의 제공을 요청할 수 있습니다.\n④ 과오납금의 환급은 서비스를 이용하고 있는 모바일 기기의 운영체제 종류에 따라 각 오픈마켓 사업자 또는 회사의 환급정책에 따라 진행됩니다.\n⑤ 청약철회로 인한 환급 사유 또는 과오납금의 환급 사유에 해당하지 않는 경우에도 회사는 별도의 환급정책에 따라 환급을 진행할 수 있습니다.\n⑥ 애플리케이션의 다운로드 또는 네트워크 서비스의 이용으로 인해 발생한 통신요금(통화료, 데이터 통화료 등)은 23조의 환급 대상에서 제외될 수 있습니다.\n\n제24조(계약 해지 등)\n① 이용자는 언제든지 서비스 이용을 원하지 않는 경우 서비스 탈퇴를 통해 이용계약을 해지할 수 있습니다. 관계법령에 의해 회사가 정보를 보유하는 경우를 제외하고, 서비스 탈퇴로 인해 이용자가 게임서비스 내에서 보유한 게임이용정보는 모두 삭제되어 복구가 불가능하게 됩니다.\n② 제1항의 서비스 탈퇴는 고객센터 또는 서비스 내 탈퇴 절차를 통하여 할 수 있습니다.서비스 탈퇴를 신청한 경우 회사는 이용자의 본인 여부를 확인할 수 있으며, 해당 이용자가 본인으로 확인되는 경우에 이용자의 신청에 따른 조치를 취합니다.\n③ 회사는 이용자가 이 약관 및 그에 따른 운영정책, 서비스 정책에서 금지하는 행위를 하는 등 본 계약을 유지할 수 없는 중대한 사유가 있는 경우에는 상당한 기간 전에 최고하고 기간을 정하여 서비스 이용을 중지하거나 이용계약을 해지할 수 있습니다.다만 긴급한 사유가 있는 경우에는 사전 통지나 최고 없이 즉시 이용계약을 해지할 수 있습니다.\n④ 제3항 단서의 경우, 이용자는 유료 서비스 및 유료 콘텐츠의 사용권한을 상실하고 이로 인한 환불 및 손해배상을 청구할 수 없습니다.";
        }

        if (storePanel.activeSelf)
        {
            if (energyAmount >= energyMax)
            {
                adviceRemainText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "20분마다 무료!" : "Free every 20 minutes!"; 
                adviceRemainText.GetComponentInParent<Button>().interactable = true;
            }
                
            else
            {
                adviceRemainText.text = ((int)rechargeRemainTime / 60) + " : " + string.Format("{0:D2}", (int)rechargeRemainTime % 60);
                adviceRemainText.GetComponentInParent<Button>().interactable = false;
            }
        }
        

        if (adCount >= 5)
            drawCardPanel.transform.GetChild(2).GetChild(1).gameObject.GetComponent<Button>().interactable = false;
        else
            drawCardPanel.transform.GetChild(2).GetChild(1).gameObject.GetComponent<Button>().interactable = true;

        if (Input.GetKey(KeyCode.Escape))
        {
            closeAppUI.SetActive(true);
        }

        goldText.text = gold.ToString();
        //diamondText.text = diamond.ToString();

        goldText2.text = gold.ToString();
        //diamondText2.text = diamond.ToString();

        adCountText.text = (5 - adCount).ToString();
    }

    public void OnApplicationFocus(bool focus)
    {
        if (!isLogin) return;

        if (focus)
        {
            Debug.LogError("들어옴");
            BackendServerManager.GetInstance().RefreshInfo();

            //LoadHeartInfo();
            //LobbyUI.GetInstance().LoadAppQuitTime();
            //LobbyUI.GetInstance().SetRechargeScheduler();
        }

        else
        {
            Debug.LogError("나감");
            BackendServerManager.GetInstance().RefreshInfo();

            //SaveHeartInfo();
            //LobbyUI.GetInstance().SaveAppQuitTime();

            if (rechargeTimerCoroutine != null)
                StopCoroutine(rechargeTimerCoroutine);
        }
    }

    private void OnApplicationQuit()
    {
        //SaveHeartInfo();

        if (!isLogin) return;
        SaveAppQuitTime();
    }

    #region 번역
    public void SetLangauge()
    {
        if (PlayerPrefs.GetInt("LangIndex") == 0)
        {
            titleObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "title_e 02", true);
            PlayerPrefs.SetInt("LangIndex", 1);
            settingPanel.transform.GetChild(8).GetChild(0).GetComponent<Text>().text = "English";
        }
        else
        {
            titleObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "title 02", true);
            PlayerPrefs.SetInt("LangIndex", 0);
            settingPanel.transform.GetChild(8).GetChild(0).GetComponent<Text>().text = "한국어";
        }
    }
    #endregion

    #region 게임 시작 버튼 (40% 확률로 광고)
    public void GameStart()
    {
        SceneManager.LoadScene("3. Game");
            
    }
    #endregion

    #region 로그아웃
    public void LogOut()
    {
        //errorObject.SetActive(true);
        //errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "안정적인 로그아웃을 위해 앱을 종료합니다." : "Close the app for reliable logout.";
        //errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
        //{
        //    BackendServerManager.GetInstance().LogOut();
        //    Application.Quit();
        //});
        BackendServerManager.GetInstance().LogOut();
        SceneManager.LoadScene("2. Lobby");
    }
    #endregion

    #region 닉네임 설정
    private void setNickName()
    {
        var name = BackendServerManager.GetInstance().myNickName;
        if (name.Equals(string.Empty))
        {
            Debug.Log("닉네임 불러오기 실패");
            name = "Error";
        }

        nickNameText.text = name;
    }
    #endregion

    #region 상점 요소
    public void BuyWithGold(int num)
    {
        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().BuyCard(num, true, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                loadingObject.SetActive(false);
                if (result)
                {
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = error;
                errorObject.SetActive(true);
            });
        });
    }

    public void BuyWithDiamond(int num)
    {
        loadingObject.SetActive(true);
        BackendServerManager.GetInstance().BuyCard(num, false, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                loadingObject.SetActive(false);
                if (result)
                {
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = error;
                errorObject.SetActive(true);
            });
        });
    }

    public void BuyWithAd()
    {
        if ((5 - adCount) <= 0) return;

        

        ADManager.GetInstance().ShowRewardedAd(() =>
        {
            loadingObject.SetActive(true);
            BackendServerManager.GetInstance().DrawCard(true, (bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    loadingObject.SetActive(false);
                    if (result)
                    {
                        BackendServerManager.GetInstance().SaveAd(adCount + 1);
                        return;
                    }
                    errorObject.GetComponentInChildren<Text>().text = error;
                    errorObject.SetActive(true);
                });
            });
        },
        ()=>
        {
        },
        () =>
        {
            errorObject.GetComponentInChildren<Text>().text = "네트워크가 불안정하여 취소되었습니다.";
            errorObject.SetActive(true);
            loadingObject.SetActive(false);
        });
           
    }
    #endregion

    #region 랭크
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
                    return;
                }
                errorObject.GetComponentInChildren<Text>().text = error;
                errorObject.SetActive(true);
            });
        });
    }
    #endregion
    public void QuitGame() => Application.Quit();
    public void ShowRankUI() => rankPanel.SetActive(true);

    public Sprite[] cardBackImage;
    public void setCardInfo(int cardNum, int count, int reward)
    {
        Cards[cardNum].GetComponent<Image>().sprite = PlayerPrefs.GetInt("LangIndex") == 0 ? cardBackImage[0] : cardBackImage[1];
        int star = 0;
        if (cardNum > 95) star = 4;
        else if (cardNum > 74) star = 3;
        else if (cardNum > 54) star = 2;
        else if (cardNum > 29) star = 1;
        else star = 0;         

        if (count == 0)
        {
            Cards[cardNum].GetComponentsInChildren<Image>()[1].sprite = CardImage[0];
            Cards[cardNum].GetComponentsInChildren<Image>()[2].enabled = false;
            Cards[cardNum].GetComponentInChildren<Text>().enabled = false;
        }
        else
        {
            Cards[cardNum].GetComponentsInChildren<Image>()[1].sprite = CardImage[cardNum + 1];
            Cards[cardNum].GetComponentsInChildren<Image>()[2].enabled = true;
            Cards[cardNum].GetComponentsInChildren<Image>()[2].sprite = cardOutLine[star];
            Cards[cardNum].GetComponentInChildren<Text>().enabled = true;
        }

        Cards[cardNum].GetComponentInChildren<Text>().text = count.ToString();
        Cards[cardNum].GetComponent<Button>().onClick.RemoveAllListeners();
        Cards[cardNum].GetComponent<Button>().onClick.AddListener(() => OpenCardInfo(cardNum, count.ToString(), reward, star));

        cardBookObjs[0].GetComponent<ScrollRect>().velocity = new Vector2(0, 0); //스크롤 하면서 다른 창으로 넘어갈 때 스크롤 저항 생김
        cardBookObjs[1].GetComponent<RectTransform>().position = new Vector2(cardBookObjs[1].GetComponent<RectTransform>().position.x, 0); //다른 창으로 넘길때 맨 위로 이동
    }

    public void PurchaseSound()
    {
        SoundManager.Instance.PlayEffect(4);
    }

    public void OpenCardInfo(int cardNum, string count, int reward, int star)
    {
        if (count != "0")
        {
            cardInfo.SetActive(true);

            cardInfo.transform.GetChild(3).GetComponent<Text>().text = count;
            cardInfo.transform.GetChild(2).GetComponent<Image>().sprite = CardImage[cardNum + 1];
            cardInfo.GetComponentsInChildren<Image>()[3].sprite = cardOutLine[star];

            cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                DiscardCard(cardNum, int.Parse(count), reward);
            });

            cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => SoundManager.Instance.PlayEffect(0));
        }
    }

    public void CloseCardInfo()
    {
        cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
    }

    void DiscardCard(int cardNum, int cardCount, int reward)
    {
        int getRubyNum = 0;

        if(cardCount >= 30)
        {
                if (reward == 0) getRubyNum = 17300;
                else if (reward == 1) getRubyNum = 17000;
                else if (reward == 2) getRubyNum = 15000;
                else if (reward == 3) getRubyNum = 10000;
                else if (reward == 4) getRubyNum = 0;
            reward = 4;
        }

        else if(cardCount >= 20 && cardCount < 30)
        {
                if (reward == 0) getRubyNum = 7300;
                else if (reward == 1) getRubyNum = 7000;
                else if (reward == 2) getRubyNum = 5000;
                else if (reward == 3) getRubyNum = 0;
            reward = 3;
        }

        else if (cardCount >= 10 && cardCount < 20)
        {
                if (reward == 0) getRubyNum = 2300;
                else if (reward == 1) getRubyNum = 2000;
                else if (reward == 2) getRubyNum = 0;
            reward = 2;
        }

        else if (cardCount >= 3 && cardCount < 10)
        {
                if (reward == 0) getRubyNum = 300;
                else if (reward == 1) getRubyNum = 0;
            reward = 1;
        }


        if(getRubyNum != 0)
        {
            getRubyInfo.SetActive(true);
            getRubyInfo.transform.GetChild(2).GetComponent<Text>().text = getRubyNum.ToString();
            Debug.LogError(gold + ", " + getRubyNum);
            BackendServerManager.GetInstance().SaveGold(gold + getRubyNum);
            BackendServerManager.GetInstance().SetCard(cardNum + 1, string.Format("{0:D3}", cardCount) + "+" + reward);
            cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void TEST()
    {
        BackendServerManager.GetInstance().SaveGold(gold + 5000);
    }

    public void ResetCard()
    {
        BackendServerManager.GetInstance().GetUserCards(0);
    }

    public void URL()
    {
        Application.OpenURL("https://www.youtube.com/c/%EA%B8%89%EC%8B%9D%EC%99%95");
    }

    public void BuyCash(int num)
    {
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

    //카드
    public void CollectionValue()
    {
        BackendServerManager.GetInstance().GetUserCards(0);
        //switch (collectionGroup.GetFirstActiveToggle().name)
        //{
        //    case "MenuToggle_0":
        //        gameNameText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "선생님 몰래" : "Don't Let the Teacher Know";
        //        BackendServerManager.GetInstance().GetUserCards(0);
        //        break;
        //    case "MenuToggle_1":
        //        gameNameText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "두더지의 급식시간" : "Mole's School Lunch Time";
        //        break;
        //    default:
        //        break;
        //}
    }

    public void CheckBuyItems()
    {
        if(PlayerPrefs.GetInt("PACKAGE") == 1)
        {
            storePanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
    }

    #region 광고 시간 체크
    private DateTime AppQuitTime = new DateTime(2001, 6, 30).ToLocalTime();
    private float rechargeRemainTime = 0;
    private Coroutine rechargeTimerCoroutine = null;
    public int energyAmount = 0; //현재 에너지 개수
    public int energyMax = 1; //최대 에너지 개수
    public int energyRechargeInterval = 30; //에너지 재충전 시간

    public void StartCoroutine()
    {
        //LoadHeartInfo();
        LoadAppQuitTime();
        SetRechargeScheduler();
    }

    public void StartCoroutine2()
    {
        //SaveHeartInfo();
        LobbyUI.GetInstance().SaveAppQuitTime();
    }

    public bool LoadHeartInfo()
    {
        bool result = false;
        try
        {
            if (PlayerPrefs.HasKey("HeartAmount"))
            {
                energyAmount = PlayerPrefs.GetInt("HeartAmount");
                Debug.LogError(energyAmount);
                if (energyAmount < 0)
                {
                    energyAmount = 0;
                }
            }
            else
            {
                print("띠용?");
                energyAmount = energyMax;
            }
            //heartAmountLabel.text = m_HeartAmount.ToString();
            result = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("LoadHeartInfo Failed (" + e.Message + ")");
        }
        return result;
    }
    public bool SaveHeartInfo()
    {
        bool result = false;
        try
        {
            PlayerPrefs.SetInt("HeartAmount", energyAmount);
            PlayerPrefs.Save();
            result = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveHeartInfo Failed (" + e.Message + ")");
        }
        return result;
    }


    public bool LoadAppQuitTime()
    {
        bool result = false;
        try
        {
            if (PlayerPrefs.HasKey("AppQuitTime"))
            {
                var appQuitTime = string.Empty;
                appQuitTime = PlayerPrefs.GetString("AppQuitTime");
                this.AppQuitTime = DateTime.FromBinary(Convert.ToInt64(appQuitTime));
            }
            else
            {
                AppQuitTime = DateTime.Now.ToLocalTime();
            }

            result = true;
        }
        catch (Exception e)
        {
            Debug.LogError("LoadAppQuitTime Failed : " + e.Message);
        }

        return result;
    }

    public bool SaveAppQuitTime()
    {
        bool result = false;
        try
        {
            var appQuitTime = DateTime.Now.ToLocalTime().ToBinary().ToString();
            PlayerPrefs.SetString("AppQuitTime", appQuitTime);
            PlayerPrefs.SetFloat("RemainTime", rechargeRemainTime);
            PlayerPrefs.Save();

            result = true;
        }
        catch (Exception e)
        {
            Debug.LogError("SaveAppQuitTime Failed : " + e.Message);
        }

        return result;
    }

    public void SetRechargeScheduler()
    {

        if (rechargeTimerCoroutine != null)
            StopCoroutine(rechargeTimerCoroutine);

        var timeDifferenceInSec = (float)(DateTime.Now.ToLocalTime() - AppQuitTime).TotalSeconds;

        float originTimeDifferenceInSec = timeDifferenceInSec;

        if (energyAmount >= energyMax)
        {
            if (storePanel.activeSelf)
            {
                adviceRemainText.GetComponentInParent<Button>().interactable = true;
                adviceRemainText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "20분마다 무료!" : "Free every 20 minutes!"; 
            }
            
            return;
        }

        float remainTime = 0;
        int energyToAdd;
        if (timeDifferenceInSec > 0)
        {
            timeDifferenceInSec = PlayerPrefs.GetFloat("RemainTime") - timeDifferenceInSec;

            if (timeDifferenceInSec <= 0)
            {
                energyAmount++;

                if (energyAmount >= energyMax)
                    energyAmount = energyMax;
                timeDifferenceInSec = Mathf.Abs(timeDifferenceInSec);
                energyToAdd = Mathf.FloorToInt(timeDifferenceInSec / energyRechargeInterval);
                if (energyToAdd == 0)
                    remainTime = energyRechargeInterval - timeDifferenceInSec;
                else
                    remainTime = energyRechargeInterval - (timeDifferenceInSec % energyRechargeInterval);
            }
            else
            {
                energyToAdd = Mathf.FloorToInt(timeDifferenceInSec / energyRechargeInterval);
                if (energyToAdd == 0)
                    remainTime = PlayerPrefs.GetFloat("RemainTime") - originTimeDifferenceInSec;
            }

            energyAmount += energyToAdd;
        }

        else if (timeDifferenceInSec <= 0)
            print("??");

        if (energyAmount >= energyMax)
        {
            energyAmount = energyMax;

            if (storePanel.activeSelf)
            {
                adviceRemainText.GetComponentInParent<Button>().interactable = true;
                adviceRemainText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "20분마다 무료!" : "Free every 20 minutes!"; 
            }
            
        }
        else
            rechargeTimerCoroutine = StartCoroutine(DoRechargeTimer(remainTime));
    }

    public void UseEnergy(int amount)
    {
        if (energyAmount <= 0)
            return;

        

        ADManager.GetInstance().ShowRewardedAd(() =>
        {
            energyAmount -= amount;

            

            if (rechargeTimerCoroutine == null)
            {
                rechargeTimerCoroutine = StartCoroutine(DoRechargeTimer(energyRechargeInterval));
            }
            //rechargeTimerCoroutine = StartCoroutine(DoRechargeTimer(energyRechargeInterval));
            BackendServerManager.GetInstance().SaveGold(gold + 200);
            //StartCoroutine(DoRechargeTimer(energyRechargeInterval));
            
        },
        () =>
        {
        },
        () =>
        {
            errorObject.GetComponentInChildren<Text>().text = "네트워크가 불안정하여 취소되었습니다.";
            errorObject.SetActive(true);
        });
    }

    private IEnumerator DoRechargeTimer(float remainTime)
    {

        if (energyAmount >= energyMax)
        {


            rechargeRemainTime = 0;
            StopAllCoroutines();

            adviceRemainText.GetComponentInParent<Button>().interactable = true;
            adviceRemainText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "20분마다 무료!" : "Free every 20 minutes!";
            Debug.LogError("AAD : " + energyAmount + " / " + energyMax);
        }
        else
        {
            if (remainTime <= 0)
                rechargeRemainTime = energyRechargeInterval;
            else
                rechargeRemainTime = remainTime;

            while (rechargeRemainTime > 0)
            {
                if (storePanel.activeSelf)
                {
                    adviceRemainText.text = ((int)rechargeRemainTime / 60) + " : " + string.Format("{0:D2}", (int)rechargeRemainTime % 60).ToString();
                    adviceRemainText.GetComponentInParent<Button>().interactable = false;
                }
                Debug.LogError("AAA : " + ((int)rechargeRemainTime / 60) + " : " + string.Format("{0:D2}", (int)rechargeRemainTime % 60).ToString());

                rechargeRemainTime--;

                yield return new WaitForSeconds(1);
            }

            energyAmount++;

            if (energyAmount >= energyMax)
            {
                if (storePanel.activeSelf)
                {

                    adviceRemainText.GetComponentInParent<Button>().interactable = true;
                    adviceRemainText.text = PlayerPrefs.GetInt("LangIndex") == 0 ? "20분마다 무료!" : "Free every 20 minutes!"; 
                }
                energyAmount = energyMax;
                rechargeRemainTime = 0;
                Debug.LogError("AAB : " + energyAmount + " / " + energyMax);

                rechargeTimerCoroutine = null;
            }
            else
            {
                rechargeTimerCoroutine = StartCoroutine(DoRechargeTimer(energyRechargeInterval));
            }

        }
    }
    #endregion


    public static LobbyUI GetInstance()
    {
        if (instance == null) return null;

        return instance;
    }

    void Awake()
    {
        if (!instance) instance = this;

        if (PlayerPrefs.GetInt("PACKAGE") == 1)
        {
            offPackageObject2.GetComponent<SkeletonGraphic>().color = new Color(0, 0, 0, 0);

        }
    }
}
