using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct JointInfo
{
    public string inputAxis;
    public GameObject robotPart;
    public KeyCode key;
}

public class RobotController : MonoBehaviour
{
    public JointInfo[] joints;

    public bool IsRobotActivate { get; private set; }

    public float[] JointAngles
    {
        
        get
        {
            float[] pPoints = new float[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                GameObject pPart = joints[i].robotPart;
                JointController pJoint = pPart.GetComponent<JointController>();
                if (pJoint == null) continue;
                pPoints[i] = pJoint.CurrentPosition;
            }

            return pPoints;
        }
    }

    public JointPoint JointPos => JointPoint.FromPosition("CurrentPos", JointAngles);

    public CartesianPoint CartesianPos => JointPos.ToCartesianPoint(CommonFactory.RobotKinematics);

    public Vector3 RealEndPos
    {
        get
        {
            int nToolIndex = joints.Length - 1;
            return joints[nToolIndex].robotPart.transform.position;
        }
    }

    public float KinematicsAccuracy
    {
        get
        {
            Vector3 pDiff = CartesianPos.Position - RealEndPos;
            return pDiff.magnitude;
        }
    }

    public OperationMode ControlMode { get; set; }

    private bool[] _pJointStatusFlags;

    // Start is called before the first frame update
    private void Start()
    {
        _pJointStatusFlags = new bool[joints.Length];
    }

    // Update is called on every frames
    private void Update()
    {
        if (ControlMode == OperationMode.Teaching)
        {
            for (int i = 0; i < joints.Length; i++)
            {
                GameObject pPart = joints[i].robotPart;
                JointController pJoint = pPart.GetComponent<JointController>();
                pJoint.ControlMode = Input.GetKey(joints[i].key) ? OperationMode.Teaching : OperationMode.Auto;
                pJoint.Break = (pJoint.ControlMode == OperationMode.Teaching) ? BreakStatus.Release : BreakStatus.Hold;
            }
        }
    }

    private void InitRobot()
    {
        IsRobotActivate = false;
        for (int i = 0; i < joints.Length; i++)
        {
            _pJointStatusFlags[i] = false;
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            pJoint.ControlMode = OperationMode.Auto;
        }
    }

    private void ExitRobot()
    {
        IsRobotActivate = false;
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            if (pJoint == null) continue;
            pJoint.OnMoveEvent -= OnJointMoveEvent;
            pJoint.OnStopEvent -= OnJointStopEvent;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnJointMoveEvent(object sender, MoterEventArgs e)
    {
        if (!IsRobotActivate) IsRobotActivate = true;
        JointController pObject = (JointController)sender;
        _pJointStatusFlags[pObject.Index] = true;
        Debug.Log(
            $"[MOVE] Joint={pObject.Name} InputPos={e.TargetPosition} CurrentPos={e.CurrentPosition:F2} Speed={e.Speed:F2}");
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnJointStopEvent(object sender, MoterEventArgs e)
    {
        JointController pObject = (JointController)sender;
        _pJointStatusFlags[pObject.Index] = false;
        Debug.Log($"[STOP] Joint={pObject.Name} CurrentPos={e.CurrentPosition:F2} Speed={e.Speed:F2}");
    }


    public IEnumerator Move(ITeachingPoint pTarget)
    {
        yield return pTarget switch
        {
            JointPoint pJointPosition => MoveAbsoluteJoints(pJointPosition),
            CartesianPoint pCartPosition => MoveAbsoluteCartesian(pCartPosition),
            _ => throw new ArgumentOutOfRangeException(nameof(pTarget), pTarget, null)
        };
    }

    public IEnumerator MoveAbsoluteJoints(params float[] pTargetPositions)
    {
        yield return StartCoroutine(RotateJoints(pTargetPositions, 100));
    }

    public IEnumerator MoveAbsoluteJoints(JointPoint pTargetPoint)
    {
        yield return StartCoroutine(RotateJoints(pTargetPoint.Values, pTargetPoint.FrameCount));
    }

    public IEnumerable MoveAbsoluteCartesian(float fX, float fY, float fZ)
    {
        CartesianPoint pTargetPoint = new CartesianPoint("Target", fX, fY, fZ);
        yield return MoveAbsoluteCartesian(pTargetPoint);
    }

    public IEnumerable MoveAbsoluteCartesian(float fX, float fY, float fZ, float fRx, float fRy, float fRz)
    {
        CartesianPoint pTargetPoint = new CartesianPoint("Target", fX, fY, fZ, fRx, fRy, fRz);
        yield return MoveAbsoluteCartesian(pTargetPoint);
    }

    public IEnumerable MoveAbsoluteCartesian(CartesianPoint pTargetPoint)
    {
        JointPoint pConvertPoint = pTargetPoint.ToJointPoint(CommonFactory.RobotKinematics);
        yield return StartCoroutine(RotateJoints(pConvertPoint.Values, pConvertPoint.FrameCount));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator RotateJoints(float[] pTargetPositions, int nFrameCount, SpeedRule eMode = SpeedRule.Trapezoid)
    {
        if (joints?.Length != pTargetPositions.Length) yield break;
        Debug.Log($"Move the absolute joint position [{string.Join(",", pTargetPositions)}]");
        if (pTargetPositions.SequenceEqual(JointPos.Values))
        {
            Debug.LogWarning(
                $"The target position is the same as current position [{string.Join(",", pTargetPositions)}]");
            yield break;
        }
        InitRobot();
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            if (pJoint is null) continue;
            pJoint.Index = i;
            pJoint.Name = joints[i].inputAxis;
            pJoint.SpeedMode = eMode;
            pJoint.OnMoveEvent += OnJointMoveEvent;
            pJoint.OnStopEvent += OnJointStopEvent;
            pJoint.FrameCount = nFrameCount;
            pJoint.TargetPosition = pTargetPositions[i];
            pJoint.UpdateParameter();
        }

        yield return new WaitUntil(() => IsRobotActivate);
        yield return new WaitUntil(() => _pJointStatusFlags.All(bFlag => !bFlag));
        ExitRobot();
        Debug.Log($"Exit the absolute move, current = [{string.Join(",", JointPos.Values)}]");
    }
}