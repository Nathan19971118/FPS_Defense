using System.Collections.Generic;
using System.Linq;
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
    public bool isShooting;
    public float damage;
    public float range = 100f;
    public float fireRate;
    private float nextTimeToFire = 0f;
    public List<WeaponFireType> allowedFireTypes;
    public WeaponFireType currentFireType; // Fire Mode
    public float bulletVelocity = 100f;
    public float grenadeForce = 20f;

    [Header("Ammo Control")]
    public int maxAmmo;
    public int currentAmmo;
    public int grenadeMaxAmmo;
    public int grenadeCurrentAmmo;
    

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
        if (currentAmmo > 0 && grenadeCurrentAmmo > 0)
        {
            if (isShooting && currentFireType == WeaponFireType.SemiAuto)
            {
                Shoot();
                currentAmmo--;
                isShooting = false;
            }

            if (isShooting && currentFireType == WeaponFireType.FullyAuto && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
                currentAmmo--;
            }

            if (isShooting && currentFireType == WeaponFireType.GrenadeLauncher)
            {
                ShootGrenade();
                grenadeCurrentAmmo--;
                isShooting = false;
            }
        }
        else
        {
            isShooting = false;
        }
    }

    public void CycleFireType()
    {
        int currentIndex = allowedFireTypes.IndexOf(currentFireType);

        currentIndex = (currentIndex + 1) % allowedFireTypes.Count;

        currentFireType=allowedFireTypes[currentIndex];
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, true);
            }
        }

        GameObject instantBullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletSpawn.forward * bulletVelocity;
    }

    private void ShootGrenade()
    {
        GameObject instantGrenade = Instantiate(grenadePrefab, grenadeSpawn.position, grenadeSpawn.rotation);
        Rigidbody grenadeRigid = instantGrenade.GetComponent<Rigidbody>();
        grenadeRigid.AddForce(gameObject.transform.forward * grenadeForce, ForceMode.Impulse);
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
