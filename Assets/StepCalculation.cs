using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    public Text testText;
    // Complementary filter parameters
    private float alpha = 0.9f; // Weight for gyroscope data
    private float fusedPitch = 0.0f; // Fused pitch value
    private float fusedPitchMagnitude = 0.0f; // Fused pitch magnitude

    // Step detection parameters
    private float pitchThreshold = 25f; // Threshold for detecting steps
    private float lastPitch = 0.0f;
    private bool isStepDetected = false;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Time interval between steps
    private float smoothedPitch = 0.0f;

    void Start()
    {
        Input.gyro.enabled = true;
    }

    void Update()
    {
        // Get accelerometer data
        Vector3 acceleration = Input.gyro.userAcceleration;

        // Calculate pitch from accelerometer (using trigonometry)
        float accelerometerPitch = Mathf.Atan2(-acceleration.x, Mathf.Sqrt(acceleration.y * acceleration.y + acceleration.z * acceleration.z));

        // Get gyroscope data (rotation rate in rad/s)
        float gyroPitch = -Input.gyro.rotationRateUnbiased.z;

        // Apply the complementary filter
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;

        fusedPitchMagnitude = Mathf.Abs(fusedPitch);

        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitchMagnitude, 0.1f);

        testText.text = "Fused Pitch: " + smoothedPitch.ToString("F2") + 
        "\n" + "gyroPitchDelta: " + gyroPitch.ToString("F2") + "\n" + 
        "accelerometerPitch: " + accelerometerPitch.ToString("F2")+ "\n" +
        "stepCount: " + stepCount;
                        

        // Use the fused pitch value for step detection
        DetectSteps(smoothedPitch);
    }

    void DetectSteps(float pitch)
    {
        // Check if the pitch and magnitude value crosses the threshold, is less than the last pitch value and the time interval has passed
        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            Debug.Log("Pitch: " + pitch);
            if (!isStepDetected)
            {
                stepCount++;
                stepInterval = Time.time + 0.5f; // Reset the time interval
                isStepDetected = true;
                Debug.Log("Step count: " + stepCount);
                Debug.Log("Pitch: " + pitch);
            }
        }
        else
        {
            isStepDetected = false;
        }

        // Update the last pitch value
        lastPitch = pitch;
    }
}