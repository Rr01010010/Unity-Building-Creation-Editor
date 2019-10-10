using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

#region custom editor button

[UnityEditor.CustomEditor(typeof(NodeCreator))]
public class NodeCreatorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NodeCreator myScript = (NodeCreator)target;
        if (GUILayout.Button("Create Points"))
        {
            string serializelist = JsonConvert.SerializeObject(myScript.WallNodes);

            NodeCreator.WriteJsonAtJsonsFolder(serializelist, myScript.PathToJsonsFolder, myScript.LevelName, "Nodes.txt");
        }
        if (GUILayout.Button("Download Nodes"))
        {
            string path = NodeCreator.ReturnPathToJsonFile(myScript.PathToJsonsFolder, myScript.LevelName, "Nodes.txt");

            if (File.Exists(path))
            {

                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(path))
                    {
                        // Read the stream to a string, and write the string to the console.
                        string line = sr.ReadToEnd();
                        myScript.WallNodes = JsonConvert.DeserializeObject<List<Vector3>>(line);
                        Debug.Log(line);
                    }
                }
                catch (IOException e)
                {
                    Debug.LogWarning("The file could not be read:");
                    Debug.LogWarning(e.Message);
                }
            }
        }
    }

}
#endregion
public class NodeCreator : MonoBehaviour
{
    public string PathToJsonsFolder = "Assets/ThirdParty/RoomCreationSystem/Jsons";
    public string LevelName = "";

    public GameObject PrefabWall;
    public Camera Camera;

    void Start()
    {
    }

    // Update is called once per frame
    [SerializeField] private float _speedMovement;
    public float SpeedMovement
    {


        get => _speedMovement;
        set
        {
            if (_speedMovement <= 0) _speedMovement = 1;
            else _speedMovement = value;
        }
    }
    void Update()
    {

        WASD_Movement();
        Scrolling();
        ChangeSpeedOfCamera();
        SightMovement();
        CreateNodeOnClick();
    }
    #region Inputs

    [SerializeField] Vector3 Sight = new Vector3(0,0,0);
    [SerializeField] float SensetiveX = 1, SensetiveY = 1;
    private void WASD_Movement()
    {
        float deltaX = Input.GetAxis("Horizontal") * SpeedMovement;
        float deltaZ = Input.GetAxis("Vertical") * SpeedMovement;

        Vector3 movement = new Vector3(deltaZ, 0, -deltaX);
        movement *= (Time.deltaTime);
        Camera.transform.position += movement;
    }
    private void Scrolling()
    {
        float dScroll = Input.mouseScrollDelta.y;
        Camera.orthographicSize -= dScroll;
        if (Camera.orthographicSize <= 0) { Camera.orthographicSize = 1; }
    }
    private void ChangeSpeedOfCamera()
    {
        if (Input.GetKey(KeyCode.E)) { SpeedMovement++; }
        if (Input.GetKey(KeyCode.V)) { SpeedMovement--; }
    }
    private void SightMovement()
    {
        float addX, addY;
        addX = Input.GetAxis("Mouse X") * SensetiveX;
        addY = Input.GetAxis("Mouse Y") * SensetiveY;
        Sight += new Vector3(addY, 0, -addX);
    }

    private void CreateNodeOnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Sight.x = ((int)((Sight.x + Mathf.Sign(Sight.x) * CellSize / 2) / CellSize)) * CellSize;
            Sight.z = ((int)((Sight.z + Mathf.Sign(Sight.z) * CellSize / 2) / CellSize)) * CellSize;

