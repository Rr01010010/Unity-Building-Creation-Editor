using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

#region custom editor button

[UnityEditor.CustomEditor(typeof(WallCreator))]
public class WallCreatorEditor : UnityEditor.Editor
{
    public List<Vector3> Nodes { get => _nodes; set => _nodes = value; }
    private List<Vector3> _nodes = new List<Vector3>();
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WallCreator myScript = (WallCreator)target;
        if (GUILayout.Button("Download Nodes"))
        {
            string path = NodeCreator.ReturnPathToJsonFile(myScript.PathToJsonsFolder, myScript.LevelName, "Nodes.txt");

            if (File.Exists(path))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string line = sr.ReadToEnd();
                        Nodes = JsonConvert.DeserializeObject<List<Vector3>>(line);

                        #region удаляем и создаем новый контейнер
                        if (myScript.NodeContainer != null)
                        {
                            while (myScript.NodeContainer.childCount > 0)
                            {
                                DestroyImmediate(myScript.NodeContainer.GetChild(0).gameObject);
                            }
                            DestroyImmediate(myScript.NodeContainer.gameObject);
                        }
                        myScript.NodeContainer = Instantiate(myScript.PrefabNodeContainer, new Vector3(0, 0, 0), Quaternion.identity);
                        #endregion

                        if (myScript.LinkNodes != null) myScript.LinkNodes.Clear();
                        myScript.LinkNodes = new List<WallCreator.LinkNodeElement>();

                        for(int i = 0; i < Nodes.Count; i++)
                        {
                            WallCreator.LinkNodeElement newElem = new WallCreator.LinkNodeElement();
                            newElem.myBox = Instantiate(myScript.PrefabNode, Nodes[i], Quaternion.identity, myScript.NodeContainer);
                            newElem.myBox.Index = i;
                            newElem.myBox.WCreator = myScript;
                            newElem.indexToNode = new List<int>();
                            newElem.save = false;

                            myScript.LinkNodes.Add(newElem);
                        }
                    }
                }
                catch (IOException e)
                {
                    Debug.LogWarning("The file could not be read:");
                    Debug.LogWarning(e.Message);
                }
            }
        }
        if (GUILayout.Button("Save ListNodes"))
        {
            List<LinkNodeElement> ThirdStageLinks = new List<LinkNodeElement>();

            foreach (WallCreator.LinkNodeElement elem in myScript.LinkNodes)
            {
                LinkNodeElement newElem = new LinkNodeElement();

                newElem.NodePosition = elem.myBox.transform.position;
                newElem.toNode = new List<Vector3>();
                foreach (int idx in elem.indexToNode)
                {
                    Vector3 vect = myScript.LinkNodes[idx].myBox.transform.position;
                    newElem.toNode.Add(vect);
                }

                if(elem.save) ThirdStageLinks.Add(newElem);
            }


            string serializelist = JsonConvert.SerializeObject(ThirdStageLinks);

            NodeCreator.WriteJsonAtJsonsFolder(serializelist, myScript.PathToJsonsFolder, myScript.LevelName, "ListNodes.txt");

        } 
        if (GUILayout.Button("Download ListNodes")) 
        {
            string path = NodeCreator.ReturnPathToJsonFile(myScript.PathToJsonsFolder, myScript.LevelName, "ListNodes.txt");


            if (File.Exists(path))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string line = sr.ReadToEnd();

                        #region удаляем и создаем новый контейнер
                        if (myScript.NodeContainer != null)
                        {
                            while (myScript.NodeContainer.childCount > 0)
                            {
                                DestroyImmediate(myScript.NodeContainer.GetChild(0).gameObject);
                            }
                            DestroyImmediate(myScript.NodeContainer.gameObject);
                        }
                        myScript.NodeContainer = Instantiate(myScript.PrefabNodeContainer, new Vector3(0, 0, 0), Quaternion.identity);
                        #endregion

                        List<LinkNodeElement> ThirdStageLinks = JsonConvert.DeserializeObject<List<LinkNodeElement>>(line);

                        #region обработка пустых ячеек листа, а также текстовый вывод содержимого
                        for (int i = 0; i < ThirdStageLinks.Count; i++)
                        {
                            if(ThirdStageLinks[i].toNode == null || ThirdStageLinks[i].toNode.Count == 0)
                            {
                                ThirdStageLinks.RemoveAt(i);
                            }
                        }
                        #endregion

                        myScript.LinkNodes = new List<WallCreator.LinkNodeElement>();

                        #region Аккуратное преобразование векторного листа из TXT в индексовый лист используемый в сцене
                        for (int i = 0; i < ThirdStageLinks.Count; i++)
                        {
                            NodeController controller = Instantiate(myScript.PrefabNode, ThirdStageLinks[i].NodePosition, Quaternion.identity, myScript.NodeContainer);
                            controller.WCreator = myScript;
                            controller.Index = i;

                            WallCreator.LinkNodeElement newElem = new WallCreator.LinkNodeElement();
                            newElem.myBox = controller;
                            newElem.save = true;
                            newElem.indexToNode = new List<int>();

                            //for(int )
                            foreach(Vector3 to in ThirdStageLinks[i].toNode)
                            {
                                int subIndex = int.MaxValue;
                                for(int searched_i = 0; searched_i < ThirdStageLinks.Count; searched_i++)
                                {
                                    if (ThirdStageLinks[searched_i].NodePosition.Equals(to)) subIndex = searched_i;
                                }
                                if(subIndex != int.MaxValue)
                                {
                                    newElem.indexToNode.Add(subIndex);
                                }
                            }
                            myScript.LinkNodes.Add(newElem);
                        }
                        #endregion

                        #region вывод используемого в работе на второй сцене листа
                        // /*
                        for (int i = 0; i < myScript.LinkNodes.Count; i++)
                        {
                            string output = $"transform position = {myScript.LinkNodes[i].myBox.transform.position} , i_endpositions = ";
                            foreach (int endpos in myScript.LinkNodes[i].indexToNode)
                            {
                                output = output.Insert(output.Length, endpos.ToString());
                            }
                            Debug.Log(output);
                        }
                        //*/
                        #endregion
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
    public struct LinkNodeElement
    {

        [JsonProperty("toNode")]
        public List<Vector3> toNode { get; set; }

        [JsonProperty("NodePosition")]
        public Vector3 NodePosition { get; set; }
    }
}
#endregion
public class WallCreator : MonoBehaviour
{
    public string PathToJsonsFolder = "Assets/ThirdParty/RoomCreationSystem/Jsons";
    public string LevelName = "";
    
