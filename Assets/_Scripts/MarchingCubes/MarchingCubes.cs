using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class MarchingCubes : MonoBehaviour
{
    [SerializeField] private bool smoothShading;
    private bool smoothTerrain;
    [Range(0.0f,1.0f)] [SerializeField] private float smoothIndex;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> vertexColor = new List<Color>();

    private MeshFilter meshFilter;

    private int _configIndex = -1;

    [SerializeField] private float terrainSurface = 0.5f;
    [SerializeField] int width = 32;
    [SerializeField] int length = 32;
    [SerializeField] int height = 8;
    float[,,] terrainMap;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        terrainMap = new float[width + 1, height + 1, length + 1];

        PopulateTerrainMap();
        CreateMeshData();
        BuildMesh();
    }

    private void CreateMeshData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private void PopulateTerrainMap()
    {
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < length + 1; z++)
                {
                    float thisHeight = (float)height * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f);

                    terrainMap[x, y, z] = (float)y - thisHeight;
                }
            }
        }
    }

    int FindVertexMatch(Vector3 v)
    {
        //Find if there is a vertex that matches v
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i] == v)
                return i;
        }

        //If there is no match add the vertex to the list
        vertices.Add(v);
        vertexColor.Add(sampleColors(v));
        return vertices.Count - 1;
    }

    private Color sampleColors(Vector3 v)
    {
        Color c = Color.white;

        if(v.y < (float)height/1.5f)
        {
            c = Color.black;
        }

        return c;
    }

    int GetCubeConfiguration(float[] cube)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > terrainSurface)
                configIndex |= 1 << i;
        }

        return configIndex;
    }

    void MarchCube(Vector3Int position) 
    {
        //Sample terrain at each corner of the cube
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + MarchingTable.Corners[i]);
        }

        int configIndex = GetCubeConfiguration(cube); //Config index is index of Marching table

        if (configIndex == 0 || configIndex == 255) return; //Son los límites que se detectan como aire

        int edgeIndex = 0;
        for (int i = 0; i < 5; i++) //5 es el número máximo de triangulos que hay en la maya
        {
            for (int j = 0; j < 3; j++) //3 es el número de vértices del triángulo
            {
                int index = MarchingTable.Triangles[configIndex, edgeIndex]; //Coge el index del triángulo que toca

                if (index == -1) //-1 significa que no hay triángulo
                    return;

                Vector3 vert1 = position + MarchingTable.Corners[MarchingTable.EdgeIndexes[index, 0]]; 
                Vector3 vert2 = position + MarchingTable.Corners[MarchingTable.EdgeIndexes[index, 1]];

                Vector3 vertPosition;
                smoothTerrain = smoothIndex > 0;
                if (smoothTerrain)
                {
                    //Get the terrain values at the ends of the current edge
                    float vert1Sample = cube[MarchingTable.EdgeIndexes[index, 0]];
                    float vert2Sample = cube[MarchingTable.EdgeIndexes[index, 1]];

                    //Calculate the difference
                    float diff = vert2Sample - vert1Sample;

                    //If the diff is 0, the the terrain goes in the middle
                    if (diff == 0)
                        diff = terrainSurface;
                    else
                        diff = (terrainSurface - vert1Sample) / diff;

                    //Calculate the point where the terrain is
                    vertPosition = vert1 + ((vert2 - vert1) * diff * smoothIndex);
                }
                else
                {
                    vertPosition = (vert1 + vert2) / 2f;
                }

                if (smoothShading)
                {
                    triangles.Add(FindVertexMatch(vertPosition));
                }
                else
                {
                    vertices.Add(vertPosition);
                    triangles.Add(vertices.Count - 1);
                }
                edgeIndex++;
            }
        }
    }

    private float SampleTerrain (Vector3Int point)
    {
        return terrainMap[point.x, point.y, point.z];
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.colors = vertexColor.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
