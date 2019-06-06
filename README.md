# Unity - PolyExtruder

The purpose of this project is to provide the functionality to create custom meshes (polygons) in Unity3D based on a collection (array) of vertices directly at runtime. These 2D meshes are created along the x- and z-dimensions in the 3D space. Furthermore, the created custom mesh can be *extruded* (into a 3D prism) along the y-dimension in the 3D space.

#### Background

Some of my research work required me to visualize country borders (geo-spatial coordinates; received from various open data sources) as individual meshes (2D polygons / 3D prisms) in the 3D space in Unity3D. Some examples of the usage of this project are provided beneath in the section **Screenshots - SCB Kommun RT90**, demonstrating the visualization of all individual municipalities in the country of Sweden.

## Features

### Triangulation.cs

The `Triangulation.cs` class features a partial implementation of the original [Triangle](http://www.cs.cmu.edu/~quake/triangle.html) and [Triangle.NET](https://archive.codeplex.com/?p=triangle) libraries in order to create render triangles for a custom mesh. The implemented triangulation supports holes in the mesh.

### PolyExtruder.cs

The `PolyExtruder.cs` class is responsible for handling the input data and creating all Unity3D GameObjects (incl. the actual mesh; 2D polygon / 3D prism) using the features provided through `Triangulation.cs` class in the process. Created 3D prisms (extruded 2D polygons) consist of three GameObjects, namely 1) the bottom mesh (y = 0), 2) the top mesh (y = dynamically assigned; greater than 0), and 3) the surrounding mesh connecting 1 and 2 on their outline accordingly.

Furthermore, the `PolyExtruder.cs` class provides some *quality-of-life* features, such as:

- select whether the mesh should be 2D (polygon) or 3D (extruded, prism)
- calculation of the mesh's (2D polygon's) area
- calculation of the mesh's (2D polygon's) centroid
- set extrusion length ("height")

The main idea is to visualize the 2D input data along the x- and z-dimensions, while the (potential) extrusion is always conducted along the y-dimension. 

## Dependencies

This project has been built using the following specifications:

