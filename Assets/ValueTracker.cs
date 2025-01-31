using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class IMUDataUpdater : MonoBehaviour
{
    public Text sensorDataText; // Reference to the Text UI element

    void Start()
    {
        // Enable the gyroscope
        Input.gyro.enabled = true;
    }

    void Update()
    {
        // Get gravity data
        Vector3 gravity = Input.gyro.gravity;
        
        // Get acceleration data
        Vector3 acceleration = Input.gyro.userAcceleration;

        // Get gyroscope data
        Vector3 gyroscopeData = Input.gyro.rotationRate;

        // Update the text with sensor data
        if (sensorDataText != null)
        {
            sensorDataText.text = $"Gravity: " + gravity + "\n" +
                                  $"Acceleration: " + acceleration + "\n" +
                                  $"Gyroscope:\nX: {gyroscopeData.x:F2}\nY: {gyroscopeData.y:F2}\nZ: {gyroscopeData.z:F2}";
        }
    }
}
