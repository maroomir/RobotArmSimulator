using System;
using System.Collections.Generic;
using UnityEngine;

public enum OperationMode
{
    Teaching = 0,
    Auto
}

public enum SpeedRule
{
    None = 0,
    Triangle,
    Trapezoid,
}

public enum SyncRule
{
    Async = 0,
    FrameSync,
}

public enum BreakStatus
{
    Release = 0,
    Hold,
}

public interface ISpeedControl
{
    public int MaxFrame { get; set; }
    public float MaxSpeed { get; set; }

    public void SetPosition(float fStartPos, float fTargetPos);
    public float GetSpeed(int iX);
    public float GetPosition(int iX);
}

public interface ITeachingPoint
{
    public string Name { get; }
    public int AxisNum { get; }
    public float MaxSpeed { get; set; }
    public int MaxFrame { get; set; }
    public SyncRule SyncMode { get; set; }
    public float[] Values { get; }

    public string Print();
    public void Scan(string strValue);
}

public interface IMotorControl
{
    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition { get; }
    public float TargetPosition { get; }
    public float MaxSpeed { get; set; }
    public int MaxFrame { get; set; }
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

public static class CommonFunctions
{
    public static bool IsInputKeys(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                return true;
        return false;
    }

    public static KeyCode GetInputKeys(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                return eKey;
        return KeyCode.None;
    }
    
    public static float CalculatePositionByRatio(float fStart, float fEnd, float fRatio, float fScale = 1.0F)
    {
        Debug.Log($"[TEMP] CalculatePositionByRatio: Start={fStart}, End={fEnd}, Ratio={fRatio}");
        float fCurrent = Mathf.Lerp(fStart, fEnd, fRatio);
        return (fCurrent - fStart) * fScale;
    }
}