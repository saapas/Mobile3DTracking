using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    public Text debugText; // UI text for debugging
    public Transform playerObject; // The GameObject that moves (e.g., Player)

    private float alpha = 0.9f; // Complementary filter weight
    private float fusedPitch = 0.0f; // Fused pitch value
    private float pitchThreshold = 3f; // Step detection threshold
    private float stepTime = 0.75f; // Time between steps
    private float stepLength = 0.7f; // Default step length
    private float lastPitch = 0.0f;
    private bool isStepDetected = false;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Step timing
    public static float smoothedPitch = 0.0f; // Smoothed pitch
    private int stepType = 0; // 0 = flat, 1 = down, 2 = up
    private Vector3 acceleration; // Accelerometer data

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
        float deltaTime = currentTime - lastTime;
        lastTime = currentTime;

        // Get sensor data
        acceleration = Input.gyro.userAcceleration;
        float accelerometerPitch = Mathf.Atan2(-acceleration.z, Mathf.Sqrt(acceleration.y * acceleration.y + acceleration.x * acceleration.x));
        float gyroPitch = -Input.gyro.rotationRateUnbiased.x;
        float gyroRoll = -Input.gyro.rotationRateUnbiased.y; // Heading change

        // Apply complementary filter for pitch estimation
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;
        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        // Integrate gyro yaw to estimate heading
        estimatedHeading += gyroRoll * deltaTime; // Update heading in radians

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

    void DetectSteps(float pitch, float accMagnitude)
    {
        float absPitch = Mathf.Abs(pitch);

        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            if (!isStepDetected)
            {
                stepCount++;
                stepInterval = Time.time + stepTime; // Reset step timer
                isStepDetected = true;

                // Determine step type based on pitch
                if (pitch > 8)
                    stepType = 2; // Up (stairs or incline)
                else if (pitch < 4.5f)
                    stepType = 1; // Down (stairs or decline)
                else
                    stepType = 0; // Flat ground

                // Adjust step length dynamically
                stepLength = 0.5f + (pitch / 10.0f);

                MoveObject(stepLength, stepType);
            }
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

        // Keep movement in the X-Z plane
        movementDirection.y = 0;
        movementDirection.Normalize();

        // Final movement vector
        Vector3 movement = movementDirection * stepLength;
        currentPosition += movement;

        // Smoothly move the player
        playerObject.position = currentPosition;
    }
}