using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    public Text debugText; // UI text for debugging
    public Transform playerObject; // The GameObject that moves (e.g., Player)

    private readonly float alpha = 0.85f; // Complementary filter weight
    private float fusedPitch = 0.0f; // Fused pitch value
    private readonly float pitchThreshold = 4f; // Step detection threshold
    private readonly float stepTime = 0.75f; // Time between steps
    private float stepLength;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Step timing
    private float smoothedPitch = 0.0f; // Smoothed pitch
    private int stepType = 0; // 0 = flat, 1 = down, 2 = up
    private Vector3 acceleration; // Accelerometer data

    private float lastPitch = 0.0f;
    private float attitudeLastPitch;

    private Vector3 currentPosition;

    void Start()
    {
        Input.gyro.enabled = true;
        currentPosition = playerObject.position;
    }

    void Update()
    {
        // Get linear Acceleration from accelrometer by substracting gravity vector.
        acceleration = Input.acceleration - Input.gyro.gravity;

        // Make so that the value stays between allowed values for Asin()
        float clampedY = Mathf.Clamp(-acceleration.y, -1f, 1f);

        // Calculate pitch from acceleromater data
        float accelerometerPitch = Mathf.Asin(clampedY);

        // Get pitch value from gyroscope
        float gyroPitch = -Input.gyro.rotationRateUnbiased.x;

        // Apply complementary filter for pitch estimation
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;
        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        // Step detection
        DetectSteps(smoothedPitch, acceleration.y);

        // Debugging
        debugText.text = $"Fused Pitch: {smoothedPitch:F2}\n" +
                         $"Gyro Pitch Delta: {gyroPitch:F2}\n" +
                         $"Accelerometer Pitch: {accelerometerPitch:F2}\n" +
                         $"Step Count: {stepCount}\n" +
                         $"Step Type: {stepType}\n" +
                         $"Step Length: {stepLength:F2}";
    }

    private float peakPitch = float.MinValue;
    private float lowestPitch = float.MaxValue;

    void DetectSteps(float pitch, float accPitch)
    {
        // Set highest and lowest pitch
        if (pitch > peakPitch) peakPitch = pitch;
        if (pitch < lowestPitch) lowestPitch = pitch;

        // Check if pitch has crossed the threshold and is decreasing and has passed the time interval
        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            stepCount++;
            stepInterval = Time.time + stepTime; // Reset step timer
            Debug.Log("peak" + peakPitch);
            Debug.Log("Low" + lowestPitch);

            // Determine step type based on pitch
            if (peakPitch > 9.5f)
            {
                stepType = 2; // up
                stepLength = 0.5f;
            }
            else
            {
                if (peakPitch + lowestPitch < 2 && lowestPitch < -3)
                {
                    stepType = 1;
                    stepLength = 0.5f;
                }
                else
                {
                    stepType = 0;
                    stepLength = ((peakPitch - lowestPitch) / 30); // Dynamically adjust steplength
                }
            }
            // Reset highest and lowest pitch
            peakPitch = float.MinValue;
            lowestPitch = float.MaxValue;

            MoveObject(stepLength, stepType);
        }
        lastPitch = pitch;
    } 

    void MoveObject(float stepLength, int stepType)
    {
        // Get forward direction based on the GameObject's local space
        Vector3 movementDirection = playerObject.forward; // Moves relative to rotation

        // Adjust vertical movement based on step type
        if (stepType == 2) // Up (stairs or incline)
        {
            movementDirection.y = 0.5f; // Add some vertical movement
        }
        else if (stepType == 1) // Down (stairs or decline)
        {
            movementDirection.y = -0.5f; // Subtract some vertical movement
        }
        else // Flat ground
        {
            movementDirection.y = 0; // No vertical movement
        }

        // Keep movement in the X-Z plane
        movementDirection.Normalize();

        // Final movement vector
        Vector3 movement = movementDirection * stepLength;
        currentPosition += movement;

        // Smoothly move the player
        playerObject.position = currentPosition;
    }
}