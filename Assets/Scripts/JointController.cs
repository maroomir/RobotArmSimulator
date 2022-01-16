using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JointEventArgs : EventArgs
{
    public float CurrentPosition { get; set; }
    public float TargetPosition { get; set; }
    public float Speed { get; set; }

    public JointEventArgs(float fCurr, float fTarget, float fSpeed)
    {
        CurrentPosition = fCurr;
        TargetPosition = fTarget;
        Speed = fSpeed;
    }
}

public enum SpeedRule
{
    None = 0,
    Triangle,
    Trapezoid,
}

public enum SyncRule
{
    Async=0,
    FrameSync,
}

public class JointController : MonoBehaviour
{
    public delegate void JointMoveCallback(object sender, JointEventArgs e);

    public int Index { get; set; }
    public string Name { get; set; }
    public int MaxFrame { get; set; }
    public float MaxSpeed { get; set; }
    public SpeedRule SpeedMode { get; set; } = SpeedRule.Trapezoid;
    public SyncRule SyncMode { get; set; } = SyncRule.Async;

    public float CurrentPosition => (_pArticulation == null) ? 0.0F : Mathf.Rad2Deg * _pArticulation.jointPosition[0];

    public float TargetPosition { get; set; }

    private ArticulationBody _pArticulation;
    private int _nCurrFrame = 0;
    private ISpeedControl _pSpeedController;

    public event JointMoveCallback OnJointMoveEvent;
    public event JointMoveCallback OnJointStopEvent;

    // Start is called before the first frame update
    private void Start()
    {
        _pArticulation = GetComponent<ArticulationBody>();
        _pSpeedController = SpeedMode switch
        {
            SpeedRule.None => new NormalControl(Time.fixedDeltaTime),
            SpeedRule.Trapezoid => new TrapezoidControl(Time.fixedDeltaTime),
            SpeedRule.Triangle => new TriangleControl(Time.fixedDeltaTime),
            _ => _pSpeedController
        };

        UpdateParameter();
    }

    public void UpdateParameter()
    {
        _nCurrFrame = 0;
        if (_pSpeedController == null) return;
        switch (SyncMode)
        {
            case SyncRule.Async:
                _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
                _pSpeedController.MaxSpeed = MaxSpeed;
                MaxFrame = _pSpeedController.MaxFrame;
                break;
            case SyncRule.FrameSync:
                _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
                _pSpeedController.MaxFrame = MaxFrame;
                MaxSpeed = _pSpeedController.MaxSpeed;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Update is called on fixed frequency
    private void FixedUpdate()
    {
        if (_nCurrFrame == MaxFrame)
        {
            OnJointStopEvent?.Invoke(this, new JointEventArgs(CurrentPosition, CurrentPosition, 0.0F));
            return;
        }

        float fTargetPos = _pSpeedController.GetPosition(_nCurrFrame);
        OnJointMoveEvent?.Invoke(this, new JointEventArgs(CurrentPosition, fTargetPos, _pSpeedController.GetSpeed(_nCurrFrame)));
        ArticulationDrive pDrive = _pArticulation.xDrive;
        pDrive.target = fTargetPos;
        _pArticulation.xDrive = pDrive;
        _nCurrFrame += 1;
    }
}