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
    SpeedSync=0,
    StepSync,
}

public class JointController : MonoBehaviour
{
    public delegate void JointMoveCallback(object sender, JointEventArgs e);

    public int Index { get; set; }
    public string Name { get; set; }
    public int MaxStep { get; set; }
    public float MaxSpeed { get; set; }
    public SpeedRule SpeedMode { get; set; } = SpeedRule.Trapezoid;
    public SyncRule SyncMode { get; set; } = SyncRule.SpeedSync;

    public float CurrentPosition => (_pArticulation == null) ? 0.0F : Mathf.Rad2Deg * _pArticulation.jointPosition[0];

    public float TargetPosition { get; set; }

    private ArticulationBody _pArticulation;
    private int _nCurrStep = 0;
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
        _nCurrStep = 0;
        if (_pSpeedController == null) return;
        switch (SyncMode)
        {
            case SyncRule.SpeedSync:
                _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
                _pSpeedController.MaxSpeed = MaxSpeed;
                MaxStep = _pSpeedController.MaxStep;
                break;
            case SyncRule.StepSync:
                _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
                _pSpeedController.MaxStep = MaxStep;
                MaxSpeed = _pSpeedController.MaxSpeed;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Update is called on fixed frequency
    private void FixedUpdate()
    {
        if (_nCurrStep == MaxStep)
        {
            OnJointStopEvent?.Invoke(this, new JointEventArgs(CurrentPosition, CurrentPosition, 0.0F));
            return;
        }

        float fTargetPos = _pSpeedController.GetPosition(_nCurrStep);
        OnJointMoveEvent?.Invoke(this, new JointEventArgs(CurrentPosition, fTargetPos, _pSpeedController.GetSpeed(_nCurrStep)));
        ArticulationDrive pDrive = _pArticulation.xDrive;
        pDrive.target = fTargetPos;
        _pArticulation.xDrive = pDrive;
        _nCurrStep += 1;
    }
}