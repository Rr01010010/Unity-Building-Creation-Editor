using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FloorManagerController : ClassNode
{
    public FloorController floorContollerPrefab;
    private FloorController floor;
    
    [SerializeField] float timer = 0.7f;
    private float timeCounter = 0;

    public List<FloorController> FloorsControllers { get => _controllers; set => _controllers = value; }
    private List<FloorController> _controllers = new List<FloorController>();
    bool notLibrary = true;
    private void OnDrawGizmos()
    {
        if (notLibrary)
        {
            if (!transform.position.Equals(Vector3.zero))
            {
                timeCounter += Time.deltaTime;
                if (timeCounter > timer)
                {
                    timeCounter = 0;
                    floor = Instantiate(floorContollerPrefab, transform.position, Quaternion.identity, transform);
                    _controllers.Add(floor);

                    floor.StarterVariables(transform.position * 2, transform.childCount - 1, this);
                    Selection.activeGameObject = null;


                    transform.position = Vector3.zero;

                    if (transform.childCount > 1)
                    {
                        _controllers[0].Indexes.Add(1);
                        _controllers[1].Indexes.Add(0);
                        //Destroy(this);
                        notLibrary = false;
                        //transform.gameObject.SetActive(false);
                    }
                }
            }



            for (int i = 0; i < transform.childCount; i++)
            {
                Transform trChild = transform.GetChild(i);
                Gizmos.DrawLine(trChild.position, transform.position);
            }
        }
    }
}
