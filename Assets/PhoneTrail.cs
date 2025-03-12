using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PhoneTrail : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private int positionCount = 0;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        positionCount++;
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPosition(positionCount - 1, transform.position);
    }
}
