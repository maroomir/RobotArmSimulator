using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct FingerInfo
{
    public string name;
    public GameObject robotPart;
    public float openLimit;
    public float closedLimit;
}

public class GripperController : MonoBehaviour
{
    public FingerInfo[] fingers;

    public bool IsGripperActivate { get; private set; }
    public int FrameCount { get; private set; } = 10;
    
    public Vector3 EndPoint { get; }
    
    public OperationMode ControlMode { get; set; }
    public FingerStatus Status => fingers[0].robotPart.GetComponent<FingerController>().Status;

    private bool[] _pFingerStatusFlag;
    
    // Start is called before the first frame update
    private void Start()
    {
        _pFingerStatusFlag = new bool[fingers.Length];
    }

    private void Update()
    {
        if (ControlMode != OperationMode.Teaching) return;
        switch (CommonFactory.GetInputKeyDown(new[] {KeyCode.G, KeyCode.R}))
        {
            case KeyCode.G:
                StartCoroutine(Close());
                break;
            case KeyCode.R:
                StartCoroutine(Open());
                break;
        }
    }

    private void OnFingerMoveEvent(object sender, MoterEventArgs e)
    {
        if (!IsGripperActivate) IsGripperActivate = true;
        FingerController pObject = (FingerController) sender;
        _pFingerStatusFlag[pObject.Index] = true;
        Debug.Log($"[MOVE] Finger={pObject.Name} Speed={e.Speed} CurrentPos={e.CurrentPosition}");
    }

    private void OnFingerStopEvent(object sender, MoterEventArgs e)
    {
        FingerController pObject = (FingerController) sender;
        _pFingerStatusFlag[pObject.Index] = false;
        Debug.Log($"[STOP] Finger={pObject.Name} CurrentPos={e.CurrentPosition}");
    }

    private void InitGripper()
    {
        IsGripperActivate = false;
        for (int i = 0; i < fingers.Length; i++) _pFingerStatusFlag[i] = false;
    }

    private void ExitGripper()
    {
        IsGripperActivate = false;
        for (int i = 0; i < fingers.Length; i++)
        {
            GameObject pPart = fingers[i].robotPart;
            FingerController pFinger = pPart.GetComponent<FingerController>();
            if(pFinger == null) continue;
            pFinger.OnMoveEvent -= OnFingerMoveEvent;
            pFinger.OnStopEvent -= OnFingerStopEvent;
        }
    }

    public IEnumerator Do(FingerStatus eStatus)
    {
        if(eStatus == Status) yield break;
        switch (eStatus)
        {
            case FingerStatus.Close:
                yield return Close();
                break;
            case FingerStatus.Open:
                yield return Open();
                break;
        }
    }

    public IEnumerator Open()
    {
        InitGripper();
        for (int i = 0; i < fingers.Length; i++)
        {
            GameObject pPart = fingers[i].robotPart;
            FingerController pFinger = pPart.GetComponent<FingerController>();
            if(pFinger == null) continue;
            pFinger.Index = i;
            pFinger.Name = fingers[i].name;
            pFinger.SetLimit(fingers[i].openLimit, fingers[i].closedLimit);
            pFinger.FrameCount = FrameCount;
            pFinger.OnMoveEvent += OnFingerMoveEvent;
            pFinger.OnStopEvent += OnFingerStopEvent;
            pFinger.Open();
        }
        yield return new WaitUntil(() => IsGripperActivate);
        yield return new WaitUntil(() => _pFingerStatusFlag.All(bFlag => !bFlag));
        ExitGripper();
    }

    public IEnumerator Close()
    {
        InitGripper();
        for (int i = 0; i < fingers.Length; i++)
        {
            GameObject pPart = fingers[i].robotPart;
            FingerController pFinger = pPart.GetComponent<FingerController>();
            if(pFinger == null) continue;
            pFinger.Index = i;
            pFinger.Name = fingers[i].name;
            pFinger.SetLimit(fingers[i].openLimit, fingers[i].closedLimit);
            pFinger.FrameCount = FrameCount;
            pFinger.OnMoveEvent += OnFingerMoveEvent;
            pFinger.OnStopEvent += OnFingerStopEvent;
            pFinger.Close();
        }
        yield return new WaitUntil(() => IsGripperActivate);
        yield return new WaitUntil(() => _pFingerStatusFlag.All(bFlag => !bFlag));
        ExitGripper();
    }
}