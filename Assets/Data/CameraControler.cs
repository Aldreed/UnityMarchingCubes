using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToTarget = 10;
    [SerializeField] public float speed = 5.0f;
    private Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * 180;
            float rotationAroundXAxis = direction.y * 180;

            cam.transform.position = target.position;


            transform.eulerAngles += new Vector3(rotationAroundXAxis , rotationAroundYAxis, 0);

            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

            previousPosition = newPosition;
        }
        
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            distanceToTarget -= Input.GetAxis("Mouse ScrollWheel");

            if (distanceToTarget < 10)
            {
                distanceToTarget = 10;
            }
            if (distanceToTarget > 500)
            {
                distanceToTarget = 500;
            }
            cam.transform.position = target.position;
            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));
        }

        float deltaX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float deltaY = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        cam.transform.position = target.position;
        transform.eulerAngles += new Vector3(deltaY, deltaX, 0);
        cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

    }

}
