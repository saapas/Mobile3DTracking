using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TrueHeading : MonoBehaviour
{
    public Text text;
    private Quaternion orientation; // Current orientation as a quaternion
    public Transform playerObject;

    private float compassHeading = 0.0f;
    List<float> compassBuffer = new List<float>();
    private float compassError = 0.0f;
    private float predictedHeading = 0.0f;

    private float stateEstimateError = 1.0f;
    private float kalmanHeading = 0.0f;
    private float gyroDeltaHeading= 0.0f;
    private float gyroHeadingPrev = 0.0f;

    void Start()
    {
        orientation = Quaternion.identity; // Initialize orientation
        Input.compass.enabled = true;

        //Initialize Kalman filter
        kalmanHeading = 0;
    }

    void Update()
    {
        // -- Gyro Prediction Step --
        // Get gyroscope data
        Quaternion attitude = Input.gyro.attitude;

        // Update the quaternion orientation using gyroscope data
        orientation = attitude;

        // Calculate the heading from the quaternion and make it match the compass value by making it negative
        float gyroHeading = -CalculateHeading();

        // Add gyro error to state error
        stateEstimateError += Time.deltaTime;

        // Compute delta from previous heading
        gyroDeltaHeading = gyroHeading - gyroHeadingPrev;

        // Predict state using gyro (integrate delta)
        predictedHeading = kalmanHeading + gyroDeltaHeading;
        if (predictedHeading < 0) predictedHeading += 360;
        if (predictedHeading > 360) predictedHeading -= 360;

        // -- Compass Update Step --
        // Get compass data
        compassHeading = Input.compass.trueHeading;

        // Get "pitch" value to determine when to use compass (gravity vector)
        float pitch = Input.gyro.gravity.y;
        if (pitch < 0.75f) compassError = 10000;
        else compassError = EstimateCompassError();

        // Calculate Kalman Gain
        float kalmanG = stateEstimateError / (stateEstimateError + compassError);

        // Calculate kalman heading based on prediction from gyroscope, kalman gain and the compass heading
        kalmanHeading = predictedHeading + kalmanG * (NormalizeAngle(compassHeading - predictedHeading));

        // Calculating the kalman error
        stateEstimateError = (1 - kalmanG) * stateEstimateError;

        // set used gyro heading to previous heading
        gyroHeadingPrev = gyroHeading;

        // Apply rotation
        playerObject.rotation = Quaternion.Euler(0, kalmanHeading, 0);

        text.text = $"heading: {gyroHeading:F2}\n" +
                    $"pitch: {pitch:F2}\n" +
                    $"kalmanError: {stateEstimateError:F2}\n" +
                    $"compassError: {compassError:F2}\n" +
                    $"predictedHeading: {predictedHeading:F2}\n" +
                    $"kalmanHeading: {kalmanHeading}\n" +
                    $"compassHeading: {compassHeading}";
    }
    float EstimateCompassError()
    {
        // Adding new values to CompassBuffer, removing when count over 20
        if (compassBuffer.Count >= 20) compassBuffer.RemoveAt(0);
        compassBuffer.Add(compassHeading);

        if (compassBuffer.Count() < 2) return 1.0f; // Default Error

        // Estimating error based on variance of the latest values
        float mean = compassBuffer.Average();
        float sumOfSquares = compassBuffer.Sum(x => (x - mean) * (x - mean));
        float variance = sumOfSquares / compassBuffer.Count();

        return variance + 0.01f;
    }

    float CalculateHeading()
    {
        // Extract quaternion components
        float q0 = orientation.w;
        float q1 = orientation.x;
        float q2 = orientation.y;
        float q3 = orientation.z;

        // Calculate the heading using the quaternion
        float heading = Mathf.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (q2 * q2 + q3 * q3));

        return heading * Mathf.Rad2Deg;
    }
    // Helper function to normalize angles to the range [-180, 180]
    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}
