using System;
using UnityEngine;

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

public class FingerController : MonoBehaviour, IMotorControl
{
    public delegate void FingerMoveCallback(object sender, FingerEventArgs e);

    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition => Mathf.InverseLerp(_fOpenPos, _fClosedPos, transform.localPosition.z);
    public float MaxSpeed { get; set; }
    public int MaxFrame { get; set; }
    public FingerStatus Status { get; set; }

    public event FingerMoveCallback OnFingerMoveEvent;
    public event FingerMoveCallback OnFingerStopEvent;

    private ArticulationBody _pArticulation;
    private Vector3 _pInitPoint;
    private float _fStroke;
    private int _nCurrFrame;
    private float _fGripSpeed;
    private float _fStartRate;
    private float _fOpenPos;
    private float _fClosedPos;

    // Start is called before the first frame update
    private void Start()
    {
        _pInitPoint = transform.localPosition;
        _pArticulation = GetComponent<ArticulationBody>();
        Status = FingerStatus.Open;
    }

    public void SetLimit(float fOpenLimit, float fClosedLimit)
    {
        _fOpenPos = fOpenLimit;
        _fClosedPos = fClosedLimit;
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.lowerLimit = Mathf.Min(_fOpenPos, _fClosedPos);
        pDrive.upperLimit = Mathf.Max(_fOpenPos, _fClosedPos);
        _pArticulation.zDrive = pDrive;
    }

    public void UpdateParameter()
    {
        _nCurrFrame = 0;
        switch (Status)
        {
            case FingerStatus.Open:
                _fStartRate = (float) FingerStatus.Closed;
                _fGripSpeed = - 1.0F/MaxFrame;
                break;
            case FingerStatus.Closed:
                _fStartRate = (float) FingerStatus.Open;
                _fGripSpeed = 1.0F/MaxFrame;
                break;
        }
    }

    // Update is called on fixed frequency
    private void FixedUpdate()
    {
        if (_nCurrFrame == MaxFrame || Status == FingerStatus.Fixed)
        {
            OnFingerStopEvent?.Invoke(this, new FingerEventArgs(0, 0.0F, CurrentPosition));
            return;
        }

        Vector3 localScale = transform.parent.localScale;
        float fRatio = _fStartRate + _nCurrFrame * _fGripSpeed;
        float fTargetPos = CommonFunctions.CalculatePositionByRatio(_fOpenPos, _fClosedPos, fRatio, localScale.z);
        Debug.Log($"[TEMP] {Name}: Start={_fStartRate}, Frame={_nCurrFrame}, Speed={_nCurrFrame}, Curr={CurrentPosition}, Scale={localScale.z}");
        OnFingerMoveEvent?.Invoke(this, new FingerEventArgs(_nCurrFrame, _fGripSpeed, CurrentPosition));
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.target = fTargetPos;
        _pArticulation.zDrive = pDrive;
        _nCurrFrame += 1;
    }
}