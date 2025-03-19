using UnityEngine;
using UnityEngine.UI;

public class GyroHeading : MonoBehaviour
{
    public Text text;
    private Quaternion orientation; // Current orientation as a quaternion
    public Transform playerObject;

    private float kalmanHeading;
    private float kalmanP;
    private float Q = 0.03f; // gyro drift
    private float R = 0.4f; // noise (compass)
    private float headingOffset;

    void Start()
    {
        // Enable the gyroscope
        Input.gyro.enabled = true;
        orientation = Quaternion.identity; // Initialize orientation
        Input.compass.enabled = true;

        //Initialize Kalman filter
        kalmanHeading = 0;
        kalmanP = Q;

        // Store the initial compass heading as the offset
        headingOffset = Input.compass.trueHeading;
        if (headingOffset > 180) headingOffset -= 360; // Convert to -180 to 180 range
    }

    void Update()
    {
        // Get gyroscope data
        Quaternion attitude = Input.gyro.attitude;

        //Set offset based on compass heading
        attitude = attitude * Quaternion.Euler(0, headingOffset, 0);

        // Update the quaternion orientation using gyroscope data
        orientation = attitude;

        // Calculate the heading from the quaternion and make it match the compass value by making it negative
        float heading = -CalculateHeading();

        // Get "pitch" value to determine when to use compass (gravity vector)
        float pitch = Input.gyro.gravity.y;

        if (pitch > 0.75f && Input.compass.timestamp > 0)
        {
            float compassHeading = Input.compass.trueHeading;

            if (compassHeading > 180) compassHeading -= 360; // Convert to -180 to 180 range

            //Check if compass works
            Debug.Log(compassHeading);

            //Kalman filter update
            float prediction = kalmanHeading;
            float predictionP = kalmanP + Q * Time.deltaTime;

            float K = predictionP / (predictionP + R);
            kalmanHeading = prediction + K * (compassHeading - prediction);
            kalmanP = (1 - K) * predictionP;

            // Apply rotation
            playerObject.rotation = Quaternion.Euler(0, kalmanHeading, 0);
        }
        else
        {
            //switches to just heading
            kalmanHeading = heading;
            kalmanP = Q * Time.deltaTime;
        }

        text.text = $"heading: {heading:F2}\n" +
                    $"pitch: {pitch:F2}\n" +
                    $"kalmanHeading: {kalmanHeading}\n";
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
}
