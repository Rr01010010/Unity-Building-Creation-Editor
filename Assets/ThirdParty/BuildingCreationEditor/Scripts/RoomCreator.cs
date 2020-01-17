using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using Unity.Collections;

#region

[UnityEditor.CustomEditor(typeof(RoomCreator))]
public class RoomCreatorEditor : UnityEditor.Editor
{
    #region кастомный инспектор
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomCreator myScript = (RoomCreator)target;

        #region Кнопка загрузки данных о точках - данные о началах стен и концах стен
        if (GUILayout.Button("Download ListNodes"))
        {
            string path = NodeCreator.ReturnPathToJsonFile(myScript.PathToJsonsFolder, myScript.LevelName, "ListNodes.txt");

            if (File.Exists(path))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        if (myScript.LinkNodes != null) myScript.LinkNodes.Clear();

                        string line = sr.ReadToEnd();
                        myScript.LinkNodes = JsonConvert.DeserializeObject<List<WallCreatorEditor.LinkNodeElement>>(line);
                    }
                }
                catch (IOException e)
                {
                    Debug.LogWarning("The file could not be read:");
                    Debug.LogWarning(e.Message);
                }
            }
            #region //Вроде как определение уровней здания
            /*
            myScript.LevelPosition = new List<Vector2>();
            for (int i = 0; i < myScript.LinkNodes.Count; i++)
            {
                bool exist = false;

                float Y = myScript.LinkNodes[i].NodePosition.y;
                //if()
                foreach (Vector2 elem in myScript.LevelPosition)
                {
                    if (Mathf.Approximately(elem.y, Y)) exist = true;
                }
                if (!exist) myScript.LevelPosition.Add(new Vector2(Y, Y));
            }
            */
            #endregion

        }
        #endregion
        #region //Button("Update Level Positions")
        /*
        if (GUILayout.Button("Update Level Positions"))
        {
            for (int i = 0; i < myScript.LinkNodes.Count; i++)
            {
                Debug.Log($"i = {i}");
                int iOrigin = int.MaxValue;
                for (int j = 0; j < myScript.LevelPosition.Count; j++)
                {
                    Debug.Log($"j = {j}");
                    if (myScript.LinkNodes[i].NodePosition.y == myScript.LevelPosition[j].y) { iOrigin = j; j = int.MaxValue; j--; }
                }

                Debug.Log($"i_i = {i}");
                Vector3 highground = myScript.LinkNodes[i].NodePosition;
                highground.y = myScript.LevelPosition[iOrigin].x;

                WallCreatorEditor.LinkNodeElement newLink = myScript.LinkNodes[i];
                newLink.NodePosition = highground;
                myScript.LinkNodes[i] = newLink;
            }


            myScript.LevelPosition.Clear();
            for (int i = 0; i < myScript.LinkNodes.Count; i++)
            {
                bool exist = false;

                float Y = myScript.LinkNodes[i].NodePosition.y;
                //if()
                foreach (Vector2 elem in myScript.LevelPosition)
                {
                    if (Mathf.Approximately(elem.y, Y)) exist = true;
                }
                if (!exist) myScript.LevelPosition.Add(new Vector2(Y, Y));
            }
        }
        */
        #endregion
        if (GUILayout.Button("Build"))
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

                for(int j = 0; j < myScript.LinkNodes[i].toNode.Count; j++)
                {
                    Transform wall = Instantiate(myScript.WallPrefab, (myScript.LinkNodes[i].NodePosition + myScript.LinkNodes[i].toNode[j] + new Vector3(0,myScript.Height,0)) / 2, Quaternion.identity,myScript.WallContainer);
                    myScript.ConverterMesh.GetPolygonTree(wall.gameObject);
                    keys = myScript.ConverterMesh.GetKeys();

                    
                    /* ПРОСТАЯ растоновка стен
                    wall.LookAt(myScript.LinkNodes[i].NodePosition + new Vector3(0, myScript.Height / 2, 0));
                    wall.localScale = new Vector3(myScript.Width, myScript.Height, Vector3.Distance(myScript.LinkNodes[i].NodePosition, myScript.LinkNodes[i].toNode[j]) + myScript.Width);
                    //*/
                    InstantinateWallWithEditMesh(myScript, wall, myScript.LinkNodes[i].toNode[j], myScript.LinkNodes[i].NodePosition);
                }                

            }
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
    public int SidesChange(ref Vector3 T, ref Vector3 R, ref Vector3 S, RoomCreator myScript,Vector3 FrontSide, Vector3 Back,bool offset = true) 
    {
        int key = myScript.ConverterMesh.NearestKeySearch(FrontSide - Back);
        Debug.Log($"keys[{key}] = {keys[key].direction} , Direction = {FrontSide - Back} wB={FrontSide}, wE={Back}");


        float yRot = ConverterMesh.AngleBtwVectors(keys[key].direction, FrontSide - Back, new Vector3(1, 0, 1));

        float[,] Normal = new float[1, 4];
        Normal[0, 0] = keys[key].direction.x;
        Normal[0, 1] = keys[key].direction.y;
        Normal[0, 2] = keys[key].direction.z;
        Normal[0, 3] = 1;
        Normal = ConverterMesh.RotateFigureY(Normal, 1, yRot);
        Vector3 dir = new Vector3(Normal[0, 0], Normal[0, 1], Normal[0, 2]);
        //dir= FrontSide-Back

        R = new Vector3(0, yRot, 0);

        //Debug.Log($"keys[{key}] = {keys[key].direction} , Direction = {FrontSide - Back}");
        Debug.Log($"Dir = {dir}");
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

            //T = new Vector3(T.x, T.y, T.z);
            if (Equals(keys[key].direction, new Vector3(0, 0, 1)) || Equals(keys[key].direction, new Vector3(0, 0, -1)))
            { T = new Vector3(T.x * (0.5f / myScript.Width), T.y / 2, T.z / 2); }
            if (Equals(keys[key].direction, new Vector3(1, 0, 0)) || Equals(keys[key].direction, new Vector3(-1, 0, 0)))
            { T = new Vector3(T.x / 2, T.y / 2, T.z*(0.5f/myScript.Width)); }

        }
        //else { T = FrontSide - Back + dir - dir * (myScript.Width); }

        
        //T /= 2;
        if (T.y != 0) { T = new Vector3(T.x, (T.y - T.y / 2), T.z); }
        //if (T.y != 0) { T = new Vector3(T.x / 2, (T.y - T.y / 2) / 2, T.z / 2); }
        //else T /= 2;

        //T = wallBeginning - wall.position;

        //Debug.Log($"R = {R}, T= {T}");
        //T = new Vector3(T.x / wall.localScale.x, T.y / wall.localScale.y, T.z / wall.localScale.z);
        //S = new Vector3(1, 1, 1) - keys[Key1].direction;
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
    public ConverterMesh ConverterMesh;
    public string PathToJsonsFolder = "Assets/ThirdParty/RoomCreationSystem/Jsons";
    public string LevelName = "";

    public Transform WallPrefab { get => _wallPrefab_1x1x1; set => _wallPrefab_1x1x1 = value; }
    [SerializeField] private Transform _wallPrefab_1x1x1;
    public Transform PrefabNodeContainer { get => _prefabNodeContainer; set => _prefabNodeContainer = value; }
    [SerializeField] private Transform _prefabNodeContainer;

    public float Height;
    public float Width;
    public List<WallCreatorEditor.LinkNodeElement> LinkNodes { get => _linkNodes; set { _linkNodes = value; count = _linkNodes.Count; } }

    public Transform WallContainer { get => _wallContainer; set => _wallContainer = value; }

    [SerializeField] private Transform _wallContainer;

    [SerializeField] private List<WallCreatorEditor.LinkNodeElement> _linkNodes = new List<WallCreatorEditor.LinkNodeElement>();
    //public List<Vector2> LevelPosition { get => _levelPosition; set => _levelPosition = value; }
    //[SerializeField] private List<Vector2> _levelPosition = new List<Vector2>();

    [SerializeField] private int count;
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

            for (int to_i=0;to_i< LinkNodes[i].toNode.Count; to_i++)
            {
                Gizmos.color = GizmoColors;

                
                Vector3 to = LinkNodes[i].toNode[to_i];

                Gizmos.DrawLine(from + offset1, to + offset1);
                Gizmos.DrawLine(from + offset2, to + offset2);

                Gizmos.DrawLine(from - offset1, to - offset1);
                Gizmos.DrawLine(from - offset2, to - offset2);




                Gizmos.DrawLine(from + offset1+ offset3, to + offset1 + offset3);
                Gizmos.DrawLine(from + offset2 + offset3, to + offset2 + offset3);

                Gizmos.DrawLine(from - offset1 + offset3, to - offset1 + offset3);
                Gizmos.DrawLine(from - offset2 + offset3, to - offset2 + offset3);





                //Gizmos.color = Color.red;
                //Gizmos.DrawLine(LinkNodes[i].toNode[to_i] - new Vector3(semiW, 0, semiW), LinkNodes[i].toNode[to_i] + new Vector3(semiW, semiH, semiW));
            }
        }


        //if (LinkNodes != null)
        //for(int i = 0; i < LinkNodes.Count; i++)
    }
    /*
    private void DrawLinesOfPoints()
    {
        Gizmos.color = GizmoColors;
        if (LinkNodes != null)
        for (int i = 0; i < LinkNodes.Count; i++)
        {
            int iOrigin = int.MaxValue;
            for (int j = 0; j < LevelPosition.Count; j++)
            {
                if (LinkNodes[i].NodePosition.y == LevelPosition[j].y) iOrigin = j;
            }
            if (iOrigin == int.MaxValue)
            {
                i = LinkNodes.Count;
                Debug.LogError("Ошибка, оригинальная высота не была найдена. Перезаписываю LevelPosition с нуля");

                LevelPosition = new List<Vector2>();

                if (LinkNodes != null)
                for (int j = 0; j < LinkNodes.Count; j++)
                {
                    bool exist = false;

                    float Y = LinkNodes[j].NodePosition.y;
                    //if()
                    foreach (Vector2 elem in LevelPosition)
                    {
                        if (Mathf.Approximately(elem.y, Y)) exist = true;
                    }
                    if (!exist) LevelPosition.Add(new Vector2(Y, Y));
                }
            }


            //if (LinkNodes[i].toNode != null)
            foreach (Vector3 to in LinkNodes[i].toNode)
            {
                Gizmos.DrawLine(LinkNodes[i].NodePosition, to);
                if (LinkNodes[i].NodePosition.y != LevelPosition[iOrigin].x)
                {
                    Vector3 from = LinkNodes[i].NodePosition;
                    from.y = LevelPosition[iOrigin].x;

                    Vector3 vect = to;
                    vect.y = LevelPosition[iOrigin].x;

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(from, vect);
                    Gizmos.color = GizmoColors;
                }
            }
        }
    }
    */
    #endregion
    private void OnDrawGizmos()
    {
        DrawLinesOfPoints();
    }

    #endregion
}
