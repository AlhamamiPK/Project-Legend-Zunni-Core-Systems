using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [Header("Settings")]
    //[SerializeField] private float moveSpeed = PlayerStats.Instance.moveSpeed;
    [SerializeField] private Camera mainCamera;
    [SerializeField] public Vector3 PushFromEnemy;
    [SerializeField] Rigidbody2D RB;
    [SerializeField] private Animator anim;
    private InputAction pointAction;
    private InputAction clickAction;
    private InputAction moveAction;

    private Vector2 mouseWorldPos;
    private Vector2 currentMovementInput;
    private bool isHoveringUI;
    public static bool isPlayerRunningWithZeroInterrputtion;
    private float knockBackTimer = 0f;

    private void Awake()
    {
        if(instance == null) instance= this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        clickAction = InputSystem.actions.FindAction("Click");
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {// STOPS movement and facing logic during knockback
        if (knockBackTimer > 0)
        {
            knockBackTimer -= Time.deltaTime;
            return;
        }
        // Just read the static variable from ShopManager!
        if (ShopManager.isHoveringShop)
        {
            anim.SetBool("IsRunning", false); // Force idle animation
        }

        // Only run animation if we are moving AND not in the shop
        if (Mathf.Abs(RB.linearVelocity.x) > 0.1f && !ShopManager.isHoveringShop)
        {   
            isPlayerRunningWithZeroInterrputtion = true;
            anim.SetBool("IsRunning", true);
        }
        else
        {
            isPlayerRunningWithZeroInterrputtion=false;
            anim.SetBool("IsRunning", false);

            PlayerStats.Instance.currentMultiplier = 1f;
            PlayerStats.Instance.virtualSpeed = 0f;
        }

        Vector2 screenPos = pointAction.ReadValue<Vector2>();
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        HandleMovement();
        
        // Only face mouse if we aren't using the UI
        if (!ShopManager.isHoveringShop) FaceMouse();
    }
    #region Custome void funcs
    private void FixedUpdate()
    {
        // STOPS clicking movement during knockback
        if (knockBackTimer > 0) return;

        if (ShopManager.isHoveringShop)
        {
            RB.linearVelocity = new Vector2(0, RB.linearVelocity.y);
            return;
        }
        if (clickAction.IsPressed())
        {
            MoveToMouse();
        } else
        if (currentMovementInput.x != 0)
        {


            Vector2 forceDirection = new Vector2(currentMovementInput.x, 0).normalized;
            RB.linearVelocity = new Vector2(currentMovementInput.x * PlayerStats.Instance.currentMoveSpeed, RB.linearVelocityY);

        }
        else
        {


            RB.linearVelocity = new Vector2(0, RB.linearVelocityY);

        }
        
        
    }
    void HandleMovement()
    {
        currentMovementInput = moveAction.ReadValue<Vector2>();
        Vector2 movement = moveAction.ReadValue<Vector2>();
        

    }
    void MoveToMouse()
    {
        Vector2 targetPos = new Vector2(mouseWorldPos.x, transform.position.y);

        Vector2 direction = targetPos - (Vector2)transform.position;
        direction.Normalize();

        RB.linearVelocity = new Vector2(direction.x * PlayerStats.Instance.currentMoveSpeed, RB.linearVelocity.y);
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
    public void TakeKnockBack(Vector2 force, float duration)
    {
        knockBackTimer = duration;
        RB.linearVelocity = Vector2.zero;
        RB.AddForce(force, ForceMode2D.Impulse);
    }
    public void TriggerAttackAnimation()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack"); // This "flips the switch" in the Animator
        }

    }
}
