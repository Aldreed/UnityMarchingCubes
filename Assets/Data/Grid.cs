using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region GridPoint
public class GridPoint
{
    public Vector3 location;
    public float value;

    public GridPoint(Vector3 loc, float val)
    {
        location = loc;
        value = val;
    }
}
#endregion

#region GridCell
public class GridCell
{
    public GridPoint[] cells;

    public GridCell(ref GridPoint a, ref GridPoint b, ref GridPoint c, ref GridPoint d, ref GridPoint e, ref GridPoint f, ref GridPoint g, ref GridPoint h)
    {
        cells = new GridPoint[8];
        cells[0] = a;
        cells[1] = b;
        cells[2] = c;
        cells[3] = d;
        cells[4] = e;
        cells[5] = f; 
        cells[6] = g;
        cells[7] = h;

    }
    public GridCell() {
        cells = new GridPoint[8];
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = new GridPoint(new Vector3(0, 0, 0), 0);
        }
    }
}
#endregion

#region GridCellColor
public struct GridCellColor
{
    public Color[] cells;

    public GridCellColor(ref Color a, ref Color b, ref Color c, ref Color d, ref Color e, ref Color f, ref Color g, ref Color h)
    {
        cells = new Color[8];
        cells[0] = a;
        cells[1] = b;
        cells[2] = c;
        cells[3] = d;
        cells[4] = e;
        cells[5] = f;
        cells[6] = g;
        cells[7] = h;

    }
}
#endregion


#region Triangles
public class Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
    
    // TODO Getter Setter ...

}
#endregion

#region TrianglesColor
public class TriangleColor
{
    public Color a;
    public Color b;
    public Color c;

    public TriangleColor(Color a, Color b, Color c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    // TODO Getter Setter ...

}

public struct TriangleGPU
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;


    // TODO Getter Setter ...

}
#endregion

