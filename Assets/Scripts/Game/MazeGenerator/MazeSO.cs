using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MazeSO : ScriptableObject
{
    public int width, height;
    public int[,] layout;
    //public GameObject tileObject;

    [Multiline]
    const char tileChar = 'O';
    public string layoutDisplay;

    public void Init(int width, int height, Transform[,] layout)
    {
        this.width = width;
        this.height = height;
        this.layout = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (layout[x, y] != null)
                {
                    this.layout[x, y] = 1;
                    layoutDisplay += tileChar;
                }
                else
                {
                    this.layout[x, y] = 0;
                    layoutDisplay += ' ';
                }
                //Debug.Log(layoutDisplay);
            }
            layoutDisplay += '\n';
        }
    }
}