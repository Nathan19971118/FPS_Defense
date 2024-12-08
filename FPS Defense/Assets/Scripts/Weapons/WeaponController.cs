using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerController playerController;

    [Header("References")]
    public Animator weaponAnimator;
    public GameObject bulletPrefab;
    public GameObject grenadePrefab;
    public Transform bulletSpawn;
    public Transform grenadeSpawn;
    public TrailRenderer trailEffect;

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

    private bool isGroundedTrigger;

    public float fallingDelay;

    [Header("Weapon Sway")]
    public Transform weaponSwayObject;
    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;

    private float swayTime;
    private Vector3 swayPosition; // Breathing offset

    [Header("Sights")]
    public Transform sightTarget;
    public float sightOffset;
    public float aimingInTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;
    [HideInInspector]
    public bool isAimingIn;

    [Header("Shooting")]
    public float rateOfFire;
    private float currentFireRate;
    public List<WeaponFireType> allowedFireTypes;
    public WeaponFireType currentFireType; // Fire Mode
    public float bulletVelocity = 10f;
    public float grenadeVelocity = 10f;
    [HideInInspector]
    public bool isShooting;

    #region - Start / Update -

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;

        currentFireType = allowedFireTypes.First();
    }

    private void Update()
    {
        if (!isInitialised)
        {
            return;
        }

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();
        CalculateShooting();
    }

    #endregion

    #region - Shooting -

    private void CalculateShooting()
    {
        if (isShooting)
        {
            StartCoroutine("Shoot");
        }
    }

    public void CycleFireType()
    {
        int currentIndex = allowedFireTypes.IndexOf(currentFireType);

        currentIndex = (currentIndex + 1) % allowedFireTypes.Count;

        currentFireType=allowedFireTypes[currentIndex];
    }

    IEnumerator Shoot()
    {
        if (currentFireType == WeaponFireType.SemiAuto)
        {
            isShooting = false;
            // Fire Bullet
            GameObject instantBullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
            Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
            bulletRigid.velocity = bulletSpawn.forward * bulletVelocity;

            StartCoroutine(FireRateHandler());
        }
        if (currentFireType == WeaponFireType.FullyAuto)
        {
            // Fire Bullet
            GameObject instantBullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
            Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
            bulletRigid.velocity = bulletSpawn.forward * bulletVelocity;

            StartCoroutine(FireRateHandler());
        }
        else if (currentFireType == WeaponFireType.GrenadeLauncher)
        {
            isShooting = false;
            // Fire Grenade
            GameObject instantGrenade = Instantiate(grenadePrefab, grenadeSpawn.position, grenadeSpawn.rotation);
            Rigidbody grenadeRigid = instantGrenade.GetComponent<Rigidbody>();
            grenadeRigid.velocity = grenadeSpawn.forward * grenadeVelocity;

            StartCoroutine(FireRateHandler());
        }

        yield return null;
    }

    IEnumerator FireRateHandler()
    {
        if (currentFireType == WeaponFireType.SemiAuto)
        {
            currentFireRate = 1 / rateOfFire;
            yield return new WaitForSeconds(currentFireRate);
            isShooting = false;
        }
        if (currentFireType == WeaponFireType.FullyAuto)
        {
            currentFireRate = 1 / rateOfFire * 5;
            yield return new WaitForSeconds(currentFireRate);
            isShooting = false;
        }
        else if (currentFireType == WeaponFireType.GrenadeLauncher)
        {
            currentFireRate = 1 / (rateOfFire / 5);
            yield return new WaitForSeconds(currentFireRate);
            isShooting = false;
        }

        yield return null;
    }

    #endregion


    #region - Initialise -

    public void Initialise(PlayerController PlayerController)
    {
        playerController = PlayerController;
        isInitialised = true;
    }

    #endregion

    #region - Aming In -

    private void CalculateAimingIn()
    {
        var targetPosition = transform.position;

        if (isAimingIn)
        {
            targetPosition = playerController._camera.transform.position + (weaponSwayObject.transform.position - sightTarget.transform.position) + (playerController._camera.transform.forward * sightOffset);
        }

        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = weaponSwayPosition + swayPosition;
    }

    #endregion

    #region - Jumping -

    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jump");
    }

    #endregion

    #region - Rotation -

    private void CalculateWeaponRotation()
    {
        //weaponAnimator.speed = playerController.weaponAnimationSpeed;

        // Rotation sway
        targetWeaponRotation.y += (isAimingIn ? settings.swayAmount / 3 : settings.swayAmount) * (settings.swayXInverted ? -playerController.inputView.x : playerController.inputView.x) * Time.deltaTime;
        targetWeaponRotation.x += (isAimingIn ? settings.swayAmount / 3 : settings.swayAmount) * (settings.swayYInverted ? playerController.inputView.y : -playerController.inputView.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);
        targetWeaponRotation.z = isAimingIn ? 0 : targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.swayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.swaySmoothing);

        // Movement sway
        targetWeaponMovementRotation.z = (isAimingIn ? settings.movementSwayX / 3 : settings.movementSwayX) * (settings.movementSwayXInverted ? -playerController.inputMovement.x : playerController.inputMovement.x);
        targetWeaponMovementRotation.x = (isAimingIn ? settings.movementSwayY / 3 : settings.movementSwayY) * (settings.movementSwayYInverted ? -playerController.inputMovement.y : playerController.inputMovement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.movementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.movementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    #endregion

    #region - Animations -

    private void SetWeaponAnimations()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (playerController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            //Debug.Log("Trigger Land");
            weaponAnimator.SetTrigger("Land");
            isGroundedTrigger = true;
        }
        else if (!playerController.isGrounded && isGroundedTrigger)
        {
            Debug.Log("Trigger Falling");
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }

        weaponAnimator.SetBool("isSprinting", playerController.isSprint);
        weaponAnimator.SetFloat("WeaponAnimationSpeed", playerController.weaponAnimationSpeed);
    }

    #endregion

    #region - Sway -

    private void CalculateWeaponSway()
    {
        // As swayScale value becomes larger, weapon sway become less
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / (isAimingIn ? swayScale * 10 : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.3f)
        {
            swayTime = 0;
        }
    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    #endregion
}
