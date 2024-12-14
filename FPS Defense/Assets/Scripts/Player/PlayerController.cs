using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    public Transform _camera;
    public Transform feetTransform;
    public Image crosshair;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70f;
    public float viewClampYMax = 80f;
    public LayerMask playerMask;
    public LayerMask groundMask;

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
    [HideInInspector]
    public bool isSprint;
    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public WeaponController currentWeapon;
    public float weaponAnimationSpeed;
    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;
    public int ammo;
    public int grenadeAmmo;
    public bool isReload;

    [Header("Weapon Swap")]
    public int weaponIndicator;
    public GameObject[] weapons = new GameObject[3];

    [Header("Leaning")]
    public Transform leanPivot;
    private float currentLean; // Actual value of the lean
    private float targetLean;
    public float leanAngle;
    public float leanSmoothing;
    private float leanVelocity;

    private bool isLeaningLeft;
    private bool isLeaningRight;

    [Header("Aiming In")]
    public bool isAimingIn;

    #region - Awake / Start -

    private void Awake()
    {
        InputSystem();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newPlayerRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon)
        {
            currentWeapon.Initialise(this);
        }
    }

    private void Start()
    {
        SwitchWeapons(0);
    }

    #endregion

    #region - Update -

    private void Update()
    {
        SetIsGrounded();
        SetIsFalling();
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
        CalculateLeaning();
        CalculateAimingIn();
    }

    #endregion

    #region - Shooting -

    public void ShootingPressed()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = true;
        }
    }

    private void ShootingReleased()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = false;
        }
    }

    #endregion

    #region - Input -

    void InputSystem()
    {
        defaultInput = new DefaultInput();

        // Player input
        defaultInput.Player.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        defaultInput.Player.View.performed += e => inputView = e.ReadValue<Vector2>();
        defaultInput.Player.Jump.performed += e => Jump();
        defaultInput.Player.Crouch.performed += e => Crouch();
        defaultInput.Player.Prone.performed += e => Prone();
        defaultInput.Player.Sprint.performed += e => ToggleSprint();
        defaultInput.Player.SprintReleased.performed += e => StopSprint();
        defaultInput.Player.WeaponSwap.performed += e => SwitchWeapons((weaponIndicator < 2) ? weaponIndicator + 1 : 0);

        // Lean
        defaultInput.Player.LeanRightPressed.performed += e => isLeaningRight = true;
        defaultInput.Player.LeanRightReleased.performed += e => isLeaningRight = false;

        defaultInput.Player.LeanLeftPressed.performed += e => isLeaningLeft = true;
        defaultInput.Player.LeanLeftReleased.performed += e => isLeaningLeft = false;

        // Weapon input
        defaultInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleased();

        defaultInput.Weapon.Fire1Pressed.performed += e => ShootingPressed();
        defaultInput.Weapon.Fire1Released.performed += e => ShootingReleased();

        defaultInput.Weapon.FireModeSwitch.performed += e => currentWeapon.CycleFireType();
        defaultInput.Weapon.Reload.performed += e => ReloadAmmo();

        defaultInput.Enable();
    }

    #endregion

    #region - Aiming In -

    private void AimingInPressed()
    {
        isAimingIn = true;
    }

    private void AimingInReleased()
    {
        isAimingIn = false;
    }

    private void CalculateAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }

        // isAimingIn of WeaponController.cs is equal with PlayerController.cs's counterpart
        currentWeapon.isAimingIn = isAimingIn;
    }

    #endregion

    #region - isFalling / isGrounded -

    private void SetIsGrounded()
    {
        isGrounded = Physics.CheckSphere(feetTransform.position, playerSettings.isGroundedRadius, groundMask);
    }

    private void SetIsFalling()
    {
        isFalling = (!isGrounded && characterController.velocity.magnitude >= playerSettings.isFallingSpeed);
    }

    #endregion

    #region - View / Movement -

    private void CalculateView()
    {
        newPlayerRotation.y += (isAimingIn ? playerSettings.viewXSensitivity * playerSettings.aimingSensitivityEffector : playerSettings.viewXSensitivity) * (playerSettings.viewXInverted ? -inputView.x : inputView.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newPlayerRotation);

        newCameraRotation.x += (isAimingIn ? playerSettings.viewYSensitivity * playerSettings.aimingSensitivityEffector : playerSettings.viewXSensitivity) * (playerSettings.viewYInverted ? inputView.y : -inputView.y) * Time.deltaTime;
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
        if (!isGrounded)
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
        else if (isAimingIn)
        {
            playerSettings.speedEffector = playerSettings.aimingSpeedEffector;
        }
        else
        {
            playerSettings.speedEffector = 1;
        }

        weaponAnimationSpeed = characterController.velocity.magnitude / (playerSettings.walkingForwardSpeed * playerSettings.speedEffector);

        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
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

        if (playerGravity < -0.1f && isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    #endregion

    #region - Leaning -

    private void CalculateLeaning()
    {
        if (isLeaningLeft)
        {
            targetLean = leanAngle;
        }
        else if (isLeaningRight)
        {
            targetLean = -leanAngle;
        }
        else
        {
            targetLean = 0;
        }

        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);

        leanPivot.localRotation = Quaternion.Euler(new Vector3(0, 0, currentLean));
    }

    #endregion

    #region - Jumping -

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.jumpingFalloff);
    }

    private void Jump()
    {
        if (!isGrounded || playerStance == PlayerStance.Prone)
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
        currentWeapon.TriggerJump();
    }

    #endregion

    #region - Stance -

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

    #endregion

    #region - Sprinting -

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

    #endregion

    #region - Reload -

    public void ReloadAmmo()
    {
        if (currentWeapon.currentFireType == WeaponFireType.GrenadeLauncher)
        {
            int reAmmo = grenadeAmmo > currentWeapon.grenadeMaxAmmo ? grenadeAmmo : currentWeapon.grenadeMaxAmmo;
            currentWeapon.grenadeCurrentAmmo = reAmmo;
            grenadeAmmo -= reAmmo;
            isReload = false;
        }

        else
        {
            int reAmmo = ammo > currentWeapon.maxAmmo ? ammo : currentWeapon.maxAmmo;
            currentWeapon.currentAmmo = reAmmo;
            ammo -= reAmmo;
            isReload = false;
        }
    }

    #endregion

    #region - Weapon Swap -

    public void SwitchWeapons(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }
        
        weaponIndicator = index;
        currentWeapon = weapons[index].GetComponent<WeaponController>();
        weapons[index].SetActive(true);
    }

    #endregion

    #region - Gizmos - 

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransform.position, playerSettings.isGroundedRadius);
    }

    #endregion
}
