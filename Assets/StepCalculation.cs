using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    public Text debugText; // UI text for debugging
    public Transform playerObject; // The GameObject that moves (e.g., Player)

    private float alpha = 0.9f; // Complementary filter weight
    private float fusedPitch = 0.0f; // Fused pitch value
    private float pitchThreshold = 5f; // Step detection threshold
    private float stepTime = 0.75f; // Time between steps
    private float stepLength = 0.7f; // Default step length
    private float lastPitch = 0.0f;
    private bool isStepDetected = false;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Step timing
    public static float smoothedPitch = 0.0f; // Smoothed pitch
    private int stepType = 0; // 0 = flat, 1 = down, 2 = up
    private Vector3 acceleration; // Accelerometer data
    private float accZ = 0f;

    private float estimatedHeading = 0.0f; // Gyro-based heading estimation
    private float lastTime;
    private Vector3 currentPosition;

    void Start()
    {
        Input.gyro.enabled = true;
        lastTime = Time.time;
        currentPosition = playerObject.position;
    }

    void Update()
    {
        float currentTime = Time.time;
        lastTime = currentTime;

        // Get sensor data
        acceleration = Input.gyro.userAcceleration;
        float accelerometerPitch = Mathf.Atan2(-acceleration.z, Mathf.Sqrt(acceleration.y * acceleration.y + acceleration.x * acceleration.x));
        float gyroPitch = -Input.gyro.rotationRateUnbiased.x;
        accZ = (acceleration.z / Input.gyro.gravity.y) * 10f;

        // Apply complementary filter for pitch estimation
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;
        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        // Step detection
        DetectSteps(smoothedPitch, acceleration.magnitude);

        // Apply rotation to the GameObject
        playerObject.rotation = Quaternion.Euler(0, estimatedHeading * Mathf.Rad2Deg, 0);

        // Debugging
        debugText.text = $"Fused Pitch: {smoothedPitch:F2}\n" +
                         $"Gyro Pitch Delta: {gyroPitch:F2}\n" +
                         $"Step Count: {stepCount}\n" +
                         $"Step Type: {stepType}\n" +
                         $"Step Length: {stepLength:F2}\n" +
                         $"Heading: {estimatedHeading * Mathf.Rad2Deg:F2}°";
    }

    private float peakPitch = float.MinValue;
    private float lowestPitch = float.MaxValue;
    private float peakAccZ = float.MinValue;
    private float lowestAccZ = float.MaxValue;

    void DetectSteps(float pitch, float accMagnitude)
    {
        float absPitch = Mathf.Abs(pitch);

        // Track peak and lowest pitch within a step cycle
        if (pitch > peakPitch)
            peakPitch = pitch;
        if (pitch < lowestPitch)
            lowestPitch = pitch;
        if (accZ > peakAccZ)
            peakAccZ = accZ;
        if (accZ < lowestAccZ)
            lowestAccZ = accZ;

        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            if (!isStepDetected)
            {
                stepCount++;
                stepInterval = Time.time + stepTime; // Reset step timer
                isStepDetected = true;

                // Determine step type based on pitch
                if (pitch > 13)
                {
                    stepType = 2; // Up (stairs or incline)
                    stepLength = 0.3f;
                }
                else
                {
                    if (peakAccZ - lowestAccZ < 12)
                    {
                        stepType = 1;
                        stepLength = 0.3f;
                    } 
                    else 
                    {
                    stepType = 0;
                    // Adjust step length dynamically
                    stepLength = 0.5f + (pitch / 15.0f);
                    }
                }


                // Log the peak and lowest pitch for this step
                Debug.Log($"Step {stepCount}: Peak accZ = {peakAccZ:F2}, Lowest accZ = {lowestAccZ:F2}, accZ Difference = {peakAccZ - lowestAccZ}");

                MoveObject(stepLength, stepType);

            }
            // Reset values for the next cycle
            peakPitch = float.MinValue;
            lowestPitch = float.MaxValue;
            peakAccZ = float.MinValue;
            lowestAccZ = float.MaxValue;
        }
        else
        {
            isStepDetected = false;
        }

        lastPitch = absPitch;
    }


    void MoveObject(float stepLength, int stepType)
    {
        // Get forward direction based on the GameObject's local space
        Vector3 movementDirection = playerObject.forward; // Moves relative to rotation

        // Adjust vertical movement based on step type
        if (stepType == 2) // Up (stairs or incline)
        {
            movementDirection.y = 0.2f; // Add some vertical movement
        }
        else if (stepType == 1) // Down (stairs or decline)
        {
            movementDirection.y = -0.2f; // Subtract some vertical movement
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