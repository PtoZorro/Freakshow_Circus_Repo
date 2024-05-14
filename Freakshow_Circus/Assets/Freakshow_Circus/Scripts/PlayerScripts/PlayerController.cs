using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Referencias privadas
    Rigidbody rb;
    Animator anim;

    [Header("Movement & Look Stats")]
    [SerializeField] GameObject camHolder;
    public float speed, sprintSpeed, crouchSpeed, maxForce, sensitivity;
    bool isSprinting;
    bool isCrouching;

    [Header("Jumping & GroundCheck Configuration")]
    public float jumpForce;
    [SerializeField] GameObject groundCheck;
    [SerializeField] bool isGrounded;
    [SerializeField] float groundDetectRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    [Header("Feedback & Graphics")]
    [SerializeField] GameObject handsGun;
    [SerializeField] GameObject handsAxe;
    [SerializeField] GameObject handsFlash;
    Animator gunAnim;
    Animator axeAnim;
    Animator flashAnim;

    //Valores privados
    Vector2 move;
    Vector2 look;
    float lookRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        gunAnim = handsGun.GetComponent<Animator>();
        axeAnim = handsAxe.GetComponent<Animator>();
        flashAnim = handsFlash.GetComponent<Animator>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDetectRadius, groundLayer);

        if (move == Vector2.zero)
        {
            gunAnim.SetBool("iddle", true);
            axeAnim.SetBool("iddle", true);
            flashAnim.SetBool("iddle", true);
            gunAnim.SetBool("walk", false);
            axeAnim.SetBool("walk", false);
            flashAnim.SetBool("walk", false);
        }
        else
        {
            gunAnim.SetBool("iddle", false);
            axeAnim.SetBool("iddle", false);
            flashAnim.SetBool("iddle", false);
            gunAnim.SetBool("walk", true);
            axeAnim.SetBool("walk", true);
            flashAnim.SetBool("walk", true);
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void LateUpdate()
    {
        CameraMoveLook();
    }

    void Movement()
    {
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);
        targetVelocity *= isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : speed);

        //Alinear la dirección con la orientación correcta
        targetVelocity = transform.TransformDirection(targetVelocity);

        //Calcular las fuerzas que afectan al movimiento
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        //Limitar la fuerza máxima
        Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void CameraMoveLook()
    {
        //Girar
        transform.Rotate(Vector3.up * look.x * sensitivity);
        //Mirar
        lookRotation += (-look.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y, camHolder.transform.eulerAngles.z);
    }

    void Jump()
    {
        Vector3 jumpForces = rb.velocity;
        if (isGrounded)
        {
            jumpForces.y = jumpForce;
        }

        rb.velocity = jumpForces;
    }
    #region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = context.ReadValueAsButton();
        anim.SetBool("isCrouching", isCrouching);
    }
    #endregion
}
