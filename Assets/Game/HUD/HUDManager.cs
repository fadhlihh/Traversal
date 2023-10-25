using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _climbKeyInfo;
    [SerializeField]
    private GameObject _glideKeyInfo;
    [SerializeField]
    private GameObject _cancelKeyInfo;
    [SerializeField]
    private TMP_Text _cancelKeyInfoText;
    public void ShowClimbKeyInfo()
    {
        _climbKeyInfo.SetActive(true);
    }

    public void HideClimbKeyInfo()
    {
        _climbKeyInfo.SetActive(false);
    }


    public void ShowGlideKeyInfo()
    {
        _glideKeyInfo.SetActive(true);
    }

    public void HideGlideKeyInfo()
    {
        _glideKeyInfo.SetActive(false);
    }

    public void ShowCancelKeyInfo(string value)
    {
        _cancelKeyInfoText.text = $"Cancel {value}";
        _cancelKeyInfo.SetActive(true);
    }

    public void HideCancelKeyInfo()
    {
        _cancelKeyInfo.SetActive(false);
    }
}
