using System;
using UnityEngine;

public class Pest : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform spawnPoint;

    private Rigidbody rb;
    private Transform currentPoint;
    private int nPoint = 0;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        currentPoint = spawnPoint ? spawnPoint : null;
    }

    void Update()
    {

    }

    Transform GetNextPoint()
    {
        nPoint = (nPoint + 1) % patrolPoints.Length;
        return patrolPoints[nPoint];
    }

    void MoveToNextPoint(Transform nextPoint)
    {
        Vector3 direction = nextPoint.position - currentPoint.position;
    }
}
