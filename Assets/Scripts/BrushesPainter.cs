using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushesPainter : MonoBehaviour
{
    public Button freeSpaceButton;
    public Button obstacleButton;
    public Button startButton;
    public Button endButton;

    public void Start()
    {
        freeSpaceButton.image.color = ImageDisplayer.Instance.freeCellColor;
        obstacleButton.image.color = ImageDisplayer.Instance.obstacleColor;
        startButton.image.color = ImageDisplayer.Instance.startColor;
        endButton.image.color = ImageDisplayer.Instance.endColor;

        freeSpaceButton.GetComponentInChildren<Text>().color =
            ImageDisplayer.Instance.freeCellColor.grayscale < 0.5f ? Color.white : Color.black;

        obstacleButton.GetComponentInChildren<Text>().color =
            ImageDisplayer.Instance.obstacleColor.grayscale < 0.5f ? Color.white : Color.black;

        startButton.GetComponentInChildren<Text>().color = 
            ImageDisplayer.Instance.startColor.grayscale < 0.5f ? Color.white : Color.black;

        endButton.GetComponentInChildren<Text>().color =
            ImageDisplayer.Instance.endColor.grayscale < 0.5f ? Color.white : Color.black;
    }
}
