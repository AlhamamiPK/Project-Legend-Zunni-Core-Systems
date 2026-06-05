using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// PlayerController: Handles player input, movement, animation.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Inspector Fields
    [Header("References")]
    [Tooltip("To control the camera")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("To control the rigidbody")]
    [SerializeField] Rigidbody2D rigidBody;
    [Tooltip("To control the animator")]
    [SerializeField] private Animator anim;

    [Header("KnockBack")]
    [Tooltip("The strength of the push")]
    [SerializeField] private Vector3 pushFromEnemy;

    [Header("Runtime")]
    public static bool isPlayerRunningWithZeroInterruption;
    private bool IsKnockedBack => knockBackTimer > 0;



    #endregion

    #region Constants
    private const string AnimIsRunning = "IsRunning";
    private const string AnimAttack = "Attack";
    #endregion

    #region Inputs
    private InputAction pointAction;
    private InputAction clickAction;
    private InputAction moveAction;
    #endregion

    #region Runtime Variables
    private Vector2 mouseWorldPos;
    private Vector2 currentMovementInput;
    private float knockBackTimer = 0f;
    #endregion

    #region Singleton
    public static PlayerController Instance { get; private set; }
    private void Awake()
    {
       if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
       Instance = this;
    }
    #endregion
    #region Unity Lifecycle
    void Start()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        clickAction = InputSystem.actions.FindAction("Click");
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        bool hoveringShop = ShopManager.isHoveringShop;
        // STOPS movement and facing logic during knockback
        if (IsKnockedBack)
        {
            knockBackTimer -= Time.deltaTime;
            return;
        }
        // Checks if the mouse is hovering over the shop
        if (hoveringShop)
        {
            anim.SetBool(AnimIsRunning, false); // Force idle animation
        }

        // Only run animation if we are moving AND not in the shop
        if (Mathf.Abs(rigidBody.linearVelocity.x) > 0.1f && !hoveringShop)
        {
            isPlayerRunningWithZeroInterruption = true;
            anim.SetBool(AnimIsRunning, true);
        }
        else
        {
            isPlayerRunningWithZeroInterruption = false;
            anim.SetBool(AnimIsRunning, false);

            PlayerStats.Instance.currentMultiplier = 1f;
            PlayerStats.Instance.virtualSpeed = 0f;
        }

        Vector2 screenPos = pointAction.ReadValue<Vector2>();
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        HandleMovement();
        
        // Only face mouse if we aren't using the UI
        if (!hoveringShop) FaceMouse();
    }
    
    private void FixedUpdate()
    {
        bool hoveringShop = ShopManager.isHoveringShop;
        // STOPS clicking movement during knockback
        if (IsKnockedBack) return;

        if (hoveringShop)
        {
            rigidBody.linearVelocity = new Vector2(0, rigidBody.linearVelocity.y);
            return;
        }
        if (clickAction.IsPressed())
        {
            MoveToMouse();
        } else
        if (currentMovementInput.x != 0)
        {


            rigidBody.linearVelocity = new Vector2(currentMovementInput.x * PlayerStats.Instance.currentMoveSpeed, rigidBody.linearVelocityY);

        }
        else
        {


            rigidBody.linearVelocity = new Vector2(0, rigidBody.linearVelocityY);

        }
        
        
    }
    #endregion

    #region Movement
    void HandleMovement()
    {
        currentMovementInput = moveAction.ReadValue<Vector2>();
        

    }
    void MoveToMouse()
    {
        Vector2 targetPos = new Vector2(mouseWorldPos.x, transform.position.y);

        Vector2 direction = targetPos - (Vector2)transform.position;
        direction.Normalize();

        rigidBody.linearVelocity = new Vector2(direction.x * PlayerStats.Instance.currentMoveSpeed, rigidBody.linearVelocity.y);
    }
    void FaceMouse()
    {
        if (mouseWorldPos.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (mouseWorldPos.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    #endregion

    #region Animation
    public void TriggerAttackAnimation()
    {
        if (anim != null)
        {
            anim.SetTrigger(AnimAttack); // This "flips the switch" in the Animator
        }

    }
    #endregion

    #region Knockback
    public void TakeKnockBack(Vector2 force, float duration)
    {
        knockBackTimer = duration;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.AddForce(force, ForceMode2D.Impulse);
    }
    #endregion
}
