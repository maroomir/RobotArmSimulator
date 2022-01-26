using System;
using System.Collections.Generic;
using System.IO;
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

public enum BreakStatus
{
    Release = 0,
    Hold,
}

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
    public string Name { get; }
    public int AxisNum { get; }
    public int FrameCount { get; set; }
    public float[] Values { get; }

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

public static class CommonFunctions
{
    public static bool VerifyDirectory(string strPath)
    {
        if (string.IsNullOrEmpty(strPath)) return false;
        if (!Directory.Exists(strPath))
        {
            Directory.CreateDirectory(strPath);
            if (Directory.Exists(strPath))
                return true;
        }
        else
            return true;

        return false;
    }

    public static bool VerifyFilePath(string strPath, bool bCreateFile = true)
    {
        if (string.IsNullOrEmpty(strPath)) return false;
        FileInfo fi = new FileInfo(strPath);
        if (!VerifyDirectory(fi.DirectoryName))
            return false;

        try
        {
            if (!fi.Exists)
            {
                if (!bCreateFile) return false;
                FileStream fs = fi.Create();
                fs.Close();
                return true;
            }
            else
                return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return false;
    }
    
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
}