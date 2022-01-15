using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class SampleDirector : MonoBehaviour
{
    public GameObject robot;

    private RobotController _pRobotControl;

    private IEnumerator TechnoMotionScript()
    {
        JointPosition pInitPos = JointPosition.FromPosition(0.0F, 0.0F, 90.0F, 90.0F, 0.0F, 0.0F, 0.0F);
        JointPosition pPos1 = JointPosition.FromPosition(-90.0F, 0.0F, 45.0F, -90.0F, 30.0F, 90.0F, 0.0F);
        JointPosition pPos2 = JointPosition.FromPosition(90.0F, 0.0F, -45.0F, 90.0F, -30.0F, -90.0F, 0.0F);
        JointPosition pHomePos = JointPosition.FromPosition(0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F);
        pPos1.MaxStep = 12;
        pPos2.MaxStep = 12;
        yield return _pRobotControl.MoveAbsoluteJoints(pInitPos);
        for (int i = 0; i < 20; i++)
        {
            yield return _pRobotControl.MoveAbsoluteJoints(pPos1);
            yield return _pRobotControl.MoveAbsoluteJoints(pPos2);
        }

        pHomePos.MaxSpeed = 100.0F;
        yield return _pRobotControl.MoveAbsoluteJoints(pHomePos);
    }

    // Start is called before the first frame update
    void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        StartCoroutine(TechnoMotionScript());
    }
}
