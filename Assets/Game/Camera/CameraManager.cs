using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum CameraState
{
    ThirdPerson,
    FirstPerson
}

public class CameraManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    private InputManager _input;

    [Header("Camera")]
    [SerializeField]
    public CameraState CameraState;
    [SerializeField]
    private CinemachineFreeLook _tpsCamera;
    [SerializeField]
    private CinemachineVirtualCamera _fpsCamera;

    public void SetFPSClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        if (isClamped)
        {
            _fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_Wrap = false;
            _fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MinValue = playerRotation.y - 45;
            _fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxValue = playerRotation.y + 45;
        }
        else
        {
            _fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MinValue = -180;
            _fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxValue = 180;
            _fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_Wrap = true;
        }
    }

    public void SetTPSFieldOfView(float fieldOfView)
    {
        _tpsCamera.m_Lens.FieldOfView = fieldOfView;
    }

    private void Start()
    {
        _input.OnPOVInput += SwitchCamera;
    }

    private void OnDestroy()
    {
        _input.OnPOVInput -= SwitchCamera;
    }

    private void SwitchCamera()
    {
        if (CameraState == CameraState.ThirdPerson)
        {
            CameraState = CameraState.FirstPerson;
            _tpsCamera.gameObject.SetActive(false);
            _fpsCamera.gameObject.SetActive(true);
        }
        else
        {
            CameraState = CameraState.ThirdPerson;
            _tpsCamera.gameObject.SetActive(true);
            _fpsCamera.gameObject.SetActive(false);
        }
    }
}
