using UnityEngine;

public class AttachToPlayrsCamera : MonoBehaviour
{
    private void Awake()
    {
        Canvas myCanvas = GetComponent<Canvas>();
        Camera mainCam = Camera.main;

        if(mainCam !=null)
        {
            myCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            myCanvas.worldCamera = mainCam;
            myCanvas.planeDistance = 100f;
        }
        else
        {

            Debug.LogError("YOYO, no Main Camera found! Make sure your actual scene camera has the 'MainCamera' tag at the top of its inspector!");
        }
    }
}
