using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;

    [Header("Movement")]
    public float speed = 5.0f;
    public float sprintSpeed = 8.0f;
    public float jumpHeight = 1.5f;
    
    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 2.0f;

    [Header("Surface Check")]
    public LayerMask groundMask = ~0; // Default to everything
    public float groundCheckDistance = 0.2f;
    public GameObject surfaceCheck;

    [Header("Animation")]
    public Animator animator;
    public Rifle activeRifle;

    private CharacterController controller;
    private float rotationX = 0f;
    private float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Hide and lock cursor to screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleWeaponSwitching();
        HandleLook();
        HandleMovement();
        HandleGravityAndJump();
        UpdateAnimation();
    }

    void HandleWeaponSwitching()
    {
        if (activeRifle == null) return;

        // Press '1' to equip rifle
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activeRifle.gameObject.SetActive(true);
        }
        // Press '3' to holster rifle (unarmed)
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            activeRifle.gameObject.SetActive(false);
        }
    }

    void HandleLook()
    {
        // Mouse Look Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // WASD Movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        // Jump and Gravity with Surface Check
        isGrounded = CheckSurface();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Clamp to ground
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
            {
                animator.SetBool("Jump", true);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    bool CheckSurface()
    {
        RaycastHit hit;
        Vector3 origin;
        float distance;

        if (surfaceCheck != null)
        {
            origin = surfaceCheck.transform.position;
            distance = groundCheckDistance;
        }
        else
        {
            // Fallback to controller bottom if surfaceCheck is not assigned
            origin = new Vector3(transform.position.x, controller.bounds.min.y + 0.1f, transform.position.z);
            distance = 0.1f + groundCheckDistance;
        }

        if (Physics.Raycast(origin, Vector3.down, out hit, distance, groundMask))
        {
            return true;
        }

        return false;
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Get movement inputs to drive locomotion
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        // Detect if the player is holding an active rifle
        bool holdingRifle = activeRifle != null && activeRifle.gameObject.activeInHierarchy;

        // Determine states based on movement inputs, weapon, and ground status
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        
        // Split Walk vs RifleWalk cleanly
        bool isWalking = isMoving && !isSprinting && isGrounded && !holdingRifle;
        bool isRifleWalking = isMoving && !isSprinting && isGrounded && holdingRifle;
        
        bool isRunning = isMoving && isSprinting && isGrounded;
        
        // Idle is only true if we are stationary, grounded, and NOT holding a rifle
        // (If holding a rifle and stationary, we play Rifle Aim ready stance instead)
        bool isIdle = !isMoving && isGrounded && !holdingRifle;

        // Update basic locomotion booleans in the animator
        animator.SetBool("Walk", isWalking);
        animator.SetBool("RifleWalk", isRifleWalking);
        animator.SetBool("Run", isRunning);
        animator.SetBool("Idle", isIdle);
        animator.SetBool("Jump", !isGrounded);

        // Update action and combat booleans
        // If holding a rifle, Aim is true when stationary (ready stance) OR when explicitly holding right click to aim
        bool isAiming = holdingRifle && (!isMoving || Input.GetMouseButton(1));
        bool isReloading = holdingRifle ? activeRifle.IsReloading : false; // Use rifle's reload state
        bool isFiring = Input.GetMouseButton(0); // Left click to shoot/punch

        animator.SetBool("Aim", isAiming);
        animator.SetBool("Reloading", isReloading);

        // Shoot Walk (firing weapon while moving - rifle only)
        animator.SetBool("Shoot Walk", holdingRifle && isFiring && isMoving);

        // Shooting (firing weapon while stationary and aiming - rifle only)
        animator.SetBool("Shooting", holdingRifle && isFiring && !isMoving && isAiming);

        // Punch (punching while stationary and not aiming - unarmed only)
        animator.SetBool("Punch", !holdingRifle && isFiring && !isMoving);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Player took " + damage + " damage. Current health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died");
        // Implement death behavior here (e.g., reloading the level)
    }
}

