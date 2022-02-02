using System.Collections;
using System.Linq;
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
        JointPoint pInitPos = JointPoint.FromPosition("InitPos", 0.0F, 0.0F, 90.0F, 90.0F, 0.0F, 0.0F, 180.0F);
        JointPoint pPos1 = JointPoint.FromPosition("Pos1", -90.0F, 0.0F, 45.0F, -90.0F, 45.0F, 90.0F, -90.0F);
        JointPoint pPos2 = JointPoint.FromPosition("Pos2", 90.0F, 0.0F, -45.0F, 90.0F, -45.0F, -90.0F, 90.0F);
        pInitPos.FrameCount = 100;
        yield return _pRobotControl.Move(pInitPos);
        yield return _pGripperControl.Close();
        pPos1.FrameCount = 100;
        yield return _pRobotControl.Move(pPos1);
        pPos1.FrameCount = 10;
        pPos2.FrameCount = 10;
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
        JointPoint pInitPos = JointPoint.FromPosition("InitPos", 180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 0.0F, 0.0F);
        JointPoint pPos1 = JointPoint.FromPosition("Pos1", 180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 180.0F, 90.0F);
        JointPoint pPos2 = JointPoint.FromPosition("Pos2", 180.0F, 0.0F, 90.0F, 90.0F, 90.0F, 180.0F, -90.0F);
        CartesianPoint pSpanX = new CartesianPoint("Span", 10, 0.1F, 0.0F, 0.0F);
        CartesianPoint pSpanY = new CartesianPoint("Span", 10, 0.0F, 0.1F, 0.0F);
        CartesianPoint pSpanZ = new CartesianPoint("Span", 10, 0.0F, 0.0F, 0.1F);
        pInitPos.FrameCount = 100;
        yield return _pRobotControl.Move(pInitPos);
        for (int i = 0; i < 10; i++)
        {
            yield return _pRobotControl.Move(pPos1);
            yield return _pGripperControl.Close();
            yield return new WaitForSeconds(1);
            for (int j = 0; j < 10; j++)
            {
                yield return _pRobotControl.Move(pPos1 + pSpanX);
                yield return _pRobotControl.Move(pPos1 - pSpanX);
                yield return _pRobotControl.Move(pPos1);
                yield return _pRobotControl.Move(pPos1 + pSpanY);
                yield return _pRobotControl.Move(pPos1 - pSpanY);
                yield return _pRobotControl.Move(pPos1);
                yield return _pRobotControl.Move(pPos1 + pSpanZ);
                yield return _pRobotControl.Move(pPos1 - pSpanZ);
                yield return _pRobotControl.Move(pPos1);
            }
            yield return _pRobotControl.Move(pPos2);
            yield return _pGripperControl.Open();
        }

        yield return _pRobotControl.Move(pInitPos);
        yield return _pRobotControl.Move(pHomePos);
    }

    public IEnumerator KinematicsTestScript()
    {
        JointPoint pHomePos = JointPoint.Home(_nAxisNum);
        Debug.Log($"Rate={_pRobotControl.KinematicsAccuracy}, " + 
                  $"Real=({_pRobotControl.RealEndPos.x:F6}, {_pRobotControl.RealEndPos.y:F6}, {_pRobotControl.RealEndPos.z:F6}), " + 
                  $"Estimated=({_pRobotControl.CartesianPos.X:F6}, {_pRobotControl.CartesianPos.Y:F6}, {_pRobotControl.CartesianPos.Z:F6})");
        JointPoint pInitPos1 = JointPoint.FromPosition("Init1", 180.0F, 0.0F, 90.0F, 90.0F, 0.0F, 0.0F, 0.0F);
        yield return _pRobotControl.Move(pInitPos1);
        Debug.Log($"Rate={_pRobotControl.KinematicsAccuracy}, " + 
                  $"Real=({_pRobotControl.RealEndPos.x:F6}, {_pRobotControl.RealEndPos.y:F6}, {_pRobotControl.RealEndPos.z:F6}), " + 
                  $"Estimated=({_pRobotControl.CartesianPos.X:F6}, {_pRobotControl.CartesianPos.Y:F6}, {_pRobotControl.CartesianPos.Z:F6})");
        JointPoint pInitPos2 = JointPoint.FromPosition("Init2", -180.0F, 0.0F, 90.0F, 90.0F, 30.0F, 30.0F, 30.0F);
        yield return _pRobotControl.Move(pInitPos2);
        Debug.Log($"Rate={_pRobotControl.KinematicsAccuracy}, " + 
                  $"Real=({_pRobotControl.RealEndPos.x:F6}, {_pRobotControl.RealEndPos.y:F6}, {_pRobotControl.RealEndPos.z:F6}), " + 
                  $"Estimated=({_pRobotControl.CartesianPos.X:F6}, {_pRobotControl.CartesianPos.Y:F6}, {_pRobotControl.CartesianPos.Z:F6})");
        JointPoint pInitPos3 = JointPoint.FromPosition("Init3", -90.0F, 0.0F, 45.0F, -90.0F, 45.0F, 90.0F, -90.0F);
        yield return _pRobotControl.Move(pInitPos3);
        Debug.Log($"Rate={_pRobotControl.KinematicsAccuracy}, " + 
                  $"Real=({_pRobotControl.RealEndPos.x:F6}, {_pRobotControl.RealEndPos.y:F6}, {_pRobotControl.RealEndPos.z:F6}), " + 
                  $"Estimated=({_pRobotControl.CartesianPos.X:F6}, {_pRobotControl.CartesianPos.Y:F6}, {_pRobotControl.CartesianPos.Z:F6})");
    }

    // Start is called before the first frame update
    private void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _pGripperControl = gripper.GetComponent<GripperController>();
        _nAxisNum = _pRobotControl.joints.Length;
        _pRobotControl.ControlMode = OperationMode.Auto;
        InitCommons();

        //StartCoroutine(TechnoMotionScript());
        StartCoroutine(GripperRotateMotionScript());
        //StartCoroutine(KinematicsTestScript());
    }

    private void InitCommons()
    {
        GameObject[] pBaseObjects = {robot};
        GameObject[] pRobotObjects = _pRobotControl.joints.Select(pObj => pObj.robotPart).ToArray();
        GameObject[] pTotalObjects = pBaseObjects.Grouping(pRobotObjects);
        CommonFactory.RobotKinematics = new KinematicsCalculator(pTotalObjects);
    }
}
