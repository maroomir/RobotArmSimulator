using System;
using UnityEngine;

public enum GripperStatus
{
    Fixed = -1,
    Open = 0,
    Closed = 1,
}

public class FingerEventArgs : EventArgs
{
    public GripperStatus Status { get; set; }
    public float CurrentPosition { get; set; }
    public FingerEventArgs(float fSpeed, float fPosition)
    {
        Status = (fSpeed == 0) ? GripperStatus.Fixed : ((fSpeed < 0) ? GripperStatus.Open : GripperStatus.Closed);
        CurrentPosition = fPosition;
    }
}

public class FingerController : MonoBehaviour
{
    public delegate void FingerMoveCallback(object sender, FingerEventArgs e);

    public float stroke;
    
    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition => Mathf.InverseLerp(_pInitPoint.z, stroke, transform.localPosition.z);
    public int MaxFrame { get; set; }

    public event FingerMoveCallback OnFingerMoveEvent;
    public event FingerMoveCallback OnFingerStopEvent;

    private ArticulationBody _pArticulation;
    private Vector3 _pInitPoint;
    private int _nCurrFrame = 0;
    private float _fSpeed;
    private float _fStartPos;
    private float _fTargetPos;

    // Start is called before the first frame update
    private void Start()
    {
        _pInitPoint = transform.localPosition;
        _pArticulation = GetComponent<ArticulationBody>();
        SetLimit();
    }

    private void SetLimit()
    {
        float fOpenPos = ToDrive((float)GripperStatus.Open);
        float fClosedPos = ToDrive((float)GripperStatus.Closed);
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.lowerLimit = Mathf.Min(fOpenPos, fClosedPos);
        pDrive.upperLimit = Mathf.Max(fOpenPos, fClosedPos);
        _pArticulation.zDrive = pDrive;
    }

    public void UpdateParameter(GripperStatus eStatus)
    {
        _nCurrFrame = 0;
        _fStartPos = CurrentPosition;
        switch (eStatus)
        {
            case GripperStatus.Open:
                _fSpeed = -1.0F/MaxFrame;
                break;
            case GripperStatus.Closed:
                _fSpeed = 1.0F/MaxFrame;
                break;
        }
    }

    private float ToDrive(float fRatio)
    {
        float fCurrentPos = Mathf.Lerp(_pInitPoint.z, stroke, fRatio);
        return (fCurrentPos - _pInitPoint.z) * transform.parent.localScale.z;
    }

    // Update is called on fixed freqency
    private void FixedUpdate()
    {
        if (_nCurrFrame == MaxFrame)
        {
            OnFingerStopEvent?.Invoke(this, new FingerEventArgs(_fSpeed, CurrentPosition));
            return;
        }

        float fTargetPos = _fStartPos + ToDrive(_nCurrFrame * _fSpeed);
        OnFingerMoveEvent?.Invoke(this, new FingerEventArgs(_fSpeed, CurrentPosition));
        ArticulationDrive pDrive = _pArticulation.xDrive;
        pDrive.target = fTargetPos;
        _pArticulation.xDrive = pDrive;
        _nCurrFrame += 1;
    }
}