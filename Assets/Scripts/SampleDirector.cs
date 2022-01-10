using System;
using UnityEngine;

public class SampleDirector : MonoBehaviour
{
    public enum SpeedRule { None = 0, Triangle, Trapezoid, Hemisphere}
    
    public GameObject robot;
    public string selectedAxis;
    public int targetDegree;
    public float maxSpeed;
    
    private RobotController _pRobotControl;
    private int _nTargetStep;
    private int _nCurrentStep;
    private RotationDirection _eCurrentDir;
    private SpeedRule _eSpeedControl;
    
    // Start is called before the first frame update
    void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _nTargetStep = (int) (targetDegree / (maxSpeed * Time.fixedDeltaTime));
        _nCurrentStep = 0;
        _eCurrentDir = targetDegree == 0
            ? RotationDirection.None
            : targetDegree > 0 ? RotationDirection.Positive : RotationDirection.Negative;
        _eSpeedControl = SpeedRule.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (_eCurrentDir == RotationDirection.None ||
            _nCurrentStep == _nTargetStep)
            return;
        
        // Debug.Log($"Current={_nCurrentRevolution}, Target={targetRevolution}");
        RotateJoint(selectedAxis);
        // Debug.Log("Frame End!");
    }

    void RotateJoint(int nAxis)
    {
        if (nAxis < 0 || nAxis >= _pRobotControl.joints.Length)
            return;
        _nCurrentStep += _eCurrentDir == RotationDirection.Positive ? 1 : -1;
        // Rotate the joint according to the rules
        float fSpeed = 0.0F;
        switch (_eSpeedControl)
        {
            case SpeedRule.None:
                fSpeed = maxSpeed;
                break;
            case SpeedRule.Trapezoid:
                fSpeed = Trapezoid(_nCurrentStep, _nTargetStep, maxSpeed);
                break;
            case SpeedRule.Triangle:
                fSpeed = Triangle(_nCurrentStep, _nTargetStep, maxSpeed);
                break;
            case SpeedRule.Hemisphere:
                fSpeed = Hemisphere(_nCurrentStep, _nTargetStep, maxSpeed);
                break;
        }
        _pRobotControl.RotateJoint(nAxis, _eCurrentDir, fSpeed);
        Debug.Log($"Step={_nCurrentStep}, Degree={_nCurrentStep / (fSpeed * Time.fixedDeltaTime)}, Speed={fSpeed}");
        // Check the end frame
        if (_nCurrentStep == _nTargetStep)
            _pRobotControl.RotateJoint(nAxis, RotationDirection.None);
    }

    void RotateJoint(string strAxis)
    {
        for (int i = 0; i < _pRobotControl.joints.Length; i++)
        {
            if (selectedAxis != _pRobotControl.joints[i].inputAxis) continue;
            RotateJoint(i);
            return;
        }
    }

    static float Trapezoid(int nIndex, int nMax, float fMax, float fRegularRatio = 0.5F)
    {
        int nSlantedCount = (int) ((1 - fRegularRatio) * nMax / 2);
        if (0 <= nIndex && nIndex < nSlantedCount)
            return fMax / nSlantedCount * nIndex;
        if (nSlantedCount <= nIndex && nIndex < nMax - nSlantedCount)
            return fMax;
        if (nMax - nSlantedCount <= nIndex && nIndex < nMax)
            return -fMax / nSlantedCount * (nIndex - nMax);
        return 0.0F;
    }

    static float Hemisphere(int nIndex, int nMax, float fMax)
    {
        float fX0 = nMax / 2.0F;
        float fA2 = nMax * nMax / 4.0F;
        float fB2 = fMax * fMax;
        return Mathf.Sqrt(fB2 - fB2 / fA2 * (nIndex - fX0) * (nIndex - fX0));
    }

    static float Triangle(int nIndex, int nMax, float fMax)
    {
        if (0 <= nIndex && nIndex < nMax / 2)
            return 2 * fMax / nMax * nIndex;
        if (nMax / 2 <= nIndex && nIndex < nMax)
            return 2 * fMax - 2 * fMax / nMax * nIndex;
        return 0.0F;
    }
}
