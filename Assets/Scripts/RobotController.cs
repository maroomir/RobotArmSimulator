using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Schema;
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

    public float[] CurrentPosition
    {
        get
        {
            float[] pResult = new float[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                GameObject pPart = joints[i].robotPart;
                JointController pJoint = pPart.GetComponent<JointController>();
                if (pJoint == null) continue;
                pResult[i] = pJoint.CurrentPosition;
            }

            return pResult;
        }
    }

    private bool[] _pJointStatusFlags;

    private void InitRobot()
    {
        IsRobotActivate = false;
        if (_pJointStatusFlags?.Length != joints.Length)
            _pJointStatusFlags = new bool[joints.Length];
    }

    private void ExitRobot()
    {
        IsRobotActivate = false;
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            if (pJoint == null) continue;
            pJoint.OnJointMoveEvent -= OnJointMoveEvent;
            pJoint.OnJointStopEvent -= OnJointStopEvent;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnJointMoveEvent(object sender, JointEventArgs e)
    {
        if (!IsRobotActivate) IsRobotActivate = true;
        JointController pObject = (JointController) sender;
        _pJointStatusFlags[pObject.Index] = true;
        Debug.Log(
            $"[MOVE] Joint={pObject.Name} InputPos={e.TargetPosition} CurrentPos={e.CurrentPosition:F2} Speed={e.Speed:F2}");
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnJointStopEvent(object sender, JointEventArgs e)
    {
        JointController pObject = (JointController) sender;
        _pJointStatusFlags[pObject.Index] = false;
        Debug.Log($"[STOP] Joint={pObject.Name} CurrentPos={e.CurrentPosition:F2} Speed={e.Speed:F2}");
    }

    public IEnumerator MoveAbsoluteJoints(params float[] pTargetPositions)
    {
        yield return StartCoroutine(RotateJoints(pTargetPositions, 200.0F));
    }

    public IEnumerator MoveAbsoluteJoints(JointPosition pTargetPosition)
    {
        yield return pTargetPosition.SyncMode switch
        {
            SyncRule.SpeedSync => StartCoroutine(RotateJoints(pTargetPosition.Position, pTargetPosition.MaxSpeed)),
            SyncRule.StepSync => StartCoroutine(RotateJoints(pTargetPosition.Position, pTargetPosition.MaxStep)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator RotateJoints(float[] pTargetPositions, float fSpeed, SpeedRule eMode = SpeedRule.Trapezoid)
    {
        if (joints?.Length != pTargetPositions.Length) yield break;
        Debug.Log($"Move the absolute joint position [{string.Join(",", pTargetPositions)}]");
        if (pTargetPositions.SequenceEqual(CurrentPosition))
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
            pJoint.SyncMode = SyncRule.SpeedSync;
            pJoint.SpeedMode = eMode;
            pJoint.OnJointMoveEvent += OnJointMoveEvent;
            pJoint.OnJointStopEvent += OnJointStopEvent;
            pJoint.MaxSpeed = fSpeed;
            pJoint.TargetPosition = pTargetPositions[i];
            pJoint.UpdateParameter();
        }

        yield return new WaitUntil(() => IsRobotActivate);
        yield return new WaitUntil(() => _pJointStatusFlags.All(bFlag => !bFlag));
        ExitRobot();
        Debug.Log($"Exit the absolute move, current = [{string.Join(",", CurrentPosition)}]");
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator RotateJoints(float[] pTargetPositions, int nStep, SpeedRule eMode = SpeedRule.Trapezoid)
    {
        if (joints?.Length != pTargetPositions.Length) yield break;
        Debug.Log($"Move the absolute joint position [{string.Join(",", pTargetPositions)}]");
        if (pTargetPositions.SequenceEqual(CurrentPosition))
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
            pJoint.SyncMode = SyncRule.StepSync;
            pJoint.SpeedMode = eMode;
            pJoint.OnJointMoveEvent += OnJointMoveEvent;
            pJoint.OnJointStopEvent += OnJointStopEvent;
            pJoint.MaxStep = nStep;
            pJoint.TargetPosition = pTargetPositions[i];
            pJoint.UpdateParameter();
        }

        yield return new WaitUntil(() => IsRobotActivate);
        yield return new WaitUntil(() => _pJointStatusFlags.All(bFlag => !bFlag));
        ExitRobot();
        Debug.Log($"Exit the absolute move, current = [{string.Join(",", CurrentPosition)}]");
    }
}
