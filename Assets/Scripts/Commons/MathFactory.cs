using UnityEngine;

public class KinematicsCalculator
{
    public class DHParameter
    {
        // Distance of the Z-axis based on the X-axis
        public float a = 0;
        // Rotation angle on the Z-axis based on the X-axis
        public float alpha = 0;
        // Distance of the X-axis based on the Z-axis
        public float d = 0;
        // Rotation angle on the X-axis based on the Z-axis
        public float theta = 0;

        private float SpanAngle(float fAngle)
        {
            return Mathf.Round(fAngle / 90.0F) * 90.0F;
        }

        public DHParameter(float fA, float fAlpha, float fD, float fTheta)
        {
            a = fA;
            alpha = SpanAngle(fAlpha);
            d = fD;
            theta = SpanAngle(fTheta);
        }

        public DHParameter(GameObject pPrevAxis, GameObject pCurrAxis)
        {
            Vector3 pDistance = pCurrAxis.transform.position - pPrevAxis.transform.position;
            Vector3 pRotation = pCurrAxis.transform.eulerAngles - pPrevAxis.transform.eulerAngles;
            a = pDistance.z;
            alpha = SpanAngle(pRotation.z);
            d = pDistance.x;
            theta = SpanAngle(pRotation.x);
        }

        public Vector3 RotationAxis => new Vector3(theta / 90.0F, 0.0F, alpha / 90.0F);
        public Vector3 Diff => new Vector3(d, 0.0F, a);
    }

    public DHParameter[] Parameters { get; private set; }
    public Vector3 BasePosition { get; set; }
    public int Length => Parameters.Length;

    private float _fSampleDistance = 0.0F;
    private float _fThreshold = 0.0F;
    private float _fLearningRate = 0.0F;

    public KinematicsCalculator(GameObject[] pJoints, float fSampleDistance = 0.1F, float fThreshold = 0.1F, float fLearningRate = 1.0F)
        : this(fSampleDistance, fThreshold, fLearningRate)
    {
        int nLength = pJoints.Length - 1;
        BasePosition = pJoints[0].transform.position;
        Parameters = new DHParameter[nLength];
        for (int i = 0; i < nLength; i++)
        {
            Parameters[i] = new DHParameter(pJoints[i], pJoints[i + 1]);
        }
    }

    public KinematicsCalculator(DHParameter[] pParams, float fSampleDistance = 0.1F, float fThreshold = 0.1F, float fLearningRate = 1.0F)
        : this(fSampleDistance, fThreshold, fLearningRate)
    {
        Parameters = pParams;
    }

    public KinematicsCalculator(float fSampleDistance = 0.1F, float fThreshold = 0.1F, float fLearningRate = 1.0F)
    {
        _fSampleDistance = fSampleDistance;
        _fThreshold = fThreshold;
        _fLearningRate = fLearningRate;
    }

    public Vector3 ForwardKinematics(float[] pAngles)
    {
        if (Length != pAngles.Length)
            throw new MissingReferenceException("Invalid counter of DH Parameters");
        Vector3 pResultPos = BasePosition;
        Quaternion pRotation = Quaternion.identity;
        for (int i = 1; i < Length; i++)
        {
            pRotation *= Quaternion.AngleAxis(pAngles[i - 1], Parameters[i - 1].RotationAxis);
            Vector3 pNextPos = pResultPos + pRotation * Parameters[i].Diff;
            pResultPos = pNextPos;
        }
        return pResultPos;
    }

    public float[] InverseKinematics(Vector3 pInput)
    {
        return InverseKinematics(pInput, new float[Length]);
    }

    public float[] InverseKinematics(Vector3 pInput, float[] pPrevAngles)
    {
        if (Length != pPrevAngles.Length)
            throw new MissingReferenceException("Invalid counter of DH Parameters");
        float[] pResults = pPrevAngles;
        while (GetDistance(pInput, pResults) > _fThreshold)
        {
            for (int i = 0; i < Length; i++)
            {
                float fGradient = GetGradient(pInput, i, pResults);
                pResults[i] -= _fLearningRate * fGradient;
            }
        }
        return pResults;
    }

    private float GetDistance(Vector3 pTargetPos, float[] pAngles)
    {
        Vector3 pConvertPos = ForwardKinematics(pAngles);
        return Vector3.Distance(pTargetPos, pConvertPos);
    }

    private float GetGradient(Vector3 pTargetPos, int iIndex, float[] pAngles)
    {
        float fPrevDist = GetDistance(pTargetPos, pAngles);
        pAngles[iIndex] += _fSampleDistance;
        float fNextDist = GetDistance(pTargetPos, pAngles);
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
