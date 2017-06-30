using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.1f;
    public float gravity = -12;
    

    private Animator animator;
    private float currentSpeed;
    private float turnSmoothVelocity;
    private float speedSmoothVelocity;
    private CharacterController controller;
    private float velocityY;

    Transform cameraT;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        cameraT = Camera.main.transform;
        controller = GetComponent<CharacterController>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }
        bool running = !Input.GetKey(KeyCode.RightShift);
        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = (transform.forward * currentSpeed) + (Vector3.up * velocityY);
        controller.Move(velocity * Time.deltaTime);

        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded) velocityY = 0;

        float animationSpeedPercent = (running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f;
        animator.SetFloat("SpeedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        
        //if (animator.GetFloat("SpeedPercent") < 0.5f && animator.GetFloat("SpeedPercent") < 1) animator.speed = 2f;
	}
}
