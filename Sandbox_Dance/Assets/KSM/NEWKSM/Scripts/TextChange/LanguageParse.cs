using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Language
{
    public string languageName, langLocalize; //
    public List<string> value = new List<string>(); // ����� Text value ���� ����ش�.
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

    // �� �ٲ�� ���� �˷��ֱ� ���� Action ����

    public int curLangIndex; // ���� ����� �ε���
    public List<Language> Languages; // ��� ������ Ŭ������ ����Ʈ

    // InitLang �Լ������� �����س��� ��� �ε������� �ִٸ� ��������, ���ٸ� �⺻����� �ε���
    void InitLanguage()
    {
        int languageIndex = PlayerPrefs.GetInt("LangIndex", -1);
        int systemIndex = Languages.FindIndex(x => x.languageName.ToLower() == Application.systemLanguage.ToString().ToLower());
        if (systemIndex == -1) systemIndex = 0;
        int index = languageIndex == -1 ? systemIndex : languageIndex;

        SetLangIndex(index); //���� ������ �� SetLangIndex �� �Ű������� �־��ش�.
    }

    public void SetLangIndex(int index)
    {
        curLangIndex = index; // initLanguage ���� ���� ����� �ε��� ���� curLangIndex �� �־���
        PlayerPrefs.SetInt("LangIndex", curLangIndex); // ����
        LocalizeChnaged(); // �ؽ�Ʈ�� ���� ���� ����
        LocalizeSettingChanged(); // ����ٿ��� value ����
    }

    [ContextMenu("��� ��������")] // �������� �ƴҶ����� ���డ��
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
        // ������ �迭 ����

        string[] row = tsv.Split('\n'); // �����̽��� ������ �� �з�
        int rowSize = row.Length;
        int columnSize = row[0].Split('\t').Length; // ���� �������� �� �з�
        string[,] Sentence = new string[rowSize, columnSize];

        // ������ �迭�� ������ �־��ֱ�
        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split('\t');
            for (int j = 0; j < columnSize; j++)
            {
                Sentence[i, j] = column[j];
            }
        }

        Languages = new List<Language>(); // ���ο� Language ��ü ����

        for (int i = 1; i < columnSize; i++)
        {
            Language language = new Language();
            language.languageName = Sentence[0, i]; // ����� ���� �̸�
            language.langLocalize = Sentence[1, i]; // ����� ����ȭ �̸�

            for (int j = 2; j < rowSize; j++)
            {
                language.value.Add(Sentence[j, i]);
            }

            Languages.Add(language); //  ������ ����ִ� ����Ʈ�� �߰�
        }
    }
}
