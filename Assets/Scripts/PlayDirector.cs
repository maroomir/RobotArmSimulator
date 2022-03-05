using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ScriptInfo
{
    public int order = 0;
    public string name;
    
    private string _strRoot = Path.Combine(Directory.GetCurrentDirectory(), "Teaching");
    
    public string FilePath => Path.Combine(_strRoot, name);
    public Dictionary<string, JointPoint> Points
    {
        get
        {
            if (!CommonFactory.VerifyFileExtension(FilePath, ".json"))
                throw new FileLoadException($"File location does not exist : {FilePath}");
            return TeachingFactory.LoadTeachingPoints(FilePath);
        }
    }
}

public class PlayDirector : MonoBehaviour
{
    public ScriptInfo[] scripts;
    public GameObject robot;
    public GameObject gripper;
    public GameObject logger;

    private List<ScriptInfo> _pListScripts;
    private RobotController _pRobotControl;
    private GripperController _pGripperControl;
    private TextMeshProUGUI _pDisplayLog;

    private IEnumerator StartScripts()
    {
        _pDisplayLog.text = "Play Scripts";
        yield return new WaitForSeconds(1);
        _pDisplayLog.text = "Home position";
        JointPoint pHomePos = JointPoint.Home();
        yield return _pRobotControl.Move(pHomePos);
        foreach (ScriptInfo pScript in _pListScripts)
        {
            _pDisplayLog.text = $"{pScript.order}:{pScript.name}";
            yield return RunScript(pScript);
        }
    }

    private IEnumerator RunScript(ScriptInfo pScript)
    {
        foreach (var pPoint in pScript.Points)
        {
            Debug.Log($"[PLAY]name={pScript.name}, detailed={pPoint.Key}, {pPoint.Value.Print()}");
            yield return _pRobotControl.Move(pPoint.Value);
            if (pPoint.Value.GripStatus != _pGripperControl.Status)
                yield return _pGripperControl.Do(pPoint.Value.GripStatus);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _pListScripts = new List<ScriptInfo>(scripts);
        _pListScripts.Sort((pX, pY) => pX.order.CompareTo(pY.order));
        _pRobotControl = robot.GetComponent<RobotController>();
        _pGripperControl = gripper.GetComponent<GripperController>();
        _pRobotControl.ControlMode = OperationMode.Auto;
        _pGripperControl.ControlMode = OperationMode.Auto;
        _pDisplayLog = logger.GetComponent<TextMeshProUGUI>();
        // Start the game logics
        StartCoroutine(StartScripts());
    }
}
