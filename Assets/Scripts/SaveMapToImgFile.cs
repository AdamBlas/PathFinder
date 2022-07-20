using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveMapToImgFile : MonoBehaviour
{
    public static string gameName;
    public static string mapName;
    public Image sourceImage;


    public void ExportToPngFile()
    {
        byte[] pngData = sourceImage.sprite.texture.EncodeToPNG();
        string fileName = gameName + "_" + mapName + "_" + System.DateTime.Now.ToString("yy-MM-dd--HH-mm-ss") + ".png";
        System.IO.File.WriteAllBytes(fileName, pngData);
        System.Diagnostics.Process.Start(fileName);
    }

}
