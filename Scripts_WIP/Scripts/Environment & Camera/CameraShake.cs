using UnityEngine;
using Cinemachine; // Don't forget this!

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    // Keeping your exact variable name so it works instantly with your other scripts
    public static CameraShake cameraShakeForOtherObjectsToUseAndGrab;
    private CinemachineImpulseSource impulseSource;

    [Header("Shake Settings")]
    [Tooltip("How hard the camrea shake by defult")]
    public float baseShakeForce = 1.5f;

    private void Awake()
    {
        // Set up the static instance
        if (cameraShakeForOtherObjectsToUseAndGrab == null)
        {
            cameraShakeForOtherObjectsToUseAndGrab = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Grab the Cinemachine Impulse Source component attached to this object
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Your PlayerStats script already calls this!
    public void Play(float forceMultiplier=1f)
    {
        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);

        Vector3 randomDirection = new Vector3(randomX, randomY, 0f).normalized;
        Vector3 finalVelocity = randomDirection * baseShakeForce*forceMultiplier;
        // This one line replaces all the math and Coroutines
        impulseSource.GenerateImpulse(finalVelocity);
    }
}