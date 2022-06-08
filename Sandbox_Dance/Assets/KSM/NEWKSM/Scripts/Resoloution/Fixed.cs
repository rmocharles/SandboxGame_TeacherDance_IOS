using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fixed : MonoBehaviour
{
    public int rw, ry;
    public float x;
    public float y;

    [HideInInspector]
    public float value;

    private static Fixed instance;
    public static Fixed GetInstance()
    {
        if (instance == null) return null;

        return instance;
    }

    private float GetScale(int width, int height, CanvasScaler canvasScaler)
    {
        var scalerReferenceResolution = canvasScaler.referenceResolution;
        var widthScale = width / scalerReferenceResolution.x;
        var heightScale = height / scalerReferenceResolution.y;

        switch (canvasScaler.screenMatchMode)
        {
            case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                var matchWidthOrHeight = canvasScaler.matchWidthOrHeight;

                return Mathf.Pow(widthScale, 1f - matchWidthOrHeight) *
                      Mathf.Pow(heightScale, matchWidthOrHeight);

            case CanvasScaler.ScreenMatchMode.Expand:
                return Mathf.Min(widthScale, heightScale);

            case CanvasScaler.ScreenMatchMode.Shrink:
                return Mathf.Max(widthScale, heightScale);
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

    void Awake()
    {
        if (!instance) instance = this;

        GameObject.Find("Canvas").GetComponent<CanvasScaler>().referenceResolution = new Vector2(Screen.width, Screen.height);

        x = GameObject.Find("Canvas").GetComponent<CanvasScaler>().referenceResolution.x / rw;
        y = GameObject.Find("Canvas").GetComponent<CanvasScaler>().referenceResolution.y / ry;

        value = GetScale(rw, ry, GameObject.Find("Canvas").GetComponent<CanvasScaler>());
    }
}
