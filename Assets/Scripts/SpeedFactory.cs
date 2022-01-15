using System;
using UnityEngine;

public interface ISpeedControl
{
    public int MaxStep { get; set; }
    public float MaxSpeed { get; set; }
    
    public void SetPosition(float fStartPos, float fTargetPos);
    public float GetSpeed(int iX);
    public float GetPosition(int iX);
}

public class NormalControl : ISpeedControl
{
    public int MaxStep
    {
        get => _nMaxStep;
        set
        {
            _nMaxStep = value;
            _fMaxSpeed = _fPositionDelta / _nMaxStep;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed/ _fDeltaFrame;
        set
        {
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nMaxStep = (int) (_nMoveDir * _fPositionDelta / _fMaxSpeed);
        }
    }

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMaxStep = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public NormalControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nMaxStep) iX = _nMaxStep;
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
        return _fMaxSpeed;
    }
}

public class TrapezoidControl : ISpeedControl
{
    public int MaxStep
    {
        get => _nMaxStep;
        set
        {
            _nMaxStep = value;
            _fMaxSpeed = 2 * _fPositionDelta / (_fRegularRatio + 1) / _nMaxStep;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed / _fDeltaFrame;
        set
        {
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nMaxStep = (int) (2 * _fPositionDelta / (_fRegularRatio + 1) / _fMaxSpeed);
        }

    }

    private readonly float _fDeltaFrame;
    private readonly float _fRegularRatio;
    private float _fStartPostion = 0.0F;
    private float _fTargetPostion = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMaxStep = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public TrapezoidControl(float fDelta, float fRegularRatio = 0.5F)
    {
        _fDeltaFrame = fDelta;
        _fRegularRatio = fRegularRatio;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nMaxStep) iX = _nMaxStep - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * _nMaxStep / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fStartPostion + GetSpeed(iX) * iX * 0.5F;
        if (nSlantedCount <= iX && iX < _nMaxStep - nSlantedCount)
            return _fStartPostion + _fMaxSpeed * (iX - 0.5F * nSlantedCount);
        if (_nMaxStep - nSlantedCount <= iX && iX <= _nMaxStep)
            return _fStartPostion
                   + _fMaxSpeed * (_nMaxStep - 1.5F * nSlantedCount)
                   + (_fMaxSpeed + GetSpeed(iX)) * (iX - _nMaxStep + nSlantedCount) * 0.5F;
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
        if (iX > _nMaxStep) iX = _nMaxStep - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * _nMaxStep / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fMaxSpeed / nSlantedCount * iX;
        if (nSlantedCount <= iX && iX < _nMaxStep - nSlantedCount)
            return _fMaxSpeed;
        if (_nMaxStep - nSlantedCount <= iX && iX <= _nMaxStep)
            return -_fMaxSpeed / nSlantedCount * (iX - _nMaxStep);
        return 0.0F;
    }
}

public class TriangleControl : ISpeedControl
{
    public int MaxStep
    {
        get => _nMaxStep;
        set
        {
            _nMaxStep = value;
            _fMaxSpeed = 2 * _fPositionDelta / _nMaxStep;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed / _fDeltaFrame;
        set
        {
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nMaxStep = (int) (2 * _fPositionDelta / _fMaxSpeed);
        }
    }

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fTargetPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMaxStep = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public TriangleControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nMaxStep) iX = _nMaxStep - 1;
        if (0 <= iX && iX < _nMaxStep / 2)
            return _fStartPosition + GetSpeed(iX) * iX * 0.5F;
        if (_nMaxStep / 2 <= iX && iX <= _nMaxStep)
            return _fStartPosition
                   + 0.25F * _fMaxSpeed * _nMaxStep
                   + 0.5F * (GetSpeed(iX) + _fMaxSpeed) * (iX - 0.5F * _nMaxStep);
        return _fTargetPosition;
    }

    public void SetPosition(float fStartPos, float fTargetPos)
    {
        _fStartPosition = fStartPos;
        _fTargetPosition = fTargetPos;
        _fPositionDelta = fTargetPos - fStartPos;
        _nMoveDir = (_fPositionDelta >= 0.0F) ? 1 : -1;
    }

    public float GetSpeed(int iX)
    {
        if (0 <= iX && iX < _nMaxStep / 2)
            return 2 * _fMaxSpeed * iX / _nMaxStep;
        if (_nMaxStep / 2 <= iX && iX <= _nMaxStep)
            return 2 * _fMaxSpeed - 2 * _fMaxSpeed * iX / _nMaxStep;
        return 0.0F;
    }
}