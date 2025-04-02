using System.Collections;
using UnityEngine;

public class DeviceLocation : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetData());
    }

    IEnumerator GetData()
    {
        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        // Continuously update GPS location every 10 seconds
        while (true)
        {
            Debug.Log("Location: " + Input.location.lastData.latitude + " " +
                      Input.location.lastData.longitude + " " +
                      Input.location.lastData.altitude + " " +
                      Input.location.lastData.horizontalAccuracy + " " +
                      Input.location.lastData.timestamp);
            yield return new WaitForSeconds(10);
        }
    }
}
