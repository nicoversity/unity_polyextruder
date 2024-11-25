/*
 * PolyExtruderLight.cs
 *
 * Description: Lightweight implementation of the original PolyExtruder.cs class
 *              combining the three original meshes (bottom, top, surround) into one mesh at runtime.
 * 
 * Compared to the original PolyExtruder.cs class, the PolyExtruderLight.cs features the following differences:
 * - PolyExtruderLight is ALWAYS setting up the GameObject as an extruded 3D prism!
 * - Outline Renderer not implemented
 * - Collider Component not implemented
 * 
 * For additional detailed documentation, please refer to the PolyExtruder.cs class.
 *
 * ATTENTION: No holes-support in polygon extrusion (Prism 3D) implemented!
 * Although Triangulation.cs supports "holes" in the 2D polygon, the support of holes as part of the PolyExtruderLight *has not* been implemented in this version.
 * 
 * 
 * === VERSION HISTORY | FEATURE CHANGE LOG ===
 * 2024-11-25:
 * - Initial lightweight implementation.
 * ============================================
 * 
 * 
 * Supported Unity version: 2022.3.20f1 Personal (tested)
 * 
 * Version: 2024.11
 * Author: Nico Reski
 * GitHub: https://github.com/nicoversity
 * 
 */

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class to create a (extruded) polygon based on a collection of vertices.
/// </summary>
public class PolyExtruderLight : MonoBehaviour
{
    #region Properties

    [Header("Prism Configuration")]
    public string prismName;                        // reference to name of the prism
    public Color32 prismColor;                      // reference to prism color
    public float polygonArea;                       // reference to area of (top) polygon
    public Vector2 polygonCentroid;                 // reference to centroid of (top) polygon


    // private properties
    //

    // reference to extrusion height (Y axis)
    // Note: default y (height) of extruded polygon = 1.0f; default y (height) of bottom polygon = 0.0f;
    // -> scaling is applied using the GameObject transform's localScale y-value
    private static readonly float DEFAULT_BOTTOM_Y = 0.0f;
    private static readonly float DEFAULT_TOP_Y = 1.0f;
    private float extrusionHeightY = 1.0f;

    // reference to original input vertices of Polygon in Vector2 Array format
    private Vector2[] originalPolygonVertices;
    public Vector2[] OriginalPolygonVertices { get { return originalPolygonVertices; } }

    // cached references to GameObject Components
    private Transform prismTransform;
    private MeshFilter prismMeshFilter;
    private MeshRenderer prismMeshRenderer;

    #endregion


    #region MeshCreator

