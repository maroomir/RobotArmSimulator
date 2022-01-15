using System;
using UnityEngine;

public interface IMathematicalSpeedControl
{
    public int MaxStep { get; }
    public float MaxSpeed { get; set; }
    
    public void SetPosition(float fStartPos, float fTargetPos);
    public float GetSpeed(int iX);
    public float GetPosition(int iX);
}

public class NormalControl : IMathematicalSpeedControl
{
    public int MaxStep => (int) (_fPositionDelta / RealMaxSpeed());

    public float MaxSpeed { get; set; } = 100.0F;

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMoveDir = 1;

    public NormalControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    private float RealMaxSpeed()
    {
        return _nMoveDir * MaxSpeed * _fDeltaFrame;
    }

    public float GetPosition(int iX)
    {
        if (iX > MaxStep) iX = MaxStep;
        return _fStartPosition + GetSpeed(iX) * iX;
    }

    public void SetPosition(float fStartPos, float fTargetPos)
    {
        _fStartPosition = fStartPos;
        _fPositionDelta = fTargetPos - fStartPos;
        _nMoveDir = (_fPositionDelta >= 0.0F) ? 1 : -1;
    }

    public float GetSpeed(int iX)
    {
        return RealMaxSpeed();
    }
}

public class TrapezoidControl : IMathematicalSpeedControl
{
    public int MaxStep =>
        (int) (_fPositionDelta / RealMaxSpeed() / (0.5F + 0.5F * _fRegularRatio));

    public float MaxSpeed { get; set; } = 100.0F;

    private readonly float _fDeltaFrame;
    private readonly float _fRegularRatio;
    private float _fStartPostion = 0.0F;
    private float _fTargetPostion = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMoveDir = 1;

    public TrapezoidControl(float fDelta, float fRegularRatio = 0.5F)
    {
        _fDeltaFrame = fDelta;
        _fRegularRatio = fRegularRatio;
    }

    private float RealMaxSpeed()
    {
        return _nMoveDir * MaxSpeed * _fDeltaFrame;
    }

    public float GetPosition(int iX)
    {
        if (iX > MaxStep) iX = MaxStep - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * MaxStep / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fStartPostion + GetSpeed(iX) * iX * 0.5F;
        if (nSlantedCount <= iX && iX < MaxStep - nSlantedCount)
            return _fStartPostion + RealMaxSpeed() * (iX - 0.5F * nSlantedCount);
        if (MaxStep - nSlantedCount <= iX && iX <= MaxStep)
            return _fStartPostion
                   + RealMaxSpeed() * (MaxStep - 1.5F * nSlantedCount)
                   + (RealMaxSpeed() + GetSpeed(iX)) * (iX - MaxStep + nSlantedCount) * 0.5F;
        return _fTargetPostion;
    }

    public void SetPosition(float fStartPos, float fTargetPos)
    {
        _fStartPostion = fStartPos;
        _fTargetPostion = fTargetPos;
        _fPositionDelta = fTargetPos - fStartPos;
        _nMoveDir = (_fPositionDelta >= 0.0F) ? 1 : -1;
    }

    public float GetSpeed(int iX)
    {
        if (iX > MaxStep) iX = MaxStep - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * MaxStep / 2);
        if (0 <= iX && iX < nSlantedCount)
            return RealMaxSpeed() / nSlantedCount * iX;
        if (nSlantedCount <= iX && iX < MaxStep - nSlantedCount)
            return RealMaxSpeed();
        if (MaxStep - nSlantedCount <= iX && iX <= MaxStep)
            return -RealMaxSpeed() / nSlantedCount * (iX - MaxStep);
        return 0.0F;
    }
}

public class HemisphereControl : IMathematicalSpeedControl
{
    public int MaxStep => (int)(_fPositionDelta / RealMaxSpeed() * 4.0F / Mathf.PI);
    public float MaxSpeed { get; set; } = 100.0F;

    private readonly float _fDeltaFrame;
    private float _fStartPostion = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMoveDir = 1;

    public HemisphereControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    private float RealMaxSpeed()
    {
        return _nMoveDir * MaxSpeed * _fDeltaFrame;
    }
    
    public float GetPosition(int iX)
    {
        if (iX > MaxStep) iX = MaxStep - 1;
        float fArea = 0.0F;
        for (int i = 0; i < iX; i++)
            fArea += GetSpeed(iX);
        return _fStartPostion + fArea;
    }
    
    public void SetPosition(float fStartPos, float fTargetPos)
    {
        _fStartPostion = fStartPos;
        _fPositionDelta = fTargetPos - fStartPos;
        _nMoveDir = (_fPositionDelta >= 0.0F) ? 1 : -1;
    }

    public float GetSpeed(int iX)
    {
        if (iX > MaxStep) iX = MaxStep - 1;
        float fX0 = MaxStep / 2.0F;
        float fA2 = MaxStep * MaxStep / 4.0F;
        float fB2 = RealMaxSpeed() * RealMaxSpeed();
        return _nMoveDir * Mathf.Sqrt(fB2 - fB2 / fA2 * (iX - fX0) * (iX - fX0));
    }
}

public class TriangleControl : IMathematicalSpeedControl
{
    public int MaxStep => (int) (_fPositionDelta * 2.0F / RealMaxSpeed());
    public float MaxSpeed { get; set; } = 100.0F;

    private readonly float _fDeltaFrame;
    private float _fStartPostion = 0.0F;
    private float _fTargetPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMoveDir = 1;

    public TriangleControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    private float RealMaxSpeed()
    {
        return _nMoveDir * MaxSpeed * _fDeltaFrame;
    }

    public float GetPosition(int iX)
    {
        if (iX > MaxStep) iX = MaxStep - 1;
        if (0 <= iX && iX < MaxStep / 2)
            return _fStartPostion + GetSpeed(iX) * iX * 0.5F;
        if (MaxStep / 2 <= iX && iX <= MaxStep)
            return _fStartPostion
                   + 0.25F * RealMaxSpeed() * MaxStep
                   + 0.5F * (GetSpeed(iX) + RealMaxSpeed()) * (iX - 0.5F * MaxStep);
        return _fTargetPosition;
    }

    public void SetPosition(float fStartPos, float fTargetPos)
    {
        _fStartPostion = fStartPos;
        _fTargetPosition = fTargetPos;
        _fPositionDelta = fTargetPos - fStartPos;
        _nMoveDir = (_fPositionDelta >= 0.0F) ? 1 : -1;
    }

    public float GetSpeed(int iX)
    {
        if (0 <= iX && iX < MaxStep / 2)
            return 2 * RealMaxSpeed() * iX / MaxStep;
        if (MaxStep / 2 <= iX && iX <= MaxStep)
            return 2 * RealMaxSpeed() - 2 * RealMaxSpeed() * iX / MaxStep;
        return 0.0F;
    }
}