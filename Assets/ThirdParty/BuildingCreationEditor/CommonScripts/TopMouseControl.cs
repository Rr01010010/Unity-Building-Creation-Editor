using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopMouseControl : MonoBehaviour
{
    [SerializeField] private Camera Camera;
    [SerializeField] private Vector3 _sight;
    [SerializeField] private float SensetiveX = 1, SensetiveY = 1;
    private Vector3 pos1, pos2;
    private float UpdateSens;


    void Start() { Cursor.lockState = CursorLockMode.Locked; }

    
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl)) { StateCursor = CursorLockMode.Confined; }

        if (StateCursor == CursorLockMode.Locked)
        {
            Scrolling();
            SightMovement();
        }
    }



    public CursorLockMode StateCursor
    {
        get => Cursor.lockState;
        set
        {
            if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public Vector3 Sight { get => _sight; set => _sight = value; }


    private void Scrolling()
    {


        float dScroll = Input.mouseScrollDelta.y;
        Camera.orthographicSize -= (dScroll * Camera.orthographicSize / 13);
        if (Camera.orthographicSize <= 0) { Camera.orthographicSize = 1; }

        Vector3 center = Camera.transform.position;

        UpdateSens = Camera.orthographicSize / 10;

        pos1 = center - new Vector3(Camera.orthographicSize * (Screen.height / 600.01f), center.y, Camera.orthographicSize * (Screen.width / 600.01f));
        pos2 = center + new Vector3(Camera.orthographicSize * (Screen.height / 600.01f), -center.y, Camera.orthographicSize * (Screen.width / 600.01f));
    }
    private void SightMovement()
    {
        if (StateCursor == CursorLockMode.Locked)
        {
            float addX, addY;
            addX = Input.GetAxis("Mouse X") * SensetiveX * UpdateSens;
            addY = Input.GetAxis("Mouse Y") * SensetiveY * UpdateSens;
            Sight += new Vector3(addY, 0, -addX);

            if (Sight.x < pos1.x) Sight = new Vector3(pos1.x, Sight.y, Sight.z);
            if (Sight.z < pos1.z) Sight = new Vector3(Sight.x, Sight.y, pos1.z);


            if (Sight.x > pos2.x) Sight = new Vector3(pos2.x, Sight.y, Sight.z);
            if (Sight.z > pos2.z) Sight = new Vector3(Sight.x, Sight.y, pos2.z);
        }
    }


    #region DrawSight
    private void DrawZoneOfView()
    {
        Gizmos.color = Color.red;
        Vector3 medium = new Vector3(pos1.x, pos2.y, pos2.z);
        Gizmos.DrawLine(pos1, medium);
        Gizmos.DrawLine(pos2, medium);
        medium = new Vector3(pos2.x, pos2.y, pos1.z);
        Gizmos.DrawLine(pos1, medium);
        Gizmos.DrawLine(pos2, medium);
    }

    private void OnDrawGizmos()
    {
        DrawZoneOfView();
    }
    #endregion
}
