using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Interpolation Settings")]
    [Range(1f, 20f)]
    [SerializeField] private float movementInterpolationSpeed = 10f;
    [Range(1f, 20f)]
    [SerializeField] private float horizontalRotationInterpolationSpeed = 10f;
    [Range(1f, 20f)]
    [SerializeField] private float verticalRotationInterpolationSpeed = 10f;
    [Range(1f, 20f)]
    [SerializeField] private float zoomInterpolationSpeed = 10f;
    
    [Header("Vertical Rotation Calculation Settings")]
    [Range(1f, 25f)]
    [SerializeField] private float cameraOffsetAngleCalculationDelta = 5f;
    [Range(1f, 2f)]
    [SerializeField] private float cameraMaxZoomModifier = 1.5f;
    [Range(5f, 45f)]
    [SerializeField] private float cameraBaseHorizontalAngle = 25f;
    [Range(1f, 5f)]
    [SerializeField] private float cameraAngleInterpolationStep = 1f;
    [Range(1f, 45f)]
    [SerializeField] private float cameraMaxAngleOffset = 45f;
    
    [Header("Speed Settings")]
    [Range(1f, 1000f)]
    [SerializeField] private float movingSpeed = 100f;
    [Range(1f, 1000f)]
    [SerializeField] private float rotationSpeed = 100f;
    [Range(1f, 100f)]
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Zoom Settings")]
    [Range(1f, 20f)]
    [SerializeField] private float zoomDistanceMin = 10f;
    [Range(1f, 40f)]
    [SerializeField] private float zoomDistanceMax = 20f;
    [Range(1f, 90f)]
    [SerializeField] private float zoomAngleMin = 22.5f;
    [Range(1f, 90f)]
    [SerializeField] private float zoomAngleMax = 45f;
    
    [Header("Map Settings")]
    [SerializeField] private float mapWidth = 100f;
    [SerializeField] private float mapHeight = 100f;
    [SerializeField] private float maxDistanceFromCharacter = 25f;
        
    [Header("Component References")]
    [SerializeField] private Transform horizontalAxisTransform;
    [SerializeField] private Transform verticalAxisTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform character;
    
    private float _zoom = 1f;
    private float _cameraHeight;
    private float _cameraAngleOffset;
    
    private Vector3 _targetPosition;
    private Vector3 _targetCameraPosition;
    private Quaternion _targetHorizontalRotation;
    private Quaternion _targetVerticalRotation;

    private const float SkyHeight = 1000f;
    private const float CameraRayVerticalAngleOffset = 5f;
    private const float CameraRayUpwardOriginOffset = 0.3f;
    private const float CameraRayForwardOriginOffset = 1f;
    
    public void TranslateCamera(Vector3 position)
    {
        horizontalAxisTransform.position = position;
    }
    
    private void Awake()
    {
        _targetPosition = new Vector3();
        _targetCameraPosition = new Vector3();
        _targetHorizontalRotation = new Quaternion();
        _targetVerticalRotation = new Quaternion();
    }

    private void Update()
    {
        ProcessInput();
        InterpolateTransforms();
    }

    private void FixedUpdate()
    {
        ProcessPhysics();
    }

    private void ProcessInput()
    {
        GetInputPosition();
        GetInputRotation();
        GetInputZoom();
    }

    private void GetInputPosition()
    {
        _targetPosition = horizontalAxisTransform.position +
                          horizontalAxisTransform.right * (Input.GetAxis("Horizontal") * Time.deltaTime * movingSpeed) +
                          horizontalAxisTransform.forward * (Input.GetAxis("Vertical") * Time.deltaTime * movingSpeed);
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, 0f, mapHeight);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, 0f, mapWidth);

        if (maxDistanceFromCharacter > 0f)
        {
            Vector3 position = character.position;
            Vector3 allowedPosition = _targetPosition - position;
            allowedPosition = Vector3.ClampMagnitude(allowedPosition, maxDistanceFromCharacter);
            _targetPosition = position + allowedPosition;
        }
        
        _targetPosition.y = _cameraHeight;
    }

    private void GetInputRotation()
    {
        _targetHorizontalRotation = Quaternion.Euler(0,
            _targetHorizontalRotation.eulerAngles.y + (Input.GetAxis("Rotation") * -rotationSpeed * Time.deltaTime), 0);
    }

    private void GetInputZoom()
    {
        _zoom += Input.mouseScrollDelta.y * Time.deltaTime * -zoomSpeed;
        _zoom = Mathf.Clamp(_zoom, 0f, 1f);
        _targetCameraPosition.z = Mathf.Lerp(zoomDistanceMin, zoomDistanceMax, _zoom) * -1f;
    }
        
    private void InterpolateTransforms()
    {
        horizontalAxisTransform.position = Vector3.Lerp(horizontalAxisTransform.position, _targetPosition,
            Time.deltaTime * movementInterpolationSpeed);
        
        horizontalAxisTransform.rotation = Quaternion.Lerp(horizontalAxisTransform.rotation,
            _targetHorizontalRotation, Time.deltaTime * horizontalRotationInterpolationSpeed);
        
        verticalAxisTransform.localRotation = Quaternion.Lerp(verticalAxisTransform.localRotation,
            _targetVerticalRotation, Time.deltaTime * verticalRotationInterpolationSpeed);
        
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
            _targetCameraPosition, Time.deltaTime * zoomInterpolationSpeed);
    }

    private void ProcessPhysics()
    {
        CalculateCameraHeight();
        CalculateVerticalRotation();
    }

    private void CalculateCameraHeight()
    {
        Vector3 position = horizontalAxisTransform.position;
        
        bool skyRay = Physics.Raycast(
            new Vector3(position.x, SkyHeight, position.z), Vector3.down,
            out RaycastHit hit, Mathf.Infinity);

        if (skyRay)
        {
            _cameraHeight = hit.point.y;
        }
    }

    private void CalculateVerticalRotation()
    {
        Vector3 originPosition = horizontalAxisTransform.position + (horizontalAxisTransform.forward * CameraRayForwardOriginOffset) + (horizontalAxisTransform.up * CameraRayUpwardOriginOffset);
        float targetCameraAngle = Mathf.Lerp(zoomAngleMin, zoomAngleMax, _zoom);
        float cameraDistance = Vector3.Distance(horizontalAxisTransform.position, cameraTransform.position) + CameraRayVerticalAngleOffset * (cameraMaxZoomModifier - _zoom);
        
        bool downRay = CastRayFromPointByAngle(originPosition, targetCameraAngle - cameraOffsetAngleCalculationDelta + _cameraAngleOffset - CameraRayVerticalAngleOffset, 0f, cameraDistance);
        bool downLeftRay = CastRayFromPointByAngle(originPosition, targetCameraAngle - cameraOffsetAngleCalculationDelta + _cameraAngleOffset, -cameraBaseHorizontalAngle * (cameraMaxZoomModifier - _zoom), cameraDistance);
        bool downRightRay = CastRayFromPointByAngle(originPosition, targetCameraAngle - cameraOffsetAngleCalculationDelta + _cameraAngleOffset, cameraBaseHorizontalAngle * (cameraMaxZoomModifier - _zoom), cameraDistance);
        
        bool directRay = CastRayFromPointByAngle(originPosition, targetCameraAngle + _cameraAngleOffset - CameraRayVerticalAngleOffset, 0f, cameraDistance);
        bool directLeftRay = CastRayFromPointByAngle(originPosition,  targetCameraAngle + _cameraAngleOffset, -cameraBaseHorizontalAngle * (cameraMaxZoomModifier - _zoom), cameraDistance);
        bool directRightRay = CastRayFromPointByAngle(originPosition, targetCameraAngle + _cameraAngleOffset, cameraBaseHorizontalAngle * (cameraMaxZoomModifier - _zoom), cameraDistance);
        
        bool downRays = downRay || downLeftRay || downRightRay;
        bool directRays = directRay || directLeftRay || directRightRay;

        if (downRays && !directRays)
        {
            SetTargetVerticalRotation(targetCameraAngle);
            return;
        }

        if (!downRays && !directRays)
        {
            _cameraAngleOffset -= cameraAngleInterpolationStep;
            SetTargetVerticalRotation(targetCameraAngle);
            return;
        }

        if (!downRays)
        {
            return;
        }
        
        _cameraAngleOffset += cameraAngleInterpolationStep;
        SetTargetVerticalRotation(targetCameraAngle);
    }

    private void SetTargetVerticalRotation(float targetCameraAngle)
    {
        _cameraAngleOffset = Mathf.Clamp(_cameraAngleOffset, 0, cameraMaxAngleOffset);
        _targetVerticalRotation = Quaternion.Euler(targetCameraAngle + _cameraAngleOffset, 0, 0);
    }

    private bool CastRayFromPointByAngle(Vector3 fromPosition, float verticalAngle, float horizontalAngle, float distance)
    {
        Vector3 directionWithOffset = Quaternion.AngleAxis(verticalAngle, horizontalAxisTransform.right) *
                                        Quaternion.AngleAxis(horizontalAngle, horizontalAxisTransform.up) *
                                           (horizontalAxisTransform.forward * -1);
        
        return Physics.Raycast(fromPosition, directionWithOffset, distance);
    }
}