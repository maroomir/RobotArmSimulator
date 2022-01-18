using System.Collections;
using System.Linq;
using UnityEngine;

public class GripperController : MonoBehaviour
{
    public GameObject fingerA;
    public GameObject fingerB;

    public bool IsGripperActivate { get; private set; }

    private FingerController _pFingerAController;
    private FingerController _pFingerBController;
    private readonly bool[] _pFingerStatusFlag = new bool[2];

    public Vector3 EndPoint { get; }

    // Start is called before the first frame update
    private void Start()
    {
        _pFingerAController = fingerA.GetComponent<FingerController>();
        _pFingerAController.Index = 0;
        _pFingerAController.Name = "FingerA";
        _pFingerBController = fingerB.GetComponent<FingerController>();
        _pFingerAController.Index = 1;
        _pFingerAController.Name = "FingerB";
    }

    private void OnFingerMoveEvent(object sender, FingerEventArgs e)
    {
        if (!IsGripperActivate) IsGripperActivate = true;
        FingerController pObject = (FingerController) sender;
        _pFingerStatusFlag[pObject.Index] = true;
        Debug.Log($"[MOVE] Joint={pObject.Name} Status={e.Status} CurrentPos={e.CurrentPosition}");
    }

    private void OnFingerStopEvent(object sender, FingerEventArgs e)
    {
        FingerController pObject = (FingerController) sender;
        _pFingerStatusFlag[pObject.Index] = false;
        Debug.Log($"[STOP] Joint={pObject.Name} Status={e.Status} CurrentPos={e.CurrentPosition}");
    }

    private void InitEvents()
    {
        _pFingerAController.OnFingerMoveEvent += OnFingerMoveEvent;
        _pFingerAController.OnFingerStopEvent += OnFingerStopEvent;
        _pFingerBController.OnFingerMoveEvent += OnFingerMoveEvent;
        _pFingerBController.OnFingerStopEvent += OnFingerStopEvent;
    }

    private void CloseEvents()
    {
        _pFingerAController.OnFingerMoveEvent -= OnFingerMoveEvent;
        _pFingerAController.OnFingerStopEvent -= OnFingerStopEvent;
        _pFingerBController.OnFingerMoveEvent -= OnFingerMoveEvent;
        _pFingerBController.OnFingerStopEvent -= OnFingerStopEvent;
    }

    public IEnumerator Open()
    {
        InitEvents();
        _pFingerAController.MaxFrame = 10;
        _pFingerAController.UpdateParameter(GripperStatus.Open);
        _pFingerAController.MaxFrame = 10;
        _pFingerBController.UpdateParameter(GripperStatus.Open);
        yield return new WaitUntil(() => IsGripperActivate);
        yield return new WaitUntil(() => _pFingerStatusFlag.All(bFlag => !bFlag));
        IsGripperActivate = false;
        CloseEvents();
    }

    public IEnumerator Close()
    {
        InitEvents();
        _pFingerAController.MaxFrame = 10;
        _pFingerAController.UpdateParameter(GripperStatus.Closed);
        _pFingerAController.MaxFrame = 10;
        _pFingerBController.UpdateParameter(GripperStatus.Closed);
        yield return new WaitUntil(() => IsGripperActivate);
        yield return new WaitUntil(() => _pFingerStatusFlag.All(bFlag => !bFlag));
        IsGripperActivate = false;
        CloseEvents();
    }
}