using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    private float pitchVariance = 0.0f;

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

    private float highestPitch = float.MinValue;
    private float lowestPitch = float.MaxValue;
    private float lowGravity = float.MaxValue;

    void DetectSteps(float pitch, float gravity)
    {
        // Set highest and lowest pitch
        if (pitch > highestPitch) highestPitch = pitch;
        if (pitch < lowestPitch) lowestPitch = pitch;
        if (gravity < lowGravity) lowGravity = gravity;

        // Check if pitch has crossed the threshold and is decreasing and has passed the time interval
        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            stepCount += 2;
            stepInterval = Time.time + stepTime; // Reset step timer
            pitchVariance = PitchVariance(highestPitch, lowestPitch);

            /*suora 0.82, 0.040, 9.07, 0.76, -5.19, 0.85
            yl�s 0.56, 0.02, 12, 0.7, -8, 0.57
            alas 0.72, 0.047, 8, 2.4, -4, 1.47 */

            Debug.Log("peakPitch " + highestPitch);
            Debug.Log("LowPitch " + lowestPitch);
            Debug.Log("Gravity " + lowGravity);

            // Determine step type based on pitch
            if (lowGravity < 0.7f && highestPitch > 9.5f)
            {
                stepType = 2; //up
                stepLength = 0.5f;
            }
            if (lowGravity > 0.88f)
            {
                stepType = 0; //flat
                stepLength = (highestPitch - lowestPitch) / 11;
            }
            else
            {
                float gravityThreshold = ComputeGThreshold(pitchVariance);
                if (lowGravity < gravityThreshold)
                {
                    stepType = 1; // Down
                    stepLength = 0.5f;
                }
                else
                {
                    stepType = 0; //flat
                    stepLength = (highestPitch - lowestPitch) / 11;
                }
            }
            // Reset highest and lowest pitch
            highestPitch = float.MinValue;
            lowestPitch = float.MaxValue;
            lowGravity = float.MaxValue;

            MoveObject(stepLength, stepType);
        }
        lastPitch = pitch;
    }

    float PitchVariance(float peakPitch, float lowPitch) 
    {
        if (peakList.Count() > 5) peakList.RemoveAt(0);
        peakList.Add(peakPitch);

        if (lowList.Count() > 5) lowList.RemoveAt(0);
        lowList.Add(lowPitch);

        float peakAverage = peakList.Average();
        float lowAverage = lowList.Average();
        float sumOfSquares = peakList.Sum(x => (x - peakAverage) * (x - peakAverage));
        float variance = sumOfSquares / peakList.Count();
        float sumOfSquares2 = lowList.Sum(x => (x - lowAverage) * (x - lowAverage));
        float variance2 = sumOfSquares2 / lowList.Count();
        Debug.Log("peakAverage " + peakAverage);
        Debug.Log("peakVariance " + variance);
        Debug.Log("LowAverage " + lowAverage);
        Debug.Log("LowVariance " + variance2);
        return variance + variance2;
    }

    float ComputeGThreshold(float variance)
    {
        float minVar = 0.3f;
        float maxVar = 2.5f;

        float threshold = Mathf.Clamp01((variance - minVar) / (maxVar - minVar));

        return 0.78f + threshold * 0.1f;
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