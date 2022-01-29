using UnityEngine;

public class KinematicsCalculator
{
    public class DHParameter
    {
        // Distance of the Y(Z)-axis based on the Z(X)-axis
        public float a = 0;
        // Rotation angle on the Y(Z)-axis based on the Z(X)-axis
        public float alpha = 0;
        // Distance of the Z(X)-axis based on the Y(Z)-axis
        public float d = 0;
        // Rotation angle on the Z(X)-axis based on the Y(Z)-axis
        public float theta = 0;

        private float SpanAngle(float fAngle)
        {
            fAngle %= 360.0F;
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
            ArticulationBody pPrevBody = pPrevAxis.GetComponent<ArticulationBody>();
            ArticulationBody pCurrBody = pCurrAxis.GetComponent<ArticulationBody>();
            Vector3 pDistance = pCurrAxis.transform.position - pPrevAxis.transform.position;
            Vector3 pRotation = pCurrBody.anchorRotation.eulerAngles - pPrevBody.anchorRotation.eulerAngles;
            a = pDistance.y;
            alpha = SpanAngle(pRotation.y);
            d = pDistance.z;
            theta = SpanAngle(pRotation.z);
        }

        public Vector3 RotationAxis => new Vector3(0.0F, alpha / 90.0F, theta / 90.0F);
        public Vector3 Diff => new Vector3(0.0F, a, d);
    }

    public DHParameter[] Parameters { get; private set; }
    public Vector3 BasePosition { get; set; }
    public int Length => Parameters.Length;

    private readonly float _fSampleDistance;
    private readonly float _fThreshold;

    public KinematicsCalculator(GameObject[] pJoints, float fSampleDistance = 0.1F, float fThreshold = 0.1F)
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

    public KinematicsCalculator(DHParameter[] pParams, float fSampleDistance = 0.1F, float fThreshold = 0.1F)
        : this(fSampleDistance, fThreshold)
    {
        Parameters = pParams;
    }

    public KinematicsCalculator(float fSampleDistance = 0.1F, float fThreshold = 0.1F)
    {
        _fSampleDistance = fSampleDistance;
        _fThreshold = fThreshold;
    }

    public Vector3 ForwardKinematics(float[] pAngles)
    {
        if (Length != pAngles.Length)
            throw new MissingReferenceException($"Invalid counter of DH Parameters, Cal={Length}/Input={pAngles.Length}");
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
        while (true)
        {
            for (int i = 0; i < Length; i++)
                pResults[i] += (float)PartialGradient(pInput, i, pResults);
            if(PartialDistance(pInput, pResults) <= _fThreshold)
                break;
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
