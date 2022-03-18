using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForcedDirector : MonoBehaviour
{
    public GameObject robot;
    public GameObject panel;
    public GameObject logger;

    private List<TMP_InputField> _pListJointFields;
    private RobotController _pRobotControl;
    private TextMeshProUGUI _pDisplayLog;
    private JointPoint _pForcedPoint;
    
    // Start is called before the first frame update
    void Start()
    {
        _pRobotControl = robot.GetComponent<RobotController>();
        _pListJointFields = new List<TMP_InputField>();
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            GameObject pObject = panel.transform.GetChild(i).gameObject;
            TMP_InputField pField = pObject.GetComponent<TMP_InputField>();
            if (pField != null)
            {
                pField.text = "0.00";  // Set the default texts
                _pListJointFields.Add(pField);
            }
        }
        _pListJointFields.Sort(delegate(TMP_InputField pX, TMP_InputField pY)
        {
            string strOrderX = pX.name.Substring(pX.name.Length - 1);
            string strOrderY = pX.name.Substring(pY.name.Length - 1);
            return Convert.ToInt16(strOrderX).CompareTo(Convert.ToInt16(strOrderY));
        });
        _pDisplayLog = logger.GetComponent<TextMeshProUGUI>();
        _pDisplayLog.text = "Run Forced Director";
        panel.SetActive(true);
        // Init Events
        GameObject pObjectButton = panel.transform.Find("ButtonAdjust").gameObject;
        Button pButtonAdjust = pObjectButton.GetComponent<Button>();
        pButtonAdjust.onClick.AddListener(() => OnPointAdjustEvent(this, EventArgs.Empty));
    }

    private void OnPointAdjustEvent(object sender, EventArgs e)
    {
        float[] pTargetPos = new float[_pListJointFields.Count];
        for (int i = 0; i < _pListJointFields.Count; i++)
        {
            string strTarget = _pListJointFields[i].text;
            pTargetPos[i] = float.Parse(strTarget);
        }

        _pForcedPoint = JointPoint.FromPosition("forced", pTargetPos);
        Debug.Log(_pForcedPoint.Print());
        _pDisplayLog.text = $"Move Forced Position [{_pForcedPoint.Print()}]";
        _pRobotControl.ForcedMove(_pForcedPoint);
    }
}