* OS X 10.14.5
* [Unity3D](https://unity3d.com) 2019.1.5f1 Personal (OS X release)

*Note:* Generally, Unity source code should work also within their Windows counterparts. Please check out the above stated dependencies for troubleshooting.

### Resources

Additional resources used to create this project have been accessed as follows:

* (Original) Triangle library implementation *by* Jonathan Richard Shewchuk ([project web page](http://www.cs.cmu.edu/~quake/triangle.html))
* Triangle.NET *by* Christian Woltering ([CodePlex Archive](https://archive.codeplex.com/?p=triangle), [GitHub snapshot](https://github.com/garykac/triangle.net))
* Using Triangle.NET in Unity ([YouTube video; not available anymore as of 2019-06-04](https://www.youtube.com/watch?v=wByVhzokWPo))
* Determine order (clockwise vs. counter-clockwise) of input vertices ([StackOverflow](https://stackoverflow.com/a/1165943))
* Polygons and meshes *by* Paul Bourke ([project web page](http://paulbourke.net/geometry/polygonmesh/))
* Geo-spatial data about the island of Gotland (Sweden) ([Swedish Statistiska centralbyrån (SCB); accessed 2019-02-06](https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/digitala-granser/))

## How to use

#### Import assets to Unity3D project

In order to add the features provided by this project to your Unity3D project, I recommend to add the assets by simply importing the pre-compiled `nicoversity-unity_polyextruder.unitypackage`. Alternatively, the repository directory `unity_src` features a directory titled `UnityPolyExtruder`, which contains an already setup Unity3D project (that was used to export the provided `.unitypackage` in the first place).

#### PolyExtruder.cs class

```cs
// prepare data and options
Vector2[] MyCustomMeshData = new Vector2[]
{
    new Vector2(0.0f, 0.0f),
    new Vector2(10.0f, 0.0f),
    new Vector2(10.0f, 10.0f),
    // ... and more vertices
};
float extrusionHeight = 10.0f;
bool is3D = true;

// create new GameObject (as a child)
GameObject polyExtruderGO = new GameObject();
polyExtruderGO.transform.parent = this.transform;

// add PolyExtruder script to newly created GameObject,
// keep track of its reference, and name it
PolyExtruder polyExtruder = polyExtruderGO.AddComponent<PolyExtruder>();
polyExtruderGO.name = "MyCustomMeshName";

// run poly extruder according to input data
polyExtruder.createPrism(polyExtruderGO.name, extrusionHeight, MyCustomMeshData, Color.grey, is3D);

// access calculated area and centroid
float area = polyExtruder.polygonArea;
Vector2 centroid = polyExtruder.polygonCentroid;
```

#### Further documentation

All Unity3D scripts in this project are well documented directly within the source code. Please refer directly to the individual scripts to get a better understanding of the implementation.

### Examples

The imported Unity3D assets provide two demonstration scenes:

- `TriangleNET_Test.unity`, illustrating and testing the implementation of the `Triangulation.cs` class via `TriangulationTest.cs` script.
- `PolyExtruder_Demo.unity`, illustrating the usage of the `PolyExtruder.cs` class via `PolyExtruderDemo.cs` script. The `PolyExtruderDemo.cs` script allows the user to make selections using the Unity3D Inspector accordingly to a) select an example data set for the custom mesh creation (Triangle, Square, Cross, SCB Kommun RT90 Gotland), b) indicate whether the custom mesh should be created in 2D (polygon) or 3D (prism), c) the length ("height") of the extrusion, and d) whether the extrusion length should be dynamically scaled at runtime (oscillated movement example). 

Please refer to these scenes and scripts to learn more about the examples.

### Screenshots - Example Data

Following, some visual impressions of the included example data, visualized using the `Triangulation.cs` and `PolyExtruder.cs` classes.

#### Triangulation Test: Cross
![Triangulation Test: Cross](docs/test_triangulation_cross.png)

#### PolyExtruder: Cross 3D
![Cross 3D](docs/demo_cross_3D.png)

#### PolyExtruder: Triangle 3D
![Triangle 3D](docs/demo_triangle_3D.png)

#### PolyExtruder: Square 3D
![Square 3D](docs/demo_square_3D.png)

#### PolyExtruder: SCB Kommun RT90 Gotland 3D
![SCB Kommun RT90 Gotland 3D](docs/demo_SCB_Kommun_RT90_Gotland_3D.png)

#### PolyExtruder: SCB Kommun RT90 Gotland 3D (movement enabled)
![SCB Kommun RT90 Gotland 3D with movement enabled](docs/demo_SCB_Kommun_RT90_Gotland_3D_movementEnabled.gif)

### Screenshots - SCB Kommun RT90

Following, some visual impressions of the earlier stated use case of visualizing all municipalities in the country of Sweden (see *Background*) using the `PolyExtruder.cs` class. The data has been received from the [Swedish Statistiska centralbyrån (SCB)](https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/digitala-granser/) (accessed: 2019-02-06; **Note:** The data is *not* included as part of this project).

#### PolyExtruder: SCB Kommun RT90 2D
![SCB Kommun RT90 2D](docs/dataNotIncluded_demo_SCB_Kommun_RT90_2D.png)

#### PolyExtruder: SCB Kommun RT90 3D

A random extrusion length ("height") for each municipality has been applied to emphasize the 3D scenario.

![SCB Kommun RT90 3D](docs/dataNotIncluded_demo_SCB_Kommun_RT90_3D.png)

## Known issues

#### Triangulation.cs

1. The function `public static bool triangulate(...)` always returns `true`. In the future, an error list could be implemented to capture errors that occur during the triangulation process.

#### PolyExtruder.cs

1. No holes-support for extrusion (3D prism) is implemented. Although the `Triangulation.cs` script supports holes in the 2D polygon mesh, the support for holes as part of the `PolyExtruder.cs` class *has not* been implemented in this version.
 
## License
MIT License, see [LICENSE.md](LICENSE.md)
