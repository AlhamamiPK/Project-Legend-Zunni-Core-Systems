using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class CustomMouse : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera mainCamera;
    //[SerializeField] private ParticleSystem cliclEffect;
    [SerializeField] public Image imageForMyMouse;
    public static CustomMouse Instance;

    private InputAction pointAction;
    private InputAction cliclAction;
     void Start()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        cliclAction = InputSystem.actions.FindAction("Click");
        Cursor.visible = false;
    }

     void Update()
    {
        FollowMousePosition();
        HandleMouseClicks();
    }
    private void Awake()
    {
        Camera mainCam = Camera.main;
        if(mainCam == null)
        {
            mainCam = Camera.main;
        }
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    void FollowMousePosition()
    {
        Vector2 screenPos = pointAction.ReadValue<Vector2>();

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        worldPos.z = 0f;
        transform.position = worldPos;
    }
    void HandleMouseClicks()
    {
        if (cliclAction.WasPerformedThisFrame())
        {
            //Debug.Log("I CKILLKED MTHA FUUCKA");
           // cliclEffect.startSize = +0.3f; 
        }
    }
}
