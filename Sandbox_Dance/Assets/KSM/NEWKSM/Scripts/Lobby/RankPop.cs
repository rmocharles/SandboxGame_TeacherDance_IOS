using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RankPop : MonoBehaviour
{
	[System.Serializable]
	public struct RankRow
	{
		public Text nickNameText;
		public Text scoreText;
		public Text rankText;
	}
	private RankRow[] topRankRows = new RankRow[10];
	public GameObject rankList;
	public RankRow myRankRow;
	private Color32 topRankcolor = new Color32(255, 236, 109, 255);
	private Color32 normalRankcolor = new Color32(212, 244, 248, 255);

	void Start()
    {
		for (int i = 0; i < 10; i++)
		{
			GameObject tmp = rankList.transform.GetChild(i).gameObject;
			topRankRows[i].rankText = tmp.transform.GetComponentsInChildren<Text>()[0];
			topRankRows[i].nickNameText = tmp.transform.GetComponentsInChildren<Text>()[1];
			topRankRows[i].scoreText = tmp.transform.GetComponentsInChildren<Text>()[2];
		}
        SetRank();
    }
	void OnEnable()
    {
		SetRank();
    }

	public void SetRank()
    {
		try
		{
			RankItem myRankData = BackendServerManager.GetInstance().myRankData;
			myRankRow.nickNameText.text = myRankData.nickname;
			myRankRow.scoreText.text = myRankData.score;
			myRankRow.rankText.text = myRankData.rank;
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}

		finally
		{
			List<RankItem> rankTop10DataList = BackendServerManager.GetInstance().rankTopList;
			for (int i = 0; i < rankTop10DataList.Count; i++)
			{
				topRankRows[i].nickNameText.text = rankTop10DataList[i].nickname;
				topRankRows[i].scoreText.text = rankTop10DataList[i].score;
				topRankRows[i].rankText.text = rankTop10DataList[i].rank;

				topRankRows[i].rankText.GetComponentInParent<Image>().color = int.Parse(rankTop10DataList[i].rank) < 4 ? (Color)topRankcolor : (Color)normalRankcolor;
			}
		}
    }
}
