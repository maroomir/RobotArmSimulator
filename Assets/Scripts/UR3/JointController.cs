using UnityEngine;
using static UnityEngine.KeyCode;

public class JointController : MonoBehaviour, IMotorControl
{
    public int Index { get; set; }
    public string Name { get; set; }
    public float CurrentPosition => (_pArticulation == null) ? 0.0F : Mathf.Rad2Deg * _pArticulation.jointPosition[0];
    public int FrameCount { get; set; }
    public float TargetPosition { get; set; }
    public float Stroke => float.MaxValue;
    public SpeedRule SpeedMode { get; set; }
    public OperationMode ControlMode { get; set; }
    public BreakStatus Break { get; set; }

    private ArticulationBody _pArticulation;
    private int _nCurrFrame = 0;
    private ISpeedControl _pSpeedController;
    private float _fOperatedPos = 0.0F;

    public event MoterMoveCallback OnMoveEvent;
    public event MoterMoveCallback OnStopEvent;
    public event CollisionCallback OnCollisionEnterEvent;
    public event CollisionCallback OnCollisionLeaveEvent;

    // Start is called before the first frame update
    private void Start()
    {
        FrameCount = 0;
        SpeedMode = SpeedRule.Trapezoid;
        ControlMode = OperationMode.Auto;
        Break = BreakStatus.Hold;

        _pArticulation = GetComponent<ArticulationBody>();
        _fOperatedPos = CurrentPosition;
        _pSpeedController = SpeedMode switch
        {
            SpeedRule.None => new NormalControl(Time.fixedDeltaTime),
            SpeedRule.Trapezoid => new TrapezoidControl(Time.fixedDeltaTime),
            SpeedRule.Triangle => new TriangleControl(Time.fixedDeltaTime),
            _ => _pSpeedController
        };
    }

    public void SetLimit(float fNegLimit, float fPosLimit)
    {
        Debug.LogWarning("The stroke of the joint is infinite");
    }

    public void UpdateParameter()
    {
        _nCurrFrame = 0;
        if (_pSpeedController == null) return;
        _pSpeedController.SetPosition(CurrentPosition, TargetPosition);
        _pSpeedController.FrameCount = FrameCount;
        Break = BreakStatus.Release;
    }

    public void ForcedUpdate(float fPosition, float fOffset = 0.0F)
    {
        if (ControlMode != OperationMode.Forced) return;
        if (Break == BreakStatus.Hold)
        {
            _fOperatedPos = CurrentPosition;
            return;
        }

        ArticulationDrive pDrive = _pArticulation.xDrive;
        if (fOffset != 0.0F)
        {
            if (Mathf.Abs(fPosition - pDrive.target) > fOffset)
                fPosition = (fPosition > pDrive.target) ? fPosition + fOffset : fPosition - fOffset;
        }

        _fOperatedPos = fPosition;
        pDrive.target = _fOperatedPos;
        _pArticulation.xDrive = pDrive;
        Break = BreakStatus.Hold;
    }

    private void Update()
    {
        if (ControlMode != OperationMode.Teaching) return;
        if (Break == BreakStatus.Hold)
        {
            _fOperatedPos = CurrentPosition;
            return;
        }

        float fJogSpeed = 100.0F;
        _fOperatedPos += CommonFactory.GetInputKey(new[] {LeftArrow, UpArrow, RightArrow, DownArrow}) switch
        {
            LeftArrow => -fJogSpeed * Time.fixedDeltaTime / 5.0F,
            UpArrow => -fJogSpeed * Time.fixedDeltaTime,
            RightArrow => fJogSpeed * Time.fixedDeltaTime / 5.0F,
            DownArrow => fJogSpeed * Time.fixedDeltaTime,
            _ => 0.0F
        };
        Debug.Log($"[TEACHING] Index={Index} CurrentPos={CurrentPosition} TargetPos={_fOperatedPos} Speed={fJogSpeed}");
        ArticulationDrive pDrive = _pArticulation.xDrive;
        pDrive.target = _fOperatedPos;
        _pArticulation.xDrive = pDrive;
    }

    // Update is called on fixed frequency
    private void FixedUpdate()
    {
        if (ControlMode != OperationMode.Auto) return;
        if (Break == BreakStatus.Hold)
        {
            _fOperatedPos = CurrentPosition;
            return;
        }

        if (_nCurrFrame >= FrameCount)
        {
            OnStopEvent?.Invoke(this, new MoterEventArgs(0, 0.0F, CurrentPosition, CurrentPosition));
            Break = BreakStatus.Hold;
            return;
        }

        _fOperatedPos = _pSpeedController.GetPosition(_nCurrFrame);
        OnMoveEvent?.Invoke(this,
            new MoterEventArgs(_nCurrFrame, _pSpeedController.GetSpeed(_nCurrFrame), CurrentPosition,
                _fOperatedPos));
        ArticulationDrive pDrive = _pArticulation.xDrive;
        pDrive.target = _fOperatedPos;
        _pArticulation.xDrive = pDrive;
        _nCurrFrame += 1;
    }

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[COLLISION] Collision between {name} and {collision.gameObject.name}");
        OnCollisionEnterEvent?.Invoke(this, new CollisionEventArgs(name, collision.gameObject));
    }

    public void OnCollisionExit(Collision other)
    {
        OnCollisionLeaveEvent?.Invoke(this, new CollisionEventArgs(name, other.gameObject));
    }
}
