using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public AllEvents events;

    public void OnZoom(InputValue value)
    {
        events.CallOnCameraZoom(value.Get<Vector2>());
    }


    public void OnMove(InputValue value)
    {
        events.CallOnCameraMove(value.Get<Vector2>());
    }
}
