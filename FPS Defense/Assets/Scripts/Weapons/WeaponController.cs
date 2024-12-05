using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerController playerController;

    [Header("References")]
    public Animator weaponAnimator;

    [Header("Settings")]
    public WeaponSettingModel settings;

    bool isInitialised;

    // Weapon sway by player view
    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    // Weapon sway by player movement
    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;
    }

    public void Initialise(PlayerController PlayerController)
    {
        playerController = PlayerController;
        isInitialised = true;
    }

    private void Update()
    {
        if (!isInitialised)
        {
            return;
        }

        // Rotation sway
        targetWeaponRotation.y += settings.swayAmount * (settings.swayXInverted ? -playerController.inputView.x : playerController.inputView.x) * Time.deltaTime;
        targetWeaponRotation.x += settings.swayAmount * (settings.swayYInverted ? playerController.inputView.y : -playerController.inputView.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);
        targetWeaponRotation.z = targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.swayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.swaySmoothing);

        // Movement sway
        targetWeaponMovementRotation.z = settings.movementSwayX * (settings.movementSwayXInverted ? -playerController.inputMovement.x : playerController.inputMovement.x);
        targetWeaponMovementRotation.x = settings.movementSwayY * (settings.movementSwayYInverted ? -playerController.inputMovement.y : playerController.inputMovement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.movementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.movementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }


}
