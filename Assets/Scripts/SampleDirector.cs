using System.Collections;
using System.Threading;
using UnityEngine;

public class SampleDirector : MonoBehaviour
{
    public GameObject robot;
    public GameObject gripper;

    private RobotController _pRobotControl;
    private GripperController _pGripperControl;
    private int _nAxisNum;

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator TechnoMotionScript()
    {
        JointPoint pHomePos = JointPoint.Home();
        JointPoint pInitPos = JointPoint.FromPosition(0.0F, 0.0F, 90.0F, 90.0F, 0.0F, 0.0F, 180.0F);
        JointPoint pPos1 = JointPoint.FromPosition(-90.0F, 0.0F, 45.0F, -90.0F, 45.0F, 90.0F, -90.0F);
        JointPoint pPos2 = JointPoint.FromPosition(90.0F, 0.0F, -45.0F, 90.0F, -45.0F, -90.0F, 90.0F);
        pInitPos.MaxSpeed = 100;
        yield return _pRobotControl.Move(pInitPos);
        yield return _pGripperControl.Close();
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
        yield return _pGripperControl.Open();
    }

    public IEnumerator GripperRotateMotionScript()
    {
        JointPoint pHomePos = JointPoint.Home(_nAxisNum);
        JointPoint pInitPos = JointPoint.FromPosition(180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 0.0F, 0.0F);
        JointPoint pPos1 = JointPoint.FromPosition(180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 180.0F, 90.0F);
        JointPoint pPos2 = JointPoint.FromPosition(180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 180.0F, -90.0F);
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
        _pRobotControl.ControlMode = OperationMode.Auto;
        StartCoroutine(TechnoMotionScript());
        //StartCoroutine(GripperRotateMotionScript());
    }
}
