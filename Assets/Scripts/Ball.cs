using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _isGhost;

    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    public void Init(Vector3 velocity, bool isGhost = false)
    {
        _isGhost = isGhost;
        _rb.AddForce(velocity);
    }
}
