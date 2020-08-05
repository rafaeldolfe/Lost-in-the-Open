using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public class CameraManager : MonoBehaviour
    {
        private float minX = Mathf.Infinity;
        private float maxX = Mathf.NegativeInfinity;
        private float minY = Mathf.Infinity;
        private float maxY = Mathf.NegativeInfinity;
        private float camOffset = Constants.TILE_OFFSET;
        private Camera mainCamera;

        private List<GameObject> attachedToCamera = new List<GameObject>();

        public float cameraMoveSpeed;

        void Awake()
        {
            mainCamera = gameObject.GetComponent<Camera>();
            StartCoroutine(CalculateBounds());
        }
        private IEnumerator CalculateBounds()
        {
            yield return new WaitForEndOfFrame();
            foreach (Vector2Int bound in bounds)
            {
                if (minX > bound.x) minX = bound.x;
                else if (maxX < bound.x) maxX = bound.x;
                if (minY > bound.y) minY = bound.y;
                else if (maxY < bound.y) maxY = bound.y;
            }
            minX = minX - camOffset;
            minY = minY - camOffset;
            maxX = maxX + camOffset;
            maxY = maxY + camOffset;
        }
        void Update()
        {
            Vector3 camPos = mainCamera.transform.position;
            float camVertSize = mainCamera.orthographicSize;
            float camHoriSize = camVertSize * mainCamera.aspect;
            if (Input.GetKey("w"))
            {
                if (camPos.y + camVertSize + 1 * cameraMoveSpeed <= maxY)
                {
                    mainCamera.transform.position += new Vector3(0, 1, 0) * cameraMoveSpeed;
                    foreach (GameObject attached in attachedToCamera)
                    {
                        attached.transform.position += new Vector3(0, 1, 0) * cameraMoveSpeed;
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
                if (camPos.y - camVertSize - 1 * cameraMoveSpeed >= minY)
                {
                    mainCamera.transform.position += new Vector3(0, -1, 0) * cameraMoveSpeed;
                    foreach (GameObject attached in attachedToCamera)
                    {
                        attached.transform.position += new Vector3(0, -1, 0) * cameraMoveSpeed;
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
        public void SetPosition(float x, float y)
        {
            Vector3 transposition = new Vector3(x - transform.position.x, y - transform.position.y, 0);
            foreach (GameObject attached in attachedToCamera)
            {
                attached.transform.position += transposition;
            }
            transform.position = new Vector3(x, y, transform.position.z);
        }
        private List<Vector2Int> bounds = new List<Vector2Int>();
        public void AddBounds(Vector2Int position)
        {
            bounds.Add(position);
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
}