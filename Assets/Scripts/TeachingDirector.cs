using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Apple.ReplayKit;

public class TeachingDirector : MonoBehaviour
{
    public GameObject robot;

    private RobotController _pRobotControl;
    private List<ITeachingPoint> _pListPoints;
    private int _nCount = 0;
    private string _strTeachingRoot;
    
    private IEnumerator ReplayScript()
    {
        JointPoint pHomePos = JointPoint.Home();
        yield return _pRobotControl.Move(pHomePos);
        foreach(ITeachingPoint pPoint in _pListPoints)
        {
            pPoint.MaxFrame = 10;
            yield return _pRobotControl.Move(pPoint);
        }
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _pRobotControl.ControlMode = OperationMode.Teaching;
        _pListPoints = new List<ITeachingPoint>();
        _strTeachingRoot = Path.Combine(Directory.GetCurrentDirectory(), "Teaching");
    }
    
    // Update is called on every frames
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadEquals))
        {
            JointPoint pPoint = _pRobotControl.CurrentPosition;
            pPoint.Name = $"Pos{_nCount}";
            _pListPoints.Add(pPoint);
            _nCount++;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (_pListPoints.Count > 0)
            {
                string strFilePath = Path.Combine(_strTeachingRoot, "Points.json");
                TeachingFactory.SaveTeachingPoints(_pListPoints, strFilePath);
            }

            _pRobotControl.ControlMode = OperationMode.Auto;
            StartCoroutine(ReplayScript());
            _pRobotControl.ControlMode = OperationMode.Teaching;
        }
    }
}