            bool addNewNode = true;
            for (int i = 0; i < WallNodes.Count; i++)
            {
                if (WallNodes[i].Equals(Sight)) { WallNodes.RemoveAt(i); addNewNode = false; }
            }
            if (addNewNode) WallNodes.Add(Sight);
        }
    }

    #endregion

    #region Gizmos

    [Header("Gizmos")]

    #region DrawGrid

    [SerializeField] private Color GizmoColors;
    public float CellSize=1;
    public Vector3 GridSize = new Vector3(1280, 0, 720);
    private void DrawGrid()
    {
        Gizmos.color = GizmoColors;
        if (CellSize < 0.2f) CellSize = 0.2f;
        float x, y, z;
        y = Camera.transform.position.y - 50;

        float semiGridSize_x = GridSize.x / 2, semiGridSize_z = GridSize.z / 2;
        for (x = CellSize/2; x <= semiGridSize_x; x += CellSize)
        {
            Gizmos.DrawLine(new Vector3(x, y, -semiGridSize_z), new Vector3(x, y, semiGridSize_z));
            Gizmos.DrawLine(new Vector3(-x, y, -semiGridSize_z), new Vector3(-x, y, semiGridSize_z));
        }

        for (z = CellSize/2; z <= semiGridSize_z; z += CellSize)
        {
            Gizmos.DrawLine(new Vector3(-semiGridSize_x, y, z), new Vector3(semiGridSize_x, y, z));
            Gizmos.DrawLine(new Vector3(-semiGridSize_x, y, -z), new Vector3(semiGridSize_x, y, -z));
        }
    }
    #region Alternative Version
    /*
    public Vector3 GridRect_1Point = new Vector3(640, 0, 360);
    public Vector3 GridRect_2Point = new Vector3(-640, 0, -360);
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColors;
        if (CellSize < 0.2f) CellSize = 0.2f;
        float x, y, z;
        y = Camera.transform.position.y - 50;
        y = 0;
        for (x = 0; x <= GridRect_1Point.x; x += CellSize)
        {
            Gizmos.DrawLine(new Vector3(x, y, GridRect_1Point.z), new Vector3(x, y, GridRect_2Point.z));
        }
        for (x = 0; x >= GridRect_2Point.x; x -= CellSize)
        {
            Gizmos.DrawLine(new Vector3(x, y, GridRect_1Point.z), new Vector3(x, y, GridRect_2Point.z));
        }

        for (z = 0; z <= GridRect_1Point.z; z += CellSize)
        {
            Gizmos.DrawLine(new Vector3(GridRect_1Point.x, y, z), new Vector3(GridRect_2Point.x, y, z));
        }
        for (z = 0; z >= GridRect_2Point.z; z -= CellSize)
        {
            Gizmos.DrawLine(new Vector3(GridRect_1Point.x, y, z), new Vector3(GridRect_2Point.x, y, z));
        }
    }
    //*/
    #endregion

    #endregion
    #region DrawPonts
    [SerializeField] private Color NodeColors;
    [SerializeField] public List<Vector3> WallNodes = new List<Vector3>();
    private void DrawPoints()
    {
        Gizmos.color = NodeColors;
        float highGround = Camera.transform.position.y - 50;

        foreach (Vector3 node in WallNodes)
        {
            Gizmos.DrawSphere(node, CellSize / 2);
        }
    }
    #endregion
    #region DrawSight
    private void DrawSight()
    {
        Gizmos.color = NodeColors;
        Gizmos.DrawCube(Sight, new Vector3(CellSize, CellSize, CellSize));
    }
    #endregion



    #region DrawLinesWithPoints
    public Color AvailableLinks;

    private void DrawLinesOfPoints()
    {
        Gizmos.color = AvailableLinks;

        foreach (Vector3 node in WallNodes)
        {
            foreach (Vector3 node2 in WallNodes)
            {
                Gizmos.DrawLine(node, node2);
            }
        }
    }
    #endregion
    private void OnDrawGizmos()
    {
        Sight.y = Camera.transform.position.y - 50;
        DrawPoints();
        DrawGrid();
        DrawSight();

        DrawLinesOfPoints(); //delete
    }

    #endregion



    public static void WriteJsonAtJsonsFolder(string serializelist, string pathToJsonsFolder, string levelName, string nameOfFile)
    {
        string path = ReturnPathToJsonFile(pathToJsonsFolder, levelName, nameOfFile);

        Debug.Log(path);
        using (StreamWriter sw = File.CreateText(path))
        {
            sw.WriteLine(serializelist);
        }
    }

    public static string ReturnPathToJsonFile(string pathToJsonsFolder, string levelName, string nameOfFile)
    {
        string path = Directory.GetCurrentDirectory();
        path = System.IO.Path.Combine(path, pathToJsonsFolder, levelName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path = System.IO.Path.Combine(path, nameOfFile);
        return path.Replace('/', @"\"[0]);
    }

}
