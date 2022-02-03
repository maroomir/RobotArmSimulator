using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    
    private readonly float _fScrollSensitivity = 0.01F;
    private readonly float _fRotateSensitivity = 0.5F;
    private readonly float _fMinZoom = 0.5F;
    private readonly float _fMaxZoom = 2.0F;
    private Vector3 _pTargetDistance = new Vector3(0, 1.0F, -1.5F);
    private float _fCurrZoom = 1.0F;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKey(KeyCode.C)) return;
        Vector3 pTargetPosition = target.position;
        switch (CommonFactory.GetInputKey(new[]
                    {KeyCode.LeftArrow, KeyCode.RightArrow}))
        {
            case KeyCode.LeftArrow:
                transform.RotateAround(pTargetPosition, Vector3.up, -_fRotateSensitivity);
                _pTargetDistance = transform.position - pTargetPosition;
                break;
            case KeyCode.RightArrow:
                transform.RotateAround(pTargetPosition, Vector3.up, _fRotateSensitivity);
                _pTargetDistance = transform.position - pTargetPosition;
                break;
            default:
                break;
        }

        _fCurrZoom -= _fScrollSensitivity * Input.GetAxis("Mouse ScrollWheel");
        _fCurrZoom = Mathf.Clamp(_fCurrZoom, _fMinZoom, _fMaxZoom);
    }

    // Update is called last activate
    private void LateUpdate()
    {
        transform.position = target.position + _pTargetDistance * _fCurrZoom;
        transform.LookAt(target);
    }
}
