using System.Collections.Generic;
using UnityEngine;

public class InfiniteTrackGenerator : MonoBehaviour
{
    [Header("Base Settings")]
    public GameObject trackSegmentPrefab;
    public float segmentLength = 1.5f; // D�ugo�� pojedynczego segmentu
    public float trackWidth = 8f;
    public float maxCurvature = 45f;
    public Transform vehicle; // Referencja do pojazdu

    [Header("Generation Settings")]
    public int segmentsAhead = 40; // Liczba segment�w do generowania przed pojazdem
    public int segmentsBehind = 20; // Liczba segment�w do utrzymania za pojazdem
    public float cleanupCheckInterval = 0.5f; // Co ile sekund sprawdza� do usuni�cia

    [Header("Noise Settings")]
    public float noiseFrequency = 0.1f;
    private Vector2 noiseOffset;

    private Queue<GameObject> segmentsQueue = new Queue<GameObject>();
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float nextCleanupTime;
    private int lastCleanSegmentIndex = 0;

    void Start()
    {
        if (vehicle == null)
        {
            Debug.LogError("Vehicle reference not set in TrackGenerator!");
            enabled = false;
            return;
        }

        noiseOffset = new Vector2(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f)
        );

        // Inicjalizacja pozycji startowej
        lastPosition = vehicle.position - vehicle.forward * segmentsBehind * segmentLength;
        lastRotation = vehicle.rotation;

        // Generowanie pocz�tkowej drogi
        GenerateInitialTrack();
    }

    void Update()
    {
        // Generuj nowe segmenty je�li potrzeba
        if (segmentsQueue.Count == 0 ||
            Vector3.Distance(vehicle.position, lastPosition) < segmentsAhead * segmentLength)
        {
            GenerateSegment();
        }

        // Okresowe czyszczenie starych segment�w
        if (Time.time >= nextCleanupTime)
        {
            CleanupOldSegments();
            nextCleanupTime = Time.time + cleanupCheckInterval;
        }
    }

    void GenerateInitialTrack()
    {
        for (int i = 0; i < segmentsAhead + segmentsBehind; i++)
        {
            GenerateSegment();
        }
    }

    void GenerateSegment()
    {
        // Oblicz now� rotacj� na podstawie szumu Perlina
        float noiseValue = Mathf.PerlinNoise(
            segmentsQueue.Count * noiseFrequency + noiseOffset.x,
            noiseOffset.y
        );

        float targetAngle = Mathf.Lerp(-maxCurvature, maxCurvature, noiseValue);
        lastRotation *= Quaternion.Euler(0, targetAngle * Time.deltaTime, 0);

        // Oblicz now� pozycj� (skr�con� o 1/10 d�ugo�ci segmentu)
        float adjustedSegmentLength = segmentLength * 0.9f;
        lastPosition += lastRotation * Vector3.forward * adjustedSegmentLength;

        // Utw�rz segment (zachowaj oryginaln� skal�)
        GameObject segment = Instantiate(trackSegmentPrefab, lastPosition, lastRotation);
        segment.transform.localScale = new Vector3(trackWidth, 1f, segmentLength); // Zachowaj oryginaln� d�ugo�� w skali
        segment.transform.SetParent(transform);

        segmentsQueue.Enqueue(segment);
    }

    void CleanupOldSegments()
    {
        while (segmentsQueue.Count > segmentsAhead + segmentsBehind)
        {
            Destroy(segmentsQueue.Dequeue());
        }

        // Dodatkowe czyszczenie zbyt odleg�ych segment�w
        var segmentsArray = segmentsQueue.ToArray();
        for (int i = lastCleanSegmentIndex; i < segmentsArray.Length; i++)
        {
            if (Vector3.Distance(vehicle.position, segmentsArray[i].transform.position) >
                (segmentsBehind + 10) * segmentLength)
            {
                Destroy(segmentsArray[i]);
                lastCleanSegmentIndex = i + 1;
            }
            else
            {
                break;
            }
        }
    }

    public void ResetTrack()
    {
        // Usu� wszystkie istniej�ce segmenty
        while (segmentsQueue.Count > 0)
        {
            Destroy(segmentsQueue.Dequeue());
        }

        // Zresetuj pozycj� generowania
        lastPosition = vehicle.position - vehicle.forward * segmentsBehind * segmentLength;
        lastRotation = vehicle.rotation;

        // Wygeneruj nowy odcinek pocz�tkowy
        GenerateInitialTrack();

        // Nowy offset szumu dla �wie�ej trasy
        noiseOffset = new Vector2(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f)
        );
    }
}