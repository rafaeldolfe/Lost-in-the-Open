using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;
    private Camera mainCamera;

    private List<GameObject> attachedToCamera;

    public GameObject floor;
    public float cameraMoveSpeed;

    void Awake()
    {
        attachedToCamera = new List<GameObject>();
        mainCamera = gameObject.GetComponent<Camera>();
        Bounds bounds = floor.GetComponent<MeshRenderer>().bounds;
        minX = bounds.min.x;
        maxX = bounds.max.x; 
        minZ = bounds.min.z;
        maxZ = bounds.max.z;
    }

    void Update()
    {
        Vector3 camPos = mainCamera.transform.position;
        float camVertSize = mainCamera.orthographicSize;
        float camHoriSize = camVertSize * mainCamera.aspect;
        if (Input.GetKey("w"))
        {
            if (camPos.z + camVertSize + 1 * cameraMoveSpeed <= maxZ)
            {
                mainCamera.transform.position += new Vector3(0, 0, 1) * cameraMoveSpeed;
                foreach(GameObject attached in attachedToCamera)
                {
                    attached.transform.position += new Vector3(0, 0, 1) * cameraMoveSpeed;
                }
            }
        }
        if (Input.GetKey("a"))
        {
            if (camPos.x - camHoriSize - 1 * cameraMoveSpeed >= minX)
            {
                mainCamera.transform.position += new Vector3(-1, 0, 0) * cameraMoveSpeed;
                foreach (GameObject attached in attachedToCamera)
                {
                    attached.transform.position += new Vector3(-1, 0, 0) * cameraMoveSpeed;
                }
            }
        }
        if (Input.GetKey("s"))
        {
            if (camPos.z - camVertSize - 1 * cameraMoveSpeed >= minZ)
            {
                mainCamera.transform.position += new Vector3(0, 0, -1) * cameraMoveSpeed;
                foreach (GameObject attached in attachedToCamera)
                {
                    attached.transform.position += new Vector3(0, 0, -1) * cameraMoveSpeed;
                }
            }
        }
        if (Input.GetKey("d"))
        {
            if (camPos.x + camHoriSize + 1 * cameraMoveSpeed <= maxX)
            {
                mainCamera.transform.position += new Vector3(1, 0, 0) * cameraMoveSpeed;
                foreach (GameObject attached in attachedToCamera)
                {
                    attached.transform.position += new Vector3(1, 0, 0) * cameraMoveSpeed;
                }
            }
        }
    }

    public void AttachToCamera(GameObject gameObject)
    {
        attachedToCamera.Add(gameObject);
    }

    public void Deattach(GameObject gameObject)
    {
        attachedToCamera.Remove(gameObject);
    }
}
