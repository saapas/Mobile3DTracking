using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class StepDetector : MonoBehaviour
{
    public Text debugText; // UI text for debugging
    public Transform playerObject; // The GameObject that moves (e.g., Player)
    private Vector3 currentPosition;

    private readonly float pitchThreshold = 1f; // Step detection threshold
    private readonly float stepTime = 0.75f; // Time between steps
    private float stepLength;
    private int stepCount = 0;
    private float stepInterval = 0.0f; // Step timing
    private int stepType = 0; // 0 = flat, 1 = down, 2 = up
    private Vector3 acceleration; // Accelerometer data
    List<float> peakList = new List<float>(); // For calculating variance
    List<float> lowList = new List<float>(); // For calculating variance
    private float peakPitchVariance = 0.0f;
    private float lowPitchVariance = 0.0f;
    private float lastPitch = 0.0f;
    private float accelerometer;
    private Vector3 gravity;
    private float gravityPitch;

    public ModelClassifier classifier;

    void Start()
    {
        Input.gyro.enabled = true;
        currentPosition = playerObject.position;
    }

    void Update()
    {
        // Get gravity vector
        gravity = Input.gyro.gravity;

        // Get Y component to act as pitch
        gravityPitch = gravity.y;

        // Get linear Acceleration from accelrometer by substracting gravity vector.
        acceleration = Input.acceleration - gravity;

        // Get the y component for helping to determine flat or downstairs
        accelerometer = acceleration.y;

        // Get change in pitch value from gyroscope
        float gyroPitch = -Input.gyro.rotationRateUnbiased.x;

        // Step detection
        DetectSteps(gyroPitch, accelerometer, gravityPitch);

        // Debugging
        debugText.text = $"Gyro Pitch Delta: {gyroPitch:F2}\n" +
                         $"Accelerometer Pitch: {gravityPitch:F2}\n" +
                         $"Step Count: {stepCount}\n" +
                         $"Step Type: {stepType}\n" +
                         $"Step Length: {stepLength:F2}";
    }

    // Initialize values
    private float highestPitch = float.MinValue;
    private float lowestPitch = float.MaxValue;
    private float lowGravity = float.MaxValue;
    private float maxAcc = float.MinValue;
    private float minAcc = float.MaxValue;

    void DetectSteps(float pitch, float accelerometer, float gravity)
    {
        // Set highest and lowest pitch
        if (pitch > highestPitch) highestPitch = pitch;
        if (pitch < lowestPitch) lowestPitch = pitch;
        if (gravity < lowGravity) lowGravity = gravity;
        if (accelerometer > maxAcc) maxAcc = accelerometer;
        if (accelerometer < minAcc) minAcc = accelerometer;

        // Check if pitch has crossed the threshold and is decreasing and has passed the time interval
        if (pitch > pitchThreshold && pitch <= lastPitch && Time.time > stepInterval)
        {
            stepCount += 2;
            stepInterval = Time.time + stepTime; // Reset step timer
            
            // Keep list short so less calculating and faster reaction to change
            if (peakList.Count() > 4) peakList.RemoveAt(0);
            peakList.Add(highestPitch);

            if (lowList.Count() > 4) lowList.RemoveAt(0);
            lowList.Add(lowestPitch);

            // Calculate pitch variance
            peakPitchVariance = PitchVariance(peakList);
            lowPitchVariance = PitchVariance(peakList);

            Debug.Log("peakPitchVariance " + peakPitchVariance);
            Debug.Log("lowPitchVariance " + lowPitchVariance);
            Debug.Log("peakPitch " + highestPitch);
            Debug.Log("LowPitch " + lowestPitch);
            Debug.Log("maxAccelerometer " + maxAcc);
            Debug.Log("minAccelerometer " + minAcc);
            Debug.Log("lowGravity" + lowGravity);

            // Predict step using ModelClassifier
            float[] stepData = new float[5] {
                highestPitch,
                lowestPitch,
                maxAcc,
                minAcc,
                lowGravity
            };

            // catch clear cases
            if (lowGravity > 0.9f) 
            {
                stepType = 0;
            }
            else if (lowGravity < 0.7f) 
            {
                stepType = 2;
            }

            // run the model when not so clear
            else 
            {
                stepType = classifier.PredictStep(stepData);
            }

            // Reset highest and lowest pitch
            highestPitch = float.MinValue;
            lowestPitch = float.MaxValue;
            lowGravity = float.MaxValue;
            maxAcc = float.MinValue;
            minAcc = float.MaxValue;

            // Move the game object
            MoveObject(stepLength, stepType);
        }
        // Set new last pitch
        lastPitch = pitch;
    }

    float PitchVariance(List<float> list) 
    {
        // --Calculate variance of both high and low pitch--
        // Calculating the average then variance
        float average = list.Average();
        float sumOfSquares = list.Sum(x => (x - average) * (x - average));
        float variance = sumOfSquares / list.Count();

        return variance;
    }

    void MoveObject(float stepLength, int stepType)
    {
        // Get forward direction based on the GameObject's local space
        Vector3 movementDirection = playerObject.forward; // Moves relative to rotation

        // Adjust vertical movement based on step type
        if (stepType == 2) // Up
        {
            movementDirection.y = 0.36f; // Add some vertical movement
        }
        else if (stepType == 1) // Down 
        {
            movementDirection.y = -0.36f; // Subtract some vertical movement
        }
        else // Flat ground
        {
            movementDirection.y = 0; // No vertical movement
        }

        // Normalize the vector before adding the length
        movementDirection.Normalize();

        // Final movement vector
        Vector3 movement = movementDirection * stepLength;
        currentPosition += movement;

        // Move the player
        playerObject.position = currentPosition;
    }
}
