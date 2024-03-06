using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float interpolatingSpeed = 10f;
    [SerializeField] private float movingSpeed = 100f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float zoomBaseDistance = 20f;
    [SerializeField] private float zoomBaseAngle = 45f;
    [SerializeField] private float zoomModifierMin = 0.5f;
    [SerializeField] private float zoomModifierMax = 1f;
    [SerializeField] private float angleOffsetStep = 1f;
    [SerializeField] private float maxAngleOffset = 45f;
    [SerializeField] private float skyHeight = 1000f;
    [SerializeField] private float mapWidth = 100f;
    [SerializeField] private float mapHeight = 100f;
        
    [Header("Component References")]
    [SerializeField] private Transform horizontalAxisTransform;
    [SerializeField] private Transform verticalAxisTransform;
    [SerializeField] private Transform cameraTransform;
        
    private float _zoom = 1f;
    private float _cameraHeight;
        
    private Vector3 _targetPosition;
    private Vector3 _targetCameraPosition;
    private Quaternion _targetHorizontalRotation;
    private Quaternion _targetVerticalRotation;

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
        _zoom = Mathf.Clamp(_zoom, zoomModifierMin, zoomModifierMax);
        _targetCameraPosition.z = -zoomBaseDistance * _zoom;
    }
        
    private void InterpolateTransforms()
    {
        horizontalAxisTransform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * interpolatingSpeed);
        horizontalAxisTransform.rotation = Quaternion.Lerp(horizontalAxisTransform.rotation,
            _targetHorizontalRotation, Time.deltaTime * interpolatingSpeed);
        verticalAxisTransform.localRotation = Quaternion.Lerp(verticalAxisTransform.localRotation,
            _targetVerticalRotation, Time.deltaTime * interpolatingSpeed);
        cameraTransform.localPosition =
            Vector3.Lerp(cameraTransform.localPosition, _targetCameraPosition, Time.deltaTime * interpolatingSpeed);
    }

    private void ProcessPhysics()
    {
        CalculateCameraHeight();
        CalculateVerticalRotation();
    }

    private void CalculateCameraHeight()
    {
        bool skyRay = Physics.Raycast(
            new Vector3(horizontalAxisTransform.position.x, skyHeight, horizontalAxisTransform.position.z), Vector3.down,
            out RaycastHit hit, Mathf.Infinity);

        if (skyRay)
        {
            _cameraHeight = hit.point.y;
        }
    }

    private void CalculateVerticalRotation()
    {
        float targetCameraAngle = zoomBaseAngle * _zoom;
        float angleOffset = 0f;

        while (angleOffset < maxAngleOffset)
        {
            Vector3 directionWithAngle = Quaternion.AngleAxis(targetCameraAngle + angleOffset, transform.right) *
                                         (transform.forward * -1);
            bool cameraRay = Physics.Raycast(horizontalAxisTransform.position, directionWithAngle, Mathf.Infinity);

            if (!cameraRay)
            {
                break;
            }

            angleOffset += angleOffsetStep;
        }

        _targetVerticalRotation = Quaternion.Euler(targetCameraAngle + angleOffset, 0, 0);
    }
}