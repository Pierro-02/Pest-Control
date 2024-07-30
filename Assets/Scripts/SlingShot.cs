using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlingShot : MonoBehaviour
{
    [Header("SlingShot Settings")]
    [SerializeField] private Transform TransPoint1;
    [SerializeField] private Transform TransPoint2;
    [SerializeField] private Projection _projection;
    [SerializeField] private float force = 100f;

    [Header("Prefab")]
    [SerializeField] private Transform prefabObject;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Camera mainCamLocal;

    [Header("Rotation Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float rotationSpeed = 0.05f;
    [SerializeField] private float rotationLimit = 50;

    private Transform _newBall;
    private Rigidbody _newBallRigidbody;
    private Vector3 forceAtRelease = Vector3.zero;
    private float forceValue;
    private Vector3 shootDirection;
    private float initialRotationY;

    void Start()
    {
        initialRotationY = player.rotation.eulerAngles.y;
        _lineRenderer.positionCount = 2;
    }

    void Update()
    {
        HandleInput();
        UpdateLineRenderer();
        if (_newBall && _newBallRigidbody)
        {
            RotatePlayer();
            _projection.SimulateTrajectory(_newBall.gameObject, _newBall.position, forceAtRelease);
        }
    }

    private void HandleInput()
    {
        if ((Input.GetMouseButtonDown(0) ||
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            && _newBall == null)
        {
            // Debug.Log("Spawn Ball");
            SpawnNewBall();
        }

        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            && _newBall != null)
        {
            // Debug.Log("Dragging Ball");
            DragBall();
            forceValue = force * (Math.Abs((spawnPoint.position.z - _newBall.position.z)) +
                                (Math.Abs(spawnPoint.position.x - _newBall.position.x) * 0.5f) +
                                (Math.Abs(spawnPoint.position.y - _newBall.position.y) * 0.5f));
            shootDirection = (spawnPoint.position - _newBall.position).normalized;
            forceAtRelease = shootDirection * forceValue;
        }
        else if ((Input.GetMouseButtonUp(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
                && _newBall != null)
        {
            // Debug.Log("Released Ball");
            ReleaseBall();
        }
    }

    private void SpawnNewBall()
    {
        _newBall = Instantiate(prefabObject,
            spawnPoint ? new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z - 0.2f)
                        : Vector3.zero,
            Quaternion.identity);

        _newBall.SetParent(transform);
        if (_newBall)
        {
            _lineRenderer.positionCount = 3;
            Vector3 newPos = _newBall.position;
            newPos.z = spawnPoint.position.z - 0.2f;
            _lineRenderer.SetPosition(1, newPos);

            // Get the Rigidbody component and set it to kinematic initially
            _newBallRigidbody = _newBall.GetComponent<Rigidbody>();
            _newBallRigidbody.isKinematic = true;
        }
    }

    private void DragBall()
    {
        Vector3 inputPosition;

        if (Input.touchCount > 0)
        {
            // Use touch input position
            inputPosition = Input.GetTouch(0).position;
        }
        else
        {
            // Use mouse input position
            inputPosition = Input.mousePosition;
        }

        // Convert the input position to a Vector3
        Vector3 mouseScreenPos = new Vector3(inputPosition.x, inputPosition.y, mainCamLocal.WorldToScreenPoint(_newBall.position).z);
        Vector3 mouseWorldPos = mainCamLocal.ScreenToWorldPoint(mouseScreenPos);

        // Define the range for dragging
        float dragRange = 2f; // Adjust as needed
        Vector3 direction = mouseWorldPos - spawnPoint.position;
        float distance = Mathf.Clamp(direction.magnitude, 0, dragRange);
        Vector3 clampedPosition = spawnPoint.position + direction.normalized * distance;

        // Update the ball's position
        _newBall.position = new Vector3(clampedPosition.x, clampedPosition.y, clampedPosition.z + 0.1f);


        // Update the line renderer to follow the new ball position
        _lineRenderer.SetPosition(1, _newBall.position);
    }


    private void RotatePlayer()
    {
        Vector3 currentEulerAngles = player.rotation.eulerAngles;
        float newAngleY = currentEulerAngles.y;
        bool doRotation = true;
        if (newAngleY > initialRotationY + rotationLimit)
        {
            newAngleY -= 0.1f;
            doRotation = false;
        }
        if (newAngleY < initialRotationY - rotationLimit)
        {
            newAngleY += 0.1f;
            doRotation = false;
        }

        // Determine the new angle for the x-axis rotation
        if (_newBall.localPosition.x < spawnPoint.localPosition.x - 15 && doRotation)
        {
            Debug.Log("Move Camera to Right");
            newAngleY += rotationSpeed;
        }
        else if (_newBall.localPosition.x > spawnPoint.localPosition.x + 15 && doRotation)
        {
            Debug.Log("Move Camera to Left");
            newAngleY -= rotationSpeed;
        }
        else
        {
            Debug.Log("Camera Stationary");
        }

        // Create a new rotation only modifying the x-axis
        Quaternion newRotation = Quaternion.Euler(currentEulerAngles.x, newAngleY, currentEulerAngles.z);

        // Set the new position and rotation
        player.SetLocalPositionAndRotation(player.position, newRotation);
    }


    private void ReleaseBall()
    {
        // Debug.Log("Ball pos: " + _newBall.localPosition.x);
        // Debug.Log("Spawn x: " + spawnPoint.localPosition.x);

        // Debug.Log("------------------------------------");


        _projection.SetLinePositionCount(0);
        _lineRenderer.positionCount = 2;

        // Activate Rigidbody physics by setting it to non-kinematic
        _newBallRigidbody.isKinematic = false;

        // Apply force
        _newBall.GetComponent<Ball>().Init(forceAtRelease);

        // Debug.Log("NewBallPos: " + _newBall.position);
        // Debug.Log("SpawnPointPos: " + spawnPoint.position);
        // Debug.Log("Force: " + forceAtRelease);

        _newBall = null;
        _newBallRigidbody = null;
    }

    private void UpdateLineRenderer()
    {
        if (TransPoint1 && TransPoint2)
        {
            _lineRenderer.SetPosition(0, TransPoint1.position);
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, TransPoint2.position);
        }
    }
}