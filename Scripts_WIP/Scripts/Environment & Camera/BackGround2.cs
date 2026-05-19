using UnityEngine;
using System.Collections.Generic; // Needed for Lists

public class BackGround2 : MonoBehaviour
{
    [System.Serializable] // This makes the class visible in the Inspector
    public class Layer
    {
        public string name; // Optional, just to help you organize
        public Transform transform; // The background object
        [Range(0f, 1f)] public float effectStrength; // Slider from 0 to 1
    }

    #region Variables
    [Header("Setup")]
    [SerializeField] Transform CameraPos;
    [SerializeField] List<Layer> backgroundLayers = new List<Layer>(); // The list of all your layers

    private Vector3 LastCameraPos;
    #endregion

    void Start()
    {
        if (CameraPos == null)
            CameraPos = Camera.main.transform; // Auto-find camera if you forgot

        LastCameraPos = CameraPos.position;
    }

    void LateUpdate()
    {
        BackGroundMovementHandler();
    }

    void BackGroundMovementHandler()
    {
        // 1. Calculate how much the camera moved this frame
        Vector3 deltaMovement = CameraPos.position - LastCameraPos;

        float directionX = 0f;
        if(Mathf.Abs(deltaMovement.x)> 0.001f)
        {
            directionX = Mathf.Sign(deltaMovement.x);
        }

        float fakeSpeed = 0f;
        if(PlayerStats.Instance != null)
        {
            fakeSpeed = PlayerStats.Instance.virtualSpeed*Time.deltaTime;
        }
        // This is the extra distance the camera "feels" like it moved
        Vector3 virtualDelta = new Vector3(directionX * fakeSpeed, 0, 0);

        // 4. Loop through every layer and apply both Real and Fake movement
        foreach (Layer layer in backgroundLayers)
        {
            if (layer.transform != null)
            {
                // REAL MOVEMENT: Moves the layer with the camera
                Vector3 normalMove = deltaMovement * layer.effectStrength;

                // FAKE MOVEMENT (The Treadmill):
                // We move the layer BACKWARDS (-virtualDelta).
                // We multiply by (1 - effectStrength) so the Sky (1) doesn't move backwards,
                // but the Grass (0.4) moves backwards very fast!
                Vector3 treadmillMove = -virtualDelta * (1f - layer.effectStrength);

                // Apply both!
                layer.transform.position += new Vector3(normalMove.x + treadmillMove.x, normalMove.y, 0);
            }
        }

        // 5. Update the last position for the next frame
        LastCameraPos = CameraPos.position;
    
    //// 2. Loop through every layer in your list and move it
    //foreach (Layer layer in backgroundLayers)
    //{
    //    if (layer.transform != null)
    //    {
    //        // Move the layer based on the camera move * its specific strength
    //        Vector3 layerMove = deltaMovement * layer.effectStrength;

    //        // We usually only want parallax on X and Y, not Z
    //        layer.transform.position += new Vector3(layerMove.x, layerMove.y, 0);
    //    }
    //}

    //// 3. Update the last position for the next frame
    //LastCameraPos = CameraPos.position;
}
}