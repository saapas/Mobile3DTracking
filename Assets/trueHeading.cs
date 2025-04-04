using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GyroHeading : MonoBehaviour
{
    public Text text;
    private Quaternion orientation; // Current orientation as a quaternion
    public Transform playerObject;

    private float compassHeading;
    List<float> compassBuffer = new List<float>();
    private float compassError;
    private float previousTime;
    private float bias = 0.001f;
    private float scaleErrorConst = 0.007f;
    private float scaleError;
    private float headingError;

    private float stateEstimateError;
    private float kalmanHeading;
    private float headingPrevEst;
    private float headingAfterOffset = 0.0f;
    private float headingOffset = 0.0f;

    void Start()
    {
        orientation = Quaternion.identity; // Initialize orientation
        Input.compass.enabled = true;

        //Initialize Kalman filter
        kalmanHeading = 0;
    }

    void Update()
    {
        // Get gyroscope data
        Quaternion attitude = Input.gyro.attitude;

        // Update the quaternion orientation using gyroscope data
        orientation = attitude;

        // Calculate the heading from the quaternion and make it match the compass value by making it negative
        float heading = -CalculateHeading();

        if (heading < 0) heading += 360;

        headingAfterOffset = heading - headingOffset;

        if (headingAfterOffset < 0) headingAfterOffset += 360;
        if (headingAfterOffset > 360) headingAfterOffset -= 360;

        // Get "pitch" value to determine when to use compass (gravity vector)
        float pitch = Input.gyro.gravity.y;

        compassHeading = Input.compass.trueHeading;

        if (compassBuffer.Count >= 20) compassBuffer.RemoveAt(0);
        compassBuffer.Add(compassHeading);

        compassError = EstimateCompassError();

        if (pitch < 0.75f) compassError = 10000;

        headingError = EstimateHeadingError();

        if (Time.time - previousTime > 10 && compassError < 2)
        {
            headingOffset = compassHeading;
            headingOffset = heading - headingOffset;
            previousTime = Time.time;
            scaleError = 0;
        }
        //Calculate Kalman Gain
        float kalmanG = headingError / (headingError + compassError);
        kalmanHeading = headingPrevEst + kalmanG * (NormalizeAngle(compassHeading - headingPrevEst));

        stateEstimateError = (1 - kalmanG) * (headingError);
        headingPrevEst = headingAfterOffset;
        // Apply rotation
        playerObject.rotation = Quaternion.Euler(0, kalmanHeading, 0);

        text.text = $"heading: {headingAfterOffset:F2}\n" +
                    $"pitch: {pitch:F2}\n" +
                    $"kalmanError: {stateEstimateError:F2}\n" +
                    $"compassError: {compassError:F2}\n" +
                    $"headingError: {headingError:F2}\n" +
                    $"kalmanHeading: {kalmanHeading}\n" +
                    $"compassHeading: {compassHeading}";
    }
    float EstimateCompassError()
    {
        float mean = compassBuffer.Average();

        float sumOfSquares = compassBuffer.Sum(x => (x - mean) * (x - mean));
        compassError = sumOfSquares / compassBuffer.Count();
        return compassError;
    }
    float EstimateHeadingError()
    {
        scaleError += scaleErrorConst * Input.gyro.rotationRateUnbiased.magnitude;
        headingError = (bias * (Time.time - previousTime)) + scaleError;
        return headingError;
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
