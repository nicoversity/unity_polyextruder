/*
 * PolyExtruderLightDemo.cs
 *
 * Description: Class to demonstrate the application and functionalities of the PolyExtruderLight.cs script.
 * 
 * Supported Unity version: 2022.3.20f1 Personal (tested)
 * 
 * Version: 2024.11
 * Author: Nico Reski
 * GitHub: https://github.com/nicoversity
 * 
 */

using UnityEngine;

/// <summary>
/// Class to demonstrate the PolyExtruderLight class' functionalities.
/// </summary>
public class PolyExtruderLightDemo : MonoBehaviour
{
    // enum for selecting example data (to be selected via Unity Inspector)
    public enum ExampleData { Triangle, Square, Cross, SCB_Gotland };
    public ExampleData exampleData;

    // set up options (adjusted via Unity Inspector)
    public bool enableMovement = false;
    public float extrusionHeight = 10.0f;

    // reference to setup example poly extruder 
    private PolyExtruderLight polyExtruderLight;

    void Start()
    {
        // create new GameObject (as a child)
        GameObject polyExtruderGO = new GameObject();
        polyExtruderGO.transform.parent = this.transform;

        // add PolyExtruder script to newly created GameObject and keep track of its reference
        polyExtruderLight = polyExtruderGO.AddComponent<PolyExtruderLight>();

        // run poly extruder according to selected example data
        switch (exampleData)
        {
            case ExampleData.Triangle:
                polyExtruderGO.name = "Triangle";
                polyExtruderLight.createPrism(polyExtruderGO.name, extrusionHeight, MeshDataTriangle, Color.grey);
                break;
            case ExampleData.Square:
                polyExtruderGO.name = "Square";
                polyExtruderLight.createPrism(polyExtruderGO.name, extrusionHeight, MeshDataSquare, Color.grey);
                break;
            case ExampleData.Cross:
                polyExtruderGO.name = "Cross";
                polyExtruderLight.createPrism(polyExtruderGO.name, extrusionHeight, MeshDataCross, Color.grey);
                break;
            default:
            case ExampleData.SCB_Gotland:
                polyExtruderGO.name = "SCB_Kommun_RT90_Gotland";
                polyExtruderLight.createPrism(polyExtruderGO.name, extrusionHeight, MeshDataGotland, Color.grey);
                break;
        }
    }

    void Update()
    {
        // if movement is selected (via Unity Inspector), oscillate height accordingly
        if(enableMovement)
        {
            polyExtruderLight.updateHeight((Mathf.Sin(Time.time) + 1.0f) * this.extrusionHeight);
        }
    }


    #region EXAMPLE_DATA

