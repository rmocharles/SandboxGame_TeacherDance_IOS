using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// Include Backend
using BackEnd;
using static BackEnd.SendQueue;
//  Include GPGS namespace
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using LitJson;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using System.Text;
using Spine.Unity;


public class RankItem //Rank Class
{
    public string nickname { get; set; } // 닉네임
    public string score { get; set; }    // 점수
    public string rank { get; set; }     // 랭크
}

public class BackendServerManager : MonoBehaviour
{

    private static BackendServerManager instance;   // 인스턴스

    public string myNickName { get; private set; } = string.Empty;  // 로그인한 계정의 닉네임
    public string myIndate { get; private set; } = string.Empty;    // 로그인한 계정의 inDate
    private Action<bool, string> loginSuccessFunc = null;

    private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";

    public string appleToken = ""; // SignInWithApple.cs에서 토큰값을 받을 문자열

    public string userIndate;
    string scoreIndate;

    string userInDateScore;


    public string rankUuid = "";

    //=================================================================================================
    #region 서버 초기화
    void Initialize()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();
#endif

        var bro = Backend.Initialize(true);

        if (bro.IsSuccess())
        {
#if UNITY_ANDROID //안드로이드에서만 작동
            Debug.Log("GoogleHash - " + Backend.Utils.GetGoogleHash());
#endif

#if UNITY_IOS
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                Debug.LogError("IOS CHECK");
                var deserializer = new PayloadDeserializer();
                appleAuthManager = new AppleAuthManager(deserializer);
            }
#endif

#if !UNITY_EDITOR //안드로이드, iOS 환경에서만 작동
            GetVersionInfo();
#endif
            LobbyUI.GetInstance().TouchStart();
        }
        else Debug.LogError("뒤끝 초기화 실패 : " + bro);
    }
    #endregion

    #region 버전 확인 (모바일)
    private void GetVersionInfo()
    {
        Enqueue(Backend.Utils.GetLatestVersion, callback =>
        {
            if (callback.IsSuccess() == false)
            {
                Debug.LogError("버전정보를 불러오는 데 실패하였습니다.\n" + callback);
                return;
            }

            var version = callback.GetReturnValuetoJSON()["version"].ToString();

            Version server = new Version(version);
            Version client = new Version(Application.version);

            var result = server.CompareTo(client);
            if (result == 0)
            {
                // 0 이면 두 버전이 일치
                return;
            }
            else if (result < 0)
            {
                // 0 미만이면 server 버전이 client 이전 버전
                // 검수를 넣었을 경우 여기에 해당된다.
                // ex) 검수버전 3.0.0, 라이브에 운용되고 있는 버전 2.0.0, 콘솔 버전 2.0.0
                return;
            }
            else
            {
                // 0보다 크면 server 버전이 client 이후 버전
                if (client == null)
                {
                    // 클라이언트가 null인 경우 예외처리
                    Debug.LogError("클라이언트 버전정보가 null 입니다.");
                    return;
                }
            }

            // 버전 업데이트 팝업
            //LoginUI.GetInstance().OpenUpdatePopup();
        });
    }
    #endregion

    #region 토큰으로 로그인
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("토큰 로그인 성공");
                loginSuccessFunc = func; //로그인 성공 여부 데이터 입력

                OnBackendAuthorized(); //사전 유저 불러오기
                return;
            }
            else
            {
                Debug.Log("토큰 로그인 실패\n" + callback.ToString());
                func(false, string.Empty); //실패시 토큰 초기화
                Backend.BMember.DeleteGuestInfo();
                LobbyUI.GetInstance().TouchStart();
            }
        });
    }
    #endregion

    #region 커스텀 로그인
    public void CustomLogin(string id, string pw, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.CustomLogin, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("커스텀 로그인 성공");
                loginSuccessFunc = func; //로그인 여부 데이터 입력

                OnBackendAuthorized(); //사전 유저 정보 불러오기
                return;
            }

            Debug.Log("커스텀 로그인 실패\n" + callback);
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    #endregion

    #region 커스텀 회원가입
    public void CustomSignIn(string id, string pw, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.CustomSignUp, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("커스텀 회원가입 성공");
                loginSuccessFunc = func; //로그인 여부 데이터 입력

                OnBackendAuthorized(); //사전 유저 정보 불러오기
                return;
            }

            Debug.LogError("커스텀 회원가입 실패\n" + callback.ToString());
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    #endregion

    #region 구글 페더레이션 로그인 / 회원가입
    public void GoogleAuthorizeFederation(Action<bool, string> func)
    {
#if UNITY_ANDROID //안드로이드 일 경우

        if (Social.localUser.authenticated) // 이미 gpgs 로그인이 된 경우
        {
            var token = GetFederationToken();
            if (token.Equals(string.Empty)) //토큰이 존재하지 않을 경우
            {
                Debug.LogError("GPGS 토큰이 존재하지 않습니다.");
                func(false, "GPGS 인증을 실패하였습니다.\nGPGS 토큰이 존재하지 않습니다.");
                return;
            }

            Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs 인증", callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("GPGS 인증 성공");
                    loginSuccessFunc = func;

                    OnBackendAuthorized(); //사전 유저 정보 불러오기
                    return;
                }

                string ANG = "";
                switch (callback.GetErrorCode())
                {
                    case "403":

                        ANG = "차단 당한 유저입니다.";
                        break;
                }
                Debug.LogError("GPGS 인증 실패\n" + callback.ToString());
                func(false, string.Format(BackendError, ANG));
            });
        }

        else // gpgs 로그인을 해야하는 경우
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    var token = GetFederationToken();
                    if (token.Equals(string.Empty))
                    {
                        Debug.LogError("GPGS 토큰이 존재하지 않습니다.");
                        func(false, "GPGS 인증을 실패하였습니다.\nGPGS 토큰이 존재하지 않습니다.");
                        return;
                    }

                    Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs 인증", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            Debug.Log("GPGS 인증 성공");
                            loginSuccessFunc = func;

                            OnBackendAuthorized(); //사전 유저 정보 불러오기
                            return;
                        }

                        Debug.LogError("GPGS 인증 실패\n" + callback.ToString());
                        func(false, string.Format(BackendError,
                            callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                    });
                }
                else
                {
                    Debug.LogError("GPGS 로그인 실패");
                    func(false, "GPGS 인증을 실패하였습니다.\nGPGS 로그인을 실패하였습니다.");
                    return;
                }
            });
        }