    //public int CountNode;
    public Transform PrefabNodeContainer;
    public NodeController PrefabNode;
    public Camera Camera;

    [SerializeField] private TopMouseControl MouseContol;
    public Transform NodeContainer { get => _nodeContainer; set => _nodeContainer = value; }
    [SerializeField] private Transform _nodeContainer;

    public struct LinkNodeElement
    {
        [JsonProperty("save")] public bool save { get; set; }
        //[JsonProperty("toNode")] public List<Vector3> toNode { get; set; }
        //[JsonProperty("NodePosition")] public Vector3 NodePosition { get; set; }
        [JsonProperty("indexToNode")] public List<int> indexToNode { get; set; }
        [JsonIgnore] public NodeController myBox { get; set; }
    }
    public List<LinkNodeElement> LinkNodes = new List<LinkNodeElement>();



    private NodeController _targetnode;
    public NodeController Targetnode
    {
        get => _targetnode;
        set
        {
            if (LinkNodes == null || LinkNodes.Count < NodeContainer.childCount) { Debug.LogError("LinkNodes is empty, please, download nodes"); }

            if (_targetnode == null) _targetnode = value;
            else if (_targetnode.Index != value.Index)
            {
                int subIndex = int.MaxValue;

                for (int i = 0; i < LinkNodes[_targetnode.Index].indexToNode.Count; i++)
                {
                    if (LinkNodes[_targetnode.Index].indexToNode[i] == value.Index)
                    {
                        subIndex = i;
                        for (int j = 0; j < LinkNodes[value.Index].indexToNode.Count; j++)
                        {
                            if (LinkNodes[value.Index].indexToNode[j] == _targetnode.Index)
                            {
                                LinkNodes[value.Index].indexToNode.RemoveAt(j);
                                if(LinkNodes[value.Index].indexToNode.Count == 0)
                                {
                                    LinkNodeElement newlink = LinkNodes[value.Index];
                                    newlink.save = false;
                                    LinkNodes[value.Index] = newlink;
                                }

                                j = int.MaxValue - 1;
                            }
                        }
                        i = int.MaxValue - 1;
                    }
                }

                if (subIndex != int.MaxValue)
                {
                    LinkNodes[_targetnode.Index].indexToNode.RemoveAt(subIndex);
                    if (LinkNodes[_targetnode.Index].indexToNode.Count == 0)
                    {
                        LinkNodeElement newlink = LinkNodes[_targetnode.Index];
                        newlink.save = false;
                        LinkNodes[_targetnode.Index] = newlink;
                    }
                }
                else
                {
                    LinkNodeElement newlink = LinkNodes[_targetnode.Index];
                    newlink.save = true;
                    newlink.indexToNode.Add(value.Index);
                    LinkNodes[_targetnode.Index] = newlink;

                    newlink = LinkNodes[value.Index];
                    newlink.save = true;
                    newlink.indexToNode.Add(_targetnode.Index);
                    LinkNodes[value.Index] = newlink;
                }

                _targetnode = null;
            }
        }
    }


