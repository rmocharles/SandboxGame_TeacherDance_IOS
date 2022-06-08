using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeQuestion : MonoBehaviour
{
    public void MakeQuestion_(string gameName)
    {
        switch (gameName)
        {
            case "Caculate":
                MakeCaculator();
                break;

            case "ColorCheck":
                break;
        }
    }

    void MakeCaculator()
    {
        int randomNum1 = Random.Range(0,10);
        int randomNum2 = Random.Range(0,10);

        float answerNum = 0;

        int chooseCal = Random.Range(0, 4);

        switch (chooseCal)
        {
            case 0:
                answerNum = randomNum1 + randomNum2;
                break;

            case 1:
                answerNum = randomNum1 - randomNum2;
                break;

            case 2:
                answerNum = randomNum1 * randomNum2;
                break;
        }

        Debug.Log(randomNum1);
        Debug.Log(randomNum2);
        Debug.Log(answerNum);
    }
}
