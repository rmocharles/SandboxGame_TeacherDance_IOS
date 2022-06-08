using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class CardChanger : MonoBehaviour
{
    public GameObject[] oneOrMany; // 0 : 하나짜리, 1 : 여러개

    // 한개짜리 관리
    public SkeletonGraphic []OneImage; // 한개의 이미지 관리
    public GameObject SingleCard; // 한개의 카드

    // 11연 뽑 관련
    public int[] smallCardsNum; // 카드 개별 확인
    public GameObject[] smallCardsImage;
    public GameObject[] smallCardsEffects;
    public Sprite[] smallCardsOutLine; // 작은 카드용 겉면

    // 남은 확인해야할 카드
    public int maxNum = 1; // 봐야할 카드들
    public int cardStar = 1; // 카드의 등급

    int nowCardIndex;

    // 스킵 버튼
    [SerializeField] Button SkipButton;

    private Coroutine exitCoroutine = null;
    private Coroutine exitCoroutine2 = null;

    public void Complete(TrackEntry te)
    {
        SoundManager.Instance.PlayEffect(1);
        if (cardStar > 3) { SoundManager.Instance.Vibrate(); }
    }
    public SkeletonDataAsset[] cardEffect;
    public void OneCard(int cardNum) // 하나 뽑았을 때 실행
    {
        SingleCard.GetComponent<PlayableDirector>().Play();
        Debug.LogWarning(SingleCard.name);
        maxNum -= 1;
        oneOrMany[0].SetActive(true); // 하나 까기 켜주기
        CheckCardStar(cardNum);
        SetBackEffect(SingleCard);
        OneImage[0].GetComponent<SkeletonGraphic>().skeletonDataAsset = PlayerPrefs.GetInt("LangIndex") == 0 ? cardEffect[0] : cardEffect[1];
        OneImage[0].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "card_2", false);
        OneImage[0].GetComponent<SkeletonGraphic>().Initialize(true);
        OneImage[1].GetComponent<SkeletonGraphic>().skeletonDataAsset = PlayerPrefs.GetInt("LangIndex") == 0 ? cardEffect[0] : cardEffect[1];
        OneImage[1].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "card_2", false);
        OneImage[1].GetComponent<SkeletonGraphic>().Initialize(true);
        OneImage[0].GetComponentInChildren<Image>().sprite = LobbyUI.GetInstance().CardImage[cardNum];

        foreach (SkeletonGraphic cardSet in OneImage)
        {
            Debug.LogError(cardStar);
            cardSet.AnimationState.SetAnimation(0, "card_" + cardStar, false);
        }

        if (oneOrMany[1].activeSelf == true)
        {
            SkipButton.gameObject.SetActive(false);

            exitCoroutine2 = StartCoroutine(ActiveFalseOneCard(3.5f));
        }

        if (maxNum == 0)
        {
            exitCoroutine = StartCoroutine(ActiveFalse(3.5f));
        }
    }

    public void SmallCards(int cardNum, int cardIndex) // 여러개 뽑았을 떄 실행, cardNum 나올 카드 이미지 번호, cardIndex 이 카드가 몇번째 카드인지
    {
        CheckCardStar(smallCardsNum[cardIndex]);
        SetBackEffect(smallCardsEffects[cardIndex]);
        SkipButton.gameObject.SetActive(true);
        smallCardsImage[cardIndex].GetComponentsInChildren<Image>()[0].sprite = LobbyUI.GetInstance().CardImage[cardNum];
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => OneCard(cardNum));
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => SetCardIndex(cardIndex));
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ActiveFalseSmallBack(cardIndex, 3.5f)));
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => SingleCard.GetComponent<PlayableDirector>().Play());
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => SoundManager.Instance.PlayEffect(3));
    }

    void SetCardIndex(int cardIndex)
    {
        nowCardIndex = cardIndex;
        smallCardsImage[cardIndex].GetComponent<Button>().interactable = false;
    }

    void SmallCardsBackSet()
    {
        for(int i = 0; i < smallCardsImage.Length; i++)
        {
            CheckCardStar(smallCardsNum[i]);
            SetBackEffect(smallCardsImage[i]);
        }
    }

    public void Skip()
    {
        SingleCard.GetComponent<PlayableDirector>().Stop();
        SoundManager.Instance.PlayEffect(2);
        for (int i = 0; i < smallCardsImage.Length; i++)
        {
            cardStar = smallCardsNum[i];
            CheckCardStar(cardStar);
            smallCardsImage[i].GetComponentsInChildren<Image>()[2].enabled = false;
            smallCardsImage[i].GetComponent<Button>().interactable = false;
            smallCardsImage[i].GetComponentsInChildren<Image>()[1].sprite = smallCardsOutLine[cardStar];
        }
        SkipButton.gameObject.SetActive(false);
        exitCoroutine = StartCoroutine(ActiveFalse(2f));
    }

    public void SkipOne()
    {
        SingleCard.GetComponent<PlayableDirector>().Stop();
        SoundManager.Instance.PlayEffect(2);
        foreach (SkeletonGraphic cardSet in OneImage)
        {
            cardSet.AnimationState.SetAnimation(0, "card_" + cardStar + "-1", false);
        }

        if (oneOrMany[1].activeSelf == false)
        {
            if (exitCoroutine != null)
            {
                StopCoroutine(exitCoroutine);
            }
            exitCoroutine = StartCoroutine(ActiveFalse(0.5f));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ActiveFalseSmallBack(nowCardIndex, 0.5f));


            if (exitCoroutine2 != null)
            {
                StopCoroutine(exitCoroutine2);
            }
            exitCoroutine2 = StartCoroutine(ActiveFalseOneCard(0.5f));
        }
    }

    public void CheckCardStar(int cardNum)
    {
        if (cardNum > 95) cardStar = 5;
        else if (cardNum > 74) cardStar = 4;
        else if (cardNum > 54) cardStar = 3;
        else if (cardNum > 29) cardStar = 2;
        else cardStar = 1;
    }

    public void SetBackEffectOnOff(bool isOn)
    {
        foreach (GameObject effecOn in smallCardsEffects)
        {
            if (isOn)
            {
                effecOn.SetActive(true);
            }

            else
            {
                effecOn.SetActive(false);
            }
        }
    }

    public void SetBackEffect(GameObject targetCard)
    {
        SetBackEffectOnOff(true);

        for (int i = 0; i < 4; i++)
        {
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[i].Stop();
        }

        if (cardStar == 3 || cardStar == 4)
        {
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[3].Play();
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[2].Play();
        }

        if(cardStar == 5)
        {
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[0].Play();
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[1].Play();
        }
    }

    public void PauseButton()
    {
        Time.timeScale = 0;
    }

    #region 카드 꺼주는 코드들
    IEnumerator ActiveFalseSmallBack(int cardIndex, float time)
    {
        yield return new WaitForSeconds(time);
        smallCardsImage[cardIndex].GetComponentsInChildren<Image>()[1].sprite = smallCardsOutLine[cardStar];
        smallCardsImage[cardIndex].GetComponentsInChildren<Image>()[2].enabled = false;
    }

    IEnumerator ActiveFalseOneCard(float reset)
    {
        yield return new WaitForSeconds(reset);

        oneOrMany[0].SetActive(false);
        SkipButton.gameObject.SetActive(true);
        SingleCard.GetComponent<Button>().interactable = true;
    }

    IEnumerator ActiveFalse(float reset) // 카드 상태 리셋
    {
        yield return new WaitForSeconds(reset);
        this.gameObject.SetActive(false);

        foreach (GameObject smallCardBack in smallCardsImage)
        {
            smallCardBack.GetComponentsInChildren<Image>()[2].enabled = true;
            smallCardBack.GetComponent<Button>().interactable = true;
            smallCardBack.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        SingleCard.GetComponent<Button>().interactable = true;

    }
    #endregion
}