    #region Inputs
    
    [SerializeField] private Vector3 offsetOfSightForDetect = new Vector3(0, 1, 0);


    void Update()
    {
        #region Связывание узлов по нажатию
        if (Input.GetMouseButtonDown(0))
        {
            int mask = 1 << PrefabNode.gameObject.layer;

            RaycastHit hit;
            Ray ray = new Ray(MouseContol.Sight + offsetOfSightForDetect, MouseContol.Sight - Camera.transform.position);


            if (Physics.Raycast(ray, out hit, Vector3.Distance(Camera.transform.position, MouseContol.Sight), mask))//, float.MaxValue, layer))
            {
                NodeController controller = hit.transform.gameObject.GetComponent<NodeController>();
                if (controller != null) Targetnode = controller;
            }
        }
        #endregion

        #region Создание нового узла по нажатию
        if (Input.GetMouseButtonDown(1))
        {
            if (NodeContainer == null) NodeContainer = Instantiate(PrefabNodeContainer, new Vector3(0, 0, 0), Quaternion.identity);

            bool exist = false;
            foreach (LinkNodeElement element in LinkNodes)
            {
                if (Equals(element.myBox.transform.position, MouseContol.Sight)) exist = true;
            }
            if (exist == false)
            {
                NodeController controller = Instantiate(PrefabNode, MouseContol.Sight, Quaternion.identity, NodeContainer);
                controller.WCreator = this;
                controller.Index = LinkNodes.Count;

                LinkNodeElement newElem = new LinkNodeElement();
                newElem.myBox = controller;
                newElem.save = false;
                newElem.indexToNode = new List<int>();
                LinkNodes.Add(newElem);
            }            
        }
        #endregion
    }
#endregion

    #region Gizmos

    [Header("Gizmos")]

    [SerializeField] private Color GizmoColors;

    #region DrawLinesWithPoints
    private void DrawLinesOfPoints()
    {
        Gizmos.color = GizmoColors;

        if(NodeContainer != null && NodeContainer.childCount == LinkNodes.Count)
        foreach(LinkNodeElement elem in LinkNodes)
        {
            foreach(int idx in elem.indexToNode)
            {
                Gizmos.DrawLine(elem.myBox.transform.position, LinkNodes[idx].myBox.transform.position);
            }
        }
    }
    #endregion

    public float CellSize = 0.25f;
    //public Color SightColor;// { get; set; }
    #region DrawSight
    private void DrawSight()
    {
        Gizmos.color = GizmoColors;
        Gizmos.DrawCube(MouseContol.Sight, new Vector3(CellSize, CellSize, CellSize));
    }
    #endregion
    private void OnDrawGizmos()
    {
        DrawLinesOfPoints();
        DrawSight();
    }

    #endregion
}