#endif
    }
    #endregion

    #region 애플 페더레이션 로그인 / 회원가입
    private IAppleAuthManager appleAuthManager;

    public void AppleLogin(Action<bool, string> func)
    {
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        this.appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                Debug.LogError("함수 실!!");

                // Obtained credential, cast it to IAppleIDCredential
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    // Apple User ID
                    // You should save the user ID somewhere in the device
                    var userId = appleIdCredential.User;
                    PlayerPrefs.SetString(appleToken, userId);

                    // Email (Received ONLY in the first login)
                    var email = appleIdCredential.Email;

                    // Full name (Received ONLY in the first login)
                    var fullName = appleIdCredential.FullName;

                    // Identity token
                    var identityToken = Encoding.UTF8.GetString(
                                appleIdCredential.IdentityToken,
                                0,
                                appleIdCredential.IdentityToken.Length);

                    // Authorization code
                    var authorizationCode = Encoding.UTF8.GetString(
                                appleIdCredential.AuthorizationCode,
                                0,
                                appleIdCredential.AuthorizationCode.Length);
                    Debug.LogError("Apple Login.....");

                    // And now you have all the information to create/login a user in your system
                    Enqueue(Backend.BMember.AuthorizeFederation, identityToken, FederationType.Apple, "apple 인증", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            loginSuccessFunc = func;
                            Debug.LogError("애플로그인 성");
                            OnBackendAuthorized();
                            return;
                        }

                        LobbyUI.GetInstance().loadingObject.SetActive(false);
                        LobbyUI.GetInstance().errorObject.SetActive(true);
                        LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = callback.ToString();

                        Debug.LogError("Apple 로그인 에러\n" + callback.ToString());
                        loginSuccessFunc(false, string.Format(BackendError,
                            callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                    });
                }
            },
            error =>
            {
                // Something went wrong
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogError(authorizationErrorCode);

                LobbyUI.GetInstance().loadingObject.SetActive(false);
                LobbyUI.GetInstance().errorObject.SetActive(true);
                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = authorizationErrorCode.ToString();
            });
    }
    #endregion

    #region 구글 토큰 받기
    private string GetFederationToken()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.LogError("GPGS에 접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
            return string.Empty;
        }
        // 유저 토큰 받기
        string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
        return _IDtoken;

#elif UNITY_IOS
        return string.Empty;
#else
        return string.Empty;
