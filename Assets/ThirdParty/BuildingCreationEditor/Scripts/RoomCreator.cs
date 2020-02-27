using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using Unity.Collections;

#region кастомный инспектор
[UnityEditor.CustomEditor(typeof(RoomCreator))]
public class RoomCreatorEditor : UnityEditor.Editor
{
    private string ButtonName_FloorControllerBuilding = "Create FloorController";
    private string ButtonName_FloorBuilding = "Building Floor";
    #region Кнопки кастомного инспектора
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomCreator myScript = (RoomCreator)target;

        #region Кнопка загрузки данных о точках - данные о началах стен и концах стен
        if (GUILayout.Button("Download ListNodes"))
        {
            ListNodesDownload(myScript);
            StoreysDefinition(myScript);
        }
        #endregion
        if (GUILayout.Button("Build Rooms"))
        {
            #region удаляем и создаем новый контейнер

            if (myScript.WallContainer != null)
            {
                while (myScript.WallContainer.childCount > 0)
                {
                    DestroyImmediate(myScript.WallContainer.GetChild(0).gameObject);
                }
                DestroyImmediate(myScript.WallContainer.gameObject);
            }
            myScript.WallContainer = Instantiate(myScript.PrefabNodeContainer, new Vector3(0, 0, 0), Quaternion.identity);
            #endregion

            for (int i = 0; i < myScript.LinkNodes.Count; i++)
            {
                for (int j = 0; j < myScript.LinkNodes[i].toNode.Count; j++)
                {
                    Transform wall = Instantiate(myScript.WallPrefab, (myScript.LinkNodes[i].NodePosition + myScript.LinkNodes[i].toNode[j] + new Vector3(0, myScript.Height, 0)) / 2, Quaternion.identity, myScript.WallContainer);
                    myScript.ConverterMesh.GetPolygonTree(wall.gameObject);
                    keys = myScript.ConverterMesh.GetKeys();


                    if (myScript.BuildWithMeshEditing == RoomCreator.statesBuilds.WithMeshEdit) InstantinateWallWithEditMesh(myScript, wall, myScript.LinkNodes[i].toNode[j], myScript.LinkNodes[i].NodePosition);
                    else if (myScript.BuildWithMeshEditing == RoomCreator.statesBuilds.WithoutMeshEdit)
                    {
                        wall.LookAt(myScript.LinkNodes[i].NodePosition + new Vector3(0, myScript.Height / 2, 0));
                        wall.localScale = new Vector3(myScript.Width, myScript.Height, Vector3.Distance(myScript.LinkNodes[i].NodePosition, myScript.LinkNodes[i].toNode[j]) + myScript.Width);
                    }
                    else if (myScript.BuildWithMeshEditing == RoomCreator.statesBuilds.Combination)
                    {
                        int axiesChanges = 0;
                        Vector3 vector = myScript.LinkNodes[i].NodePosition - myScript.LinkNodes[i].toNode[j];
                        if (Mathf.Abs(vector.x) > 0) axiesChanges++; if (Mathf.Abs(vector.y) > 0) axiesChanges++; if (Mathf.Abs(vector.z) > 0) axiesChanges++;


                        if (axiesChanges > 1) InstantinateWallWithEditMesh(myScript, wall, myScript.LinkNodes[i].toNode[j], myScript.LinkNodes[i].NodePosition);
                        else
                        {
                            wall.LookAt(myScript.LinkNodes[i].NodePosition + new Vector3(0, myScript.Height / 2, 0));
                            wall.localScale = new Vector3(myScript.Width, myScript.Height, Vector3.Distance(myScript.LinkNodes[i].NodePosition, myScript.LinkNodes[i].toNode[j]) + myScript.Width);
                        }
                    }
                }
            }
            if(myScript.BuildWithMeshEditing != RoomCreator.statesBuilds.WithoutMeshEdit) 
            {
                SaveAndLoadAssetWithMenu.DownloadMeshes_andSaveRooms(myScript);
            }

        }
        if (GUILayout.Button(ButtonName_FloorControllerBuilding))
        {
            FloorManagerController manager = Instantiate(myScript.FloorManagerPrefab, Vector3.zero, Quaternion.identity);
            myScript.FloorManagers.Add(manager.transform);
        }
    }
    #endregion

    #region Метод загрузки данных о точках - ListNodesDownload
    private void ListNodesDownload(RoomCreator manager) 
    {
        if (manager.LinkNodes != null) manager.LinkNodes.Clear();

        string line = NodeCreator.ReadFile(manager.PathToJsonsFolder, manager.LevelName, "ListNodes.txt");
        if (line != null)
        {
            manager.LinkNodes = JsonConvert.DeserializeObject<List<WallCreatorEditor.LinkNodeElement>>(line);
        }
    }
    #endregion
    #region Метод определения этажей здания StoreysDefinition
    private void StoreysDefinition(RoomCreator myScript)
    {
        if (myScript.StoreysPositionY == null) myScript.StoreysPositionY = new List<float>();
        else myScript.StoreysPositionY.Clear();

        for (int i = 0; i < myScript.LinkNodes.Count; i++)
        {
            bool exist = false;
            float Y = myScript.LinkNodes[i].NodePosition.y;

            foreach (float y in myScript.StoreysPositionY)
            {
                if (Mathf.Approximately(y, Y)) exist = true;
            }
            if (!exist) myScript.StoreysPositionY.Add(Y);
        }
    }
    #endregion


    #region Special Mesh Data
    ConverterMesh.normal[] keys;
    #endregion
    #region Special CHANGE THAT
    private void InstantinateWallWithEditMesh(RoomCreator myScript, Transform wall, Vector3 wallBeginning, Vector3 wallEnd)
    {
        Mesh mesh = wall.gameObject.GetComponent<MeshFilter>().mesh;
        myScript.ConverterMesh.mCollider = wall.gameObject.GetComponent<MeshCollider>();

        wallBeginning += new Vector3(0, myScript.Height, 0) / 2;
        wallEnd += new Vector3(0, myScript.Height, 0) / 2;


        bool offset = true;
        int Key1, Key2;
        Vector3 R, T, S, r, t, s;

        T = wallBeginning - wallEnd;
        if (Mathf.Abs(T.x) > 0 && Mathf.Abs(T.z) > 0) offset = false;//if (Mathf.Abs(ConverterMesh.AngleBtwVectors(wallBeginning, wallEnd, new Vector3(1, 0, 1)) % 90.0f) > 0) { offset = false; }
        s = t = r = S = T = R = Vector3.zero;
        Key1 = SidesChange(ref T, ref R, ref S, myScript, wallBeginning, wallEnd, offset);
        Key2 = SidesChange(ref t, ref r, ref s, myScript, wallEnd, wallBeginning, offset);

        myScript.ConverterMesh.PolygonTransformation(keys[Key1], T, R, S);
        myScript.ConverterMesh.PolygonTransformation(keys[Key2], t, r, s);
    }
    public int SidesChange(ref Vector3 T, ref Vector3 R, ref Vector3 S, RoomCreator myScript, Vector3 FrontSide, Vector3 Back, bool offset = true)
    {
        int key = myScript.ConverterMesh.NearestKeySearch(FrontSide - Back);
        //Debug.Log($"keys[{key}] = {keys[key].direction} , Direction = {FrontSide - Back} wB={FrontSide}, wE={Back}");


        float yRot = ConverterMesh.AngleBtwVectors(keys[key].direction, FrontSide - Back, new Vector3(1, 0, 1));

        R = new Vector3(0, yRot, 0);

        if (offset)
        {
            T = FrontSide - Back - keys[key].direction + keys[key].direction * (myScript.Width);
            if (Equals(keys[key].direction, new Vector3(0, 0, 1)) || Equals(keys[key].direction, new Vector3(0, 0, -1)))
            { T = new Vector3(T.x, T.y / 2, T.z / 2); }
            if (Equals(keys[key].direction, new Vector3(1, 0, 0)) || Equals(keys[key].direction, new Vector3(-1, 0, 0)))
            { T = new Vector3(T.x / 2, T.y / 2, T.z); }
        }
        else
        {
            T = FrontSide - Back - keys[key].direction;

            if (Equals(keys[key].direction, new Vector3(0, 0, 1)) || Equals(keys[key].direction, new Vector3(0, 0, -1)))
            { T = new Vector3(T.x * myScript.Height * (0.25f / myScript.Width), T.y / 2, T.z / 2); }
            if (Equals(keys[key].direction, new Vector3(1, 0, 0)) || Equals(keys[key].direction, new Vector3(-1, 0, 0)))
            { T = new Vector3(T.x / 2, T.y / 2, T.z * myScript.Height * (0.25f / myScript.Width)); }

        }


        if (T.y != 0) { T = new Vector3(T.x, (T.y - T.y / 2), T.z); }
        S = new Vector3(1, 1, 1) - new Vector3(Mathf.Abs(keys[key].direction.x), Mathf.Abs(keys[key].direction.y), Mathf.Abs(keys[key].direction.z));

        S = new Vector3(S.x * myScript.Width, S.y * myScript.Height, S.z * myScript.Width);
        if (S.x == 0) { S = new Vector3(1, S.y, S.z); }
        if (S.y == 0) { S = new Vector3(S.x, 1, S.z); }
        if (S.z == 0) { S = new Vector3(S.x, S.y, 1); }
        return key;
    }
    #endregion
}
#endregion

