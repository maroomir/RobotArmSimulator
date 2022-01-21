using System;
using UnityEngine;

public class NormalControl : ISpeedControl
{
    public int MaxFrame
    {
        get => _nMaxFrame;
        set
        {
            if (_fPositionDelta == 0)
            {
                Debug.LogWarning("The speed cannot set-up because the position is empty");                
            }
            _nMaxFrame = value;
            _fMaxSpeed = _fPositionDelta / _nMaxFrame;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed/ _fDeltaFrame;
        set
        {
            if (_fPositionDelta == 0)
            {
                Debug.LogWarning("The step cannot set-up because the position is empty");                
            }
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nMaxFrame = (int) (_nMoveDir * _fPositionDelta / _fMaxSpeed);
        }
    }

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMaxFrame = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public NormalControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nMaxFrame) iX = _nMaxFrame;
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
    public int MaxFrame
    {
        get => _nMaxFrame;
        set
        {
            if (_fPositionDelta == 0)
            {
                Debug.LogWarning("The speed cannot set-up because the position is empty");                
            }
            _nMaxFrame = value;
            _fMaxSpeed = 2 * _fPositionDelta / (_fRegularRatio + 1) / _nMaxFrame;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed / _fDeltaFrame;
        set
        {
            if (_fPositionDelta == 0)
            {
                Debug.LogWarning("The step cannot set-up because the position is empty");                
            }
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nMaxFrame = (int) (2 * _fPositionDelta / (_fRegularRatio + 1) / _fMaxSpeed);
        }

    }

    private readonly float _fDeltaFrame;
    private readonly float _fRegularRatio;
    private float _fStartPostion = 0.0F;
    private float _fTargetPostion = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMaxFrame = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public TrapezoidControl(float fDelta, float fRegularRatio = 0.5F)
    {
        _fDeltaFrame = fDelta;
        _fRegularRatio = fRegularRatio;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nMaxFrame) iX = _nMaxFrame - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * _nMaxFrame / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fStartPostion + GetSpeed(iX) * iX * 0.5F;
        if (nSlantedCount <= iX && iX < _nMaxFrame - nSlantedCount)
            return _fStartPostion + _fMaxSpeed * (iX - 0.5F * nSlantedCount);
        if (_nMaxFrame - nSlantedCount <= iX && iX <= _nMaxFrame)
            return _fStartPostion
                   + _fMaxSpeed * (_nMaxFrame - 1.5F * nSlantedCount)
                   + (_fMaxSpeed + GetSpeed(iX)) * (iX - _nMaxFrame + nSlantedCount) * 0.5F;
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
        if (iX > _nMaxFrame) iX = _nMaxFrame - 1;
        int nSlantedCount = (int) ((1 - _fRegularRatio) * _nMaxFrame / 2);
        if (0 <= iX && iX < nSlantedCount)
            return _fMaxSpeed / nSlantedCount * iX;
        if (nSlantedCount <= iX && iX < _nMaxFrame - nSlantedCount)
            return _fMaxSpeed;
        if (_nMaxFrame - nSlantedCount <= iX && iX <= _nMaxFrame)
            return -_fMaxSpeed / nSlantedCount * (iX - _nMaxFrame);
        return 0.0F;
    }
}

public class TriangleControl : ISpeedControl
{
    public int MaxFrame
    {
        get => _nMaxFrame;
        set
        {
            if (_fPositionDelta == 0)
            {
                Debug.LogWarning("The speed cannot set-up because the position is empty");                
            }
            _nMaxFrame = value;
            _fMaxSpeed = 2 * _fPositionDelta / _nMaxFrame;
        }
    }

    public float MaxSpeed
    {
        get => _nMoveDir * _fMaxSpeed / _fDeltaFrame;
        set
        {
            if (_fPositionDelta == 0)
            {
                Debug.LogWarning("The step cannot set-up because the position is empty");                
            }
            _fMaxSpeed = _nMoveDir * value * _fDeltaFrame;
            _nMaxFrame = (int) (2 * _fPositionDelta / _fMaxSpeed);
        }
    }

    private readonly float _fDeltaFrame;
    private float _fStartPosition = 0.0F;
    private float _fTargetPosition = 0.0F;
    private float _fPositionDelta = 0.0F;
    private int _nMaxFrame = 20;
    private float _fMaxSpeed = 2.0F;
    private int _nMoveDir = 1;

    public TriangleControl(float fDelta)
    {
        _fDeltaFrame = fDelta;
    }

    public float GetPosition(int iX)
    {
        if (iX > _nMaxFrame) iX = _nMaxFrame - 1;
        if (0 <= iX && iX < _nMaxFrame / 2)
            return _fStartPosition + GetSpeed(iX) * iX * 0.5F;
        if (_nMaxFrame / 2 <= iX && iX <= _nMaxFrame)
            return _fStartPosition
                   + 0.25F * _fMaxSpeed * _nMaxFrame
                   + 0.5F * (GetSpeed(iX) + _fMaxSpeed) * (iX - 0.5F * _nMaxFrame);
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
        if (0 <= iX && iX < _nMaxFrame / 2)
            return 2 * _fMaxSpeed * iX / _nMaxFrame;
        if (_nMaxFrame / 2 <= iX && iX <= _nMaxFrame)
            return 2 * _fMaxSpeed - 2 * _fMaxSpeed * iX / _nMaxFrame;
        return 0.0F;
    }
}