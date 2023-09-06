using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class MeshGeneration : MonoBehaviour
{

    GridPoint[,,] grid;
    GridPoint[] grid2;
    public Vector3Int SizesVector;
    public float reso;
    public Material material;


    private Mesh mesh = null;
    private MeshCollider meshCollider = null;
    private List<Triangle> triangles = new List<Triangle>();
    private List<Color> colors = new List<Color>();
    private float maxHeight = 0;
    private float minHeight = 0;

    public Transform target;

    public ComputeShader comShader;

    private Texture3D storage;

    public float HightSetting = 0.0f;

    void GenerateGridPoints()
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;

        float curX = target.position.x - SizesVector.x/2 * reso;
        float curY = target.position.y - SizesVector.y/2 * reso;
        float curZ = target.position.z - SizesVector.z/2 * reso;

        float startX = curX;
        float startY = curY;
        float startZ = curZ;

        minHeight = curY;

        grid = new GridPoint[x,y,z];
        for (int i = 0; i < x; i++)
        {
            curY = startY;
            for(int j = 0; j < y; j++)
            {
                curZ = startZ;
                for( int k = 0; k < z; k++)
                {
                    grid[i,j,k] = new GridPoint(new Vector3(curX,curY,curZ),0);
                    curZ += reso;
                }
                curY += reso;
            }
            curX += reso;
        }

        maxHeight = curY;
    }

    void GenerateGridPoints2()
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;

        //float curX = target.position.x - SizesVector.x / 2 * reso;
        //float curY = target.position.y - SizesVector.y / 2 * reso;
        //float curZ = target.position.z - SizesVector.z / 2 * reso;
        float curX = 0;
        float curY = 0;
        float curZ = 0;

        float startX = curX;
        float startY = curY;
        float startZ = curZ;

        float maxX = reso * x;
        float maxY = reso * y;
        float maxZ = reso * z;

        TextureFormat format = TextureFormat.RGBA32;
        storage = new Texture3D(x, y, z, format, false);

        minHeight = curY;

        for (int i = 0; i < x; i++)
        {
            curY = startY;
            for (int j = 0; j < y; j++)
            {
                curZ = startZ;
                for (int k = 0; k < z; k++)
                {
                    storage.SetPixel(i, j, k,new Color(curX/maxX,curY/maxY,curZ/maxZ,0)); 
                    curZ += reso;
                }
                curY += reso;
            }
            curX += reso;
        }
        maxHeight = curY;
        //target.position = new Vector3((curX - startX)/2, (curY - startY) /2, (curZ - startZ) /2);
        target.position = new Vector3(startX, startY,startZ);
        Vector3 end = new Vector3(curX, curY, curZ);
        target.position = Vector3.MoveTowards(target.position,end,Vector3.Distance(target.position,end)/2);
    }

    void SetGridPointDistances2(float distance, Vector3 point, float InRangeValue)
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;

        Vector3 temp = new Vector3(0, 0, 0);
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    Color p = storage.GetPixel(i, j, k);
                    temp.x = p.r*reso* SizesVector.x; temp.y = p.g * reso * SizesVector.y; temp.z = p.b * reso * SizesVector.z;
                    if (Vector3.Distance(temp, point) < distance)
                    { 
                        p.a = InRangeValue;
                        storage.SetPixel(i, j, k, p);
                    }

                }
            }
        }
    }

    void SetGridPointDistances(float distance, Vector3 point, float InRangeValue)
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    GridPoint p = grid[i, j, k];
                    if (Vector3.Distance(p.location, point) < distance)
                    {
                        p.value = InRangeValue;
                    }
                }
            }
        }

    }

    void SetLevel()
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;

        Vector3 temp = new Vector3(0, 0, 0);
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    Color p = storage.GetPixel(i, j, k);
                    temp.x = p.r * reso * SizesVector.x; temp.y = p.g * reso * SizesVector.y; temp.z = p.b * reso * SizesVector.z;
                    if (temp.y < HightSetting + target.position.y)
                    {
                        p.a = 1;
                        storage.SetPixel(i, j, k, p);
                    }
                    else
                    {
                        p.a = 0;
                        storage.SetPixel(i, j, k, p);
                    }

                }
            }
        }
    }

    void MarchCubes()
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;
        
        List<Triangle> triangleListTemp = new List<Triangle>();

        for (int i = 0; i < 5; i++)
        {
            triangleListTemp.Add(new Triangle(new Vector3(), new Vector3(), new Vector3()));
        }

        triangles.Clear();

        for (int i = 1; i < x; i++)
        {
            for (int j = 1; j < y; j++)
            {
                for (int k = 1; k < z; k++)
                {
                    GridPoint d = grid[i - 1, j - 1, k - 1];
                    GridPoint a = grid[i, j - 1, k - 1];
                    GridPoint e = grid[i, j, k - 1];
                    GridPoint h = grid[i - 1, j, k - 1];

                    GridPoint c = grid[i - 1, j - 1, k];
                    GridPoint b = grid[i, j - 1, k];
                    GridPoint f = grid[i, j, k];
                    GridPoint g = grid[i - 1, j, k];

                    GridCell cell = new GridCell(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);

                    int numoftriangles = Polygonise(ref cell, 1, ref triangleListTemp);

                    // Save Triangles
                    for (int l = 0; l < numoftriangles; l++)
                    {
                        triangles.Add(triangleListTemp[l]);
                    }

                    // Clear
                    triangleListTemp.Clear();
                    for (int p = 0; p < 5; p++)
                    {
                        triangleListTemp.Add(new Triangle(new Vector3(), new Vector3(), new Vector3()));
                    }

                }
            }
        }
    }

    void MarchCubes2()
    {
        int x = SizesVector.x;
        int y = SizesVector.y;
        int z = SizesVector.z;

        List<Triangle> triangleListTemp = new List<Triangle>();

        for (int i = 0; i < 5; i++)
        {
            triangleListTemp.Add(new Triangle(new Vector3(), new Vector3(), new Vector3()));
        }

        triangles.Clear();

        //x = SizesVector.x - 1;
        //y = SizesVector.y - 1;
        //z = SizesVector.z - 1;

        GridCell cell = new GridCell();
        Color[] colors = new Color[8]; 

        for (int i = 1; i < x; i++)
        {
            for (int j = 1; j < y; j++)
            {
                for (int k = 1; k < z; k++)
                {
                    colors[3] = storage.GetPixel(i - 1, j - 1, k - 1);
                    colors[0] = storage.GetPixel(i, j - 1, k - 1);
                    colors[4] = storage.GetPixel(i, j, k - 1);
                    colors[7] = storage.GetPixel(i - 1, j, k - 1);

                    colors[2] = storage.GetPixel(i - 1, j - 1, k);
                    colors[1] = storage.GetPixel(i, j - 1, k);
                    colors[5] = storage.GetPixel(i, j, k);
                    colors[6] = storage.GetPixel(i - 1, j, k);

                    GridCellFromColors(cell, colors);

                    int numoftriangles = Polygonise(ref cell, 1, ref triangleListTemp);

                    // Save Triangles
                    for (int l = 0; l < numoftriangles; l++)
                    {
                        triangles.Add(triangleListTemp[l]);
                    }

                    // Clear
                    triangleListTemp.Clear();
                    for (int p = 0; p < 5; p++)
                    {
                        triangleListTemp.Add(new Triangle(new Vector3(), new Vector3(), new Vector3()));
                    }
                }
            }
        }
    }

    void GridCellFromColors(GridCell cell, Color[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            cell.cells[i].location.x = colors[i].r * reso * SizesVector.x;
            cell.cells[i].location.y = colors[i].g * reso * SizesVector.y;
            cell.cells[i].location.z = colors[i].b * reso * SizesVector.z;
            cell.cells[i].value = colors[i].a;
        }
    }


    int calcIndex(int cubeId, int x, int y, int z)
    {
        int ret = cubeId % z + ((cubeId % (z * y)) / z) * SizesVector.z + (cubeId / (z * y)) * (SizesVector.z * SizesVector.y);
        if(ret >= SizesVector.z * SizesVector.y * SizesVector.x)
        {
            ret = ret - 1;
        }
        return ret ;
    }

    int[] edgeTable = new int[]{
0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0   };
    int[,] triTable = new int[,]
{
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
{ 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
{ 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
{ 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
{ 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
{ 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
{ 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
{ 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
{ 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
{ 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
{ 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
{ 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
{ 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
{ 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
{ 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
{ 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
{ 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
{ 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
{ 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
{ 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
{ 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
{ 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
{ 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
{ 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
{ 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
{ 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
{ 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
{ 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
{ 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
{ 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
{ 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
{ 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
{ 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
{ 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
{ 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
{ 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
{ 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
{ 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
{ 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
{ 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
{ 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
{ 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
{ 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
{ 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
{ 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
{ 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
{ 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
{ 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
{ 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
{ 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
{ 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
{ 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
{ 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
{ 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
{ 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
{ 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
{ 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
{ 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
{ 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
{ 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
{ 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
{ 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
{ 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
{ 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
{ 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
{ 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
{ 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
{ 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
{ 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
{ 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
{ 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
{ 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
{ 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
{ 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
{ 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
{ 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
{ 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
{ 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
{ 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
{ 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
{ 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
{ 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
{ 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
{ 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
{ 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
{ 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
{ 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
{ 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
{ 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
{ 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
{ 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
{ 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
{ 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
{ 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
{ 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
{ 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
{ 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
{ 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
{ 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
{ 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
{ 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
{ 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
{ 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
{ 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
{ 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
{ 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
{ 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
{ 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
{ 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
{ 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
{ 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
{ 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
{ 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
{ 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
{ 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
{ 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
{ 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
{ 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
{ 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
{ 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
{ 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
{ 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
{ 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
{ 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
{ 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
{ 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
{ 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
{ 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
{ 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
{ 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
{ 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
{ 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
{ 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
{ 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
{ 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
{ 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
{ 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
{ 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
{ 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
{ 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
{ 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
{ 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
{ 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
{ 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
{ 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
{ 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
{ 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
{ 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
{ 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
{ 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
{ 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
{ 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
{ 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
{ 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
{ 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
{ 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
{ 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
{ 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
{ 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
{ 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
{ 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
{ 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
{ 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
{ 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
{ 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
{ 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
{ 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
{ 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
{ 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
{ 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
{ 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
{ 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
{ 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
{ 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
{ 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
{ 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
{ 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
{ 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
    };


    int Polygonise(ref GridCell grid, double isolevel, ref List<Triangle> triangles)
    {
        int i, ntriang;
        int cubeindex;
        Vector3[] vertlist = new Vector3[12];

        /*
           Determine the index into the edge table which
           tells us which vertices are inside of the surface
        */
        cubeindex = 0;
        if (grid.cells[0].value < isolevel) cubeindex |= 1;
        if (grid.cells[1].value < isolevel) cubeindex |= 2;
        if (grid.cells[2].value < isolevel) cubeindex |= 4;
        if (grid.cells[3].value < isolevel) cubeindex |= 8;
        if (grid.cells[4].value < isolevel) cubeindex |= 16;
        if (grid.cells[5].value < isolevel) cubeindex |= 32;
        if (grid.cells[6].value < isolevel) cubeindex |= 64;
        if (grid.cells[7].value < isolevel) cubeindex |= 128;

        /* Cube is entirely in/out of the surface */
        if (edgeTable[cubeindex] == 0)
            return (0);

        /* Find the vertices where the surface intersects the cube */
        if ((edgeTable[cubeindex] & 1) != 0)
            vertlist[0] =
               VertexInterp(isolevel, grid.cells[0].location, grid.cells[1].location, grid.cells[0].value, grid.cells[1].value);
        if ((edgeTable[cubeindex] & 2) != 0)
            vertlist[1] =
               VertexInterp(isolevel, grid.cells[1].location, grid.cells[2].location, grid.cells[1].value, grid.cells[2].value);
        if ((edgeTable[cubeindex] & 4) != 0)
            vertlist[2] =
               VertexInterp(isolevel, grid.cells[2].location, grid.cells[3].location, grid.cells[2].value, grid.cells[3].value);
        if ((edgeTable[cubeindex] & 8) != 0)
            vertlist[3] =
               VertexInterp(isolevel, grid.cells[3].location, grid.cells[0].location, grid.cells[3].value, grid.cells[0].value);
        if ((edgeTable[cubeindex] & 16) != 0)
            vertlist[4] =
               VertexInterp(isolevel, grid.cells[4].location, grid.cells[5].location, grid.cells[4].value, grid.cells[5].value);
        if ((edgeTable[cubeindex] & 32) != 0)
            vertlist[5] =
               VertexInterp(isolevel, grid.cells[5].location, grid.cells[6].location, grid.cells[5].value, grid.cells[6].value);
        if ((edgeTable[cubeindex] & 64) != 0)
            vertlist[6] =
               VertexInterp(isolevel, grid.cells[6].location, grid.cells[7].location, grid.cells[6].value, grid.cells[7].value);
        if ((edgeTable[cubeindex] & 128) != 0)
            vertlist[7] =
               VertexInterp(isolevel, grid.cells[7].location, grid.cells[4].location, grid.cells[7].value, grid.cells[4].value);
        if ((edgeTable[cubeindex] & 256) != 0)
            vertlist[8] =
               VertexInterp(isolevel, grid.cells[0].location, grid.cells[4].location, grid.cells[0].value, grid.cells[4].value);
        if ((edgeTable[cubeindex] & 512) != 0)
            vertlist[9] =
               VertexInterp(isolevel, grid.cells[1].location, grid.cells[5].location, grid.cells[1].value, grid.cells[5].value);
        if ((edgeTable[cubeindex] & 1024) != 0)
            vertlist[10] =
               VertexInterp(isolevel, grid.cells[2].location, grid.cells[6].location, grid.cells[2].value, grid.cells[6].value);
        if ((edgeTable[cubeindex] & 2048) != 0)
            vertlist[11] =
               VertexInterp(isolevel, grid.cells[3].location, grid.cells[7].location, grid.cells[3].value, grid.cells[7].value);

        /* Create the triangle */
        ntriang = 0;
        for (i = 0; triTable[cubeindex,i] != -1; i += 3)
        {
            triangles[ntriang].a = vertlist[triTable[cubeindex, i]];
            triangles[ntriang].b = vertlist[triTable[cubeindex, i + 1]];
            triangles[ntriang].c = vertlist[triTable[cubeindex, i + 2]];
            ntriang++;
        }

        return (ntriang);
    }

    int PolygoniseColor(ref GridCellColor grid, double isolevel, ref List<Triangle> triangles)
    {
        int i, ntriang;
        int cubeindex;
        Color[] vertlist = new Color[12];

        /*
           Determine the index into the edge table which
           tells us which vertices are inside of the surface
        */
        cubeindex = 0;
        if (grid.cells[0].a < isolevel) cubeindex |= 1;
        if (grid.cells[1].a < isolevel) cubeindex |= 2;
        if (grid.cells[2].a < isolevel) cubeindex |= 4;
        if (grid.cells[3].a < isolevel) cubeindex |= 8;
        if (grid.cells[4].a < isolevel) cubeindex |= 16;
        if (grid.cells[5].a < isolevel) cubeindex |= 32;
        if (grid.cells[6].a < isolevel) cubeindex |= 64;
        if (grid.cells[7].a < isolevel) cubeindex |= 128;

        /* Cube is entirely in/out of the surface */
        if (edgeTable[cubeindex] == 0)
            return (0);

        /* Find the vertices where the surface intersects the cube */
        if ((edgeTable[cubeindex] & 1) != 0)
            vertlist[0] =
               VertexInterpColor(isolevel, grid.cells[0], grid.cells[1], grid.cells[0].a, grid.cells[1].a);
        if ((edgeTable[cubeindex] & 2) != 0)
            vertlist[1] =
               VertexInterpColor(isolevel, grid.cells[1], grid.cells[2], grid.cells[1].a, grid.cells[2].a);
        if ((edgeTable[cubeindex] & 4) != 0)
            vertlist[2] =
               VertexInterpColor(isolevel, grid.cells[2], grid.cells[3], grid.cells[2].a, grid.cells[3].a);
        if ((edgeTable[cubeindex] & 8) != 0)
            vertlist[3] =
               VertexInterpColor(isolevel, grid.cells[3], grid.cells[0], grid.cells[3].a, grid.cells[0].a);
        if ((edgeTable[cubeindex] & 16) != 0)
            vertlist[4] =
               VertexInterpColor(isolevel, grid.cells[4], grid.cells[5], grid.cells[4].a, grid.cells[5].a);
        if ((edgeTable[cubeindex] & 32) != 0)
            vertlist[5] =
               VertexInterpColor(isolevel, grid.cells[5], grid.cells[6], grid.cells[5].a, grid.cells[6].a);
        if ((edgeTable[cubeindex] & 64) != 0)
            vertlist[6] =
               VertexInterpColor(isolevel, grid.cells[6], grid.cells[7], grid.cells[6].a, grid.cells[7].a);
        if ((edgeTable[cubeindex] & 128) != 0)
            vertlist[7] =
               VertexInterpColor(isolevel, grid.cells[7], grid.cells[4], grid.cells[7].a, grid.cells[4].a);
        if ((edgeTable[cubeindex] & 256) != 0)
            vertlist[8] =
               VertexInterpColor(isolevel, grid.cells[0], grid.cells[4], grid.cells[0].a, grid.cells[4].a);
        if ((edgeTable[cubeindex] & 512) != 0)
            vertlist[9] =
               VertexInterpColor(isolevel, grid.cells[1], grid.cells[5], grid.cells[1].a, grid.cells[5].a);
        if ((edgeTable[cubeindex] & 1024) != 0)
            vertlist[10] =
               VertexInterpColor(isolevel, grid.cells[2], grid.cells[6], grid.cells[2].a, grid.cells[6].a);
        if ((edgeTable[cubeindex] & 2048) != 0)
            vertlist[11] =
               VertexInterpColor(isolevel, grid.cells[3], grid.cells[7], grid.cells[3].a, grid.cells[7].a);

        /* Create the triangle */
        ntriang = 0;
        Vector3 aVec = new Vector3(0,0, 0);
        Vector3 bVec = new Vector3(0,0, 0);
        Vector3 cVec = new Vector3(0,0, 0);
        for (i = 0; triTable[cubeindex, i] != -1; i += 3)
        {
            Color a = vertlist[triTable[cubeindex, i]];
            Color b = vertlist[triTable[cubeindex, i + 1]];
            Color c = vertlist[triTable[cubeindex, i + 2]];

            aVec.x = a.r;
            aVec.y = a.g;
            aVec.z = a.b;

            bVec.x = b.r;
            bVec.y = b.g;
            bVec.z = b.b;

            cVec.x = c.r;
            cVec.y = c.g;
            cVec.z = c.b;

            triangles[ntriang].a = aVec;
            triangles[ntriang].b = bVec;
            triangles[ntriang].c = cVec;
            ntriang++;
        }

        return (ntriang);
    }
    Vector3 VertexInterp(double isolevel, Vector3 p1, Vector3 p2, double valp1, double valp2)
{
   double mu;
   Vector3 p;

   if (Math.Abs(isolevel-valp1) < 0.00001)
      return(p1);
   if (Math.Abs(isolevel-valp2) < 0.00001)
      return(p2);
   if (Math.Abs(valp1-valp2) < 0.00001)
      return(p1);
   mu = (isolevel - valp1) / (valp2 - valp1);
   p.x = (float)(p1.x + mu* (p2.x - p1.x));
   p.y = (float)(p1.y + mu* (p2.y - p1.y));
   p.z = (float)(p1.z + mu * (p2.z - p1.z));

   return(p);
}
    Color VertexInterpColor(double isolevel, Color p1, Color p2, double valp1, double valp2)
    {
        double mu;
        Color p = new Color();

        if (Math.Abs(isolevel - valp1) < 0.00001)
            return (p1);
        if (Math.Abs(isolevel - valp2) < 0.00001)
            return (p2);
        if (Math.Abs(valp1 - valp2) < 0.00001)
            return (p1);
        mu = (isolevel - valp1) / (valp2 - valp1);
        p.r = (float)(p1.r + mu * (p2.r - p1.r));
        p.g = (float)(p1.g + mu * (p2.g - p1.g));
        p.b = (float)(p1.b + mu * (p2.b - p1.b));

        return (p);
    }

    void SetMesh(ref Mesh mesh)
    {
        mesh.Clear();

        List<Vector3> vertecies = new List<Vector3>();
        List<int> tris = new List<int>();
        colors.Clear();
        int count = 0;

        foreach (Triangle tri in triangles)
        {
            vertecies.Add(tri.a);
            vertecies.Add(tri.b);
            vertecies.Add(tri.c);

            count += 3;

            tris.Add(count-1);
            tris.Add(count-2);
            tris.Add(count-3);

            colors.Add(EvalColor(ref tri.c));
            colors.Add(EvalColor(ref tri.b));
            colors.Add(EvalColor(ref tri.a));
        }

        mesh.vertices = vertecies.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

    }
    void SetMeshCollider(ref Mesh mesh,ref MeshCollider collider)
    {
        collider.sharedMesh = mesh;
    }

    public Gradient gradient;
    

    Color EvalColor(ref Vector3 vertex)
    {
        float height = Mathf.InverseLerp(minHeight,maxHeight,vertex.y);
        
        return gradient.Evaluate(height);
    }

    Mesh InitMesh(ref GameObject obj, ref Material mat)
    {
        Mesh mesh = null;

        MeshFilter meshFilter= obj.GetComponent<MeshFilter>();
        if(meshFilter == null)
        {
            meshFilter = obj.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if(meshRenderer == null)
        {
            meshRenderer = obj.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = mat;

        mesh = meshFilter.mesh; 
        if(mesh == null)
        {
            meshFilter.mesh = new Mesh();
            mesh = meshFilter.mesh;

        }
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = "basic mesh";

        return mesh;
    }
    MeshCollider InitMeshCollider(ref GameObject obj, ref Mesh mesh) {

        MeshCollider meshCol = obj.GetComponent<MeshCollider>();
        if (meshCol == null)
        {
            meshCol = obj.AddComponent<MeshCollider>();
        }
        meshCol.sharedMesh = mesh;
        meshCol.convex = false;

        return meshCol;
    }

// Start is called before the first frame update
void Start()
    {

        GameObject go = this.gameObject;
        mesh = InitMesh(ref go,ref material);
        meshCollider = InitMeshCollider(ref go, ref mesh);

        //GenerateGridPoints();
        GenerateGridPoints2();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        offsetX = Random.Range(0f, 9999f);
        offsetZ = Random.Range(0f, 9999f);
        //SetGridPointDistances(2f, target.position, 1);
        SetGridPointDistances2(10f, new Vector3(10, 10, 10), 1);
        //Noise2D();
        Noise2DColor();
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("SET GRID: " + elapsedMs);

        var watch2 = System.Diagnostics.Stopwatch.StartNew();
        //MarchCubes();
        MarchCubes2();
        watch2.Stop();
        var elapsedMs2 = watch2.ElapsedMilliseconds;
        Debug.Log("MARCH: " + elapsedMs2);
         
        Debug.Log(triangles.Count);
        SetMesh(ref mesh);
        SetMeshCollider(ref mesh, ref meshCollider);

        //setGridPointsGPU();
        //BringToGPU(comShader);

    }

    public Vector3 screenPosition;
    public Vector3 worldPosition;
    Plane plane = new Plane(Vector3.down, 0);
    Plane plane2 = new Plane(Vector3.back,-2);

    public float scale = 2;
    public float offsetX = 50f;
    public float offsetZ = 50f;

    private void Noise2D()
    {
        for (int i = 0; i < SizesVector.x; i++)
        {
            for(int k = 0; k < SizesVector.z;k++) { 

                float f = ((i * reso /  SizesVector.x)) * scale + offsetX;
                float g = ((k * reso / SizesVector.z)) * scale + offsetZ;

                float height = Mathf.PerlinNoise(f, g) * SizesVector.y * reso;

                for(int j = 0; j < SizesVector.y; j++)
                {
                    grid[i, j, k].value = ((j * reso) > height) ? 0f : 1f;
                }
            }
        }
    }

    private void Noise2DColor()
    {
        Color point = Color.white;
        for (int i = 0; i < SizesVector.x; i++)
        {
            for (int k = 0; k < SizesVector.z; k++)
            {

                float f = ((i * reso / SizesVector.x)) * scale + offsetX;
                float g = ((k * reso / SizesVector.z)) * scale + offsetZ;

                float height = Mathf.PerlinNoise(f, g) * SizesVector.y * reso;

                for (int j = 0; j < SizesVector.y; j++)
                {
                    point = storage.GetPixel(i, j, k);
                    point.a = ((j * reso) > height) ? 0f : 1f;
                    storage.SetPixel(i, j, k, point);
                }
            }
        }
    }

    public bool GenarateMode = true;
    public float BrushSize = 1f;
    // Update is called once per frame
    void Update()
    {
        screenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if(Physics.Raycast(ray, out RaycastHit hitData)){
            worldPosition = hitData.point;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //SetGridPointDistances(BrushSize, worldPosition, (GenarateMode)? 1:0);
            SetGridPointDistances2(BrushSize, worldPosition, (GenarateMode) ? 1 : 0);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log("SET GRID: " + elapsedMs);

            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            //MarchCubes();
            MarchCubes2();
            watch2.Stop();
            var elapsedMs2 = watch.ElapsedMilliseconds;
            Debug.Log("MARCH: " + elapsedMs2);

            var watch3 = System.Diagnostics.Stopwatch.StartNew();
            SetMesh(ref mesh);
            SetMeshCollider(ref mesh, ref meshCollider);
            watch3.Stop();
            var elapsedMs3 = watch.ElapsedMilliseconds;
            Debug.Log("SET: " + elapsedMs2);

            Debug.Log(triangles.Count);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            SetLevel();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log("SET GRID: " + elapsedMs);

            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            //MarchCubes();
            MarchCubes2();
            watch2.Stop();
            var elapsedMs2 = watch.ElapsedMilliseconds;
            Debug.Log("MARCH: " + elapsedMs2);

            var watch3 = System.Diagnostics.Stopwatch.StartNew();
            SetMesh(ref mesh);
            SetMeshCollider(ref mesh, ref meshCollider);
            watch3.Stop();
            var elapsedMs3 = watch.ElapsedMilliseconds;
            Debug.Log("SET: " + elapsedMs2);

            Debug.Log(triangles.Count);
        }


        if (Input.GetKeyDown(KeyCode.D))
        {
            GenarateMode = !GenarateMode;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            BrushSize += reso;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            BrushSize -= reso;
        }
        

    }


    GridPoint[] vec;
    public void BringToGPU(ComputeShader shader)
    {
        int[] tris = new int[triTable.Length];
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = triTable[i/16,i%16];
        }

        TriangleGPU[] genTris = new TriangleGPU[(SizesVector.x - 1) * (SizesVector.y - 1) * (SizesVector.z - 1) * 5];

        uint[] createNum = { 0 };

        GraphicsBuffer _genTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, genTris.Length, sizeof(float)*3*3);
        GraphicsBuffer _edges = new GraphicsBuffer(GraphicsBuffer.Target.Structured,edgeTable.Length, sizeof(int));
        GraphicsBuffer _tris = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tris.Length, sizeof(int));
        GraphicsBuffer _points = new GraphicsBuffer(GraphicsBuffer.Target.Structured,vec.Length, sizeof(float)*4);
        GraphicsBuffer _createNum = new GraphicsBuffer(GraphicsBuffer.Target.Structured, createNum.Length, sizeof(uint));

        int idKern = shader.FindKernel("Main");

        shader.SetBuffer(idKern, "_GenTris",_genTriangles);
        shader.SetBuffer(idKern, "points", _points);
        shader.SetBuffer(idKern, "edgeTable", _edges);
        shader.SetBuffer(idKern, "triTable", _tris);
        shader.SetBuffer(idKern, "createdTris", _createNum);
        shader.SetInt("NumOfPoints",vec.Length);

        _edges.SetData(edgeTable);
        _tris.SetData(tris);
        _points.SetData(vec);
        _createNum.SetData(createNum);

        shader.GetKernelThreadGroupSizes(idKern, out uint threadGroupSize, out _, out _);
        int dispatchSize = Mathf.CeilToInt((float)vec.Length / threadGroupSize);
        shader.Dispatch(idKern, dispatchSize, 1,1);
        _genTriangles.GetData(genTris);
        _createNum.GetData(createNum);

        Debug.Log("TRI: " + genTris.Length);
        Debug.Log("Actual: " + createNum[0]);
        int t = 0;
        foreach (TriangleGPU item in genTris)
        {
            if (item.a.x != 0 || item.a.y !=0 || item.a.z !=0 || item.b.x != 0 || item.b.y != 0 || item.b.z != 0 || item.c.x != 0 || item.c.y != 0 || item.c.z != 0)
            {
                t++;
            }
        }
        Debug.Log(t);
        _genTriangles.Release();
        _points.Release();
        _edges.Release();
        _tris.Release();
        _createNum.Release();
        

    }
    
    private void setGridPointsGPU()
    {
        
        vec = new GridPoint[(SizesVector.x - 1) * (SizesVector.y - 1) * (SizesVector.z - 1) * 8];
        int count = 0;
        for (int i = 1; i < SizesVector.x; i++)
        {
            for (int j = 1; j < SizesVector.x; j++)
            {
                for (int k = 1; k < SizesVector.z; k++)
                {
                    GridPoint d = grid[i - 1, j - 1, k - 1];
                    GridPoint a = grid[i, j - 1, k - 1];
                    GridPoint e = grid[i, j, k - 1];
                    GridPoint h = grid[i - 1, j, k - 1];

                    GridPoint c = grid[i - 1, j - 1, k];
                    GridPoint b = grid[i, j - 1, k];
                    GridPoint f = grid[i, j, k];
                    GridPoint g = grid[i - 1, j, k];
                    vec[count++] = a;
                    vec[count++] = b;
                    vec[count++] = c;
                    vec[count++] = d;
                    vec[count++] = e;
                    vec[count++] = f;
                    vec[count++] = g;
                    vec[count++] = h;
                } 
            
            } 
        }
    }

    private void OnDrawGizmos()
    {
        
            //foreach (var item in grid)
            //{
            //    Gizmos.DrawSphere(item.location, 0.1f);
            //}
        
    }
}
