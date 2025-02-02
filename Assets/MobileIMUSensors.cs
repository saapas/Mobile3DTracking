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
    private Vector3 velocity = Vector3.zero; // Initialize velocity
    private Vector3 position = Vector3.zero; // Initialize position
    public float waitTime = 5f; // Time to wait before starting Update

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
            if (Input.gyro.enabled)
            {
                // Get the gyroscope attitude (rotation)
                Quaternion gyroAttitude = Input.gyro.attitude;

                // Apply an additional rotation to make x=0 correspond to 90 degrees in Unity
                Quaternion rotationOffset = Quaternion.Euler(90, 0, 0);
                gyroAttitude = rotationOffset * gyroAttitude;

                // Convert the adjusted attitude to Euler angles
                Vector3 eulerRotation = gyroAttitude.eulerAngles;

                // Apply the rotation to the Phone transform
                transform.rotation = Quaternion.Euler(eulerRotation);

                //Display the rotation in the UI Text
                rotationText.text = "Rotation (Euler Angles):\n" +
                                    "X: " + eulerRotation.x.ToString("F2") + "\n" +
                                    "Y: " + eulerRotation.y.ToString("F2") + "\n" +
                                    "Z: " + eulerRotation.z.ToString("F2") + "\n" +
                                    "W: " + gyroAttitude.w.ToString("F2");

                
                Vector3 gravity = Input.gyro.gravity; // Get the gravity vector

                // Display the gravity vector in the UI Text
                rotationText.text += "\nGravity Vector:\n" +
                                    "X: " + gravity.x.ToString("F2") + "\n" +
                                    "Y: " + gravity.y.ToString("F2") + "\n" +
                                    "Z: " + gravity.z.ToString("F2");

                float threshold = 0.04f; // Sensor noise threshold
                float dampingFactor = 0.9f; // Damping factor to reduce velocity over time
                float resetThreshold = 0.01f; // Threshold to reset velocity

                Vector3 acceleration = Input.acceleration; // Get the linear acceleration

                acceleration.y = Math.Abs(acceleration.y) < 0.1f ? 0 : acceleration.y;  // Remove noise from the y-axis

                Vector3 linearAcceleration = acceleration - gravity; // Remove gravity from acceleration

                linearAcceleration = linearAcceleration * movementSpeed;

                // Apply the threshold to remove noise
                linearAcceleration.x = Mathf.Abs(linearAcceleration.x) < threshold ? 0 : linearAcceleration.x;
                linearAcceleration.y = Mathf.Abs(linearAcceleration.y) < threshold ? 0 : linearAcceleration.y;
                linearAcceleration.z = Mathf.Abs(linearAcceleration.z) < threshold ? 0 : linearAcceleration.z;

                // Integrate acceleration to get velocity
                velocity += linearAcceleration * Time.deltaTime;

                // Apply damping to velocity
                velocity *= dampingFactor;

                // Reset velocity if it's below the reset threshold
                if (velocity.magnitude < resetThreshold)
                {
                velocity = Vector3.zero;
                }

                // Integrate velocity to get position
                position += velocity * Time.deltaTime;

                // Update the object's position
                transform.position = position *100;

                rotationText.text += "\n Position:\n" +
                                     "X: " + position.x.ToString("F2") + "\n" +
                                     "Y: " + position.y.ToString("F2") + "\n" +
                                     "Z: " + position.z.ToString("F2");

                // Display the linear velocity in the UI Text
                rotationText.text += "\nLinear Velocity:\n" + velocity.ToString("F2");
            }

            yield return null; // Wait for the next frame
        }
    }
}
