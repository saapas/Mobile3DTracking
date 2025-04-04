using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class StepDetector : MonoBehaviour
{
    public Text debugText; // UI text for debugging
    public Transform playerObject; // The GameObject that moves (e.g., Player)

    private float alpha = 0.9f; // Complementary filter weight
    private float fusedPitch = 0.0f; // Fused pitch value
    private float pitchThreshold = -70f; // Step detection threshold
    private float stepTime = 0.75f; // Time between steps
    private float stepLength = 0.7f; // Default step length
    private bool isStepDetected = false;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Step timing
    public static float smoothedPitch = 0.0f; // Smoothed pitch
    private int stepType = 0; // 0 = flat, 1 = down, 2 = up
    private Vector3 acceleration; // Accelerometer data

    Stopwatch stopwatch = new Stopwatch();
    private double pitchIntegral;
    private double lastIntegralTime;

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

        // Apply complementary filter for pitch estimation
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;
        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        // Step detection
        DetectSteps(smoothedPitch, acceleration.y);

        // Debugging
        debugText.text = $"Fused Pitch: {smoothedPitch:F2}\n" +
                         $"Gyro Pitch Delta: {gyroPitch:F2}\n" +
                         $"Step Count: {stepCount}\n" +
                         $"Step Type: {stepType}\n" +
                         $"Step Length: {stepLength:F2}";
    }

    private double PositiveIntegral;
    private double NegativeIntegral;
    private int timer;

    void DetectSteps(float pitch, float accPitch)
    {
        if (pitch > 0)
        {
            if (timer == 0 || timer == 2)
            {
                UnityEngine.Debug.Log("Neg" + NegativeIntegral);
                pitchIntegral = 0;
                stopwatch.Restart();
                PitchIntegral(pitch);
                PositiveIntegral = pitchIntegral;
                timer = 1;
            }
            else
            {
                if (stopwatch.Elapsed.TotalSeconds > 1)
                {
                    PositiveIntegral = 0;
                }
                else
                {
                    PitchIntegral(pitch);
                    PositiveIntegral = pitchIntegral;
                }
            }
        }
        if (pitch < 0)
        {
            if (timer != 2)
            {
                UnityEngine.Debug.Log("Pos" + PositiveIntegral);
                UnityEngine.Debug.Log("both" + (PositiveIntegral - NegativeIntegral)); 
                pitchIntegral = 0;
                stopwatch.Restart();
                PitchIntegral(pitch);
                NegativeIntegral = pitchIntegral;
                timer = 2;
            }
            else
            {
                
                if (stopwatch.Elapsed.TotalSeconds > 1)
                {
                    PositiveIntegral = 0;
                }
                else
                {
                    PitchIntegral(pitch);
                    PositiveIntegral = pitchIntegral;
                }
            }
        }

        if (timer == 1 && NegativeIntegral < pitchThreshold && Time.time > stepInterval)
        {
            if (!isStepDetected)
            {
                stepCount++;
                stepInterval = Time.time + stepTime; // Reset step timer
                isStepDetected = true;

                // Determine step type based on pitch
                if (PositiveIntegral - NegativeIntegral > 170)
                {
                    stepType = 2; // flat
                    stepLength = 0.5f + (pitch / 15.0f);
                }
                else
                {
                    if (PositiveIntegral > 39)
                    {
                        stepType = 1;
                        stepLength = 0.5f;
                    }
                    else
                    {
                        stepType = 0;
                        // Adjust step length dynamically
                        stepLength = 0.5f;
                    }
                }

                MoveObject(stepLength, stepType);

            }
        }
        else
        {
            isStepDetected = false;
        }
    }

    void PitchIntegral(float pitch)
    {
        pitchIntegral += pitch * lastIntegralTime - stopwatch.Elapsed.TotalSeconds;
        lastIntegralTime = stopwatch.Elapsed.TotalSeconds;
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