public class RoomCreator : MonoBehaviour
{

    public FloorManagerController FloorManagerPrefab;

    public List<Transform> FloorManagers { get => _floorManagers; set => _floorManagers = value; }
    private List<Transform> _floorManagers = new List<Transform>();

    #region Data
    public ClassNode PrefabFloorNode;
    public List<ClassNode> FloorNodes { get => _floorNodes; set => _floorNodes = value; }
    private List<ClassNode> _floorNodes;
    //public Transform ContainerNodeFloor { get => _containerNodeFloor; set => _containerNodeFloor = value; }
    //private Transform _containerNodeFloor;

    public enum statesBuilds { WithMeshEdit, Combination, WithoutMeshEdit }
    public statesBuilds BuildWithMeshEditing = statesBuilds.WithMeshEdit;
    public ConverterMesh ConverterMesh;
    public string PathToJsonsFolder = "Assets/ThirdParty/BuildingCreationEditor/Jsons";
    public string PathToAssetBundle = "Assets/ThirdParty/BuildingCreationEditor/AssetBundle";
    public string LevelName = "";

    public Transform WallPrefab { get => _wallPrefab_1x1x1; set => _wallPrefab_1x1x1 = value; }
    [SerializeField] private Transform _wallPrefab_1x1x1;
    public Transform PrefabNodeContainer { get => _prefabNodeContainer; set => _prefabNodeContainer = value; }
    [SerializeField] private Transform _prefabNodeContainer;

