using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementAndAnimations : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;
    float rotationFactorPerFrame = 15f;
    float walkMultiplier = 3f;
    float runMultiplier = 8f;

    private bool isAttacking = false;

    //punch fields
    private BoxCollider punchHitBox;
    private float punchDuration = 0.1f;
    private Vector3 knockbackDirection = Vector3.zero;
    private Rigidbody destructibleRb;
    [SerializeField]
    private float knockbackForce;
    [SerializeField] private float animationFinishTime = 0.9f;
    private float baseDamage = 1f;
    DestructibleObject obj;

    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        playerInput.CharacterControls.Move.started += OnMovementInput;
        playerInput.CharacterControls.Move.canceled += OnMovementInput;
        playerInput.CharacterControls.Move.performed += OnMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Punch.performed += context => Punch();

        punchHitBox = GetComponentInChildren<BoxCollider>();
        punchHitBox.enabled = false;
    }

    void Punch()
    {
        if (!isAttacking)
        {
            animator.SetTrigger("isAttacking");
            punchHitBox.enabled = true;
            Invoke("DisablePunchHitbox", punchDuration);
            StartCoroutine(InitializePunch());
        }
    }

    private void DisablePunchHitbox()
    {
        punchHitBox.enabled = false;
    }

    IEnumerator InitializePunch()
    {
        yield return new WaitForSeconds(0.1f);
        isAttacking = true;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * walkMultiplier;
        currentMovement.z = currentMovementInput.y * walkMultiplier;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if(isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        } else if(!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        } else if((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            float groundedGravity = -0.05f;
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        } else
        {
            float gravity = -9.8f;
            currentMovement.y += gravity;
            currentRunMovement.y += gravity;
        }
    }

    private void Update()
    {
        if(isAttacking && animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= animationFinishTime)
        {
            isAttacking = false;
        }
    }

    void FixedUpdate()
    {
        HandleRotation();
        HandleAnimation();
        HandleGravity();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        } else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        obj = other.GetComponent<DestructibleObject>();

        if (punchHitBox.enabled && obj != null)
            if (other.CompareTag("Destructible") && obj.hasBeenHit == false)
                destructibleRb = other.GetComponent<Rigidbody>();
        if (destructibleRb != null)
            knockbackDirection = other.transform.position - transform.position;
        knockbackDirection.y = 0.0f;
        knockbackDirection.Normalize();

        //destructibleRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

        obj.TakeDamage(1f);
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
