using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerController playerController;

    [Header("Settings")]
    public WeaponSettingModel settings;

    bool isInitialised;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

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

        targetWeaponRotation.y += settings.swayAmount * (settings.swayXInverted ? -playerController.inputView.x : playerController.inputView.x) * Time.deltaTime;
        targetWeaponRotation.x += settings.swayAmount * (settings.swayYInverted ? playerController.inputView.y : -playerController.inputView.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.swayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.swaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation);
    }


}
