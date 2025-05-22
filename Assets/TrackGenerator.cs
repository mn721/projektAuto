using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public GameObject roadSegmentPrefab;
    public Transform carTransform;
    public int initialSegments = 3;
    public float spawnDistance = 50f;
    public float despawnDistance = 30f;

    private Queue<GameObject> segmentsQueue = new Queue<GameObject>();
    private float segmentLength;
    private Vector3 nextSpawnPoint;

    void Start()
    {
        // Pobierz d�ugo�� segmentu z prefabrykatu
        segmentLength = 20;//roadSegmentPrefab.GetComponent<RoadSegment>().segmentLength;

        // Wygeneruj pocz�tkow� drog�
        nextSpawnPoint = Vector3.zero;
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnSegment();
        }

        // Ustaw pozycj� startow� auta
        if (carTransform != null)
        {
            carTransform.position = nextSpawnPoint + Vector3.up * 2 - Vector3.forward * segmentLength * initialSegments;
        }
    }

    void Update()
    {
        // Sprawd� czy trzeba wygenerowa� nowy segment
        if (Vector3.Distance(carTransform.position, nextSpawnPoint) < spawnDistance)
        {
            SpawnSegment();
        }

        // Usu� stare segmenty
        if (segmentsQueue.Count > 0 &&
            carTransform.position.z - segmentsQueue.Peek().transform.position.z > despawnDistance)
        {
            Destroy(segmentsQueue.Dequeue());
        }
    }

    void SpawnSegment()
    {
        GameObject newSegment = Instantiate(
            roadSegmentPrefab,
            nextSpawnPoint,
            Quaternion.identity
        );

        segmentsQueue.Enqueue(newSegment);
        nextSpawnPoint += Vector3.forward * segmentLength;
    }
}