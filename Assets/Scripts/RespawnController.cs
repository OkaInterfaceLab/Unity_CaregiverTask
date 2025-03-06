using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

public class RespawnController : MonoBehaviour
{
    // cached starting transform
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _initialScale;

    private Rigidbody _rigidBody;

    protected virtual void OnEnable()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialScale = transform.localScale;
        _rigidBody = GetComponent<Rigidbody>();
    }

    public void Respawn()
    {
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        transform.localScale = _initialScale;

        if (_rigidBody)
        {
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;

            // _rigidBody.isKinematic = true;

        }
    }
}