#endif
    }
    #endregion

    #region 닉네임 값 입력
    public void UpdateNickname(string nickname, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
        {
            // 닉네임이 없으면 매치서버 접속이 안됨
            if (!bro.IsSuccess())
            {
                if(bro.GetErrorCode().ToString() == "DuplicatedParameterException")
                {
                    func(false, PlayerPrefs.GetInt("LangIndex") == 0 ? "이미 존재하는 닉네임입니다." : "This nickname already exists.");
                }
                if(bro.GetErrorCode() == "BadParameterException")
                {
                    func(false, PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[14].kor : BackendServerManager.GetInstance().langaugeSheet[14].eng);
                }
                return;
            }
            loginSuccessFunc = func;
            OnBackendAuthorized(); //유저 사전 정보 불러오기
        });
    }
    #endregion

    #region 게스트 로그인
    public void GuestLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.GuestLogin, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("게스트 로그인 성공");
                loginSuccessFunc = func;

                OnBackendAuthorized(); //유저 사전 정보 불러오기
                return;
            }

            Debug.Log("게스트 로그인 실패\n" + callback);

            LobbyUI.GetInstance().loadingObject.SetActive(false);
            LobbyUI.GetInstance().errorObject.SetActive(true);
            LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = callback.GetErrorCode();

            string ANG = "";
            switch (callback.GetErrorCode())
            {
                case "403":
                    ANG = "차단 당한 아이디입니다.";
                    break;
            }
            print("ERROR" + ANG);
            func(false, string.Format(BackendError, ANG));
        });
    }
    #endregion

    #region 실제 유저 정보 불러오기
    private void OnBackendAuthorized()
    {
        Enqueue(Backend.BMember.GetUserInfo, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("유저 정보 불러오기 실패\n" + callback);
                loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                return;
            }
            Debug.Log("유저정보\n" + callback);

            var info = callback.GetReturnValuetoJSON()["row"];

            //GetLangaugeSheet();

            if (info["nickname"] == null) //닉네임 값이 없을 경우 닉네임 적는 UI띄우기
            {
                LobbyUI.GetInstance().ActiveNickNameObject();
                return;
            }
            myNickName = info["nickname"].ToString();
            myIndate = info["inDate"].ToString();

            if (loginSuccessFunc != null) //로그인 성공했으므로 매칭 리스트 값 불러옴
            {
                
                InitializeCheck();
            }
        });
    }
    #endregion

    #region 로그아웃
    public void LogOut() => Backend.BMember.Logout();
    #endregion
    //=================================================================================================

    #region 점수 등록
    internal void InsertScore(int _score)
    {
        Param param = new Param();
        param.Add("score", _score);

        Backend.GameData.Insert("score", param, insertScoreBro =>
        {
            Debug.Log("InsertScore - " + insertScoreBro);
            userInDateScore = insertScoreBro.GetInDate();

            Enqueue(Backend.URank.User.UpdateUserScore, rankUuid, "score", userInDateScore, param, updateScoreBro =>
            {
                Debug.Log("UpdateUserScore - " + updateScoreBro);
            });
        });
    }

    private void UpdateScore(int _score)
    {

        // 서버로 삽입할 데이터 생성
        Param param = new Param();
        param.Add("score", _score);

        Backend.URank.User.UpdateUserScore(rankUuid, "score", userInDateScore, param, updateScoreBro =>
        {
            Debug.Log("UpdateUserScore - " + updateScoreBro);
            if (updateScoreBro.IsSuccess())
            {
            }
            else
            {
            }
        });
    }
    public void UpdateScore2(int _score)
    {
        Enqueue(Backend.GameData.Get, "score", new Where(), myScoreBro =>
        {
            Debug.Log("플레이어 점수 정보 - " + myScoreBro.ToString());
            if (myScoreBro.IsSuccess())
            {
                JsonData userData = myScoreBro.GetReturnValuetoJSON()["rows"];
                // 유저 스코어가 존재하는 경우
                if (userData.Count > 0)
                {
                    userInDateScore = userData[0]["inDate"]["S"].ToString();
                    print("이미 존재 : " + (int.Parse(userData[0]["score"]["N"].ToString()) < _score));

                    // 유저 스코어 update
                    if (int.Parse(userData[0]["score"]["N"].ToString()) < _score)
                    {
                        GameManager.GetInstance().checkHighScore.SetActive(true);
                        GameManager.GetInstance().checkHighScore.GetComponent<SkeletonGraphic>().AnimationState.SetEmptyAnimation(0, 0);
                        GameManager.GetInstance().checkHighScore.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "Stamp2", false);
                        print(_score);
                        UpdateScore(_score);
                    }
                }
                // 유저 스코어가 존재하지 않는 경우
                else
                {
                    // 유저 스코어 insert
                    GameManager.GetInstance().checkHighScore.SetActive(true);
                    InsertScore(_score);
                }
            }
            else
            {
                InsertScore(_score);
            }
        });
    }
    public void UpdateScores(int point)
    {
        var bro = Backend.GameData.GetMyData("score", new Where());
        //bro.GetReturnValuetoJSON["rows"][0]["score"]["N"]
        string myScore = "0";
        print(myRankData.score == null);
        myScore = myRankData.score == null ? myRankData.score : "0";

        if (point > int.Parse(myScore))
        {
            Param param = new Param();
            param.Add("score", point);

            Enqueue(Backend.GameData.Insert, "score", param, insertScoreBro =>
            {
                Debug.Log("InsertScore - " + insertScoreBro);
                scoreIndate = insertScoreBro.GetInDate();

                Enqueue(Backend.URank.User.UpdateUserScore, rankUuid, "score", scoreIndate, param, updateScoreBro =>
                {
                    if (updateScoreBro.IsSuccess())
                        Debug.Log("UpdateUserScore - " + updateScoreBro);
                });
            });
        }
        
    }
    #endregion


    static string dash = "-";
    [HideInInspector]
    public List<RankItem> rankTopList = new List<RankItem>();
    [HideInInspector]
    public RankItem myRankData = new RankItem();

    public RankItem EmptyRank = new RankItem
    {
        nickname = dash,
        score = dash,
        rank = dash
    };

    #region 랭킹 정보 가져오기
    public void getRank(Action<bool, string> func)
    {
        RankItem item;
        int rankCount = 10;

        var callback = Backend.URank.User.GetRankList(rankUuid, rankCount);
        if (callback.IsSuccess())
        {
            //내 랭크 가져오기
            getMyRank(func);

            //랭크 저장
            JsonData rankData = callback.GetReturnValuetoJSON()["rows"];
            rankTopList.Clear();
            for (int i = 0; i < rankData.Count; i++)
            {
                if (rankData[i] != null)
                {
                    item = new RankItem
                    {
                        nickname = rankData[i].Keys.Contains("nickname") ? rankData[i]["nickname"]["S"].ToString() : dash,
                        rank = rankData[i].Keys.Contains("rank") ? rankData[i]["rank"]["N"].ToString() : dash,
                        score = rankData[i].Keys.Contains("score") ? rankData[i]["score"]["N"].ToString() : dash
                    };
                    rankTopList.Add(item);
                }
            }

            //Rank UI
            if (SceneManager.GetActiveScene().name == "2. Lobby")
            {
                LobbyUI.GetInstance().ShowRankUI();
            }

            else
            {
                GameManager.GetInstance().rankPanel.transform.GetChild(2).gameObject.SetActive(true);
            }

            func(true, string.Empty);
        }
        else
        {
            func(false, "네트워크를 확인해주세요.");
        }

    }

    // 접속한 게이머 랭킹 가져오기 (UUID지정)
    public void getMyRank(Action<bool, string> func)
    {
        var myRankBro = Backend.URank.User.GetMyRank(rankUuid);
        if (myRankBro.IsSuccess())
        {
            JsonData rankData = myRankBro.GetReturnValuetoJSON()["rows"];
            if (rankData[0] != null)
            {

                myRankData = new RankItem
                {
                    nickname = rankData[0].Keys.Contains("nickname") ? rankData[0]["nickname"]["S"].ToString() : myNickName,
                    rank = rankData[0].Keys.Contains("rank") ? rankData[0]["rank"]["N"].ToString() : dash,
                    score = rankData[0].Keys.Contains("score") ? rankData[0]["score"]["N"].ToString() : dash
                };

                Debug.Log("!1");

            }
            else
            {
                Debug.Log("!1");
                myRankData = EmptyRank;
                myRankData.nickname = myNickName;
            }
        }
        else
        {
            myRankData = EmptyRank;
            myRankData.nickname = myNickName;
            Debug.Log("getMyRank() - " + myRankBro);
        }

    }
    #endregion


    //=================================================================================================
    #region 카드 시스템

    public void SetCard(int cardNum, string cardReward)
    {
        Param param = new Param();
        param.Add("Card " + string.Format("{0:D3}", cardNum), cardReward);
        Backend.GameData.Update("Card", new Where(), param);
    }

    #region 카드 뽑기

    public void DrawCard(bool isOne, Action<bool, string> func)
    {
        var callback = Backend.Probability.GetProbabilitys("4985", isOne ? 1 : 11);

        if (callback.IsSuccess())
        {
            for (int i = 0; i < (isOne ? 1 : 11); i++)
            {
                string data = callback.GetReturnValuetoJSON()["elements"][i]["itemID"]["S"].ToString().Split('i')[1];
                string cardNum = "";
                string cardNum2 = "";

                int n = i;

                var bro = Backend.GameData.GetMyData("Card", new Where());
                if (bro.IsSuccess())
                {
                    foreach (JsonData row in BackendReturnObject.Flatten(bro.Rows()))
                    {
                        string[] ANG = row["Card " + string.Format("{0:D3}", data)].ToString().Split('+');
                        cardNum = string.Format("{0:D3}", (int.Parse(ANG[0]) + 1));
                        cardNum2 = int.Parse(ANG[1]).ToString();
                    }

                    print("Card " + data + ", " + cardNum + "+" + cardNum2);
                    Param param = new Param();
                    param.Add("Card " + data, cardNum + "+" + cardNum2);


                    var callback2 = Backend.GameData.Update("Card", new Where(), param);
                    if (callback2.IsSuccess())
                    {
                        LobbyUI.GetInstance().drawCardUI.SetActive(true);
                        if (isOne)
                        {
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().maxNum = 1;
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[1].SetActive(false);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[0].SetActive(true);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().OneCard(int.Parse(data.ToString()));
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().SetBackEffectOnOff(false);
                            func(true, string.Empty);
                        }

                        else
                        {
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().maxNum = 11;
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[0].SetActive(false);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[1].SetActive(true);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().smallCardsNum[n] = int.Parse(data);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().SmallCards(int.Parse(data), n);
                            func(true, string.Empty);
                        }

                    }
                    else func(false, "네트워크 에러");
                }
                else print("A : " + callback);

                
            }
        }
    }
    #endregion

    #region 카드 정보 가져오기
    public void GetUserCards(int num)
    {
        Enqueue(Backend.GameData.GetMyData, "Card", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                foreach (JsonData row in BackendReturnObject.Flatten(callback.Rows()))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        string[] ANG = row["Card " + string.Format("{0:D3}", i + 1)].ToString().Split('+');

                        LobbyUI.GetInstance().setCardInfo(i, int.Parse(string.Format("{0:#,0}", ANG[0])), int.Parse(ANG[1]));
                    }
                }

            }
            else print("GetUserCards() - " + callback);
        });
    }
    #endregion

    #endregion

    //=================================================================================================

    #region 상점 요소

    public void SaveItem(int num1, int num2, int num3)
    {
        Param param = new Param();
        param.Add("Item1", num1);
        param.Add("Item2", num2);
        param.Add("Item3", num3);

        var callback = Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
        if (callback.IsSuccess())
        {
            GameManager.GetInstance().item[0] = num1;
            GameManager.GetInstance().item[1] = num2;
            GameManager.GetInstance().item[2] = num3;

        }
        else
        {

        }
    }

    #region 재화 정보 저장 (골드, 다이아, 광고)
    public void SaveGold(int money)
    {
        Param param = new Param();
        param.Add("Gold", money);

        var callback = Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
        if (callback.IsSuccess())
        {
            if (SceneManager.GetActiveScene().name == "2. Lobby")
                LobbyUI.GetInstance().gold = money;
            else if (SceneManager.GetActiveScene().name == "3. Game")
                GameManager.GetInstance().gold = money;
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "2. Lobby")
            {
                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                LobbyUI.GetInstance().errorObject.SetActive(true);
                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Application.Quit();
                });
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "3. Game")
                {
                    GameManager.GetInstance().errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                    GameManager.GetInstance().errorObject.SetActive(true);
                    GameManager.GetInstance().errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Application.Quit();
                    });
                }
            }
        }
    }

    public void SaveDiamond(int money)
    {
        Param param = new Param();
        param.Add("Diamond", money);

        var callback = Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
        if (callback.IsSuccess())
        {
            if (SceneManager.GetActiveScene().name == "2. Lobby")
                LobbyUI.GetInstance().diamond = money;
        }
        else
        {
        }
    }

    public void SaveAd(int count)
    {
        Param param = new Param();
        param.Add("AD", count);

        var callback = Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
        if (callback.IsSuccess())
        {
            LobbyUI.GetInstance().adCount = count;
        }
        else
        {
        }
    }

    public void SaveAdReset(int num)
    {
        Param param = new Param();
        param.Add("ADReset", num);

        var callback = Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);

        if (callback.IsSuccess())
        {
            LobbyUI.GetInstance().adReset = num;
        }
        else
        {

        }
    }
    #endregion

    public void GiveMoeny(int num)
    {
        Enqueue(Backend.GameData.GetMyData, "User", new Where(), bro => 
        {
            if (bro.IsSuccess())
            {
                var money = bro.GetReturnValuetoJSON()["rows"][0]["Gold"]["N"].ToString();

                Param param = new Param();

                param.Add("Gold", int.Parse(money) + num);

                Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
            }
        });
    }

    #region 카드 구매
    public void BuyCard(int num, bool isGold, Action<bool, string> func)
    {
        int money = isGold ? LobbyUI.GetInstance().gold : LobbyUI.GetInstance().diamond;

        if(money >= num)
        {
            Param param = new Param();
            param.Add(isGold ? "Gold" : "Diamond", money - num);

            Enqueue(Backend.GameData.UpdateV2, "User", userIndate, Backend.UserInDate, param, callback =>
            {
                if (callback.IsSuccess())
                {
                    if (isGold)
                    {
                        LobbyUI.GetInstance().gold = money - num;
                        SoundManager.Instance.PlayEffect(4);
                        if (num == 200) DrawCard(true, func);
                        else DrawCard(false, func);
                    }
                    else
                    {
                        LobbyUI.GetInstance().diamond = money - num;
                        func(true, string.Empty);
                        if (num == 20) DrawCard(true, func);
                        else DrawCard(false, func);
                    }
                }
                else
                {
                    func(false, string.Format(BackendError, callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                }
            });
        }
        else
        {
            if (isGold)
                func(false, PlayerPrefs.GetInt("LangIndex") == 0 ? "골드가 부족합니다." : "Not enough gold.");
            else
                func(false, "루비가 부족합니다.");
        }
    }
    #endregion

    #region 다이아 획득
    public void GetGold(int num)
    {
        var bro = Backend.GameData.GetMyData("User", new Where());
            if (bro.IsSuccess())
            {
                var money = bro.GetReturnValuetoJSON(
                    )["rows"][0]["Gold"]["N"].ToString();

                Param param = new Param();
                param.Add("Gold", int.Parse(money) + num);

                Backend.GameData.Update("User", new Where(), param);
            }
    }
    #endregion

    #endregion

    //=================================================================================================

    /* 밸런스 시트*/
    public class Balance
    {
        public float minScore;
        public float maxScore;
        public float turnMinTime;
        public float turnMaxTime;
        public float rotateMinTime;
        public float rotateMaxTime;
    }

    //뒤끝 레벨 정보
    public List<Balance> balanceSheet = new List<Balance>();

    [Header("<Backend BALANCE CODE >")]
    public string balanceSheetCode = "";

    #region 뒤끝 퀴즈 시트 불러오기
    public void GetBalanceSheet(Action<bool, string> func)
    {
        Enqueue(Backend.Chart.GetChartContents, balanceSheetCode, callback =>
        {
            if (callback.IsSuccess())
            {
                JsonData json = callback.FlattenRows();

                balanceSheet = new List<Balance>();
                for (int i = 0; i < json.Count; i++)
                {
                    Balance ls = new Balance();
                    ls.minScore = float.Parse(json[i]["minScore"].ToString());
                    ls.maxScore = float.Parse(json[i]["maxScore"].ToString());
                    ls.turnMinTime = float.Parse(json[i]["turnMinTime"].ToString());
                    ls.turnMaxTime = float.Parse(json[i]["turnMaxTime"].ToString());
                    ls.rotateMinTime = float.Parse(json[i]["rotateMinTime"].ToString());
                    ls.rotateMaxTime = float.Parse(json[i]["rotateMaxTime"].ToString());

                    balanceSheet.Add(ls);
                }
                func(true, string.Empty);
            }
            else
            {
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            }
        });
    }
    #endregion

    /* 레벨 시트*/
    public class Quiz
    {
        public string quiz;
        public string answer;
        public string error1;
        public string error2;
        public string error3;
    }

    //뒤끝 레벨 정보
    public List<Quiz> quizSheet = new List<Quiz>();

    [Header("<Backend SHEET CODE >")]
    public string quizSheetCode = "";
    public string quizSheetCode2 = "";

    #region 뒤끝 퀴즈 시트 불러오기
    public void GetQuizSheet(Action<bool, string> func)
    {
        Enqueue(Backend.Chart.GetChartContents, quizSheetCode, callback =>
        {
            if (callback.IsSuccess())
            {
                JsonData json = callback.FlattenRows();

                quizSheet = new List<Quiz>();
                for (int i = 0; i < json.Count; i++)
                {
                    Quiz ls = new Quiz();
                    ls.quiz = json[i]["Quiz"].ToString();
                    ls.answer = json[i]["Answer"].ToString();
                    ls.error1 = json[i]["Error1"].ToString();
                    ls.error2 = json[i]["Error2"].ToString();
                    ls.error3 = json[i]["Error3"].ToString();

                    quizSheet.Add(ls);
                }
                func(true, string.Empty);
            }
            else
            {
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            }
        });
    }

    public void GetQuizSheet2(Action<bool, string> func)
    {
        Enqueue(Backend.Chart.GetChartContents, quizSheetCode2, callback =>
        {
            if (callback.IsSuccess())
            {
                JsonData json = callback.FlattenRows();

                quizSheet = new List<Quiz>();
                for (int i = 0; i < json.Count; i++)
                {
                    Quiz ls = new Quiz();
                    ls.quiz = json[i]["Quiz"].ToString();
                    ls.answer = json[i]["Answer"].ToString();
                    ls.error1 = json[i]["Error1"].ToString();
                    ls.error2 = json[i]["Error2"].ToString();
                    ls.error3 = json[i]["Error3"].ToString();

                    quizSheet.Add(ls);
                }
                func(true, string.Empty);
            }
            else
            {
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            }
        });
    }
    #endregion

    /* 번역 시트*/
    public class Langauge
    {
        public string kor;
        public string eng;
    }

    //뒤끝 레벨 정보
    public List<Langauge> langaugeSheet = new List<Langauge>();

    [Header("<Backend Langauge CODE >")]
    public string langaugeSheetCode = "";

    #region 뒤끝 번역 시트 불러오기
    public void GetLangaugeSheet()
    {
        Debug.LogError("작동12313");
        Enqueue(Backend.Chart.GetChartContents, langaugeSheetCode, callback =>
        {
            if (callback.IsSuccess())
            {
                JsonData json = callback.FlattenRows();

                langaugeSheet = new List<Langauge>();
                for (int i = 0; i < json.Count; i++)
                {
                    Langauge ls = new Langauge();
                    ls.kor = json[i]["kor"].ToString();
                    ls.eng = json[i]["eng"].ToString();

                    langaugeSheet.Add(ls);
                }

                Debug.LogError("작동");
                Debug.LogError(langaugeSheet[11].eng);

                LobbyUI.GetInstance().SuccessLogin(loginSuccessFunc);
            }
        });
    }
    #endregion

    #region 신규 유저 체크
    public void InitializeCheck()
    {

        Enqueue(Backend.GameData.Get, "User", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                JsonData userData = callback.GetReturnValuetoJSON()["rows"];

                //신규 유저가 아닐경우 데이터 가져옴
                if (userData.Count != 0)
                {
                    Debug.LogError("기존 유저 확인");
                    userIndate = userData[0]["inDate"]["S"].ToString();

                    GetLangaugeSheet();

                }

                //신규 유저일 경우 새롭게 데이터 만듦
                else
                {
                    Debug.LogError("신규 유저 확인");
                    CreateInitialSetting();
                }
            }
        });
        

    }

    public void CreateInitialSetting()
    {
        Param param = new Param();

        param.Add("Gold", 0);
        param.Add("Diamond", 0);

        //테스트
        param.Add("Item1", 0);
        param.Add("Item2", 0);
        param.Add("Item3", 0);

        param.Add("AD", 0);
        param.Add("ADReset", 0);

        Enqueue(Backend.GameData.Insert, "User", param, callback =>
        {
            //신규 유저 데이터 만들었을 때
            if (callback.IsSuccess())
            {
                Enqueue(Backend.GameData.Get, "User", new Where(), callback2 =>
                {
                    if (callback2.IsSuccess())
                    {
                        Debug.LogError("신규 유저 데이터 만들기 성공");
                        JsonData userData = callback2.GetReturnValuetoJSON()["rows"];
                        userIndate = userData[0]["inDate"]["S"].ToString();

                        Param param2 = new Param();
                        for (int i = 0; i < 100; i++)
                            param2.Add("Card " + string.Format("{0:D3}", (i + 1)), "000+0");

                        Enqueue(Backend.GameData.Insert, "Card", param2, callback3 =>
                        {
                            if (callback3.IsSuccess())
                            {
                                Debug.LogError("카드 정보 생성");

                                GetLangaugeSheet();
                            }

                        });
                        
                    }
                });
                
            }
        });
        

        
    }
    #endregion

    public string isGuest()
    {
        return Backend.BMember.GetGuestID();
    }

    #region 새로고침
    public void RefreshInfo()
    {
        var callback = Backend.GameData.Get("User", new Where());


        if (callback.IsSuccess())
        {
            if (SceneManager.GetActiveScene().name == "2. Lobby")
            {
                LobbyUI.GetInstance().gold = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Gold"]["N"].ToString());
                LobbyUI.GetInstance().diamond = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Diamond"]["N"].ToString());
                LobbyUI.GetInstance().adCount = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["AD"]["N"].ToString());
                LobbyUI.GetInstance().adReset = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["ADReset"]["N"].ToString());

                //switch (System.DateTime.Now.DayOfWeek)
                //{
                //    case System.DayOfWeek.Monday:
                //        LobbyUI.GetInstance().currentDay = 0;
                //        break;
                //    case System.DayOfWeek.Tuesday:
                //        LobbyUI.GetInstance().currentDay = 1;
                //        break;
                //    case System.DayOfWeek.Wednesday:
                //        LobbyUI.GetInstance().currentDay = 2;
                //        break;
                //    case System.DayOfWeek.Thursday:
                //        LobbyUI.GetInstance().currentDay = 3;
                //        break;
                //    case System.DayOfWeek.Friday:
                //        LobbyUI.GetInstance().currentDay = 4;
                //        break;
                //    case System.DayOfWeek.Saturday:
                //        LobbyUI.GetInstance().currentDay = 5;
                //        break;
                //    case System.DayOfWeek.Sunday:
                //        LobbyUI.GetInstance().currentDay = 6;
                //        break;
                //}

                //if(LobbyUI.GetInstance().currentDay != LobbyUI.GetInstance().adReset)
                //{
                //    Param param = new Param();
                //    param.Add("AD", 0);
                //    param.Add("ADReset", LobbyUI.GetInstance().currentDay);
                //    Enqueue(Backend.GameData.UpdateV2, "User", userIndate, Backend.UserInDate, param, callback =>
                //    {
                //        if (callback.IsSuccess())
                //        {
                //            LobbyUI.GetInstance().adCount = 0;
                //            LobbyUI.GetInstance().adReset = LobbyUI.GetInstance().currentDay;
                //            print("성공");
                //        }
                //        else
                //        {
                //        }
                //    });
                //}
            }

            else if (SceneManager.GetActiveScene().name == "3. Game")
            {
                GameManager.GetInstance().gold = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Gold"]["N"].ToString());
                GameManager.GetInstance().diamond = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Diamond"]["N"].ToString());


                //부활
                if (int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Item1"]["N"].ToString()) > 0)
                {
                    GameManager.GetInstance().item[0] = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Item1"]["N"].ToString());
                    GameManager.GetInstance().buyButton[0].GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "무료" : "Free";
                }

                if (int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Item2"]["N"].ToString()) > 0)
                {
                    GameManager.GetInstance().item[1] = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Item2"]["N"].ToString());
                    GameManager.GetInstance().buyButton[1].GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "무료" : "Free";
                }

                if (int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Item3"]["N"].ToString()) > 0)
                {
                    GameManager.GetInstance().item[2] = int.Parse(callback.GetReturnValuetoJSON()["rows"][0]["Item3"]["N"].ToString());
                    GameManager.GetInstance().buyButton[2].GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "무료" : "Free";
                }
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "2. Lobby")
            {
                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = "네트워크를 확인해주세요.";
                LobbyUI.GetInstance().errorObject.SetActive(true);
                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    LobbyUI.GetInstance().QuitGame();
                });
            }
        }
    }
    #endregion

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
        // 모든 씬에서 유지
        DontDestroyOnLoad(this.gameObject);

        Initialize(); //서버 초기화
    }

    public static BackendServerManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("BackendServerManager 인스턴스가 존재하지 않습니다.");
            return null;
        }

        return instance;
    }

    void Start()
    {
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            Debug.LogError("IOS CHECK");
            var deserializer = new PayloadDeserializer();
            appleAuthManager = new AppleAuthManager(deserializer);
        }
    }

    void Update()
    {
        //비동기함수 풀링
        Backend.AsyncPoll();

        if (this.appleAuthManager != null)
        {
            this.appleAuthManager.Update();
        }
    }

}