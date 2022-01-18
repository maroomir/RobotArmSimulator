using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct FingerInfo
{
    public string name;
    public GameObject robotPart;
    public float stroke;
}

public class GripperController : MonoBehaviour
{
    public FingerInfo[] fingers;

    public bool IsGripperActivate { get; private set; }
    public int MaxFrame { get; private set; } = 10;
    
    public Vector3 EndPoint { get; }

    private bool[] _pFingerStatusFlag;

    private void OnFingerMoveEvent(object sender, FingerEventArgs e)
    {
        if (!IsGripperActivate) IsGripperActivate = true;
        FingerController pObject = (FingerController) sender;
        _pFingerStatusFlag[pObject.Index] = true;
        Debug.Log($"[MOVE] Finger={pObject.Name} Status={e.Status} Speed={e.Speed} CurrentPos={e.CurrentPosition}");
    }

    private void OnFingerStopEvent(object sender, FingerEventArgs e)
    {
        FingerController pObject = (FingerController) sender;
        _pFingerStatusFlag[pObject.Index] = false;
        Debug.Log($"[STOP] Finger={pObject.Name} Status={e.Status} CurrentPos={e.CurrentPosition}");
    }

    private void InitGripper()
    {
        IsGripperActivate = false;
        if (_pFingerStatusFlag?.Length != fingers.Length)
            _pFingerStatusFlag = new bool[fingers.Length];
    }

    private void ExitGripper()
    {
        IsGripperActivate = false;
        for (int i = 0; i < fingers.Length; i++)
        {
            GameObject pPart = fingers[i].robotPart;
            FingerController pFinger = pPart.GetComponent<FingerController>();
            if(pFinger == null) continue;
            pFinger.OnFingerMoveEvent -= OnFingerMoveEvent;
            pFinger.OnFingerStopEvent -= OnFingerStopEvent;
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
            pFinger.Stroke = fingers[i].stroke;
            pFinger.Status = FingerStatus.Open;
            pFinger.MaxFrame = MaxFrame;
            pFinger.OnFingerMoveEvent += OnFingerMoveEvent;
            pFinger.OnFingerStopEvent += OnFingerStopEvent;
            pFinger.UpdateParameter();
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
            pFinger.Stroke = fingers[i].stroke;
            pFinger.Status = FingerStatus.Closed;
            pFinger.MaxFrame = MaxFrame;
            pFinger.OnFingerMoveEvent += OnFingerMoveEvent;
            pFinger.OnFingerStopEvent += OnFingerStopEvent;
            pFinger.UpdateParameter();
        }
        yield return new WaitUntil(() => IsGripperActivate);
        yield return new WaitUntil(() => _pFingerStatusFlag.All(bFlag => !bFlag));
        ExitGripper();
    }
}