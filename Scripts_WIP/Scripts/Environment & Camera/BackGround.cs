using UnityEngine;
using UnityEngine.UI;

public class BackGroundMovement : MonoBehaviour
{
    #region Variables - Transforms
    [Header("Our Transforms")]
    [SerializeField] Transform BackGround;
    [SerializeField] Transform CameraPos;
    #endregion
    #region Variables - Varaibles
    [Header("MoveVaraibles")]
    public float EffectStrength = 1f;
    public Vector3 LastCameraPos;
    public Vector3 NewBackGroundTranform;
    #endregion
    void Start()
    {
        LastCameraPos = CameraPos.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        BackGroundMovementHandler();
    }
    #region Custom Functions (Logic)
    void BackGroundMovementHandler()
    {
        NewBackGroundTranform = (CameraPos.position - LastCameraPos) * EffectStrength;
        LastCameraPos = CameraPos.position;
        BackGround.position += NewBackGroundTranform;
    }

    #endregion
}
