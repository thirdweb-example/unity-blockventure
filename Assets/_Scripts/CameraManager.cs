using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform Camera;
    public Transform Target; // The target the camera will follow (e.g., the player)
    public Vector3 Offset; // Offset from the target position
    public float Sensitivity = 100f; // Sensitivity of mouse movement
    public float ClampAngle = 85f; // Maximum vertical angle the camera can move

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize rotations based on current camera rotation
        Vector3 angles = Camera.eulerAngles;
        verticalRotation = angles.x;
        horizontalRotation = angles.y;
    }

    public void UpdateCameraMode(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.MainMenu:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.Moving:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case GameState.Gathering:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case GameState.Shopping:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.Trading:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.GameOver:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameState.Moving)
            return;

        // Get mouse movement input
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;

        // Accumulate horizontal rotation and clamp vertical rotation
        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -ClampAngle, ClampAngle);

        // Apply rotation and position to the camera
        Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
        Camera.position = Target.position + rotation * Offset;
        Camera.LookAt(Target.position + Vector3.up * Offset.y);
    }
}
