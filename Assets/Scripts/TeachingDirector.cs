using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeachingDirector : MonoBehaviour
{
    public GameObject robot;
    public GameObject gripper;
    public GameObject logger;
    public TrimMode trimMode;

    private RobotController _pRobotControl;
    private GripperController _pGripperControl;
    private Dictionary<string, JointPoint> _pDicPoints;
    private TextMeshProUGUI _pDisplayLog;
    private int _nCount = 0;
    private string _strTeachingRoot;
    
    private IEnumerator ReplayScript()
    {
        _pDisplayLog.text = "Run Replay Script";
        yield return new WaitForSeconds(1);
        _pDisplayLog.text = "Home position";
        yield return HomeScript();
        foreach(JointPoint pPoint in _pDicPoints.Values)
        {
            yield return _pRobotControl.Move(pPoint);
            if (pPoint.GripStatus != _pGripperControl.Status)
                yield return _pGripperControl.Do(pPoint.GripStatus);
        }
    }

    private IEnumerator HomeScript()
    {
        JointPoint pHomePos = JointPoint.Home();
        yield return _pRobotControl.Move(pHomePos);
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _pGripperControl = gripper.GetComponent<GripperController>();
        _pRobotControl.ControlMode = OperationMode.Teaching;
        _pGripperControl.ControlMode = OperationMode.Teaching;
        _pDicPoints = new Dictionary<string, JointPoint>();
        _strTeachingRoot = Path.Combine(Directory.GetCurrentDirectory(), "Teaching");
        _pDisplayLog = logger.GetComponent<TextMeshProUGUI>();
        _pDisplayLog.text = "Teaching Mode";
        InitCommons();
    }

    private void InitCommons()
    {
        GameObject[] pBaseObjects = {robot};
        GameObject[] pRobotObjects = _pRobotControl.joints.Select(pObj => pObj.robotPart).ToArray();
        GameObject[] pTotalObjects = pBaseObjects.Grouping(pRobotObjects);
        CommonFactory.RobotKinematics = new KinematicsCalculator(pTotalObjects);
    }

    private enum PlayMode
    {
        Wait,
        Teach,
        Save,
        Trim,
        Home,
        Replay,
    };

    private PlayMode _eUpdateMode = PlayMode.Teach;

    // Update is called on every frames
    private void Update()
    {
        if (CommonFactory.GetInputKeys(new[] {KeyCode.LeftShift, KeyCode.RightShift}).Length > 0)
        {
            // Play the script with SHIFT key
            _eUpdateMode = CommonFactory.GetInputKeyDown(new[]
                    {KeyCode.H, KeyCode.R}) switch
                {
                    KeyCode.H => PlayMode.Home,
                    KeyCode.R => PlayMode.Replay,
                    _ => _eUpdateMode
                };
        }
        else
        {
            _eUpdateMode = CommonFactory.GetInputKeyDown(new[]
                    {KeyCode.Escape, KeyCode.Return, KeyCode.Space, KeyCode.Alpha0}) switch
                {
                    KeyCode.Escape => PlayMode.Teach,
                    KeyCode.Return => PlayMode.Save,
                    KeyCode.Space => PlayMode.Trim,
                    KeyCode.Alpha0 => PlayMode.Home,
                    _ => _eUpdateMode
                };
        }

        switch (_eUpdateMode)
        {
            case PlayMode.Wait:
                break;
            case PlayMode.Teach:
                _pRobotControl.ControlMode = OperationMode.Teaching;
                _pGripperControl.ControlMode = OperationMode.Teaching;
                _pDisplayLog.text = $"Teaching Mode [{_pRobotControl.JointPos.Print()}]";
                break;
            case PlayMode.Save:
                _pRobotControl.ControlMode = OperationMode.Auto;
                JointPoint pPointSaved = _pRobotControl.JointPos;
                pPointSaved.GripStatus = _pGripperControl.Status;
                pPointSaved.Name = $"Pos{_nCount}";
                _pDisplayLog.text = $"Save the {pPointSaved.Name} [{pPointSaved.Print()}]";
                _pDicPoints.Add(pPointSaved.Name, pPointSaved);
                _nCount++;
                // Move the job flag to prevent overload
                _eUpdateMode = PlayMode.Wait;
                break;
            case PlayMode.Trim:
                // Move to the redefined position
                _pRobotControl.ControlMode = OperationMode.Auto;
                JointPoint pPointTrimmed = _pRobotControl.JointPos;
                pPointTrimmed.GripStatus = _pGripperControl.Status;
                pPointTrimmed.Trim(trimMode);
                _pDisplayLog.text = $"Move the trim position [{pPointTrimmed.Print()}]";
                StartCoroutine(_pRobotControl.Move(pPointTrimmed));
                // Move the job flag to prevent overload
                _eUpdateMode = PlayMode.Wait;
                break;
            case PlayMode.Home:
                _pRobotControl.ControlMode = OperationMode.Auto;
                _pGripperControl.ControlMode = OperationMode.Auto;
                _pDisplayLog.text = "Run Home Script";
                StartCoroutine(HomeScript());
                // Move the job flag to prevent overload
                _eUpdateMode = PlayMode.Wait;
                break;
            case PlayMode.Replay:
                _pRobotControl.ControlMode = OperationMode.Auto;
                _pGripperControl.ControlMode = OperationMode.Auto;
                if (_pDicPoints.Count > 0)
                {
                    string strFilePath = Path.Combine(_strTeachingRoot, "Points.json");
                    TeachingFactory.SaveTeachingPoints(_pDicPoints, strFilePath);
                }
                StartCoroutine(ReplayScript());
                // Move the job flag to prevent overload
                _eUpdateMode = PlayMode.Wait;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