    public static Vector2[] MeshDataTriangle = new Vector2[]
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(10.0f, 0.0f),
        new Vector2(10.0f, 10.0f),
    };

    public static Vector2[] MeshDataSquare = new Vector2[]
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(10.0f, 0.0f),
        new Vector2(10.0f, 10.0f),
        new Vector2(0.0f, 10.0f)
    };

    // copied from TriangulationTest.cs
    public static Vector2[] MeshDataCross = new Vector2[]
    {
        new Vector2(10.0f,  0.0f),
        new Vector2(20.0f,  0.0f),
        new Vector2(20.0f, 10.0f),
        new Vector2(30.0f, 10.0f),
        new Vector2(30.0f, 20.0f),
        new Vector2(20.0f, 20.0f),
        new Vector2(20.0f, 30.0f),
        new Vector2(10.0f, 30.0f),
        new Vector2(10.0f, 20.0f),
        new Vector2( 0.0f, 20.0f),
        new Vector2( 0.0f, 10.0f),
        new Vector2(10.0f, 10.0f)
    };

    // Data about the island of "Gotland" accessed and exported from the
    // Swedish Statistiska centralbyrån (SCB) (accessed: 2019-02-06)
    // Source: https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/digitala-granser/
    public static Vector2[] MeshDataGotland = new Vector2[]
    {
        new Vector2(18.480383f, 57.836077f),
        new Vector2(18.537713f, 57.845860f),
        new Vector2(18.584063f, 57.852903f),
        new Vector2(18.607002f, 57.894639f),
        new Vector2(18.653908f, 57.914347f),
        new Vector2(18.706412f, 57.929932f),
        new Vector2(18.770207f, 57.849914f),
        new Vector2(18.818223f, 57.916793f),
        new Vector2(18.859187f, 57.938769f),
        new Vector2(18.999720f, 57.919358f),
        new Vector2(19.054636f, 57.941874f),
        new Vector2(19.074290f, 57.982045f),
        new Vector2(19.149867f, 58.004555f),
        new Vector2(19.233429f, 57.988071f),
        new Vector2(19.288897f, 57.990428f),
        new Vector2(19.305797f, 57.968883f),
        new Vector2(19.276212f, 57.951471f),
        new Vector2(19.201451f, 57.952655f),
        new Vector2(19.139892f, 57.941688f),
        new Vector2(19.114865f, 57.927508f),
        new Vector2(19.134017f, 57.902146f),
        new Vector2(19.093910f, 57.861967f),
        new Vector2(19.029708f, 57.842096f),
        new Vector2(18.995426f, 57.830518f),
        new Vector2(18.997464f, 57.793935f),
        new Vector2(18.932950f, 57.794079f),
        new Vector2(18.923423f, 57.731188f),
        new Vector2(18.848564f, 57.730376f),
        new Vector2(18.797358f, 57.751154f),
        new Vector2(18.784223f, 57.697686f),
        new Vector2(18.748640f, 57.626125f),
        new Vector2(18.782142f, 57.614165f),
        new Vector2(18.766906f, 57.553601f),
        new Vector2(18.734120f, 57.523658f),
        new Vector2(18.776351f, 57.474543f),
        new Vector2(18.839200f, 57.449095f),
        new Vector2(18.900375f, 57.440142f),
        new Vector2(18.867042f, 57.397533f),
        new Vector2(18.789061f, 57.384274f),
        new Vector2(18.725852f, 57.375163f),
        new Vector2(18.654748f, 57.317569f),
        new Vector2(18.655439f, 57.286577f),
        new Vector2(18.694117f, 57.270529f),
        new Vector2(18.656802f, 57.235525f),
        new Vector2(18.543477f, 57.214247f),
        new Vector2(18.527367f, 57.193782f),
        new Vector2(18.383797f, 57.136757f),
        new Vector2(18.424005f, 57.126115f),
        new Vector2(18.318466f, 57.084012f),
        new Vector2(18.319946f, 57.020255f),
        new Vector2(18.372498f, 57.006783f),
        new Vector2(18.300445f, 56.952803f),
        new Vector2(18.189577f, 56.912896f),
        new Vector2(18.124354f, 56.916431f),
        new Vector2(18.180707f, 56.986331f),
        new Vector2(18.192386f, 57.027179f),
        new Vector2(18.258461f, 57.048989f),
        new Vector2(18.283408f, 57.077838f),
        new Vector2(18.273844f, 57.102254f),
        new Vector2(18.209382f, 57.074859f),
        new Vector2(18.188153f, 57.082046f),
        new Vector2(18.204969f, 57.140653f),
        new Vector2(18.160206f, 57.158856f),
        new Vector2(18.131182f, 57.173939f),
        new Vector2(18.136734f, 57.251644f),
        new Vector2(18.082734f, 57.261472f),
        new Vector2(18.097160f, 57.300274f),
        new Vector2(18.149878f, 57.332350f),
        new Vector2(18.165021f, 57.396553f),
        new Vector2(18.129032f, 57.423099f),
        new Vector2(18.110510f, 57.475473f),
        new Vector2(18.090931f, 57.524288f),
        new Vector2(18.108770f, 57.562839f),
        new Vector2(18.191143f, 57.614342f),
        new Vector2(18.245287f, 57.626307f),
        new Vector2(18.314267f, 57.677030f),
        new Vector2(18.390324f, 57.750775f),
        new Vector2(18.425982f, 57.813300f),
        new Vector2(18.480383f, 57.836077f),
    };

    #endregion
}
