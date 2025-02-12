using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PhoneTrail : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private int positionCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        positionCount++;
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPosition(positionCount - 1, transform.position);
    }
}
