using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region custom editor button

[UnityEditor.CustomEditor(typeof(NodeController))]
public class LinkingRequest : UnityEditor.Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NodeController myScript = (NodeController)target;
        if (GUILayout.Button("Linking Request"))
        {
            myScript.WCreator.Targetnode = myScript;
        }
    }
}
#endregion
public class NodeController : MonoBehaviour
{

    public WallCreator WCreator { get => _wCreator; set => _wCreator = value; }
    private WallCreator _wCreator;

    public int Index { get => _index; set => _index = value; }
    [SerializeField] private int _index;
}
