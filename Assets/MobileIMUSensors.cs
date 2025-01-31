using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class PhoneRotation : MonoBehaviour
{
    Rigidbody rb;
    public float movementSpeed = 3f; // Speed multiplier for Rigidbody movement
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
                                "Z: " + eulerRotation.z.ToString("F2") + "\n" +
                                "W: " + adjustedAttitude.w.ToString("F2");
            
            Vector3 gravity = Input.gyro.gravity; // Get the gravity vector

            // Display the gravity vector in the UI Text
            rotationText.text += "\nGravity Vector:\n" +
                                 "X: " + gravity.x.ToString("F2") + "\n" +
                                 "Y: " + gravity.y.ToString("F2") + "\n" +
                                 "Z: " + gravity.z.ToString("F2");

            float threshold = 0.08f; // Sensor noise threshold

            Vector3 acceleration = Input.acceleration; // Get the linear acceleration

            acceleration.y = Math.Abs(acceleration.y) < 0.3f ? 0 : acceleration.y;  // Remove noise from the y-axis

            Vector3 linearVelocity = acceleration - gravity; // Remove gravity from acceleration

            linearVelocity.x = Mathf.Abs(linearVelocity.x) < threshold ? 0 : linearVelocity.x;
            linearVelocity.y = Mathf.Abs(linearVelocity.y) < threshold ? 0 : linearVelocity.y;
            linearVelocity.z = Mathf.Abs(linearVelocity.z) < threshold ? 0 : linearVelocity.z;


            // Display the linear velocity in the UI Text
            rotationText.text += "\nLinear Velocity:\n" +
                                 "X: " + linearVelocity.x.ToString("F2") + "\n" +
                                 "Y: " + linearVelocity.y.ToString("F2") + "\n" +
                                 "Z: " + linearVelocity.z.ToString("F2");

            rb.linearVelocity = linearVelocity * movementSpeed;
        }
    }
}
