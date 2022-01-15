using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class SampleDirector : MonoBehaviour
{
    public GameObject robot;

    private RobotController _pRobotControl;

    private IEnumerator[] MotionScript => new[]
    {
        _pRobotControl.MoveAbsoluteJoint(360.0F, 90.0F, 90.0F, 0.0F, 0.0F, 0.0F, 0.0F),
        _pRobotControl.MoveAbsoluteJoint(90.0F, 90.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F),
        _pRobotControl.MoveAbsoluteJoint(0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F),
    };
    
    IEnumerator StartMotion(IEnumerator[] pScript)
    {
        for (int i = 0; i < pScript.Length; i++)
        {
            yield return StartCoroutine(pScript[i]);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        StartCoroutine(StartMotion(MotionScript));
    }
}
