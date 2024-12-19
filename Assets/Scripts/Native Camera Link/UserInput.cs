using UnityEngine;

public class UserInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (NativeCamera.IsCameraBusy()) return;

            if(Input.mousePosition.x < Screen.width / 2)
            {
                NativeCameraLink.instance.Picture(512);
            }
        }
    }
}
