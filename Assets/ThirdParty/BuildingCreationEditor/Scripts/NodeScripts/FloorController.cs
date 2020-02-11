using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region custom editor button

[UnityEditor.CustomEditor(typeof(FloorController))]
public class ManagerRequest : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FloorController myScript = (FloorController)target;
        if (GUILayout.Button(myScript.Switching))
        {
            myScript.Fixed = true;
            myScript.State = myScript.Switching;

            if (string.Equals(myScript.Switching, myScript.FixPosition))
            { myScript.Switching = myScript.DetachmentPosition; }
            else myScript.Switching = myScript.FixPosition;
        }
    }
}
#endregion
public class FloorController : ClassNode
{
    FloorManagerController _parent;
    public void StarterVariables(Vector3 pos,int idx, FloorManagerController parent)
    { transform.position = pos; Index = idx; _parent = parent; }

    private Vector3 fixedPosition;
    public bool Fixed
    {
        set
        { fixedPosition = transform.position; _fixed = !_fixed; }
    }
    [SerializeField] bool _fixed = false;

    public string State { get; set; }
    public string FixPosition { get => "Fix position"; }
    public string DetachmentPosition { get => "Detachment position"; }
    public string Switching { get => _switching; set => _switching = value; }
    private string _switching = "Fix position";




    [SerializeField] float timer = 0.7f;
    private float timeCounter = 0;
    private void OnDrawGizmos()
    {
        if (_fixed)
        {
            if (!transform.position.Equals(fixedPosition))
            {
                timeCounter += Time.deltaTime;
                if (timeCounter > timer)
                {
                    timeCounter = 0;
                    FloorController floor = Instantiate(_parent.floorContollerPrefab, transform.position, Quaternion.identity, transform.parent);
                    _parent.FloorsControllers.Add(floor);

                    floor.StarterVariables(transform.position, _parent.transform.childCount - 1, _parent);
                    UnityEditor.Selection.activeGameObject = null;
                    transform.position = fixedPosition;



                    if (Indexes.Count==1)
                    {
                        ConnectNodes(_parent.transform.childCount - 1, Indexes[0]);
                    }
                    else
                    {
                        int nearest = SeacrhNearestNode_ForNewNode(floor.transform.position, Indexes[0], Indexes[1]);

                        List<int> nearestIndexes = _parent.FloorsControllers[nearest].Indexes;
                        for (int i = 0; i < 2; i++)
                        {
                            Debug.Log(i);
                            if (Indexes.Count > 1) if (Indexes[i] == nearest) Indexes.RemoveAt(i);
                            if (nearestIndexes.Count > 1) if (nearestIndexes[i] == Index) nearestIndexes.RemoveAt(i);
                        }
                        ConnectNodes(_parent.transform.childCount - 1, nearest);
                    }
                }
            }
        }
        foreach(int idx in Indexes)
        {
            Gizmos.DrawLine(_parent.FloorsControllers[idx].transform.position, transform.position);
        }
    }
    private int SeacrhNearestNode_ForNewNode(Vector3 nodePos, int neighbor1, int neighbor2)
    {
        float dist = Vector3.Distance(transform.position, nodePos);
        float dist1 = Vector3.Distance(_parent.FloorsControllers[neighbor1].transform.position, nodePos);
        float dist2 = Vector3.Distance(_parent.FloorsControllers[neighbor2].transform.position, nodePos);
        if(dist<dist1 && dist < dist2)
        {
            if (dist1 < dist2) return neighbor2;
            else return neighbor1;
        }
        else
        {
            if (dist1 < dist2) return neighbor1;
            else return neighbor2;
        }
        
    }

    private void ConnectNodes(int newNode, int neighborNode)
    {
        Indexes.Add(newNode);
        _parent.FloorsControllers[neighborNode].Indexes.Add(newNode);
        _parent.FloorsControllers[newNode].Indexes.Add(Index);
        _parent.FloorsControllers[newNode].Indexes.Add(neighborNode);
    }
}
