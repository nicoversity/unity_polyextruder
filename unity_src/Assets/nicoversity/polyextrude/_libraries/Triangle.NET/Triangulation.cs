/*
 * Triangulation.cs
 *
 * Description: Class to triangulate (create render triangles for) a custom polygon mesh.
 * The class has been adapted from the Triangle.NET library, following the video setup tutorial at https://www.youtube.com/watch?v=wByVhzokWPo (note: unfortunately, the video is no longer available as of 2019-06-04).
 * 
 * Triangle.NET library implementation (by Christian Woltering): https://archive.codeplex.com/?p=triangle
 * (Original) Triangle library implementation (by Jonathan Richard Shewchuk): http://www.cs.cmu.edu/~quake/triangle.html
 * 
 * Supported Unity version: 2021.3.16f1 Personal (tested)
 *
 * Author: Nico Reski
 * Web: https://reski.nicoversity.com
 * Twitter: @nicoversity
 * GitHub: https://github.com/nicoversity
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;

/// <summary>
/// Class to triangulate (create render triangles for) a custom polygon mesh.
/// </summary>
public class Triangulation
{
    #region TRIANGULATION
    
    /// <summary>
    /// Perform triangulation for a custom (polygon) mesh.
    /// </summary>
    /// <returns>boolean value indicating whether the triangulation was successful (true) or not (false) (note: error list not implemented yet, therefore the function always returns true by default in this implementation).</returns>
    /// <param name="points">Vector2 List containing all the input vertices of the (polygon) mesh that the triangulation is performed on.</param>
    /// <param name="holes">List of Vector2 List containing all the input vertex list representing holes in the input (polygon) mesh.</param>
    /// <param name="vertexY">The input (polygon) mesh is created in 2D along the x- and z-dimension in the 3D space. This int parameter provides an option to set the polygon's y-dimension value.</param>
    /// <param name="outIndices">Int List representing all indexes of the successful triangulation process.</param>
    /// <param name="outVertices">Vector3 List representing all 3D vertices of the successful triangulation process.</param>
    public static bool triangulate(List<Vector2> points, List<List<Vector2>> holes, float vertexY, out List<int> outIndices, out List<Vector3> outVertices)
    {
        // create polygon with points and segments based on input parameter
        Polygon poly = new Polygon();
        for (int i = 0; i < points.Count; i++)
        {
            // add point
            poly.Add(new Vertex(points[i].x, points[i].y));

            // add segments
            // connect last point with first point to establish closing segment (closed polygon)
            if (i == points.Count - 1)
            {
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[0].x, points[0].y)));
            }
            // connect current point with following point
            else
            {
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[i + 1].x, points[i + 1].y)));
            }
        }

        // handle holes in polygon (if there are any)
        for (int i = 0; i < holes.Count; i++)
        {
            // create list of vertices for current hole
            List<Vertex> vertices = new List<Vertex>();
            for (int j = 0; j < holes[i].Count; j++)
            {
                vertices.Add(new Vertex(holes[i][j].x, holes[i][j].y));
            }
            // add current hole vertices as contour
            poly.Add(new Contour(vertices), true);
        }

        // perform TRIANGULATION
        var mesh = poly.Triangulate();

        // prepare return values
        //
        outVertices = new List<Vector3>();
        outIndices = new List<int>();

        // traverse all triangles of the mesh
        foreach (ITriangle t in mesh.Triangles)
        {
            // traverse each vertex of current triangle
            for (int j = 2; j >= 0; j--)
            {
                // check if vertex already exists in the vertex list
                bool found = false;
                for (int k = 0; k < outVertices.Count; k++)
                {
                    // note: check Z value since for some reason Unity is mixing up Z and Y
                    //if ((outVertices[k].x == t.GetVertex(j).X) && (outVertices[k].z == t.GetVertex(j).Y))     // inaccurate approach to compare floating points (see https://docs.unity3d.com/ScriptReference/Mathf.Approximately.html)
                    if ((System.Math.Abs(outVertices[k].x - t.GetVertex(j).X) < Mathf.Epsilon) &&
                        (System.Math.Abs(outVertices[k].z - t.GetVertex(j).Y) < Mathf.Epsilon))
                    {
                        // add index to found vertex
                        outIndices.Add(k);
                        found = true;
                        break;  // leave loop
                    }
                }

                // if a vertex was never found, add/create it as well as adding an index to that vertex
                if (!found)
                {
                    //outVertices.Add(new Vector3((float)t.GetVertex(j).X, 0.0f, (float)t.GetVertex(j).Y));
                    outVertices.Add(new Vector3((float)t.GetVertex(j).X, vertexY, (float)t.GetVertex(j).Y));
                    outIndices.Add(outVertices.Count - 1);  // pointing to the newly added vertex (== last one in the list)
                }
            }
        }

        // default: return true for successfull triangulation
        // TODO: create error list -> optional for implementation
        return true;
    }

    #endregion
}
