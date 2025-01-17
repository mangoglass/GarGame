using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCManager))]
public class MapCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject[] roadTiles;
    [SerializeField]
    private GameObject[] buildings;
    [SerializeField]
    private GameObject[] trees;

    [SerializeField]
    private GameObject gateLight;
    [SerializeField]
    private GameObject grassPrefab;
    [SerializeField]
    private GameObject groundPrefab;
    [SerializeField]
    private GameObject[] trashCans;
    [SerializeField]
    private GameObject surfacePrefab;
    [Range(0f, 1f)]
    [SerializeField]
    private float spawnProbability = 0.5f;

    private List<Transform> spawnPoints;
    private List<Transform> trashCanPositions;
    private NPCManager npcManager;
    float minDist = 7f;
    float scale = 1f;


    public GameObject[] clouds;

    // Start is called before the first frame update
    void Awake()
    {

        //Vector3[] points = new Vector3[]{
        //    new Vector3(-40,0,-40),
        //    new Vector3(-40,0,40),
        //    new Vector3(40,0,40),
        //    new Vector3(40,0,-40),

        //};
        //CreateMap(points, temporary);

        npcManager = GetComponent<NPCManager>();
        spawnPoints = new List<Transform>();
        trashCanPositions = new List<Transform>();


    }


    private void InitializeNPCs(Transform parentObject)
    {
        npcManager.SetUp(spawnPoints.ToArray(), trashCanPositions.ToArray(), parentObject);
    }

    public void CreateMap(GameObject parent)
    {
        Vector3[] points = new Vector3[]{
            new Vector3(-40,0,-40),
            new Vector3(-40,0,40),
            new Vector3(40,0,40),
            new Vector3(40,0,-40), };

        CreateBuildingOutline(points, parent);



        // I cannot find the map function :)
        // Maybe this baddie of a language doesn't have a map function 
        Vector3[] ground_points = new Vector3[]{
            new Vector3(-45,0,-45),
            new Vector3(-45,0,45),
            new Vector3(45,0,45),
            new Vector3(45,0,-45), };

        CreateGround(ground_points, parent.transform);


        Vector3[] road_points = new Vector3[]{
            new Vector3(-35,0,-35),
            new Vector3(-35,0,35),
            new Vector3(35,0,35),
            new Vector3(35,0,-35), };
        CreateRoad(road_points, parent);
        //GameObject gp = CreateObject(grassPrefab, new Vector3(0, -10.0F, 0), Quaternion.identity, 1f, parent);
        //gp.transform.localScale = new Vector3(20f, 0.1F, 20f);

        CreateTreesInSquare(points[0] + new Vector3(minDist + 2, 0, minDist + 2), points[2] - new Vector3(minDist + 2, 0, minDist + 2), 7, parent);
        CreateCloudsInSquare(points[0] - new Vector3(20, 0, 20), points[2] + new Vector3(20, 0, 20), 15, parent);


    }

    //Använd den här funktionen för att skapa trashcans, måste kommentera bort raden längre upp 
    public void CreateMapTrashCans(Vector3[] trashcan_positions, GameObject parent)
    {

        CreateTrashCans(trashcan_positions, parent.transform);

        // UPDATE NAVMESH
        GameObject surface = Instantiate(surfacePrefab, parent.transform);
        surface.GetComponent<NavMeshSurface>().BuildNavMesh();


        InitializeNPCs(parent.transform);
    }

    public void RemoveMap()
    {
        npcManager.Stop();
    }

    //TODO: Rodrigo & Natalie, ni kanske vill utveckla denna funtion. Den tar just nu bara in en papperskorg.
    private void CreateTrashCans(Vector3[] trashcan_positions, Transform parent)
    {
        for (int i = 0; i < trashcan_positions.Length; i++)
        {
            GameObject trashCan = Instantiate(trashCans[0], parent);
            trashCan.transform.localPosition = trashcan_positions[i];
            trashCan.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            trashCanPositions.Add(trashCan.transform);
        }
    }

    private void CreateGround(Vector3[] points, Transform parent)
    {
        if (points.Length == 4)
        {
            GameObject ground = Instantiate(groundPrefab, parent);
            Mesh mesh = new Mesh();
            mesh.vertices = points;
            int[] triangles = { 0, 1, 2, 2, 3, 0 };
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            ground.GetComponent<MeshFilter>().mesh = mesh;
            ground.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
    
    void CreateRoad(Vector3[] points, GameObject parent)
    {
        GameObject straight_tile = roadTiles[0];
        GameObject curve_tile = roadTiles[1];

        float tile_size = 5;


        for (int i = 0; i < points.Length; i++)
        {

            Vector3 a = points[i];
            Vector3 b = points[(i + 1) % points.Length];

            Vector3 diff = (b - a);

            int n = Mathf.FloorToInt(diff.magnitude / tile_size);

            float xStep = diff.x / n;
            float zStep = diff.z / n;

            Quaternion curve_rot = Quaternion.Euler(0, 90 + i * 90, 0);
            Vector3 curve_newPos = points[i] + new Vector3(0, 0.4f, 0);

            GameObject curve_road = CreateObject(curve_tile, curve_newPos, curve_rot, scale, parent);

            for (int j = 1; j < n; j++)
            {
                Vector3 newPos = points[i] + new Vector3(xStep * j, 0.4f, zStep * j);


                Quaternion rot = Quaternion.identity;
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
                {
                    rot = Quaternion.Euler(0, 90, 0);
                }


                GameObject straightRoad = CreateObject(straight_tile, newPos, rot, scale, parent);
                //newBuilding.transform.localScale = new Vector3(
                //    Random.Range(0.9f, 1.2f),
                //    Random.Range(0.8f, 1.2f),
                //    1);

                if (j % 2 == 1)
                {
                    Vector3 gl_pos = (points[i] + new Vector3(xStep * j , 0, zStep * j)) * 0.93f;

                    Quaternion gl_rot = Quaternion.Euler(0,  -90 + i * 90, 0);
                    //if (Mathf.Abs(diff.x) < Mathf.Abs(diff.z))
                    //{
                    //    gl_rot = Quaternion.Euler(0, 90, 0);
                    //}

                    GameObject gate_light = CreateObject(gateLight, gl_pos, gl_rot, scale, parent);

                }
            }


        }
    }

    void CreateCloudsInSquare(Vector3 a, Vector3 b, int n, GameObject parent)
    {
        Vector3 diff = (b - a);
        float xRange = diff.x;
        float zRange = diff.z;

        for (int i = 0; i < n; i++)
        {
            float xf = Random.Range(0f, 1f);
            float zf = Random.Range(0f, 1f);

            Vector3 pos = a + new Vector3(xf * xRange, Random.Range(40, 80), zf * zRange);
            //CreateCloud(pos, Quaternion.identity, scale, parent);
            CreateObject(clouds, pos, Quaternion.identity, scale, parent);

        }
    }

    void CreateTreesInSquare(Vector3 a, Vector3 b, int n_cells, GameObject parent)
    {
        float scale = 1.7f;

        Vector3 diff = (b - a);


        //Vector3 newAxisX = points[1] - points[0];
        //Vector3 newAxisZ = points[3] - points[0];


        float xRange = diff.x;
        float zRange = diff.z;

        float x_step = xRange / n_cells;
        float z_step = zRange / n_cells;

        for (int x = 0; x < n_cells; x++)
        {
            for (int z = 0; z < n_cells; z++)
            {
                bool trash = false;
                foreach (Transform trashcan in trashCanPositions)
                {
                    Vector3 pos = trashcan.localPosition;
                    if (pos.x >= a.x + x * x_step && pos.x <= a.x + (x + 1) * x_step &&
                        pos.z >= a.z + z * z_step && pos.z <= a.z + (z + 1) * z_step)
                    {
                        trash = true;
                        break;
                    }
                }
                if (trash) continue;

                int n_trees = 4 - Mathf.FloorToInt(Mathf.Sqrt(Random.Range(1, 17)));

                for (int i = 0; i < n_trees; i++)
                {
                    float xf = Random.Range(0f, 1f);
                    float zf = Random.Range(0f, 1f);
                    Vector3 pos = a + new Vector3((x + xf) * x_step, 0, (z + zf) * z_step);
                    CreateObject(trees, pos, Quaternion.identity, scale, parent);
                }
            }
        }

        //for (int i = 0; i < n; i++) {
        //    float xf = Random.Range(0f, 1f);
        //    float zf = Random.Range(0f, 1f);

        //    Vector3 pos = a + new Vector3(xf * xRange, 0, zf * zRange);
        //    //CreateTree(pos, Quaternion.identity, scale,parent);
        //    CreateObject(trees, pos, Quaternion.identity, scale, parent);

        //    //Vector3 pos = a + new Vector3(newAxisX * xRange, 0, newAxisZ * zRange);

        //}




    }

    void CreateBuildingOutline(Vector3[] points, GameObject parent)
    {

        //Vector3 minDistVec = new Vector3(minDist, 0, minDist);
        for (int i = 0; i < points.Length; i++)
        {

            Vector3 a = points[i];
            Vector3 b = points[(i + 1) % points.Length];

            Vector3 diff = (b - a);

            //int n = Mathf.Min(Mathf.FloorToInt(diff.x / minDist), Mathf.FloorToInt(diff.z / minDist));
            int n = Mathf.FloorToInt(diff.magnitude / minDist);

            float xStep = diff.x / n;
            float zStep = diff.z / n;

            for (int j = 0; j < n; j++)
            {
                Vector3 newPos = points[i] + new Vector3(xStep * j, 0, zStep * j);
                //Need to fix rotation
                //CreateBuilding(newPos, Quaternion.identity, scale , parent);

                Quaternion rot = Quaternion.identity;
                if (Mathf.Abs(diff.x) < Mathf.Abs(diff.z))
                {
                    rot = Quaternion.Euler(0, 90, 0);
                }

                if(j > 0)
                {
                    Vector3 disp;
                    if (Mathf.Abs(newPos.x) > Mathf.Abs(newPos.z))
                    {
                        disp = new Vector3(-Mathf.Sign(newPos.x) * 1.31f, 0.55f, 0);
                    } else
                    {
                        disp = new Vector3(0, 0.55f, -Mathf.Sign(newPos.z) * 1.31f);
                    }

                    Vector3 pathPos = points[i] + new Vector3(xStep * j, 0, zStep * j) + disp; ///new Vector3(0,0.55f,1.2f);
                    GameObject pathTile = CreateObject(roadTiles[0], pathPos, rot, scale, parent);
                }


                GameObject newBuilding = CreateObject(buildings, newPos, rot, scale, parent);
                newBuilding.transform.localScale = new Vector3(
                    Random.Range(0.9f, 1.2f),
                    Random.Range(0.8f, 1.2f),
                    1);
                //TODO LÄGG TILL ALVAROS RANDOM SAK HÄR FÖR SPAWNPOINTS
                if (Random.Range(0f, 1f) <= spawnProbability)
                {
                    spawnPoints.Add(newBuilding.transform);
                }
            }


        }

    }

    GameObject CreateObject(GameObject myObject, Vector3 position, Quaternion rotation, float scale, GameObject parent)
    {
        GameObject newObject = Instantiate(myObject, parent.transform);
        newObject.transform.localRotation = rotation;
        newObject.transform.localPosition = position;

        newObject.transform.localScale = new Vector3(scale, scale, scale);
        return newObject;
    }

    GameObject CreateObject(GameObject[] objects, Vector3 position, Quaternion rotation, float scale, GameObject parent)
    {
        GameObject myObject = GetRandomFromList(objects);
        GameObject newObject = Instantiate(myObject, parent.transform);
        newObject.transform.localRotation = rotation;
        newObject.transform.localPosition = position;

        newObject.transform.localScale = new Vector3(scale, scale, scale);
        return newObject;
    }

    GameObject GetRandomFromList(GameObject[] objects)
    {
        return objects[Random.Range(0, objects.Length)];
    }
}
