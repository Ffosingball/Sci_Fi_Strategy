using UnityEngine;

public class BackgroundBehaviour : MonoBehaviour
{
    public Camera camera;
    public AllEvents events;

    private float initialCameraSize;
    private float currentCameraSize;
    private Transform transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialCameraSize = camera.orthographicSize;
        events.OnBackgroundResize += getCurrentCameraSize;
        transform=GetComponent<Transform>();
    }

    void getCurrentCameraSize(float camScale)
    {
        currentCameraSize = camScale;
        float scale = currentCameraSize / initialCameraSize;
        transform.localScale = new Vector3(scale, scale, transform.localScale.z);
    }
}
