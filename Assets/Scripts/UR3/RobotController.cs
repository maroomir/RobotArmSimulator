using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct JointInfo
{
    public string inputAxis;
    public GameObject robotPart;
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
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            if (pJoint is null) continue;
            pJoint.Index = i;
            pJoint.Name = joints[i].inputAxis;
        }
    }

    // Update is called on every frames
    private void Update()
    {
        if (ControlMode != OperationMode.Teaching) return;

        if (CommonFactory.IsInputKeys(new[] {KeyCode.X, KeyCode.Y, KeyCode.Z}))
            StartCoroutine(PartialMovement(CommonFactory.GetInputKey(new[] {KeyCode.X, KeyCode.Y, KeyCode.Z})));
        else
        {
            for (int i = 0; i < joints.Length; i++)
            {
                GameObject pPart = joints[i].robotPart;
                JointController pJoint = pPart.GetComponent<JointController>();
                pJoint.ControlMode = Input.GetKey($"{pJoint.Index + 1}") ? OperationMode.Teaching : OperationMode.Auto;
                pJoint.Break = (pJoint.ControlMode == OperationMode.Teaching) ? BreakStatus.Release : BreakStatus.Hold;
            }
        }
    }

    private IEnumerator PartialMovement(KeyCode eCommand, float fStep = 0.01F, int nFrameCount = 5, bool bTrace = true)
    {
        float[] pMovements = new[] {0.0F, 0.0F, 0.0F};
        pMovements = eCommand switch
        {
            KeyCode.X => Input.GetKeyDown(KeyCode.LeftArrow) ? new[] {-fStep, 0.0F, 0.0F} :
                Input.GetKeyDown(KeyCode.RightArrow) ? new[] {fStep, 0.0F, 0.0F} : pMovements,
            KeyCode.Y => Input.GetKeyDown(KeyCode.LeftArrow) ? new[] {0.0F, -fStep, 0.0F} :
                Input.GetKeyDown(KeyCode.RightArrow) ? new[] {0.0F, fStep, 0.0F} : pMovements,
            KeyCode.Z => Input.GetKeyDown(KeyCode.LeftArrow) ? new[] {0.0F, 0.0F, -fStep} :
                Input.GetKeyDown(KeyCode.RightArrow) ? new[] {0.0F, 0.0F, fStep} : pMovements,
            _ => pMovements
        };
        if (pMovements.EqualByElement(new[] {0.0F, 0.0F, 0.0F})) yield break;
        CartesianPoint pMoveVector = new CartesianPoint("movements", nFrameCount, pMovements);
        if (bTrace)
        {
            CartesianPoint pTargetVector = CartesianPos + pMoveVector;
            Debug.Log($"[REF]Movement=({pMoveVector.X:F3},{pMoveVector.Y:F3},{pMoveVector.Z:F3})," +
                      $"Target=({pTargetVector.X:F5},{pTargetVector.Y:F5},{pTargetVector.Z:F5}), " +
                      $"Curr=({CartesianPos.X:F5},{CartesianPos.Y:F5},{CartesianPos.Z:F5})");
        }

        yield return StartCoroutine(MoveAbsoluteJoints(JointPos + pMoveVector));
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

    public void ForcedMove(ITeachingPoint pTarget)
    {
        ControlMode = OperationMode.Forced;
        switch (pTarget)
        {
            case JointPoint pJointPosition:
                RotatedJointsForced(pJointPosition.Values);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pTarget), pTarget, null);
        }
    }

    public IEnumerator Move(ITeachingPoint pTarget)
    {
        yield return pTarget switch
        {
            JointPoint pJointPosition => MoveAbsoluteJoints(pJointPosition),
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

    private void RotatedJointsForced(float[] pTargetPositions)
    {
        if (joints?.Length != pTargetPositions.Length) return;
        if (pTargetPositions.SequenceEqual(JointPos.Values))
            return;
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            if (pJoint is null) continue;
            pJoint.ControlMode = OperationMode.Forced;
            pJoint.Break = BreakStatus.Release;
            pJoint.ForcedUpdate(pTargetPositions[i]);
        }
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
            pJoint.OnMoveEvent += OnJointMoveEvent;
            pJoint.OnStopEvent += OnJointStopEvent;
            pJoint.SpeedMode = eMode;
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