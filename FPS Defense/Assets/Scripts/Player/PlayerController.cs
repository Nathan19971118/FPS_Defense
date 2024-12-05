using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Models;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private DefaultInput defaultInput;
    [HideInInspector]
    public Vector2 inputMovement;
    [HideInInspector]
    public Vector2 inputView;

    private Vector3 newCameraRotation;
    private Vector3 newPlayerRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70f;
    public float viewClampYMax = 80f;
    public LayerMask playerMask;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;

    private float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCentreVelocity;
    private float stanceCapsuleHeightVelocity;

    private bool isSprint;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public WeaponController currentWeapon;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Player.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        defaultInput.Player.View.performed += e => inputView = e.ReadValue<Vector2>();
        defaultInput.Player.Jump.performed += e => Jump();
        defaultInput.Player.Crouch.performed += e => Crouch();
        defaultInput.Player.Prone.performed += e => Prone();
        defaultInput.Player.Sprint.performed += e => ToggleSprint();
        defaultInput.Player.SprintReleased.performed += e => StopSprint();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newPlayerRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon)
        {
            currentWeapon.Initialise(this);
        }
    }

    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
    }

    private void CalculateView()
    {
        newPlayerRotation.y += playerSettings.viewXSensitivity * (playerSettings.viewXInverted ? -inputView.x : inputView.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newPlayerRotation);

        newCameraRotation.x += playerSettings.viewYSensitivity * (playerSettings.viewYInverted ? inputView.y : -inputView.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        if (inputMovement.y <= 0.2f)
        {
            isSprint = false;
        }

        var verticalSpeed = playerSettings.walkingForwardSpeed;
        var horizontalSpeed = playerSettings.walkingStrafeSpeed;

        if (isSprint)
        {
            verticalSpeed = playerSettings.runningForwardSpeed;
            horizontalSpeed = playerSettings.runningStrafeSpeed;
        }

        // Effectors
        if (!characterController.isGrounded)
        {
            playerSettings.speedEffector = playerSettings.fallingSpeedEffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.speedEffector = playerSettings.coruchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.speedEffector = playerSettings.proneSpeedEffector;
        }
        else
        {
            playerSettings.speedEffector = 1;
        }

        verticalSpeed *= playerSettings.speedEffector;
        horizontalSpeed *= playerSettings.speedEffector;

        newMovementSpeed =
            Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * inputMovement.x * Time.deltaTime, 0, verticalSpeed * inputMovement.y * Time.deltaTime),
            ref newMovementSpeedVelocity, characterController.isGrounded ? playerSettings.movementSmoothing : playerSettings.fallingSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }

        if (playerGravity < -0.1f && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.jumpingFalloff);
    }

    private void CalculateStance()
    {
        var currentStance = playerStandStance;

        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            currentStance = playerProneStance;
        }

        // Adjust Camera Height
        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.cameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        // Adjust Stance
        characterController.height = Mathf.SmoothDamp
            (characterController.height, currentStance.stanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp
            (characterController.center, currentStance.stanceCollider.center, ref stanceCapsuleCentreVelocity, playerStanceSmoothing);
    }

    private void Jump()
    {
        if(!characterController.isGrounded || playerStance == PlayerStance.Prone)
        {
            return;
        }

        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        // Jump
        jumpingForce = Vector3.up * playerSettings.jumpingHeight;
        playerGravity = 0;
    }

    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.stanceCollider.height))
        {
            return;
        }

        playerStance = PlayerStance.Crouch;
    }

    private void Prone()
    {
        playerStance = PlayerStance.Prone;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }

    private void ToggleSprint()
    {
        if (inputMovement.y <= 0.2f)
        {
            isSprint = false;
            return;
        }

        isSprint = !isSprint;
    }

    private void StopSprint()
    {
        if (playerSettings.sprintingHold)
        {
            isSprint = false;
        }
    }
}
