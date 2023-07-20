using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public static PopUp instance;
    private void Awake()
    {
        instance = this;
        transform.SetParent(null);
    }

    public TMPro.TMP_Text text;
    public CanvasGroup textGroup;
    float alpha = 1f;
    float fadeSpeed = 10f;

    public static void Show(string message, float time = 3, float fadeSpeed = 10)
    {
        instance.text.text = message;
        instance.CancelInvoke();
        instance.Invoke(nameof(Cancel), time);
        instance.alpha = 1f;
        instance.fadeSpeed = fadeSpeed;
        instance.textGroup.alpha = 1f;
        //Debug.Log(message);
    }

    private void Cancel()
    {
        //text.text = "";
        instance.alpha = 0f;
    }

    private void Update()
    {
        textGroup.alpha = Mathf.Lerp(textGroup.alpha, alpha, Time.deltaTime * fadeSpeed);
    }
}
