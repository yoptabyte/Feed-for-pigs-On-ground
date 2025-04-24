using UnityEngine;
using Unity.Cinemachine; 

public class CameraSwitcher : MonoBehaviour
{
  public CinemachineCamera frontCamera;
  public CinemachineCamera backCamera;

  private bool isFrontCameraActive = false; 

  void Start()
  {
    UpdateCameraPriorities();
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.C))
    {
      isFrontCameraActive = !isFrontCameraActive;
      UpdateCameraPriorities();
    }
  }

  void UpdateCameraPriorities()
  {
    if (isFrontCameraActive)
    {
      frontCamera.Priority = 10;
      backCamera.Priority = 0;
    }
    else
    {
      frontCamera.Priority = 0;
      backCamera.Priority = 10;
    }
  }
}
