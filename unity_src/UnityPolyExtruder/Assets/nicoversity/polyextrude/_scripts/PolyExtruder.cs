/*
 * PolyExtruder.cs
 *
 * Description: Class to render a custom polygon (2D) or prism (3D; simple extrusion of the polygon along the y-axis),
 * created through a generated (polygon) mesh using Triangulation.cs. Input vertices of the polygon are taken as a Vector2 array.
 * 
 * The class provides certain "quality-of-life" functions, such as for instance:
 * - calulating the area of the polygon surface
 * - calculating the centroid of the polygon
 * - automatic handling of correct surround mesh rendering, based on determination whether input vertices are ordered clockwise or counter-clockwise
 * 
 * Documentation:
 * - Prism 2D (is3D = false) = Polygon -> render created 2D mesh along x- and z-dimensions in the 3D space
 * - Prism 3D (is3D = true) = Extruded Polygon -> three components:
 *      - bottom mesh: = Prism 2D
 *      - top mesh: same as bottom mesh but at an increased position along the y-dimension
 *      - surround mesh: connecting bottom and top mesh along their outline vertices
 * - Outline Renderer = LineRenderer component based on the top most polygon, outlining the surface for better differentiation (for instance when placed next to other prisms
 *
 * ATTENTION: No holes-support in polygon extrusion (Prism 3D) implemented!
 * Although Triangulation.cs supports "holes" in the 2D polygon, the support of holes as part of the PolyExtruder *has not* been implemented in this version.
 * 
 * Supported Unity version: 2019.2.17f1 Personal (tested)
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
/// Class to create a (extruded) polygon based on a collection of vertices.
/// </summary>
public class PolyExtruder : MonoBehaviour
{
    #region Properties

    [Header("Prism Configuration")]
    public string prismName;                        // reference to name of the prism
    public Color32 prismColor;                      // reference to prism color
    public float polygonArea;                       // references to area of (top) polygon
    public Vector2 polygonCentroid;                 // references to centroid of (top) polygon

    [Header("Outline Renderer Configuration")]
    public bool isOutlineRendered = false;          // indicator if the outline should be rendered / displayed or not
    public float outlineWidth = 0.01f;              // float representing the width of the outline
    public Color outlineColor = Color.black;        // Color representing the color of the outline


    // private properties
    //

    // indicator if GameObject is extruded 3D (prism) or 2D (polygon)
    private bool is3D;

    // reference to extrusion height (Y axis)
    // Note: default y (height) of extruded polygon = 1.0f; default y (height) of bottom polygon = 0.0f;
    // -> scaling is applied using the GameObject transform's localScale y-value
    private static readonly float DEFAULT_BOTTOM_Y = 0.0f;
    private static readonly float DEFAULT_TOP_Y = 1.0f;
    private float extrusionHeightY = 1.0f;

    // reference to original input vertices of Polygon in Vector2 Array format
    private Vector2[] originalPolygonVertices;

    // references to prism (polygon -> 2D; prism -> 3D) components
    private Mesh bottomMesh;
    private Mesh topMesh;
    private Mesh surroundMesh;
    private MeshRenderer bottomMeshRenderer;
    private MeshRenderer topMeshRenderer;
    private MeshRenderer surroundMeshRenderer;

    #endregion


    #region MeshCreator

    /// <summary>
    /// Create a prism based on the input parameters.
    /// </summary>
    /// <param name="prismName">Name of the prism within the Unity scene.</param>
    /// <param name="height">Height of the prism (= distance along the y-axis between bottom and top mesh).</param>
    /// <param name="vertices">Vector2 Array representing the input data of the polygon.</param>
    /// <param name="color">Color of the prism's material.</param>
    /// <param name="is3D">Set to<c>true</c> if polygon extrusion should be applied (= 3D prism), or <c>false</c> if it is only the (2D) polygon.</param>
    public void createPrism(string prismName, float height, Vector2[] vertices, Color32 color, bool is3D)
    {
        // set data
        this.prismName = name;
        this.extrusionHeightY = height;
        this.originalPolygonVertices = vertices;
        this.prismColor = color;
        this.polygonArea = 0.0f;
        this.polygonCentroid = new Vector2(0.0f, 0.0f);
        this.is3D = is3D;

        // handle vertex order
        bool vertexOrderClockwise = areVerticesOrderedClockwise(this.originalPolygonVertices);
        if (!vertexOrderClockwise) System.Array.Reverse(this.originalPolygonVertices);

        // calculate area and centroid
        calculateAreaAndCentroid(this.originalPolygonVertices);

        // initialize meshes for prism
        initPrism();
    }

    /// <summary>
    /// Function to determine whether the input vertices are order clockwise or counter-clockwise.
    /// </summary>
    /// <returns>Returns <c>true</c> if vertices are ordered clockwise, and <c>false</c> if ordered counter-clockwise.</returns>
    /// <param name="vertices">Vector2 Array representing input vertices of the polygon.</param>
    private bool areVerticesOrderedClockwise(Vector2[] vertices)
    {
        // determine whether the order of vertices in the array is clockwise or counter-clockwise
        // this matters for the rendering of the 3D extruded surround mesh
        // implementation adapted via https://stackoverflow.com/a/1165943

        float edgesSum = 0.0f;
        for(int i = 0; i < vertices.Length; i++)
        {
            // handle last case
            if(i+1 == vertices.Length)
            {
                edgesSum = edgesSum + (vertices[0].x - vertices[i].x) * (vertices[0].y + vertices[i].y);
            }
            // handle normal case
            else
            {
                edgesSum = edgesSum + (vertices[i + 1].x - vertices[i].x) * (vertices[i + 1].y + vertices[i].y);
            }
        }

        // edges sum = positive -> clockwise
        // edges sum = negative -> counter-clockwise
        return (edgesSum >= 0.0f) ? true : false;
    }

    /// <summary>
    /// Function to calculate area and centroid of the polygon based on its input vertices.
    /// </summary>
    /// <param name="vertices">Vertices.</param>
    private void calculateAreaAndCentroid(Vector2[] vertices)
    {
        // calculate area and centroid of a polygon
        // implementation adapted via http://paulbourke.net/geometry/polygonmesh/

        // setup temporary variables for calculation
        float doubleArea = 0.0f;
        Vector2 centroid = new Vector2(0.0f, 0.0f);

        // iterate through all vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // handle last case
            if (i + 1 == vertices.Length)
            {
                doubleArea = doubleArea + (vertices[i].x * vertices[0].y - vertices[0].x * vertices[i].y);
                centroid.x = centroid.x + ((vertices[i].x + vertices[0].x) * (vertices[i].x * vertices[0].y - vertices[0].x * vertices[i].y));
                centroid.y = centroid.y + ((vertices[i].y + vertices[0].y) * (vertices[i].x * vertices[0].y - vertices[0].x * vertices[i].y));
            }
            // handle normal case
            else
            {
                doubleArea = doubleArea + (vertices[i].x * vertices[i + 1].y - vertices[i + 1].x * vertices[i].y);
                centroid.x = centroid.x + ((vertices[i].x + vertices[i + 1].x) * (vertices[i].x * vertices[i + 1].y - vertices[i + 1].x * vertices[i].y));
                centroid.y = centroid.y + ((vertices[i].y + vertices[i + 1].y) * (vertices[i].x * vertices[i + 1].y - vertices[i + 1].x * vertices[i].y));
            }
        }

        // set area
        this.polygonArea = (doubleArea < 0) ? doubleArea * -0.5f : doubleArea * 0.5f;

        // set centroid
        float sixTimesArea = doubleArea * 3.0f;
        this.polygonCentroid = new Vector2(centroid.x / sixTimesArea, centroid.y / sixTimesArea);
    }

    /// <summary>
    /// Function to initialize and setup the (meshes of the) prism based on the the PolyExtruder's set properties.
    /// </summary>
    private void initPrism()
    {
        // The prism consists of 3 meshes:
        // 1. bottom mesh with y = 0
        // 2. top mesh with y = dynamically assigned 
        // 3. surrounding polygon connecting 1 and 2 on their outline

        // Note: Meshes will be created as individual Child GameObjects.



        // 1. BOTTOM MESH
        //

        // create bottom GameObject with required components
        GameObject goB = new GameObject();
        goB.transform.parent = this.transform;
        goB.name = "bottom_" + this.prismName;
        MeshFilter mfB = goB.AddComponent<MeshFilter>();
        goB.AddComponent<MeshCollider>();
        bottomMeshRenderer = goB.AddComponent<MeshRenderer>();
        bottomMeshRenderer.material = new Material(Shader.Find("Diffuse"));

        // keep reference to bottom mesh
        this.bottomMesh = mfB.mesh;

        // init helper values to create bottom mesh
        List<Vector2> pointsB = new List<Vector2>();
        List<List<Vector2>> holesB = new List<List<Vector2>>();
        List<int> indicesB = null;
        List<Vector3> verticesB = null;

        // convert original polygon data for bottom mesh
        foreach (Vector2 v in originalPolygonVertices)
        {
            pointsB.Add(v);
        }

        // handle hole data (if existing)
        List<Vector2> holeB = new List<Vector2>();

        // perform TRIANGULATION
        Triangulation.triangulate(pointsB, holesB, DEFAULT_BOTTOM_Y, out indicesB, out verticesB);

        // assign indices and vertices and create mesh
        redrawMesh(this.bottomMesh, verticesB, indicesB);

        // reset mesh collider after (re-)creation
        goB.GetComponent<MeshCollider>().sharedMesh = this.bottomMesh;

        /*
        // generate a simple UVmap
        Vector2[] uvsB = new Vector2[this.bottomMesh.vertices.Length];
        for (int i = 0; i < uvsB.Length; i++)
        {
            uvsB[i] = new Vector2(this.bottomMesh.vertices[i].x, this.bottomMesh.vertices[i].y);
        }
        this.bottomMesh.uv = uvsB;
        */

        // if polygon is being extruded, "flip" bottom polygon
        // (otherwise it would be facing its rendered side "inside" the extrusion, thus not being visible)
        if (this.is3D)
        {
            goB.transform.localScale = new Vector3(-1.0f, -1.0f, -1.0f);
            goB.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }


        // create and render top and surround mesh only if GameObject is created as 3D
        //
        if(this.is3D)
        {
			// 2. TOP MESH
			//
			
			// create top GameObject with required components
			GameObject goT = new GameObject();
			goT.transform.parent = this.transform;
			goT.name = "top_" + this.prismName;
			MeshFilter mfT = goT.AddComponent<MeshFilter>();
			goT.AddComponent<MeshCollider>();
			topMeshRenderer = goT.AddComponent<MeshRenderer>();
			topMeshRenderer.material = new Material(Shader.Find("Diffuse"));
			
			// keep reference to top mesh
			this.topMesh = mfT.mesh;
			
			// init helper values to create top mesh
			List<Vector2> pointsT = new List<Vector2>();
			List<List<Vector2>> holesT = new List<List<Vector2>>();
			List<int> indicesT = null;
			List<Vector3> verticesT = null;
			
			// convert original polygon data for top mesh
			foreach (Vector2 v in originalPolygonVertices)
			{
				pointsT.Add(v);
			}
			
			// handle hole data (if existing)
			List<Vector2> holeT = new List<Vector2>();
			
			// perform TRIANGULATION
			Triangulation.triangulate(pointsT, holesT, DEFAULT_TOP_Y, out indicesT, out verticesT);
						
			// assign indices and vertices and create mesh
			redrawMesh(this.topMesh, verticesT, indicesT);
			
			// reset mesh collider after (re-)creation
			goT.GetComponent<MeshCollider>().sharedMesh = this.topMesh;
			
			/*
            // generate a simple UV map
			Vector2[] uvsT = new Vector2[this.topMesh.vertices.Length];
			for (int i = 0; i < uvsT.Length; i++)
			{
				uvsT[i] = new Vector2(this.topMesh.vertices[i].x, this.topMesh.vertices[i].y);
			}
            this.topMesh.uv = uvsT;
			*/
			

         	
			// 3. SURROUNDING MESH
            //
			
			// create surrounding GameObject with required components
			GameObject goS = new GameObject();
			goS.transform.parent = this.transform;
			goS.name = "surround_" + this.prismName;
			MeshFilter mfS = goS.AddComponent<MeshFilter>();
			goS.AddComponent<MeshCollider>();
			surroundMeshRenderer = goS.AddComponent<MeshRenderer>();
			surroundMeshRenderer.material = new Material(Shader.Find("Diffuse"));
			
			// keep reference to surrounding mesh
			this.surroundMesh = mfS.mesh;
			
			// init helper values to create surrounding mesh
			List<int> indicesS = new List<int>();
			List<Vector3> verticesS = new List<Vector3>();
			
			// prepare bottom and top mesh data to build surrounding mesh
			foreach (Vector3 bv in pointsB)
			{
				verticesS.Add(new Vector3(bv.x, DEFAULT_BOTTOM_Y, bv.y));
			}
			foreach (Vector2 tv in pointsT)
			{
				verticesS.Add(new Vector3(tv.x, DEFAULT_TOP_Y, tv.y));
			}
            			
			// CONSTRUCT TRIANGLES:
			// create an array of integers
			// 0 -> verticesB[0]
			// pointsB.Count -> pointsT[0]
			
			// create 2 triangles (= quad) in each loop
			int indexB = 0;
			int indexT = pointsB.Count;
			int sumQuads = verticesS.Count / 2;

            for (int i = 0; i < sumQuads; i++)
			{
				// handle closing triangles
				if (i == (sumQuads - 1))
				{
                    // second to last triangle (1st in quad)
                    indicesS.Add(indexB);
                    indicesS.Add(0);                // flipped with 3. index
                    indicesS.Add(indexT);           // flipped with 2. index

                    // last triangle (2nd in quad)
                    indicesS.Add(0);
                    indicesS.Add(pointsB.Count);    // flipped with 6. index
                    indicesS.Add(indexT);           // flipped with 5. index
                }
                // handle normal case
                else
				{
                    // triangle 1
                    indicesS.Add(indexB);
                    indicesS.Add(indexB + 1);
                    indicesS.Add(indexT);

                    // triangle 2
                    indicesS.Add(indexB + 1);
                    indicesS.Add(indexT + 1);
                    indicesS.Add(indexT);

                    // increment bottom and top index
                    indexB++;
					indexT++;
				}
			}
			
			// assign indices and vertices and create mesh
			redrawMesh(this.surroundMesh, verticesS, indicesS);

            /*
            // reset mesh collider after (re-)creation (not needed right now since no mesh collider is attached)
			goS.GetComponent<MeshCollider>().sharedMesh = this.surroundMesh;
			
			// generate a simple UV map
			Vector2[] uvsS = new Vector2[this.surroundMesh.vertices.Length];
			for (int i = 0; i < uvsS.Length; i++)
			{
				uvsS[i] = new Vector2(this.surroundMesh.vertices[i].x, this.surroundMesh.vertices[i].y);
			}
            this.surroundMesh.uv = uvsS;
			*/

            // note: for 3D prism, only keep top mesh collider activated (adapt to own preferences this if needed)
            goB.GetComponent<MeshCollider>().enabled = false;
            goT.GetComponent<MeshCollider>().enabled = true;
            goS.GetComponent<MeshCollider>().enabled = false;
        }

        // set height and color
        updateHeight(this.extrusionHeightY);
        updateColor(this.prismColor);
    }

    /// <summary>
    /// Function to redraw the mesh (for instance after it was updated).
    /// </summary>
    /// <param name="mesh">Reference to mesh component that needs to be redrawn.</param>
    /// <param name="vertices">Vector3 list of the the meshes vertices.</param>
    /// <param name="indices">Int list of the meshes vertex indices.</param>
    private void redrawMesh(Mesh mesh, List<Vector3> vertices, List<int> indices)
    {
        // clear prior mesh information
        mesh.Clear();

        // assign vertices and indices representing the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        // sync mesh information
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // init outline (if required)
        if (this.isOutlineRendered) initOutlineRenderer();
    }

    #endregion



    #region MeshManipulator

    /// <summary>
    /// Function to update the height of the prism.
    /// </summary>
    /// <param name="height">Distance between bottom and top (polygon) mesh (= length of extrusion).</param>
    public void updateHeight(float height)
    {
        // keep track of new height
        if(!this.extrusionHeightY.Equals(height)) this.extrusionHeightY = height;

        // update transform
        if(this.is3D) this.gameObject.transform.localScale = new Vector3(1.0f, this.extrusionHeightY, 1.0f);
        //else this.gameObject.transform.localPosition = new Vector3(1.0f, this.extrusionHeightY, 1.0f);
    }

    /// <summary>
    /// Function to update the color of the prism material (default material attached during prism creation proces: Diffuse).
    /// </summary>
    /// <param name="color">Color32 variable representing the new color of the prism meshes.</param>
    public void updateColor(Color32 color)
    {
        // keep track of the new color
        if (!this.prismColor.Equals(color)) this.prismColor = color;

        // update color on meshes
        bottomMeshRenderer.material.color = this.prismColor;
        if (this.is3D)
        {
            topMeshRenderer.material.color = this.prismColor;
            surroundMeshRenderer.material.color = this.prismColor;
        }
    }

    #endregion


    #region OUTLINE_RENDERER

    /// <summary>
    /// Method to initialize the display of an outline for the polygon / prism based on its initial configuration.
    /// </summary>
    /// <returns>True if the outline could be initialized, false if not.</returns>
    public bool initOutlineRenderer()
    {
        // allow initialization only if no (other) LineRenderer component is already attached to the PolyExtruder's GameObject
        if (this.gameObject.GetComponent<LineRenderer>() == true) return false;

        // outline, practically implemented as LineRenderer component, is attached only to the surface polygon
        // if 3D -> top mesh
        // if 2D (not 3D) -> bottom mesh

        // determine LineRenderer's position along the y-axis (in sync with the corresponding mesh position)
        float yPos = DEFAULT_TOP_Y;
        if (!this.is3D) yPos = DEFAULT_BOTTOM_Y;

        // attach LineRenderer component
        LineRenderer outlineRenderer = this.gameObject.AddComponent<LineRenderer>();

        // initialize all LineRenderer component related properties
        outlineRenderer.startWidth = outlineWidth;
        outlineRenderer.endWidth = outlineWidth;
        outlineRenderer.useWorldSpace = false;
        outlineRenderer.material = new Material(Shader.Find("Standard"));
        outlineRenderer.material.color = outlineColor;

        // prepare original polygon vertices for LineRenderer positions
        Vector3[] outlineRendererPositions = new Vector3[this.originalPolygonVertices.Length];
        for (int i = 0; i < this.originalPolygonVertices.Length; i++)
        {
            outlineRendererPositions[i] = new Vector3(this.originalPolygonVertices[i].x, yPos, this.originalPolygonVertices[i].y);
        }

        // update LineRenderer's positions
        outlineRenderer.positionCount = outlineRendererPositions.Length;
        outlineRenderer.loop = true;
        outlineRenderer.SetPositions(outlineRendererPositions);

        return true;
    }

    #endregion
}
