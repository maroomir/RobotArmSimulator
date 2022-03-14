using System;
using UnityEngine;

public class FingerController : MonoBehaviour, IMotorControl
{
    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition => transform.localPosition.z;
    public int FrameCount { get; set; }
    public float Stroke => (_fClosedPos - _fOpenPos) * transform.parent.localScale.z;
    public float TargetPosition { get; private set; }
    public SpeedRule SpeedMode { get; set; }
    public BreakStatus Break { get; set; }
    public FingerStatus Status { get; private set; }
    public Vector3 GlobalPosition => transform.position;

    public event MoterMoveCallback OnMoveEvent;
    public event MoterMoveCallback OnStopEvent;

    private ArticulationBody _pArticulation;
    private int _nCurrFrame;
    private ISpeedControl _pSpeedController;
    private float _fOperatedPos = 0.0F;
    private float _fOpenPos = 0.0F;
    private float _fClosedPos = 0.0F;

    // Start is called before the first frame update
    private void Start()
    {
        FrameCount = 0;
        SpeedMode = SpeedRule.None;

        _pArticulation = GetComponent<ArticulationBody>();
        _pSpeedController = SpeedMode switch
        {
            SpeedRule.None => new NormalControl(Time.fixedDeltaTime),
            SpeedRule.Trapezoid => new TrapezoidControl(Time.fixedDeltaTime),
            SpeedRule.Triangle => new TriangleControl(Time.fixedDeltaTime),
            _ => _pSpeedController
        };
    }

    public void SetLimit(float fOpenLimit, float fClosedLimit)
    {
        _fOpenPos = fOpenLimit;
        _fClosedPos = fClosedLimit;
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.lowerLimit = Mathf.Min(Stroke, 0.0F);
        pDrive.upperLimit = Mathf.Max(Stroke, 0.0F);
        _pArticulation.zDrive = pDrive;
    }

    public void Open()
    {
        TargetPosition = _fOpenPos;
        Status = FingerStatus.Open;
        UpdateParameter();
    }

    public void Close()
    {
        TargetPosition = _fClosedPos;
        Status = FingerStatus.Close;
        UpdateParameter();
    }

    public void UpdateParameter()
    {
        _nCurrFrame = 0;
        _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
        _pSpeedController.FrameCount = FrameCount;
        Break = BreakStatus.Release;
    }

    // Update is called on fixed frequency
    private void FixedUpdate()
    {
        if (Break == BreakStatus.Hold)
        {
            _fOperatedPos = CurrentPosition;
            return;
        }

        if (_nCurrFrame >= FrameCount)
        {
            OnStopEvent?.Invoke(this, new MoterEventArgs(0, 0.0F, CurrentPosition, CurrentPosition));
            Break = BreakStatus.Hold;
            return;
        }

        _fOperatedPos = (_pSpeedController.GetPosition(_nCurrFrame) - _fOpenPos) * transform.parent.localScale.z;
        OnMoveEvent?.Invoke(this,
            new MoterEventArgs(_nCurrFrame, _pSpeedController.GetSpeed(_nCurrFrame), CurrentPosition,
                _fOperatedPos));
        ArticulationDrive pDrive = _pArticulation.zDrive;
        pDrive.target = _fOperatedPos;
        _pArticulation.zDrive = pDrive;
        _nCurrFrame += 1;
    }
}