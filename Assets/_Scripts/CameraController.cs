using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 2f;

    [SerializeField] private Transform _HardLockToPlayer;

    private Vector2 _mouseInput;

    private float _pitch;

    float customRate = 0.6f; // 10 times per second
    float timer = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _sensitivity = _sensitivity * 0.022f;
    }


    private void LateUpdate()
    {

        transform.position = _HardLockToPlayer.position;
        transform.Rotate(Vector3.up, _mouseInput.x * _sensitivity * Time.deltaTime);
        _pitch -= _mouseInput.y * _sensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, -70f, 70f);

        transform.localEulerAngles = new Vector3(_pitch, transform.localEulerAngles.y, 0f);
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        _mouseInput = context.ReadValue<Vector2>();
    }
}
