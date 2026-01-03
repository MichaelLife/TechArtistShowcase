using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{
    private float terrainSurface = 0.5f;
    int width = 32;
    int length = 32;
    int height = 8;
    float[,,] terrainMap;

    private void Start()
    {
        terrainMap = new float[width, height, length];
    }

    private void PopulateTerrainMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    float thisHeight = (float)height * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f);

                    float point = 0;

                    if (y <= thisHeight - 0.5f) //inside ground
                        point = 0f;
                    else if (y > thisHeight + 0.5f) //outside 
                        point = 1f;
                    else if (y > thisHeight)
                        point = (float)y - thisHeight;
                    else
                        point = thisHeight - (float)y;

                    terrainMap[x, y, z] = point;
                }
            }
        }
    }
}
