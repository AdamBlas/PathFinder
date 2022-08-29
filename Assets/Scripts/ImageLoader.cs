using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;

public class ImageLoader : MonoBehaviour
{
    public string FreeNodeChars { get => freeNodeInputField.text; set => freeNodeInputField.text = value; }
    public string ObstacleChars { get => obstacleInputField.text; set => obstacleInputField.text = value; }
    public bool UnknownMeansFree { get => unknowToggle.isOn; set => unknowToggle.isOn = value; }

    public InputField freeNodeInputField;
    public InputField obstacleInputField;
    public Toggle unknowToggle;

    string lastPath;

    public void OpenMapFile()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Map Files", "map"),
            new ExtensionFilter("Image Files", "png", "bmp", "tiff"),
            new ExtensionFilter("Map and Image Files", "map", "png", "bmp", "tiff"),
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open map", "", extensions, false);

        if (paths.Length == 0)
            return;

        string path = paths[0];
        string extension = path.Substring(path.LastIndexOf('.') + 1);

        Map map = null;
        if (extension == "map")
        {
            lastPath = path;
            map = MapFromFile(path);
        }

        ImageDisplayer.LoadImage(map);
    }

    Map MapFromFile(string path)
    {
        // Save data to PNG exporter
        string[] directories = path.Split(Path.DirectorySeparatorChar);
        SaveMapToImgFile.gameName = directories[directories.Length - 2];
        SaveMapToImgFile.mapName = directories[directories.Length - 1].Substring(directories[directories.Length - 1].LastIndexOf('.'));

        string[] mapText = File.ReadAllLines(path);

        int height = int.Parse(mapText[1].Split(' ')[1]);
        int width = int.Parse(mapText[2].Split(' ')[1]);

        Map map = new Map(width, height);

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                char symbol = mapText[3 + height - y][x];

                if (FreeNodeChars.IndexOf(symbol) >= 0)
                {
                    map[x, y] = Map.Node.Free;
                }
                else if (ObstacleChars.IndexOf(symbol) >= 0)
                {
                    map[x, y] = Map.Node.Obstacle;
                }
                else
                {
                    Debug.LogWarning("Unknown symbol: " + symbol);
                    map[x, y] = UnknownMeansFree ? Map.Node.Free : Map.Node.Obstacle;
                }
            }
        }

        return map;
    }
    public void ReloadMap()
    {
        Map map = MapFromFile(lastPath);

        Vector2Int startCoords = Paint.StartCoordinates.Value;
        Vector2Int endCoords = Paint.EndCoordinates.Value;

        ImageDisplayer.LoadImage(map, true);

        Paint.StartCoordinates = startCoords;
        Paint.EndCoordinates = endCoords;
    }
}
