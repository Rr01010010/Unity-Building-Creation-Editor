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

    [SerializeField] private TopMouseControl MouseContol;
    [SerializeField] public List<Vector3> WallNodes = new List<Vector3>();
    

    

    private float UpdateSens;

    #region Inputs
    void Update()
    {
        CreateNodeOnClick();        
    }

    
    private void CreateNodeOnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 Sight = MouseContol.Sight;


            Sight.x = ((int)((MouseContol.Sight.x + Mathf.Sign(MouseContol.Sight.x) * CellSize / 2) / CellSize)) * CellSize;
            Sight.y = ((int)((MouseContol.Sight.y + Mathf.Sign(MouseContol.Sight.y) * CellSize / 2) / CellSize)) * CellSize;//
            Sight.z = ((int)((MouseContol.Sight.z + Mathf.Sign(MouseContol.Sight.z) * CellSize / 2) / CellSize)) * CellSize;


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
    [SerializeField] private Color NodeColors;
    public float CellSize=1;
    public Vector3 GridSize = new Vector3(1280, 0, 720);
    [SerializeField] private Color GridColor = Color.white;
    public Color AvailableLinks;

    #region DrawGrid
    private void DrawGrid()
    {
        Gizmos.color = GridColor;
        if (CellSize < 0.2f) CellSize = 0.2f;
        float x, y, z;
        //y = Camera.transform.position.y - GridSize.y;
        y = GridSize.y;

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
    #endregion
    #region DrawPonts
    private void DrawPoints()
    {
        Gizmos.color = NodeColors;

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
        Gizmos.DrawCube(MouseContol.Sight, new Vector3(CellSize, CellSize, CellSize));
    }
    #endregion
    #region DrawLinesWithPoints
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
        DrawPoints();
        DrawGrid();
        DrawSight();
        DrawLinesOfPoints();
    }
    #endregion


    #region PathOfFileSystem
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
    #endregion
}
