using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class IMUDataUpdater : MonoBehaviour
{
    public Text sensorDataText; // Reference to the Text UI element

    void Start()
    {
        // Enable the accelerometer
        Input.gyro.enabled = true;
    }

    void Update()
    {
        // Get accelerometer data
        Vector3 accelerometerData = Input.acceleration;

        // Get gyroscope data
        Vector3 gyroscopeData = Input.gyro.rotationRate;

        // Update the text with sensor data
        if (sensorDataText != null)
        {
            sensorDataText.text = $"Accelerometer:\nX: {accelerometerData.x:F2}\nY: {accelerometerData.y:F2}\nZ: {accelerometerData.z:F2}\n\n" +
                                  $"Gyroscope:\nX: {gyroscopeData.x:F2}\nY: {gyroscopeData.y:F2}\nZ: {gyroscopeData.z:F2}";
        }
    }
}