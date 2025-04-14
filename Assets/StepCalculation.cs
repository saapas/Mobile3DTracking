using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

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
    List<float> peakList = new List<float>();
    List<float> lowList = new List<float>();
    List<float> gravityList = new List<float>();
    private float peakAverage = 0;
    private float lowAverage = 0;
    private float gravityAverage = 0;

    private float lastPitch = 0.0f;

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

        // Calculate pitch from acceleromater data
        float accelerometerPitch = acceleration.y;

        // Get pitch value from gyroscope
        float gyroPitch = -Input.gyro.rotationRateUnbiased.x;

        // Apply complementary filter for pitch estimation
        fusedPitch = alpha * (fusedPitch + gyroPitch) + (1 - alpha) * accelerometerPitch;
        smoothedPitch = Mathf.Lerp(smoothedPitch, fusedPitch, 0.1f);

        // Step detection
        DetectSteps(smoothedPitch, Input.gyro.gravity.y);

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
    private float lowGracity = float.MaxValue;

    void DetectSteps(float pitch, float gravity)
    {
        // Set highest and lowest pitch
        if (pitch > peakPitch) peakPitch = pitch;
        if (pitch < lowestPitch) lowestPitch = pitch;
        if (gravity < lowGracity) lowGracity = gravity;

        // Check if pitch has crossed the threshold and is decreasing and has passed the time interval
        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            stepCount += 2;
            stepInterval = Time.time + stepTime; // Reset step timer
            peakList.Add(peakPitch);
            lowList.Add(lowestPitch);
            gravityList.Add(lowGracity);

            if (stepCount > 50)
            {
                peakAverage = peakList.Average();
                lowAverage = lowList.Average();
                gravityAverage = gravityList.Average();
                float sumOfSquares = peakList.Sum(x => (x - peakAverage) * (x - peakAverage));
                float variance = sumOfSquares / peakList.Count();
                float sumOfSquares2 = lowList.Sum(x => (x - lowAverage) * (x - lowAverage));
                float variance2 = sumOfSquares2 / lowList.Count();
                float sumOfSquares3 = gravityList.Sum(x => (x - gravityAverage) * (x - gravityAverage));
                float variance3 = sumOfSquares3 / gravityList.Count();
                Debug.Log("peakAverage " + peakAverage);
                Debug.Log("peakVariance " + variance);
                Debug.Log("LowAverage " + lowAverage);
                Debug.Log("LowVariance " + variance2);
                Debug.Log("gravityAverage " + gravityAverage);
                Debug.Log("gravityVariance " + variance3);
                /*suora 0.82, 0.040, 9.07, 0.76, -5.19, 0.85
                  ylös 0.56, 0.02, 12, 0.7, -8, 0.57
                  alas 0.72, 0.047, 8, 2.4, -4, 1.47 */
            }

            Debug.Log("peakPitch " + peakPitch);
            Debug.Log("LowPitch " + lowestPitch);
            Debug.Log("Gravity " + lowGracity);

            // Determine step type based on pitch
            if (peakPitch - lowestPitch > 16.5f)
            {
                stepType = 2; // up
                stepLength = 0.5f;
            }
            else
            {
                if (lowestPitch > -4.58f && peakPitch < 7.87f)
                {
                    stepType = 1;
                    stepLength = 0.5f;
                }
                else
                {
                    stepType = 0;
                    stepLength = ((peakPitch - lowestPitch) / 11f); // Dynamically adjust steplength
                }
            }
            // Reset highest and lowest pitch
            peakPitch = float.MinValue;
            lowestPitch = float.MaxValue;
            lowGracity = float.MaxValue;

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
            movementDirection.y = 0.36f; // Add some vertical movement
        }
        else if (stepType == 1) // Down (stairs or decline)
        {
            movementDirection.y = -0.36f; // Subtract some vertical movement
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