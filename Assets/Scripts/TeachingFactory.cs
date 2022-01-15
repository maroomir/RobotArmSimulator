using System;

public interface ITeachingPosition
{
    public int AxisNum { get; }
    public float MaxSpeed { get; set; }
    public int MaxStep { get; set; }
    public SyncRule SyncMode { get; set; }
    public float[] Position { get; }
}

public class JointPosition : ITeachingPosition
{
    public int AxisNum { get; private set; }

    public float MaxSpeed
    {
        get => _fMaxSpeed;
        set
        {
            SyncMode = SyncRule.SpeedSync;
            _fMaxSpeed = value;
        }
    }

    public int MaxStep
    {
        get => _fMaxStep;
        set
        {
            SyncMode = SyncRule.StepSync;
            _fMaxStep = value;
        }
    }

    public SyncRule SyncMode { get; set; }
    public float[] Position { get; private set; }

    private float _fMaxSpeed = 100.0F;
    private int _fMaxStep = 20;

    public JointPosition()
    {
        // 
    }

    public static JointPosition FromPosition(params float[] args) => new JointPosition
    {
        SyncMode = SyncRule.StepSync,
        MaxStep = 100,
        Position = args.Clone() as float[],
        AxisNum = args.Length,
    };
}