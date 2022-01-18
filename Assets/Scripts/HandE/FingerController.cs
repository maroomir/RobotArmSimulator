using System;
using UnityEngine;

public enum FingerStatus
{
    Fixed = -1,
    Open = 0,
    Closed = 1,
}

public class FingerEventArgs : EventArgs
{
    public int Step { get; set; }
    public float Speed { get; set; }
    public FingerStatus Status { get; set; }
    public float CurrentPosition { get; set; }
    public FingerEventArgs(int nStep, float fSpeed, float fPosition)
    {
        Step = nStep;
        Speed = fSpeed;
        Status = (fSpeed == 0) ? FingerStatus.Fixed : ((fSpeed < 0) ? FingerStatus.Open : FingerStatus.Closed);
        CurrentPosition = fPosition;
    }
}

public class FingerController : MonoBehaviour
{
    public delegate void FingerMoveCallback(object sender, FingerEventArgs e);

    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition => Mathf.InverseLerp(_pInitPoint.z, _fStroke, transform.localPosition.z);
    public int MaxFrame { get; set; }
    public float Stroke
    {
        set
        {
            _fStroke = value;
            SetLimit();
        }
    }
    public FingerStatus Status { get; set; }

    public event FingerMoveCallback OnFingerMoveEvent;
    public event FingerMoveCallback OnFingerStopEvent;

    private ArticulationBody _pArticulation;
    private Vector3 _pInitPoint;
    private float _fStroke;
    private int _nCurrFrame;
    private float _fSpeed;
    private float _fStartPos;

    // Start is called before the first frame update
    private void Start()
    {
        _pInitPoint = transform.localPosition;
        _pArticulation = GetComponent<ArticulationBody>();
        Status = FingerStatus.Open;
        SetLimit();
    }

    private void SetLimit()
    {
        float fOpenPos = ToDrive((float)FingerStatus.Open);
        float fClosedPos = ToDrive((float)FingerStatus.Closed);
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.lowerLimit = Mathf.Min(fOpenPos, fClosedPos);
        pDrive.upperLimit = Mathf.Max(fOpenPos, fClosedPos);
        _pArticulation.zDrive = pDrive;
    }

    public void UpdateParameter()
    {
        _nCurrFrame = 0;
        switch (Status)
        {
            case FingerStatus.Open:
                _fStartPos = (float) FingerStatus.Closed;
                _fSpeed = - 1.0F/MaxFrame;
                break;
            case FingerStatus.Closed:
                _fStartPos = (float) FingerStatus.Open;
                _fSpeed = 1.0F/MaxFrame;
                break;
        }
    }

    private float ToDrive(float fRatio)
    {
        float fCurrentPos = Mathf.Lerp(_pInitPoint.z, _fStroke, fRatio);
        return (fCurrentPos - _pInitPoint.z) * transform.parent.localScale.z;
    }

    // Update is called on fixed freqency
    private void FixedUpdate()
    {
        if (_nCurrFrame == MaxFrame || Status == FingerStatus.Fixed)
        {
            OnFingerStopEvent?.Invoke(this, new FingerEventArgs(0, 0.0F, CurrentPosition));
            return;
        }
        float fTargetPos = ToDrive(_fStartPos + _nCurrFrame * _fSpeed);
        OnFingerMoveEvent?.Invoke(this, new FingerEventArgs(_nCurrFrame, _fSpeed, CurrentPosition));
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.target = fTargetPos;
        _pArticulation.zDrive = pDrive;
        _nCurrFrame += 1;
    }
}