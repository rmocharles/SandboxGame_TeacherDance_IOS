using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Language
{
    public string languageName, langLocalize; //
    public List<string> value = new List<string>(); // 언어의 Text value 들을 담아준다.
}

public class LanguageParse : MonoBehaviour
{
    public static LanguageParse instance;
    public bool isLoaded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        else Destroy(this);

        GetLanguage();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isLoaded)
        {
            LanguageChanger.instance_.LocalizeChanged();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    const string googleURL = "https://docs.google.com/spreadsheets/d/1EqPPDrsSqoxyCojzLEEi8BAV1cfIv1o9X15DdUpqT9Q/export?format=tsv";
    public event System.Action LocalizeChnaged = () => { };
    public event System.Action LocalizeSettingChanged = () => { };

    // 언어가 바뀌는 것을 알려주기 위해 Action 선언

    public int curLangIndex; // 현재 언어의 인덱스
    public List<Language> Languages; // 언어 데이터 클래스의 리스트

    // InitLang 함수에서는 저장해놓은 언어 인덱스값이 있다면 가져오고, 없다면 기본언어의 인덱스
    void InitLanguage()
    {
        int languageIndex = PlayerPrefs.GetInt("LangIndex", -1);
        int systemIndex = Languages.FindIndex(x => x.languageName.ToLower() == Application.systemLanguage.ToString().ToLower());
        if (systemIndex == -1) systemIndex = 0;
        int index = languageIndex == -1 ? systemIndex : languageIndex;

        SetLangIndex(index); //값을 가져온 뒤 SetLangIndex 의 매개변수로 넣어준다.
    }

    public void SetLangIndex(int index)
    {
        curLangIndex = index; // initLanguage 에서 구한 언어의 인덱스 값을 curLangIndex 에 넣어줌
        PlayerPrefs.SetInt("LangIndex", curLangIndex); // 저장
        LocalizeChnaged(); // 텍스트들 현재 언어로 변경
        LocalizeSettingChanged(); // 드랍다운의 value 변경
    }

    [ContextMenu("언어 가져오기")] // 게임중이 아닐때에도 실행가능
    void GetLanguage()
    {
        StartCoroutine(GetLangCo());
    }

    IEnumerator GetLangCo()
    {
        UnityWebRequest www = UnityWebRequest.Get(googleURL);
        yield return www.SendWebRequest();
        SetLangList(www.downloadHandler.text);
        InitLanguage();
        if (!isLoaded)
        {
            LanguageChanger.instance_.LocalizeChanged();
            isLoaded = true;
        }
        //LanguageChanger.instance_.LocalizeChanged();
    }

    void SetLangList(string tsv)
    {
        // 이차원 배열 생성

        string[] row = tsv.Split('\n'); // 스페이스를 기준을 행 분류
        int rowSize = row.Length;
        int columnSize = row[0].Split('\t').Length; // 탭을 기준으로 열 분류
        string[,] Sentence = new string[rowSize, columnSize];

        // 이차원 배열에 데이터 넣어주기
        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split('\t');
            for (int j = 0; j < columnSize; j++)
            {
                Sentence[i, j] = column[j];
            }
        }

        Languages = new List<Language>(); // 새로운 Language 개체 생성

        for (int i = 1; i < columnSize; i++)
        {
            Language language = new Language();
            language.languageName = Sentence[0, i]; // 언어의 영어 이름
            language.langLocalize = Sentence[1, i]; // 언어의 현지화 이름

            for (int j = 2; j < rowSize; j++)
            {
                language.value.Add(Sentence[j, i]);
            }

            Languages.Add(language); //  언어들이 담겨있는 리스트에 추가
        }
    }
}
