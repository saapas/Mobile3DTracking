using UnityEngine;
using System.Collections;


public class PhoneRotation : MonoBehaviour // Class is not finished yet, has now unnecessary variables and methods that may be used in the future
{
    private Vector3 velocity = Vector3.zero; // Initialize velocity
    private Vector3 displacement = Vector3.zero; // Initialize position
    private Vector3 prevVelocity = Vector3.zero; // Initialize previous velocity
    public float waitTime = 1f; // Time to wait before starting Update
    private Vector3 prevLinearAcceleration = Vector3.zero; // Initialize previous linear acceleration

    void Start()
    {
        Input.gyro.enabled = true;
        Input.compass.enabled = true;
        // Start the coroutine to wait before updating
        StartCoroutine(WaitBeforeUpdate());
    }

    IEnumerator WaitBeforeUpdate()
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            // Get the gyroscope attitude (rotation)
            Quaternion gyroAttitude = Input.gyro.attitude;

            // Apply an additional rotation to make x=0 correspond to 90 degrees in Unity
            Quaternion rotationOffset = Quaternion.Euler(90, 0, 0);
            gyroAttitude = rotationOffset * gyroAttitude;

            // Smooth the rotation
            float smoothingFactor = 0.1f;
            Quaternion smoothedGyroAttitude = Quaternion.Slerp(transform.rotation, gyroAttitude, smoothingFactor);
            transform.rotation = smoothedGyroAttitude;
            yield return null; // Wait for the next frame
        }
    }
}
