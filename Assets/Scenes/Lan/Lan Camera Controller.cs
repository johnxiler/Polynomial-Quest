using UnityEngine;

public class LanCameraController : MonoBehaviour
{
    Transform player;
    [SerializeField] LanGameManager gmScript;
    bool hasInitialized;
    [SerializeField] float smoothSpeed;
    Vector3 cameraOffset = new(0, 0, -10);
    public void Initialize()
    {
        player = gmScript.player.transform;
        GetComponent<Camera>().enabled = true;
        hasInitialized = true;
    }


    private void LateUpdate()
    {
        if (hasInitialized && player != null)
        {
            Vector3 desiredPosition = player.position + cameraOffset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void setCameraOffset(float y)
    {
        cameraOffset = new(0, y, -10);
    }
}
