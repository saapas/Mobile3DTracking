using UnityEngine;

public class GyroHeading : MonoBehaviour
{
    private Quaternion orientation; // Current orientation as a quaternion
    public Transform playerObject;
    private Vector3 compass;

    void Start()
    {
        // Enable the gyroscope
        Input.gyro.enabled = true;
        orientation = Quaternion.identity; // Initialize orientation
        Input.compass.enabled = true;
    }

    void Update()
    {
        // Get gyroscope data (angular velocity in rad/s)
        Vector3 angularVelocity = Input.gyro.rotationRateUnbiased;

        compass = Input.compass.rawVector;

        // Update the quaternion orientation using gyroscope data
        UpdateOrientation(angularVelocity);

        // Calculate the heading from the quaternion
        float heading = CalculateHeading();

        playerObject.rotation = Quaternion.Euler(0, heading * Mathf.Rad2Deg, 0);

        Debug.Log("compass: " + compass);
    }

    void UpdateOrientation(Vector3 angularVelocity)
    {
        // Convert angular velocity to a quaternion
        Quaternion deltaRotation = Quaternion.Euler(angularVelocity * Mathf.Rad2Deg * Time.deltaTime);

        // Update the orientation
        orientation *= deltaRotation;
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

        return heading;
    }
}