    public float Height;
    public float Width;
    public List<WallCreatorEditor.LinkNodeElement> LinkNodes { get => _linkNodes; set { _linkNodes = value; } }

    public Transform WallContainer { get => _wallContainer; set => _wallContainer = value; }
    private Transform _wallContainer;

    [SerializeField] private List<WallCreatorEditor.LinkNodeElement> _linkNodes = new List<WallCreatorEditor.LinkNodeElement>();
    public List<float> StoreysPositionY { get => _levelPositionY; set => _levelPositionY = value; }
    [SerializeField] private List<float> _levelPositionY = new List<float>();
    #endregion


    #region Gizmos

    [Header("Gizmos")]

    [SerializeField] private Color GizmoColors;


    #region DrawLinesWithPoints

    private void DrawLinesOfPoints()
    {
        Gizmos.color = GizmoColors;
        if (!(Height > 0)) Height = 0.001f;
        if (!(Width > 0)) Width = 0.001f;

        float semiW = Width / 2;
        float semiH = Height / 2;

        //        if (LinkNodes != null) 
        for (int i = 0; i < LinkNodes.Count; i++)
        {

            Vector3 from = LinkNodes[i].NodePosition;
            Vector3 offset1 = new Vector3(semiW, 0, semiW);
            Vector3 offset2 = new Vector3(semiW, 0, -semiW);
            Vector3 offset3 = new Vector3(0, Height, 0);


            Vector3 offsetz = new Vector3(0, 0, Width);
            Vector3 offsetx = new Vector3(Width, 0, 0);

            for (int to_i = 0; to_i < LinkNodes[i].toNode.Count; to_i++)
            {
                Gizmos.color = GizmoColors;


                Vector3 to = LinkNodes[i].toNode[to_i];

                Gizmos.DrawLine(from + offset1, to + offset1);
                Gizmos.DrawLine(from + offset2, to + offset2);

                Gizmos.DrawLine(from - offset1, to - offset1);
                Gizmos.DrawLine(from - offset2, to - offset2);




                Gizmos.DrawLine(from + offset1 + offset3, to + offset1 + offset3);
                Gizmos.DrawLine(from + offset2 + offset3, to + offset2 + offset3);

                Gizmos.DrawLine(from - offset1 + offset3, to - offset1 + offset3);
                Gizmos.DrawLine(from - offset2 + offset3, to - offset2 + offset3);

            }
        }


    }
    #endregion
    private void DrawRegion()
    {
        foreach (ClassNode node in FloorNodes)
        {
            for (int i = 0; i < node.Indexes.Count; i++)
            {
                Debug.DrawLine(node.transform.position, FloorNodes[node.Indexes[i]].transform.position, Color.blue);
            }
        }
    }
    private void OnDrawGizmos()
    {
        DrawLinesOfPoints();
        if (FloorNodes != null) DrawRegion();

    }

    #endregion
}
