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

public class JointController : MonoBehaviour
{
    private enum SpeedRule
    {
        None = 0,
        Triangle,
        Trapezoid,
    }

    public delegate void JointMoveCallback(object sender, JointEventArgs e);

    public int Index { get; set; }
    public string Name { get; set; }
    public float MaxSpeed { get; set; }

    public float CurrentPosition => (_pArticulation == null) ? 0.0F : Mathf.Rad2Deg * _pArticulation.jointPosition[0];

    public float TargetPosition { get; set; }

    private ArticulationBody _pArticulation;
    private int _nCurrStep = 0;
    private int _nPlanStep = 0;
    private readonly float _fMinimumSpeed = 1.0F;
    private readonly SpeedRule _eSpeedControl = SpeedRule.Triangle;
    private IMathematicalSpeedControl _pSpeedController;

    public event JointMoveCallback OnJointMoveEvent;
    public event JointMoveCallback OnJointStopEvent;

    // Start is called before the first frame update
    private void Start()
    {
        _pArticulation = GetComponent<ArticulationBody>();
        _pSpeedController = _eSpeedControl switch
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
        if (MaxSpeed == 0) MaxSpeed = _fMinimumSpeed;
        _nCurrStep = 0;
        if (_pSpeedController == null) return;
        _pSpeedController.MaxSpeed = MaxSpeed;
        _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
        _nPlanStep = _pSpeedController.MaxStep;
    }

    // Update is called on fixed frequency
    private void FixedUpdate()
    {
        if (_nCurrStep == _nPlanStep)
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