using System;
using UnityEngine;

public class Player_AimController : MonoBehaviour
{
    private CameraManager cameraManager;
    private Player player;
    private PlayerControls controls;

    [Header("Aim Viusal - Laser")]
    [SerializeField] private LineRenderer aimLaser; // this component is on the waepon holder(child of a player)

    [Header("Aim control")]
    [SerializeField] private float preciseAimCamDistance = 6;
    [SerializeField] private float regularAimCamDistance = 7;
    [SerializeField] private float canChangeRate = 5;


    [Header("Aim Setup")]
    [SerializeField] private Transform aim;
    [SerializeField] private bool isAimingPrecisly;
    [SerializeField] private float offsetChangRate = 6;
    private float offsetY;

    [Header("Aim Layers")]
    [SerializeField] private LayerMask preciseAim;
    [SerializeField] private LayerMask regularAim;


    [Header("Camera control")]
    [SerializeField] private Transform cameraTarget;
    [Range(.5f, 1)]
    [SerializeField] private float minCameraDistance = 1.5f;
    [Range(1, 3f)]
    [SerializeField] private float maxCameraDistance = 4;
    [Range(3f, 5f)]
    [SerializeField] private float cameraSensetivity = 5f;




    private Vector2 mouseInput;
    private RaycastHit lastKnownMouseHit;

    private void Start()
    {
        cameraManager= CameraManager.instance;
        player = GetComponent<Player>();
        AssignInputEvents();
        Cursor.visible = false;
    }
    private void Update()
    {
        if (player.health.isDead)
            return;

        if (player.controlsEnabled == false)
            return;


        UpdateAimVisuals();
        UpdateAimPosition();
        UpdateCameraPosition();
    }

    public Transform GetAimCameraTarget()
    {
        cameraTarget.position = player.transform.position;
        return cameraTarget;
    }
    public void EnableAimLaer(bool enable) => aimLaser.enabled = enable;
    private void UpdateAimVisuals()
    {
        aim.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        aimLaser.enabled = player.weapon.WeaponReady();

        if (aimLaser.enabled == false)
            return;


        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();

        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);


        Transform gunPoint = player.weapon.GunPoint();
        Vector3 laserDirection = player.weapon.BulletDirection();

        float laserTipLenght = .5f;
        float gunDistance = player.weapon.CurrentWeapon().gunDistance;

        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hit, gunDistance))
        {
            endPoint = hit.point;
            laserTipLenght = 0;
        }

        aimLaser.SetPosition(0, gunPoint.position);
        aimLaser.SetPosition(1, endPoint);
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLenght);
    }
    private void UpdateAimPosition()
    {
        
        aim.position = GetMouseHitInfo().point;
        Vector3 newAimPosition = isAimingPrecisly ? aim.position : transform.position;

        aim.position = new Vector3(aim.position.x, newAimPosition.y + AdjustedOffsetY(), aim.position.z);
        
    }

    private float AdjustedOffsetY()
    {
        if(!isAimingPrecisly)
            offsetY=Mathf.Lerp(offsetY,0,Time.deltaTime*offsetChangRate*.5f);
        else
            offsetY = Mathf.Lerp(offsetY, 1, Time.deltaTime * offsetChangRate);
        return offsetY; 
    }




   
    public Transform Aim() => aim;
    public bool CanAimPrecisly() => isAimingPrecisly;
    public RaycastHit GetMouseHitInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(mouseInput);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, preciseAim))
        {
            lastKnownMouseHit = hitInfo;
            return hitInfo;
        }

        return lastKnownMouseHit;
    }

    private void EnablePresiceAim(bool enable)
    {
        isAimingPrecisly = !isAimingPrecisly;
        Cursor.visible = false;

        if (enable)
        {
            cameraManager.ChangeCameraDistance(preciseAimCamDistance,canChangeRate);
            Time.timeScale = .9f;
        }
        else
        {
            cameraManager.ChangeCameraDistance(regularAimCamDistance, canChangeRate);
            Time.timeScale = 1;
        }
    }

    #region Camera Region

    private void UpdateCameraPosition()
    {
        bool canMoveCamera = Vector3.Distance(cameraTarget.position, DesieredCameraPosition()) > 1;

        if (canMoveCamera == false)
        {
            return;
        }
        cameraTarget.position =
                    Vector3.Lerp(cameraTarget.position, DesieredCameraPosition(), cameraSensetivity * Time.deltaTime);
    }

    private Vector3 DesieredCameraPosition()
    {
        float actualMaxCameraDistance = player.movement.moveInput.y < -.5f ? minCameraDistance : maxCameraDistance;

        Vector3 desiredCameraPosition = GetMouseHitInfo().point;
        Vector3 aimDirection = (desiredCameraPosition - transform.position).normalized;

        float distanceToDesierdPosition = Vector3.Distance(transform.position, desiredCameraPosition);
        float clampedDistance = Mathf.Clamp(distanceToDesierdPosition, minCameraDistance, actualMaxCameraDistance);

        desiredCameraPosition = transform.position + aimDirection * clampedDistance;
        desiredCameraPosition.y = transform.position.y + 1;

        return desiredCameraPosition;
    }

    #endregion

    private void AssignInputEvents()
    {
        controls = player.controls;

        controls.Character.PreciseAim.performed += context => EnablePresiceAim(true);
        controls.Character.PreciseAim.canceled += context => EnablePresiceAim(false);

        controls.Character.Aim.performed += context => mouseInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => mouseInput = Vector2.zero;
    }

}
