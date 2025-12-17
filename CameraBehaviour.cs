using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;


public class CameraBehaviour: MonoBehaviour
{
    public AllEvents events;
	public float movementSpeed;
	public float zoomSpeed;

    private Vector2 movementDirection;
	private float cameraScale;
	private Transform transform;

	// Use this for initialization
	void Start()
	{
		events.OnCameraMove += changeMovementDirection;
        events.OnCameraZoom += changeCameraScale;
		transform = GetComponent<Transform>();
    }

	void changeMovementDirection(Vector2 direction) 
	{
		movementDirection = direction;
	}

    void changeCameraScale(Vector2 scale)
    {
        cameraScale = scale.y;
    }

    // Update is called once per frame
    void Update()
	{
		transform.Translate(movementDirection*movementSpeed*Time.deltaTime);
	}
}