    /// <summary>
    /// Create a prism based on the input parameters.
    /// </summary>
    /// <param name="prismName">Name of the prism within the Unity scene.</param>
    /// <param name="height">Height of the prism (= distance along the y-axis between bottom and top mesh).</param>
    /// <param name="vertices">Vector2 Array representing the input data of the polygon.</param>
    /// <param name="color">Color of the prism's material.</param>
    public void createPrism(string prismName, float height, Vector2[] vertices, Color32 color)
    {
        // set data
        this.prismName = name;
        this.extrusionHeightY = height;
        this.originalPolygonVertices = vertices;
        this.prismColor = color;
        this.polygonArea = 0.0f;
        this.polygonCentroid = new Vector2(0.0f, 0.0f);
        this.prismTransform = this.transform;
        this.prismMeshFilter = this.gameObject.AddComponent<MeshFilter>();
        this.prismMeshRenderer = this.gameObject.AddComponent<MeshRenderer>();

        // handle vertex order
        bool vertexOrderClockwise = areVerticesOrderedClockwise(this.originalPolygonVertices);
        if (!vertexOrderClockwise) System.Array.Reverse(this.originalPolygonVertices);

        // calculate area and centroid
        bool isAreaAndCentroidSet = calculateAreaAndCentroid(this.originalPolygonVertices);

        // initialize meshes for prism
        if(isAreaAndCentroidSet) initPrism();
        else
        {
            // log warning that this prism could not be created
            Debug.LogWarning("[PolyExtruderLight] CreatePrism not performed for prism with name: " + this.prismName);
        }
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
    /// <returns>Returns <c>true</c> once the area and centroid of the polygon are set.</returns>
    private bool calculateAreaAndCentroid(Vector2[] vertices)
    {
        // calculate area and centroid of a polygon
        // implementation adapted via http://paulbourke.net/geometry/polygonmesh/

        // setup temporary variables for calculation
        double doubleArea = 0.0;
        double centroidX = 0.0;
        double centroidY = 0.0;

        // iterate through all vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // handle last case
            if (i + 1 == vertices.Length)
            {
                doubleArea = doubleArea + ((double)vertices[i].x * (double)vertices[0].y - (double)vertices[0].x * (double)vertices[i].y);
                centroidX = centroidX + (((double)vertices[i].x + (double)vertices[0].x) * ((double)vertices[i].x * (double)vertices[0].y - (double)vertices[0].x * (double)vertices[i].y));
                centroidY = centroidY + (((double)vertices[i].y + (double)vertices[0].y) * ((double)vertices[i].x * (double)vertices[0].y - (double)vertices[0].x * (double)vertices[i].y));
            }
            // handle normal case
            else
            {
                doubleArea = doubleArea + ((double)vertices[i].x * (double)vertices[i + 1].y - (double)vertices[i + 1].x * (double)vertices[i].y);
                centroidX = centroidX + (((double)vertices[i].x + (double)vertices[i + 1].x) * ((double)vertices[i].x * (double)vertices[i + 1].y - (double)vertices[i + 1].x * (double)vertices[i].y));
                centroidY = centroidY + (((double)vertices[i].y + (double)vertices[i + 1].y) * ((double)vertices[i].x * (double)vertices[i + 1].y - (double)vertices[i + 1].x * (double)vertices[i].y));
            }
        }

        // set area
        double polygonArea = (doubleArea < 0) ? doubleArea * -0.5 : doubleArea * 0.5;
        this.polygonArea = (float)polygonArea;

        // set centroid
        double sixTimesArea = doubleArea * 3.0;
        this.polygonCentroid = new Vector2((float)(centroidX / sixTimesArea), (float)(centroidY / sixTimesArea));

        // return statement, indicating whether or not area and centroid could be successfully calculated, i.e.,
        // false if calculated area is (near) 0, or
        // true if calculated area is > 0
        if (Mathf.Approximately(0.0f, this.polygonArea)) return false;
        else return true;
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
        // 4. combine bottom, top, and surrounding meshed into one, plus cleanup


        // 1. BOTTOM MESH
        //

        // create bottom GameObject with required components
        GameObject goB = new GameObject();
        goB.transform.parent = this.transform;
        goB.name = "bottom_" + this.prismName;
        MeshFilter mfB = goB.AddComponent<MeshFilter>();

        // keep reference to bottom mesh
        UnityEngine.Mesh bottomMesh = mfB.mesh;

        // init helper values to create bottom mesh
        List<Vector2> pointsB = new List<Vector2>();
        List<List<Vector2>> holesB = new List<List<Vector2>>();
        List<int> indicesB = null;
        List<Vector3> verticesB = null;

        // convert original polygon data for bottom mesh
        foreach (Vector2 v in originalPolygonVertices)
        {
            pointsB.Add(v - polygonCentroid);   // consider calculated polygon centroid as anchor at the coordinate system's origin (0,0) for appropriate mesh manipulations at runtime (e.g., scaling)
            //pointsB.Add(v);                   // use original input vertices as is
        }

        // handle hole data (if existing)
        List<Vector2> holeB = new List<Vector2>();

        // perform TRIANGULATION
        Triangulation.triangulate(pointsB, holesB, DEFAULT_BOTTOM_Y, out indicesB, out verticesB);

        // assign indices and vertices and create mesh
        redrawMesh(bottomMesh, verticesB, indicesB);

        // if polygon is being extruded, "flip" bottom polygon
        // (otherwise it would be facing its rendered side "inside" the extrusion, thus not being visible)
        goB.transform.localScale = new Vector3(-1.0f, -1.0f, -1.0f);
        goB.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);


        // 2. TOP MESH
        //
        
        // create top GameObject with required components
        GameObject goT = new GameObject();
        goT.transform.parent = this.transform;
        goT.name = "top_" + this.prismName;
        MeshFilter mfT = goT.AddComponent<MeshFilter>();
        
