using System;

public interface ISpeedControl
{
    public int FrameCount { get; set; }
    public float MaxSpeed { get; set; }

    public void SetPosition(float fStartPos, float fTargetPos);
    public float GetSpeed(int iX);
    public float GetPosition(int iX);
}

public interface ITeachingPoint
{
    public string Name { get; set; }
    public int AxisNum { get; }
    public int FrameCount { get; set; }
    public float[] Values { get; }

    public ITeachingPoint Clone();
    public void Trim(TrimMode eMode);
    public string Print();
}

public interface IMotorControl
{
    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition { get; }
    public float TargetPosition { get; }
    public int FrameCount { get; set; }
    public float Stroke { get; }
    public SpeedRule SpeedMode { get; set; }
    public BreakStatus Break { get; set; }
    public event MoterMoveCallback OnMoveEvent;
    public event MoterMoveCallback OnStopEvent;

    public void SetLimit(float fLimit1, float fLimit2);
    public void UpdateParameter();
}

public delegate void MoterMoveCallback(object sender, MoterEventArgs e);

public class MoterEventArgs : EventArgs
{
    public int Step { get; set; }
    public float CurrentPosition { get; set; }
    public float TargetPosition { get; set; }
    public float Speed { get; set; }

    public MoterEventArgs(int nStep, float fSpeed, float fCurr, float fTarget)
    {
        Step = nStep;
        CurrentPosition = fCurr;
        TargetPosition = fTarget;
        Speed = fSpeed;
    }
}