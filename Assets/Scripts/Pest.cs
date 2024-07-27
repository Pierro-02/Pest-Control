using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class Pest : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private GameObject body;

    private Rigidbody rb;
    private Transform currentPoint;
    private int nPoint = 0;

    void Start()
    {
        rb = body ? body.GetComponent<Rigidbody>() : null;
        currentPoint = body.transform;
        if (currentPoint != null && patrolPoints.Length > 0)
        {
            StartCoroutine(Patrol());
        }
        else
        {
            Debug.LogError("Error!: Missing Patrol Points or Current Point");
        }
    }

    IEnumerator Patrol()
    {
        while (true)
        {
            Transform nextPoint = GetNextPoint();
            yield return MoveToNextPoint(nextPoint);
            yield return new WaitForSeconds(2.0f); // Wait at each patrol point for 2 seconds
        }
    }

    Transform GetNextPoint()
    {
        Transform newPoint = patrolPoints[nPoint];
        nPoint = (nPoint + 1) % patrolPoints.Length;
        return newPoint;
    }

    IEnumerator MoveToNextPoint(Transform nextPoint)
    {
        while (Vector3.Distance(rb.transform.position, nextPoint.position) > 0.1f)
        {
            Vector3 direction = (nextPoint.position - rb.transform.position).normalized;
            Vector3 move = direction * speed * Time.deltaTime;
            rb.MovePosition(rb.transform.position + move);

            Debug.Log("Direction: " + direction);
            Debug.Log("Position: " + rb.transform.position);

            yield return null; // Wait until the next frame
        }
        currentPoint = nextPoint;
    }
}
