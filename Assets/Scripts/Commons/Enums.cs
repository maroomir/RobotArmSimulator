
public enum OperationMode
{
    Teaching = 0,
    Auto
}

public enum SpeedRule
{
    None = 0,
    Triangle,
    Trapezoid,
}

public enum SyncRule
{
    Async = 0,
    FrameSync,
}

public enum BreakStatus
{
    Release = 0,
    Hold,
}

public enum FingerStatus
{
    Fixed = -1,
    Open = 0,
    Closed = 1,
}