using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditorInternal;
using UnityEngine;

public class ScriptDirector : MonoBehaviour
{
    public GameObject robot;
    public GameObject gripper;

    private RobotController _pRobotControl;
    private GripperController _pGripperControl;
    private int _nAxisNum;

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator TechnoMotionScript()
    {
        JointPosition pHomePos = JointPosition.Home();
        JointPosition pInitPos = JointPosition.FromPosition(0.0F, 0.0F, 90.0F, 90.0F, 0.0F, 0.0F, 180.0F);
        JointPosition pPos1 = JointPosition.FromPosition(-90.0F, 0.0F, 45.0F, -90.0F, 45.0F, 90.0F, -90.0F);
        JointPosition pPos2 = JointPosition.FromPosition(90.0F, 0.0F, -45.0F, 90.0F, -45.0F, -90.0F, 90.0F);
        pInitPos.MaxSpeed = 100;
        yield return _pRobotControl.Move(pInitPos);
        pPos1.MaxSpeed = 100;
        yield return _pRobotControl.Move(pPos1);
        pPos1.MaxFrame = 10;
        pPos2.MaxFrame = 10;
        Thread.Sleep(2000);
        for (int i = 0; i < 20; i++)
        {
            yield return _pRobotControl.Move(pPos1);
            yield return _pRobotControl.Move(pPos2);
        }
        yield return _pRobotControl.Move(pHomePos);
    }

    public IEnumerator GripperRotateMotionScript()
    {
        JointPosition pHomePos = JointPosition.Home(_nAxisNum);
        JointPosition pInitPos = JointPosition.FromPosition(180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 0.0F, 0.0F);
        JointPosition pPos1 = JointPosition.FromPosition(180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 180.0F, 360.0F);
        JointPosition pPos2 = JointPosition.FromPosition(180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 180.0F, -360.0F);
        pInitPos.MaxSpeed = 100;
        yield return _pRobotControl.Move(pInitPos);
        for (int i = 0; i < 10; i++)
        {
            yield return _pRobotControl.Move(pPos1);
            yield return _pGripperControl.Close();
            yield return _pRobotControl.Move(pPos2);
            yield return _pGripperControl.Open();
        }

        yield return _pRobotControl.Move(pInitPos);
        yield return _pRobotControl.Move(pHomePos);
    }

    // Start is called before the first frame update
    private void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _pGripperControl = gripper.GetComponent<GripperController>();
        _nAxisNum = _pRobotControl.joints.Length;
        //StartCoroutine(TechnoMotionScript());
        StartCoroutine(GripperRotateMotionScript());
    }
}
