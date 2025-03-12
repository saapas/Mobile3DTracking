using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    public Text testText;
    // Complementary filter parameters
    private float alpha = 0.9f; // Weight for gyroscope data
    public float fusedPitch = 0.0f; // Fused pitch value

    // Step detection parameters
    private float pitchThreshold = 3f; // Threshold for detecting steps
    private float absPitch = 0.0f; // Absolute pitch value
    private float stepTime = 0.75f; // Time interval between steps
    private float stepLenght = 0.7f; // Step length, temporary value will be calculated based on the pitch value
    private float lastPitch = 0.0f;
    private bool isStepDetected = false;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Time interval between steps, is updated in code
    public static float smoothedPitch = 0.0f; // Smoothed pitch value
    private int stepType = 0; // 0 for flat, 1 for down and 2 for up
    private Vector3 acceleration; // Accelerometer data

    void Start()
    {
        Input.gyro.enabled = true;
        Input.compass.enabled = true; // Enable the compass, not used yet
    }

    void Update()
    {
        // Get accelerometer data
        acceleration = Input.gyro.userAcceleration;

        // Calculate pitch from accelerometer (using trigonometry)
        float accelerometerPitch = Mathf.Atan2(-acceleration.z, Mathf.Sqrt(acceleration.y * acceleration.y + acceleration.x * acceleration.x));

        // Get gyroscope data (rotation rate in rad/s)
        float gyroPitch = -Input.gyro.rotationRateUnbiased.x;

        // Apply the complementary filter
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;

        // Smooth the pitch value
        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        testText.text = "Fused Pitch: " + smoothedPitch.ToString("F2") + 
        "\n" + "gyroPitchDelta: " + gyroPitch.ToString("F2") + "\n" + 
        "accelerometerPitch: " + accelerometerPitch.ToString("F2")+ "\n" +
        "stepCount: " + stepCount + 
        "\n" + "stepType: " + stepType +
        "\n" + "stepLenght: " + stepLenght;

        // Detect steps, acceleration magnitude is not yet used
        DetectSteps(smoothedPitch, acceleration.magnitude);
    }

    void DetectSteps(float pitch, float accMagnitude)
    {
        absPitch = Mathf.Abs(pitch); // Get the absolute pitch value

        // Check if the pitch value crosses the threshold, is less than the last pitch value (so is likely a peak) and the time interval has passed
        if (absPitch > pitchThreshold && absPitch <= lastPitch && Time.time > stepInterval)
        {
            if (!isStepDetected)
            {
                stepCount++;
                stepInterval = Time.time + stepTime; // Reset the time interval
                isStepDetected = true;
                // Determine the step type based on the pitch value
                if (pitch > 8)
                {
                    stepType = 2; // Up
                }
                else if (pitch < 4.5f)
                {
                    stepType = 1; // Down
                }
                else
                {
                    stepType = 0; // Flat
                }
                MoveObject(stepLenght, stepType);
                Debug.Log("Step count: " + stepCount);
                Debug.Log("Pitch: " + pitch);
                Debug.Log("Step Type: " + stepType);
                Debug.Log("Step Length: " + stepLenght);
            }
        }
        else
        {
            isStepDetected = false;
        }

        // Update the last pitch value
        lastPitch = absPitch;
    }

    private void MoveObject(float stepLenght, int stepType)
    {
        // THIS WILL NOT BE IMPLEMENTED THIS WAY, THIS IS JUST A PLACEHOLDER
        Vector3 heading = acceleration;
        // Move the object based on the step type
        switch (stepType)
        {
            case 0:
                transform.Translate(heading.x * stepLenght, 0.0f, heading.z * stepLenght);
                break;
            case 1:
                transform.Translate(heading.x * stepLenght, 0.5f, heading.z * stepLenght);
                break;
            case 2:
                transform.Translate(heading.x * stepLenght, -0.5f, heading.z * stepLenght);
                break;
        }
    }
}