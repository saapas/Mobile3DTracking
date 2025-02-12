using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target object to follow
    public Vector3 offset; // The offset distance between the camera and the target

    void Start()
    {
        // Initialize the offset based on the initial positions
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        // Update the camera's position based on the target's position and the offset
        transform.position = target.position + offset;
    }
}
