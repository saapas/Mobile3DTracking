using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class PhoneRotation : MonoBehaviour
{
    public Text rotationText;
    private Vector3 velocity = Vector3.zero; // Initialize velocity
    private Vector3 displacement = Vector3.zero; // Initialize position
    private Vector3 prevVelocity = Vector3.zero; // Initialize previous velocity
    public float waitTime = 1f; // Time to wait before starting Update
    private Vector3 prevLinearAcceleration = Vector3.zero; // Initialize previous linear acceleration

    void Start()
    {
        // Enable the gyroscope and compass
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device.");
        }

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
            if (Input.gyro.enabled)
            {
                // Get the gyroscope attitude (rotation)
                Quaternion gyroAttitude = Input.gyro.attitude;

                // Apply an additional rotation to make x=0 correspond to 90 degrees in Unity
                Quaternion rotationOffset = Quaternion.Euler(90, 0, 0);
                gyroAttitude = rotationOffset * gyroAttitude;

                // Get the compass heading (magnetic north)
                float compassHeading = Input.compass.trueHeading; // Use trueHeading for geographic north
                Quaternion compassRotation = Quaternion.Euler(0, compassHeading, 0);

                // Combine gyroscope and compass rotations
                Quaternion trueRotation = gyroAttitude * compassRotation;

                // Smooth the rotation
                float smoothingFactor = 0.1f;
                Quaternion smoothedGyroAttitude = Quaternion.Slerp(transform.rotation, trueRotation, smoothingFactor);
                transform.rotation = smoothedGyroAttitude;

                // Get the Euler angles
                Vector3 eulerRotation = smoothedGyroAttitude.eulerAngles;

                //Display the rotation in the UI Text
                rotationText.text = "Rotation:\n" + eulerRotation.ToString("F2");

                // Get the gravity, acceleration and deltatime
                Vector3 gravity = Input.gyro.gravity;
                Vector3 acceleration = Input.acceleration;
                float time = Time.deltaTime;

                // Calculate the linear acceleration
                Vector3 linearAcceleration = acceleration - gravity;

                // Remove small values
                float threshold = 0.05f;
                linearAcceleration.x = Mathf.Abs(linearAcceleration.x) < threshold ? 0 : linearAcceleration.x;
                linearAcceleration.y = Mathf.Abs(linearAcceleration.y) < threshold ? 0 : linearAcceleration.y;
                linearAcceleration.z = Mathf.Abs(linearAcceleration.z) < threshold ? 0 : linearAcceleration.z;

                // Amplify acceleration for better sensitivity
                float accelerationInMS2 = 98.1f;
                linearAcceleration *= accelerationInMS2;

                // Integrate acceleration to get velocity using the trapezoidal rule
                velocity = prevVelocity + (linearAcceleration + prevLinearAcceleration) / 2 * time;

                // Apply damping to the velocity
                float dampingFactor = 0.95f;
                velocity *= dampingFactor;

                // Reset velocity if it's too small
                float resetThreshold = 0.1f;
                if (velocity.magnitude < resetThreshold)
                {
                    velocity = Vector3.zero;
                }

                // Calculate the displacement using the trapezoidal rule
                displacement = (velocity + prevVelocity) / 2 * time;

                // Translate the object using the displacement
                transform.Translate(displacement);

                // Update the previous values
                prevVelocity = velocity;
                prevLinearAcceleration = linearAcceleration;

                // Display gravity
                rotationText.text += "\nPosition:\n" + transform.position.ToString("F2");

                // Display linear acceleration
                rotationText.text += "\nLinear Acceleration:\n" + linearAcceleration.ToString("F2");

                // Display velocity
                rotationText.text += "\nLinear Velocity:\n" + velocity.ToString("F2");

                // Display position
                rotationText.text += "\nDisplacement:\n" + displacement.ToString("F2");
            }

            yield return null; // Wait for the next frame
        }
    }
}
