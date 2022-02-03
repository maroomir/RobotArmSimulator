using System;
using UnityEngine;

public class KinematicsCalculator
{
    public class DHParameter
    {
        // Distance of the Y(Z)-axis based on the Z(X)-axis
        public float A => _fDistanceY;
        // Rotation angle on the Y(Z)-axis based on the Z(X)-axis
        public float Alpha => _fAnchorThetaZ;
        // Distance of the Z(X)-axis based on the Y(Z)-axis
        public float D => _fDistanceZ;
        // Rotation angle on the Z(X)-axis based on the Y(Z)-axis
        public float Theta => -_fAnchorThetaY;
        
        public Vector3 AnchorAxis => new Vector3(0.0F, _fAnchorThetaY / 90.0F, _fAnchorThetaZ / 90.0F);

        public Vector3 RotationAxis => new Vector3(0.0F, _fAnchorThetaZ / 90.0F, -_fAnchorThetaY / 90.0F);
        
        public Vector3 Diff => new Vector3(0.0F, _fDistanceY, _fDistanceZ);
        
        // The variable needed to use the DH Parameter in Unity Environment
        private float _fDistanceY = 0;
        private float _fDistanceZ = 0;
        private float _fAnchorThetaY = 0;
        private float _fAnchorThetaZ = 0;
        
        public DHParameter(float fA, float fAlpha, float fD, float fTheta)
        {
            // As a result of handwriting for proving the DH-parameters
            _fDistanceY = fA;
            _fAnchorThetaY = -NormalizeAngle(fTheta);
            _fDistanceZ = fD;
            _fAnchorThetaZ = NormalizeAngle(fAlpha);
        }

        public DHParameter(GameObject pPrevAxis, GameObject pCurrAxis)
        {
            ArticulationBody pCurrBody = pCurrAxis.GetComponent<ArticulationBody>();
            Vector3 pDistance = pCurrAxis.transform.position - pPrevAxis.transform.position;
            Vector3 pRotation = pCurrBody.anchorRotation.eulerAngles;
            _fDistanceY = pDistance.y;
            _fAnchorThetaY = NormalizeAngle(pRotation.y);
            _fDistanceZ = pDistance.z;
            _fAnchorThetaZ = NormalizeAngle(pRotation.z);
        }
        
        private float NormalizeAngle(float fAngle)
        {
            // Normalize the angle in the circle
            fAngle %= 360.0F;
            // Fit the angle in to the half-range
            fAngle = Mathf.Round(fAngle / 90.0F) * 90.0F;
            fAngle = fAngle < 0 ? 360.0F + fAngle : fAngle;
            fAngle = fAngle >= 180.0F ? 180.0F - fAngle : fAngle;
            return fAngle;
        }
    }

    public DHParameter[] Parameters { get; private set; }
    public Vector3 BasePosition { get; set; }
    public int Length => Parameters.Length;

    private readonly float _fSampleDistance;
    private readonly float _fThreshold;
    private readonly int _nMaxIterCount = 1000;

    public KinematicsCalculator(GameObject[] pJoints, float fSampleDistance = 0.1F, float fThreshold = 0.001F)
        : this(fSampleDistance, fThreshold)
    {
        int nLength = pJoints.Length - 1;
        BasePosition = pJoints[0].transform.position;
        Parameters = new DHParameter[nLength];
        for (int i = 0; i < nLength; i++)
        {
            Parameters[i] = new DHParameter(pJoints[i], pJoints[i + 1]);
        }
    }

    public KinematicsCalculator(DHParameter[] pParams, float fSampleDistance = 0.1F, float fThreshold = 0.001F)
        : this(fSampleDistance, fThreshold)
    {
        Parameters = pParams;
    }

    public KinematicsCalculator(float fSampleDistance = 0.1F, float fThreshold = 0.001F)
    {
        _fSampleDistance = fSampleDistance;
        _fThreshold = fThreshold;
    }

    public Vector3 ForwardKinematics(float[] pAngles, bool bTrace = false)
    {
        if (Length != pAngles.Length)
            throw new MissingReferenceException(
                $"Invalid counter of DH Parameters, Cal={Length}/Input={pAngles.Length}");
        // Add the robot base height when initialized positions 
        Vector3 pResultPos = BasePosition + Parameters[0].Diff;
        Quaternion pRotation = Quaternion.identity;
        if (bTrace) Debug.Log($"[MATH][FK] i=0, Pos=({pResultPos.x:F4}, {pResultPos.y:F4}, {pResultPos.z:F4})");
        for (int i = 1; i < Length; i++)
        {
            pRotation *= Quaternion.AngleAxis(pAngles[i - 1], Parameters[i - 1].RotationAxis);
            Vector3 pNextPos = pResultPos + pRotation * Parameters[i].Diff;
            pResultPos = pNextPos;
            if (bTrace)
                Debug.Log($"[MATH][FK] i={i}, " +
                          $"Rot=({Parameters[i - 1].RotationAxis.x:F1}, {Parameters[i - 1].RotationAxis.y:F1},{Parameters[i - 1].RotationAxis.z:F1}), " +
                          $"Diff=({Parameters[i - 1].Diff.x:F6}, {Parameters[i - 1].Diff.y:F6}, {Parameters[i - 1].Diff.z:F6}), " +
                          $"Pos=({pResultPos.x:F6}, {pResultPos.y:F6}, {pResultPos.z:F6})");
        }

        return pResultPos;
    }

    public float[] InverseKinematics(Vector3 pInput, float[] pPrevAngles)
    {
        if (Length != pPrevAngles.Length)
            throw new MissingReferenceException("Invalid counter of DH Parameters");
        float[] pResults = pPrevAngles.Clone() as float[];
        for (int i = 0; i < _nMaxIterCount; i++)
        {
            for (int j = 0; j < Length; j++)
                pResults[j] -= (float)PartialGradient(pInput, j, pResults);

            if (PartialDistance(pInput, pResults) <= _fThreshold)
                break;
            if(i == _nMaxIterCount - 1)
                Debug.LogWarning($"Failed at Inverse Kinematics (the robot cannot reach)");
        }

        return pResults;
    }

    private double PartialDistance(Vector3 pTargetPos, float[] pAngles)
    {
        Vector3 pConvertPos = ForwardKinematics(pAngles);
        return Vector3.Distance(pTargetPos, pConvertPos);
    }

    private double PartialGradient(Vector3 pTargetPos, int iIndex, float[] pAngles)
    {
        double fPrevDist = PartialDistance(pTargetPos, pAngles);
        pAngles[iIndex] += _fSampleDistance;
        double fNextDist = PartialDistance(pTargetPos, pAngles);
        return (fNextDist - fPrevDist) / _fSampleDistance;
    }
}

public static class MathFactory
{
    public static float LerpNormalization(float fStart, float fEnd, float fRatio, float fScale = 1.0F)
    {
        float fCurrent = Mathf.Lerp(fStart, fEnd, fRatio);
       return (fCurrent - fStart) * fScale;
    }
}
