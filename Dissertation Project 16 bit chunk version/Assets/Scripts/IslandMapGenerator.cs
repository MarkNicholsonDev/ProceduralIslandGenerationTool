using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Generating a mask to be applied onto height map of the map to generate islands
public class IslandMapGenerator : MonoBehaviour {
    public static float[,] GenerateIslandMap(int width, int height, float falloffStart, float falloffEnd)
    {
        float[,] mask = new float[width, height];

        //Position in the island mask:
        float x;
        float y;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                x = (float)j / width * 2 - 1;
                y = (float)i / height * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                if (value < falloffStart)
                {
                    mask[j, i] = 1;
                }
                else if (value > falloffEnd)
                {
                    mask[j, i] = 0;
                }
                else
                {
                    mask[j, i] = Mathf.SmoothStep(1, 0, Mathf.InverseLerp(falloffStart, falloffEnd, value));
                }
            }
        }

        return mask;
    }
}
