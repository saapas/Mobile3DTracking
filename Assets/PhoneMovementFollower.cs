using UnityEngine;

public class PhoneMovementFollower : MonoBehaviour
{
    // The target object to follow
    public Transform target;

    // Offset between the follower and the target
    public Vector3 offset = new Vector3(0, 0, 0);

    // Smoothing factor for following (optional)
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned for ObjectFollower script.");
            return;
        }

        // Calculate the desired position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update the position of the follower
        transform.position = smoothedPosition;
    }
}
