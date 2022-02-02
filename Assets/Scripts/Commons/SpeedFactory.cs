using System;
using UnityEngine;

public class NormalControl : ISpeedControl
{
    public int FrameCount
    {
        get => _nFrameCount;
        set
        {
            _nFrameCount = value;
            _fMaxSpeed = _fPositionDelta / _nFrameCount;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed/ _fDeltaFrame;
        set
        {
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nFrameCount = (int) (_nMoveDir * _fPositionDelta / _fMaxSpeed);
        }
    }

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nFrameCount = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public NormalControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nFrameCount) iX = _nFrameCount;
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
    public int FrameCount
    {
        get => _nFrameCount;
        set
        {
            _nFrameCount = value;
            _fMaxSpeed = 2 * _fPositionDelta / (_fRegularRatio + 1) / _nFrameCount;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed / _fDeltaFrame;
        set
        {
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nFrameCount = (int) (2 * _fPositionDelta / (_fRegularRatio + 1) / _fMaxSpeed);
        }

    }

    private readonly float _fDeltaFrame;
    private readonly float _fRegularRatio;
    private float _fStartPostion = 0.0F;
    private float _fTargetPostion = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nFrameCount = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public TrapezoidControl(float fDelta, float fRegularRatio = 0.5F)
    {
        _fDeltaFrame = fDelta;
        _fRegularRatio = fRegularRatio;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nFrameCount) iX = _nFrameCount - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * _nFrameCount / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fStartPostion + GetSpeed(iX) * iX * 0.5F;
        if (nSlantedCount <= iX && iX < _nFrameCount - nSlantedCount)
            return _fStartPostion + _fMaxSpeed * (iX - 0.5F * nSlantedCount);
        if (_nFrameCount - nSlantedCount <= iX && iX <= _nFrameCount)
            return _fStartPostion
                   + _fMaxSpeed * (_nFrameCount - 1.5F * nSlantedCount)
                   + (_fMaxSpeed + GetSpeed(iX)) * (iX - _nFrameCount + nSlantedCount) * 0.5F;
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
        if (iX > _nFrameCount) iX = _nFrameCount - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * _nFrameCount / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fMaxSpeed / nSlantedCount * iX;
        if (nSlantedCount <= iX && iX < _nFrameCount - nSlantedCount)
            return _fMaxSpeed;
        if (_nFrameCount - nSlantedCount <= iX && iX <= _nFrameCount)
            return -_fMaxSpeed / nSlantedCount * (iX - _nFrameCount);
        return 0.0F;
    }
}

public class TriangleControl : ISpeedControl
{
    public int FrameCount
    {
        get => _nFrameCount;
        set
        {
            _nFrameCount = value;
            _fMaxSpeed = 2 * _fPositionDelta / _nFrameCount;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed / _fDeltaFrame;
        set
        {
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nFrameCount = (int) (2 * _fPositionDelta / _fMaxSpeed);
        }
    }

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fTargetPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nFrameCount = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public TriangleControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nFrameCount) iX = _nFrameCount - 1;
        if (0 <= iX && iX < _nFrameCount / 2)
            return _fStartPosition + GetSpeed(iX) * iX * 0.5F;
        if (_nFrameCount / 2 <= iX && iX <= _nFrameCount)
            return _fStartPosition
                   + 0.25F * _fMaxSpeed * _nFrameCount
                   + 0.5F * (GetSpeed(iX) + _fMaxSpeed) * (iX - 0.5F * _nFrameCount);
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
        if (0 <= iX && iX < _nFrameCount / 2)
            return 2 * _fMaxSpeed * iX / _nFrameCount;
        if (_nFrameCount / 2 <= iX && iX <= _nFrameCount)
            return 2 * _fMaxSpeed - 2 * _fMaxSpeed * iX / _nFrameCount;
        return 0.0F;
    }
}