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
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomCreator myScript = (RoomCreator)target;
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
        }
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
                    wall.LookAt(myScript.LinkNodes[i].NodePosition + new Vector3(0, myScript.Height/2, 0));
                    wall.localScale = new Vector3(myScript.Width, myScript.Height, Vector3.Distance(myScript.LinkNodes[i].NodePosition, myScript.LinkNodes[i].toNode[j]) + myScript.Width);
                    //z растягивание в длину стены , x ширина , y высота
                }
                

            }
        }
    }
}
#endregion

public class RoomCreator : MonoBehaviour
{
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