        // keep reference to top mesh
        UnityEngine.Mesh topMesh = mfT.mesh;
        
        // init helper values to create top mesh
        List<Vector2> pointsT = new List<Vector2>();
        List<List<Vector2>> holesT = new List<List<Vector2>>();
        List<int> indicesT = null;
        List<Vector3> verticesT = null;
        
        // convert original polygon data for top mesh
        foreach (Vector2 v in originalPolygonVertices)
        {
            pointsT.Add(v - polygonCentroid);   // consider calculated polygon centroid as anchor at the coordinate system's origin (0,0) for appropriate mesh manipulations at runtime (e.g., scaling)
            //pointsT.Add(v);                   // use original input vertices as is
        }
        
        // handle hole data (if existing)
        List<Vector2> holeT = new List<Vector2>();
        
        // perform TRIANGULATION
        Triangulation.triangulate(pointsT, holesT, DEFAULT_TOP_Y, out indicesT, out verticesT);
                    
        // assign indices and vertices and create mesh
        redrawMesh(topMesh, verticesT, indicesT);

   
        // 3. SURROUNDING MESH
        //
        
        // create surrounding GameObject with required components
        GameObject goS = new GameObject();
        goS.transform.parent = this.transform;
        goS.name = "surround_" + this.prismName;
        MeshFilter mfS = goS.AddComponent<MeshFilter>();
        
        // keep reference to surrounding mesh
        UnityEngine.Mesh surroundMesh = mfS.mesh;
        
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
        redrawMesh(surroundMesh, verticesS, indicesS);


        // 4. MESH COMBINATION (FOR LIGHTWEIGHT IMPLEMENTATION)
        //

        // essentially: add MeshFilter and MeshRenderer to the GameObject that holds the PolyExtruderLight Component,
        // instead of having 3 separate Child GameObjects
        // utilize: [ https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html ]

        // prepare CombineInstances from current bottom, surround, and top MeshFilters
        MeshFilter[] meshFilters = new MeshFilter[] { mfB, mfS, mfT };
        CombineInstance[] combineInstances = new CombineInstance[3];
        combineInstances[0].mesh = mfB.sharedMesh;
        combineInstances[0].transform = mfB.transform.localToWorldMatrix;
        combineInstances[1].mesh = mfS.sharedMesh;
        combineInstances[1].transform = mfS.transform.localToWorldMatrix;
        combineInstances[2].mesh = mfT.sharedMesh;
        combineInstances[2].transform = mfT.transform.localToWorldMatrix;

        // setup (new combined) main Mesh
        UnityEngine.Mesh mainMesh = new UnityEngine.Mesh();
        mainMesh.name = this.prismName + "-combined_mesh";
        mainMesh.CombineMeshes(combineInstances);

        // assign combined Mesh to MeshFilter Component and set material
        this.prismMeshFilter.mesh = mainMesh;
        this.prismMeshRenderer.material = new Material(Shader.Find("Standard"));
        // this.prismMeshFilter.sharedMesh = mainMesh;
        // this.prismMeshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        // destroy generated separate GameObjects containing bottom, surround, and top Mesh information
        GameObject.Destroy(goB);
        GameObject.Destroy(goS);
        GameObject.Destroy(goT);

        // set height and color, and update position in accordance to prism 2D centroid (x/z)
        updateHeight(this.extrusionHeightY);
        updateColor(this.prismColor);
        setAnchorPosToCentroid(); 
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
        if (!Mathf.Approximately(this.extrusionHeightY, height)) this.extrusionHeightY = height;

        // update transform
        this.prismTransform.localScale = new Vector3(1.0f, this.extrusionHeightY, 1.0f);
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
        this.prismMeshRenderer.material.color = this.prismColor;
    }

    /// <summary>
    /// Function to update the local position of the prism, setting it's internal anchor (0,0) to the calculated polygon centroid and thus positining the prism in accordance to the original input vertices.
    /// </summary>
    private void setAnchorPosToCentroid()
    {
        this.gameObject.transform.localPosition = new Vector3(this.polygonCentroid.x, DEFAULT_BOTTOM_Y, this.polygonCentroid.y);
    }

    #endregion
}
