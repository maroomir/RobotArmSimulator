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

    private void OnJointMoveEvent(object sender, JointEventArgs e)
    {
        if(!IsRobotActivate) IsRobotActivate = true;
        JointController pObject = (JointController) sender;
        _pJointStatusFlags[pObject.Index] = true;
        Debug.Log($"[MOVE] Joint={pObject.Name} InputPos={e.TargetPosition} CurrentPos={e.CurrentPosition:F2} Speed={e.Speed:F2}");
    }

    private void OnJointStopEvent(object sender, JointEventArgs e)
    {
        JointController pObject = (JointController) sender;
        _pJointStatusFlags[pObject.Index] = false;
        Debug.Log($"[STOP] Joint={pObject.Name} CurrentPos={e.CurrentPosition:F2} Speed={e.Speed:F2}");
    }

    public IEnumerator MoveAbsoluteJoint(params float[] pTargetPositions)
    {
        if (joints?.Length != pTargetPositions.Length) yield break;
        Debug.Log($"Move the absolute joint position [{string.Join(",", pTargetPositions)}]");
        InitRobot();
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject pPart = joints[i].robotPart;
            JointController pJoint = pPart.GetComponent<JointController>();
            if (pJoint == null) continue;
            pJoint.Index = i;
            pJoint.Name = joints[i].inputAxis;
            pJoint.OnJointMoveEvent += OnJointMoveEvent;
            pJoint.OnJointStopEvent += OnJointStopEvent;
            pJoint.MaxSpeed = 100.0F;
            pJoint.TargetPosition = pTargetPositions[i];
            pJoint.UpdateParameter();
        }

        yield return new WaitUntil(() => IsRobotActivate);
        yield return new WaitUntil(() => _pJointStatusFlags.All(bFlag => !bFlag));
        ExitRobot();
        Debug.Log("Exit the absolute move");
    }
}
