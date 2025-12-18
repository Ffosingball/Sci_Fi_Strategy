using UnityEngine;


public class CameraBehaviour: MonoBehaviour
{
    public AllEvents events;
	public float movementSpeed;
	public float zoomSpeed;
    public Vector2 zoomConfines;

    private Vector2 movementDirection;
	private float cameraScale=0;
	private Transform transform;
    private Camera camera;

    // Use this for initialization
    void Start()
	{
		events.OnCameraMove += changeMovementDirection;
        events.OnCameraZoom += changeCameraScale;
		transform = GetComponent<Transform>();
        camera = GetComponent<Camera>();
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
		transform.Translate(movementDirection*movementSpeed*Time.deltaTime* camera.orthographicSize);

        camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + (-cameraScale*zoomSpeed*Time.deltaTime),zoomConfines.x,zoomConfines.y);
        events.CallOnBackgroundResize(camera.orthographicSize);
    }
}
