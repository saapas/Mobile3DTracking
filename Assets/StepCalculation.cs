using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    public Text testText;
    // Complementary filter parameters
    private float alpha = 0.9f; // Weight for gyroscope data
    public float fusedPitch = 0.0f; // Fused pitch value

    // Step detection parameters
    private float pitchThreshold = 17f; // Threshold for detecting steps
    private float absPitch = 0.0f; // Absolute pitch value
    private float stepTime = 0.75f; // Time interval between steps
    private float stepLenght = 0.0f; // Step length
    private float lastPitch = 0.0f;
    private bool isStepDetected = false;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Time interval between steps
    public static float smoothedPitch = 0.0f;
    private int stepType = 0; // 0 for flat, 1 for down and 2 for up
    private Vector3 acceleration;

    void Start()
    {
        Input.gyro.enabled = true;
    }

    void Update()
    {
        // Get accelerometer data
        acceleration = Input.gyro.userAcceleration;

        // Calculate pitch from accelerometer (using trigonometry)
        float accelerometerPitch = Mathf.Atan2(-acceleration.x, Mathf.Sqrt(acceleration.y * acceleration.y + acceleration.z * acceleration.z));

        // Get gyroscope data (rotation rate in rad/s)
        float gyroPitch = -Input.gyro.rotationRateUnbiased.z;

        // Apply the complementary filter
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;

        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        testText.text = "Fused Pitch: " + smoothedPitch.ToString("F2") + 
        "\n" + "gyroPitchDelta: " + gyroPitch.ToString("F2") + "\n" + 
        "accelerometerPitch: " + accelerometerPitch.ToString("F2")+ "\n" +
        "stepCount: " + stepCount + 
        "\n" + "stepType: " + stepType +
        "\n" + "stepLenght: " + stepLenght;

        DetectSteps(smoothedPitch, acceleration.magnitude);
    }

    void DetectSteps(float pitch, float accMagnitude)
    {
        absPitch = Mathf.Abs(pitch); // Convert pitch to degrees
        // Check if the pitch and magnitude value crosses the threshold, is less than the last pitch value and the time interval has passed
        if (absPitch > pitchThreshold && absPitch <= lastPitch && Time.time > stepInterval)
        {
            if (!isStepDetected)
            {
                stepCount++;
                stepInterval = Time.time + stepTime; // Reset the time interval
                isStepDetected = true;
                if (pitch < -25)
                {
                    stepType = 2;
                }
                else if (pitch > 25)
                {
                    stepType = 1;
                }
                else
                {
                    stepType = 0;
                    stepLenght = 0.7f; // Ei järkevää, mutta laitetaan tähän jotain
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