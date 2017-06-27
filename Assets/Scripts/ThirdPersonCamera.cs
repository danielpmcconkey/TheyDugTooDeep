using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    public float mouseSensitivity = 10;
    public Transform target;
    public float distanceFromTarget = 5;
    public float distanceAboveTarget = 2;
    public Vector2 pitchMixMax = new Vector2(-40, 85);
    public float rotationSmoothTime = 0.12f;
    public float pitchDefault = -23;
    public bool lockCursor = true;
    public float turnIncrement = 1f;

    private float _yaw;
    private float _pitch;
    private Vector3 _rotationSmoothVelocity;
    private Vector3 _currentRotation;

    private void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate() {
        // floating camera when right control is held down
        // todo: use an input manager instead of right control to put the camera in float mode
        if (Input.GetKey(KeyCode.RightControl))
        {
            _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, pitchMixMax.x, pitchMixMax.y);

        }
        else snapCameraRotation();

        // todo: use an input manager instead of Q and E for camera swivel
        if (Input.GetKey(KeyCode.Q)) // swivel camera 90 degrees to the left 
        {
            _yaw -= turnIncrement;
            //snapYawToCardinalDirection();
        }
        if (Input.GetKey(KeyCode.E)) // swivel camera 90 degrees to the right
        {
            _yaw += turnIncrement;
            //snapYawToCardinalDirection();
        }

        _currentRotation = Vector3.SmoothDamp(_currentRotation, new Vector3(_pitch, _yaw), ref _rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = _currentRotation;
        // finally, move the camera w/ the target
        transform.position = target.position - transform.forward * distanceFromTarget;
        transform.position = new Vector3(transform.position.x, target.position.y + distanceAboveTarget, transform.position.z);
	}

    private void snapCameraRotation()
    {
        //_yaw = (Mathf.RoundToInt(_yaw / 90)) * 90;
        _pitch = pitchDefault;
        //if (_yaw > 360) _yaw -= 360;
        //if (_yaw < 0) _yaw += 360;
    }
}
