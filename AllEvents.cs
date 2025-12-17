using UnityEngine;
using System;


public class AllEvents: MonoBehaviour
{
	public event Action<Vector2> OnCameraMove;
    public event Action<Vector2> OnCameraZoom;
    public event Action<float> OnBackgroundResize;

    public void CallOnCameraMove(Vector2 value) 
    {
        OnCameraMove?.Invoke(value);
    }

    public void CallOnCameraZoom(Vector2 value)
    {
        OnCameraZoom?.Invoke(value);
    }

    public void CallOnBackgroundResize(float value)
    {
        OnBackgroundResize?.Invoke(value);
    }
}
