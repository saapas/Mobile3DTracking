using UnityEngine;
using UnityEngine.UI;

public class GyroHeading : MonoBehaviour
{
    public Text text;
    private Quaternion orientation; // Current orientation as a quaternion
    public Transform playerObject;

    private float compassHeading;
    private float previousTime;

    private float kalmanHeading;
    private float kalmanP;
    private float Q = 0.2f; // gyro drift
    private float R = 0.1f; // noise (compass)
    private float headingAfterOffset = 0.0f;
    private float headingOffset = 0.0f;

    void Start()
    {
        // Enable the gyroscope
        Input.gyro.enabled = true;
        orientation = Quaternion.identity; // Initialize orientation
        Input.compass.enabled = true;

        //Initialize Kalman filter
        kalmanHeading = 0;
        kalmanP = Q;
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

        if (pitch > 0.75f && Input.compass.timestamp > 0)
        {
            compassHeading = Input.compass.trueHeading;

            if(Time.time - previousTime > 6)
            {
                headingOffset = compassHeading;
                headingOffset = heading - headingOffset;
                previousTime = Time.time;
            }

            //Kalman filter update
            float prediction = headingAfterOffset;
            float predictionP = kalmanP + Q * Time.deltaTime;

            // Normalize the difference between compass heading and predicted heading
            float headingDifference = NormalizeAngle(compassHeading - prediction);

            float K = predictionP / (predictionP + R);
            kalmanHeading = prediction + K * headingDifference;
            kalmanP = (1 - K) * predictionP;
        }
        else
        {
            //switches to just heading
            kalmanHeading = headingAfterOffset;
            kalmanP = Q * Time.deltaTime;
        }

        // Apply rotation
        playerObject.rotation = Quaternion.Euler(0, kalmanHeading, 0);

        text.text = $"heading: {headingAfterOffset:F2}\n" +
                    $"pitch: {pitch:F2}\n" +
                    $"kalmanHeading: {kalmanHeading}\n" +
                    $"compassHeading: {compassHeading}";
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
