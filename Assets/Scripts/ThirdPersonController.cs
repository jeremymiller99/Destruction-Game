using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    //input fields
    private ThirdPersonActionsAsset playerActionsAsset;
    private InputAction move;

    //movement fields
    private Rigidbody rb;
    [SerializeField]
    private float movementForce = 3f;
    [SerializeField]
    private float jumpForce = 10f;
    [SerializeField]
    private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;
    public bool isGrounded;

    [SerializeField]
    private Camera playerCamera;
    private Animator animator;

    //punch fields
    private BoxCollider punchHitBox;
    private float punchDuration = 0.1f;
    private Vector3 knockbackDirection = Vector3.zero;
    private Rigidbody destructibleRb;
    [SerializeField]
    private float knockbackForce;

    //damage fields
    [SerializeField]
    private float baseDamage = 1f;

    //animation fields
    private bool isWalking = false;
    private bool isRunning = false;

    private int isWalkingHash;
    private int isRunningHash;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        playerActionsAsset = new ThirdPersonActionsAsset();
        animator = this.GetComponent<Animator>();
        
        punchHitBox = GetComponentInChildren<BoxCollider>();
        punchHitBox.enabled = false;

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera.enabled = true;
    }

    private void OnEnable()
    {
        playerActionsAsset.Player.Jump.started += DoJump;
        playerActionsAsset.Player.Punch.started += DoAttack;
        playerActionsAsset.Player.Sprint.started += DoSprint;
        playerActionsAsset.Player.Sprint.canceled += DoSprint;
        move = playerActionsAsset.Player.Move;
        playerActionsAsset.Player.Enable();
    }

    private void OnDisable()
    {
        playerActionsAsset.Player.Jump.started -= DoJump;
        playerActionsAsset.Player.Punch.started -= DoAttack;
        playerActionsAsset.Player.Sprint.started -= DoSprint;
        playerActionsAsset.Player.Sprint.canceled -= DoSprint;
        playerActionsAsset.Player.Disable();
    }

    private void Update()
    {
        IsGrounded();
        HandleAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        LookAt();
    }

    private void HandleMovement()
    {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;

        rb.AddForce(forceDirection, ForceMode.Impulse);

        if (forceDirection == Vector3.zero)
        {
            isWalking = false;
            isRunning = false;
        }
        else if (forceDirection != Vector3.zero && movementForce > 4f)
        {
            isWalking = true;
            isRunning = true;
        } else
        {
            isWalking = true;
            isRunning = false;
        }

        forceDirection = Vector3.zero;


        if (rb.velocity.y < 0f)
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
    }

    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (obj.started)
            if (isGrounded)
                forceDirection += Vector3.up * jumpForce;
        
    }

    private void DoSprint(InputAction.CallbackContext obj)
    {
        if (obj.started)
            movementForce = 15f;
            isRunning = true;
        if (obj.performed)
            movementForce = 15f;
            isRunning = true;
        if (obj.canceled)
            movementForce = 4f;
            isRunning = false;
    }

    private void IsGrounded()
    {
        Ray ray = new Ray(this.transform.position, Vector3.down);
        //if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
            //isGrounded = true;
        //else
            //isGrounded = false;
    }

    private void DoAttack(InputAction.CallbackContext obj)
    {
        punchHitBox.enabled = true;
        Invoke("DisablePunchHitbox", punchDuration);
    }

    private void DisablePunchHitbox()
    {
        punchHitBox.enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        DestructibleObject obj = other.GetComponent<DestructibleObject>();
        
        if(punchHitBox.enabled && obj != null)
            if(other.CompareTag("Destructible") && obj.hasBeenHit == false)
                destructibleRb = other.GetComponent<Rigidbody>();
                if(destructibleRb != null)
                    knockbackDirection = other.transform.position - transform.position;
                    knockbackDirection.y = 0.0f;
                    knockbackDirection.Normalize();

                    //destructibleRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

            obj.TakeDamage(baseDamage);     
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void HandleAnimations()
    {
        if (isWalking)
            animator.SetBool(isWalkingHash, true);
        else
            animator.SetBool(isWalkingHash, false);
        if(isRunning)
            animator.SetBool(isRunningHash, true);
        else
            animator.SetBool(isRunningHash, false);
        if(!isWalking && !isRunning)
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
    }
}