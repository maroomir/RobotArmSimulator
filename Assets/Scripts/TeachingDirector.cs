using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachingDirector : MonoBehaviour
{
    public GameObject robot;

    private RobotController _pRobotControl;
    
    // Start is called before the first frame update
    void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _pRobotControl.ControlMode = OperationMode.Teaching;
    }
}
