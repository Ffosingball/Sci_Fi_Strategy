using UnityEngine;
using System;


public class AllEvents: MonoBehaviour
{
	public event Action<Vector2> OnCameraMove;
    public event Action<Vector2> OnCameraZoom;

    public void CallOnCameraMove(Vector2 value) 
    {
        OnCameraMove?.Invoke(value);
    }

    public void CallOnCameraZoom(Vector2 value)
    {
        OnCameraMove?.Invoke(value);
    }
}
