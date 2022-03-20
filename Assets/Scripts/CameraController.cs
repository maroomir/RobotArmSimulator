using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform robot;
    public Transform gripper;
    
    private readonly float _fScrollSensitivity = 0.01F;
    private readonly float _fRotateSensitivity = 0.5F;
    private readonly float _fMinZoom = 0.25F;
    private readonly float _fMaxZoom = 4.0F;
    private readonly float _fRotationLimit = 0.1F;
    private Vector3 _pTargetDistance = new Vector3(0, 1.0F, -1.5F);
    private Transform _pTargetTransform;
    private float _fCurrZoom = 1.0F;

    // Start is called before the first frame update
    void Start()
    {
        _pTargetTransform = robot;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKey(KeyCode.C)) return;
        switch (CommonFactory.GetInputKey(new[] {KeyCode.R, KeyCode.G}))
        {
            case KeyCode.R:
                _fCurrZoom = 1.0F;
                _pTargetTransform = robot;
                break;
            case KeyCode.G:
                _fCurrZoom = 1.0F;
                _pTargetTransform = gripper;
                break;
        }

        Vector3 pTargetPosition = _pTargetTransform.position;
        // Find a vector that satisfy both the view and the robot direction 
        Vector3 pViewRayVector = transform.position - pTargetPosition;
        Vector3 pNormalVector = Vector3.Cross(pViewRayVector.normalized, Vector3.up);
        pNormalVector = CorrectRotationLimit(pNormalVector);
        switch (CommonFactory.GetInputKey(new[]
                    {KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow}))
        {
            case KeyCode.LeftArrow:
                _fCurrZoom = 1.0F;
                transform.RotateAround(pTargetPosition, Vector3.up, -_fRotateSensitivity);
                _pTargetDistance = transform.position - pTargetPosition;
                break;
            case KeyCode.RightArrow:
                _fCurrZoom = 1.0F;
                transform.RotateAround(pTargetPosition, Vector3.up, _fRotateSensitivity);
                _pTargetDistance = transform.position - pTargetPosition;
                break;
            case KeyCode.UpArrow:
                _fCurrZoom = 1.0F;
                transform.RotateAround(pTargetPosition, pNormalVector.normalized, _fRotateSensitivity);
                _pTargetDistance = transform.position - pTargetPosition;
                break;
            case KeyCode.DownArrow:
                _fCurrZoom = 1.0F;
                transform.RotateAround(pTargetPosition, pNormalVector.normalized, -_fRotateSensitivity);
                _pTargetDistance = transform.position - pTargetPosition;
                break;
        }

        _fCurrZoom -= _fScrollSensitivity * Input.GetAxis("Mouse ScrollWheel");
        _fCurrZoom = Mathf.Clamp(_fCurrZoom, _fMinZoom, _fMaxZoom);
    }

    Vector3 CorrectRotationLimit(Vector3 pVector)
    {
        if (pVector.x < 0 && pVector.x > -_fRotationLimit)
            pVector.x = -_fRotationLimit;
        if (pVector.x > 0 && pVector.x < _fRotationLimit)
            pVector.x = _fRotationLimit;
        if (pVector.z < 0 && pVector.z > -_fRotationLimit)
            pVector.z = -_fRotationLimit;
        if (pVector.z > 0 && pVector.z < _fRotationLimit)
            pVector.z = _fRotationLimit;
        return pVector;
    }

    // Update is called last activate
    private void LateUpdate()
    {
        transform.position = _pTargetTransform.position + _pTargetDistance * _fCurrZoom;
        transform.LookAt(_pTargetTransform);
    }
}
