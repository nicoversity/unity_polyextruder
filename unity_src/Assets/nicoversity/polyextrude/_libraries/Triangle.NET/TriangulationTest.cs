/*
 * TriangulationTest.cs
 *
 * Description: Class to test the implementation of the Triangulation.cs script.
 * The class creates a mesh representing a cross with a square hole inside (coordinates are hard-coded; see implementation).
 * The source of this class has been adapted from the video setup tutorial at https://www.youtube.com/watch?v=wByVhzokWPo (note: unfortunately, the video is no longer available as of 2019-06-04).
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

/// <summary>
/// Class to test the implementation of the Triangulation.cs script.
/// </summary>
public class TriangulationTest : MonoBehaviour {

	void Start () {

        // create a new game object (as a child) and add required components
        GameObject go = new GameObject();
        go.transform.parent = this.transform;
        go.name = "Cross";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshCollider mc = go.AddComponent<MeshCollider>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));

        // create collections of input vectors and indices representing the (hard-coded) cross and its hole
        List<Vector2> points = new List<Vector2>();
        List<List<Vector2>> holes = new List<List<Vector2>>();
        List<int> indices = null;
        List<Vector3> vertices = null;

        // manually hard-coded 2D vectors representing the cross
        points.Add(new Vector2(10,0));
        points.Add(new Vector2(20,0));
        points.Add(new Vector2(20,10));
        points.Add(new Vector2(30,10));
        points.Add(new Vector2(30,20));
        points.Add(new Vector2(20,20));
        points.Add(new Vector2(20,30));
        points.Add(new Vector2(10,30));
        points.Add(new Vector2(10,20));
        points.Add(new Vector2(0,20));
        points.Add(new Vector2(0,10));
        points.Add(new Vector2(10,10));

        // manually hard-coded 2D vectors representing the square-shaped hole inside the cross
        List<Vector2> hole = new List<Vector2>();
        hole.Add(new Vector2(12,12));
        hole.Add(new Vector2(18,12));
        hole.Add(new Vector2(18,18));
        hole.Add(new Vector2(12,18));
        holes.Add(hole);

        // perform TRIANGULATION
        Triangulation.triangulate(points, holes, 0.0f, out indices, out vertices);

        // create mesh instance and assign indices and vertices representing the (newly triangulated) mesh
        Mesh mesh = mf.mesh;
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();

        // reset mesh collider after (re-)creation
        go.GetComponent<MeshCollider>().sharedMesh = mesh;

        // generate simple UV map
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
        mesh.uv = uvs;
	}
}
