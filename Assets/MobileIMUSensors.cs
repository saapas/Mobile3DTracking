using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PhoneRotation : MonoBehaviour
{
    Rigidbody rb;
    public float movementSpeed = 10f; // Speed multiplier for Rigidbody movement
    public float tiltSensitivity = 2f;
    public float gravityValue = 1.03f;
    public Text rotationText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Enable the gyroscope and compass
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device.");
        }
        try
        {
            Input.compass.enabled = true; // Attempt to enable the compass
            Debug.Log("Compass and Magnetic Field Sensor enabled.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Compass and Magnetic Field Sensor not supported on this device: " + e.Message);
        }
    }

    void Update()
    {
        if (Input.gyro.enabled)
        {
            // Get the gyroscope attitude (rotation)
            Quaternion gyroAttitude = Input.gyro.attitude;

            // Adjust the gyroscope attitude to align with Unity's coordinate system
            Quaternion adjustedAttitude = new Quaternion(gyroAttitude.x, gyroAttitude.y, -gyroAttitude.z, -gyroAttitude.w);

            // Apply an additional rotation to make x=0 correspond to 90 degrees in Unity
            Quaternion rotationOffset = Quaternion.Euler(90, 0, 0);
            adjustedAttitude = rotationOffset * adjustedAttitude;

            // Convert the adjusted attitude to Euler angles for easier understanding
            Vector3 eulerRotation = adjustedAttitude.eulerAngles;

            // Apply the rotation to the Rigidbody
            rb.rotation = Quaternion.Euler(eulerRotation);
            //Display the rotation in the UI Text
            rotationText.text = "Rotation (Euler Angles):\n" +
                                "X: " + eulerRotation.x.ToString("F2") + "\n" +
                                "Y: " + eulerRotation.y.ToString("F2") + "\n" +
                                "Z: " + eulerRotation.z.ToString("F2");
        }

        Vector3 gravityDirection = Input.acceleration.normalized;
        
    }
}