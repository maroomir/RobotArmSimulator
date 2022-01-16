using UnityEngine;

public class FingerController : MonoBehaviour
{
    public float closedLength;
    
    private Vector3 _pOpenPosition;
    private ArticulationBody _pArticulation;

    private void Start()
    {
        _pArticulation = GetComponent<ArticulationBody>();
    }
}