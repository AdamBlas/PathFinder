using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutputMessageManager : MonoBehaviour
{
    public static OutputMessageManager Instance;

    public Text message1;
    public Text message2;
    public Text message3;
    public Color okColor;
    public Color errorColor;

    void Awake()
    {
        Instance = this;
    }

    public static void SetMessage(string message, bool isError = false, int column = 1)
    {
        Text text = column switch
        {
            1 => Instance.message1,
            2 => Instance.message2,
            3 => Instance.message3,
            _ => throw new System.IndexOutOfRangeException(),
        };
        text.text = message;
        text.color = isError ? Instance.errorColor : Instance.okColor;
    }
}
