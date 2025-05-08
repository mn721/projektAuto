using UnityEngine;

public class SmoothTrackGenerator : MonoBehaviour
{
    [Header("Base Settings")]
    public GameObject trackSegmentPrefab;
    public int totalSegments = 20;
    public float segmentLength = 10f;
    public float trackWidth = 8f;
    public float maxAngle = 30f;

    [Header("Visuals")]
    public Material trackMaterial;
    public bool addGuardrails = true;
    public GameObject guardrailPrefab;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private GameObject lastSegment;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        GenerateConnectedTrack();
    }

    void GenerateConnectedTrack()
    {
        for (int i = 0; i < totalSegments; i++)
        {
            // Losowy k�t zakr�tu (bardziej p�ynne przej�cia)
            float angle = Mathf.Lerp(-maxAngle, maxAngle, Mathf.PerlinNoise(i * 0.3f, 0));

            // Oblicz now� pozycj� i rotacj�
            Quaternion newRotation = lastRotation * Quaternion.Euler(0, angle, 0);
            Vector3 newPosition = lastPosition + newRotation * Vector3.forward * segmentLength;

            // Stw�rz segment
            GameObject segment = Instantiate(
                trackSegmentPrefab,
                newPosition,
                newRotation
            );

            // Dostosuj skal� i nazw�
            segment.transform.localScale = new Vector3(trackWidth, 1f, segmentLength);
            segment.name = "TrackSegment_" + i;

            // Dodaj collider (je�li prefab go nie ma)
            if (segment.GetComponent<MeshCollider>() == null)
            {
                MeshCollider collider = segment.AddComponent<MeshCollider>();
                collider.convex = false;
            }

            // Po��cz z poprzednim segmentem (wa�ne dla p�ynno�ci)
            if (lastSegment != null)
            {
                ConnectSegments(lastSegment, segment);
            }

            //// Dodaj barierki (opcjonalne)
            //if (addGuardrails && guardrailPrefab != null)
            //{
            //    AddGuardrails(segment);
            //}

            lastPosition = newPosition;
            lastRotation = newRotation;
            lastSegment = segment;
        }
    }

    void ConnectSegments(GameObject prevSegment, GameObject nextSegment)
    {
        // Dopasuj pozycj�, aby unikn�� przerw
        Vector3 correctedPosition = prevSegment.transform.position +
                                   prevSegment.transform.forward * (segmentLength * 0.4f) +
                                   nextSegment.transform.forward * (segmentLength * 0.4f);

        nextSegment.transform.position = correctedPosition;
    }

    //void AddGuardrails(GameObject segment)
    //{
    //    Vector3 leftRailPos = segment.transform.position + (-segment.transform.right * (trackWidth * 0.5f));
    //    Vector3 rightRailPos = segment.transform.position + (segment.transform.right * (trackWidth * 0.5f));

    //    Instantiate(guardrailPrefab, leftRailPos, segment.transform.rotation, segment.transform);
    //    Instantiate(guardrailPrefab, rightRailPos, segment.transform.rotation, segment.transform);
    //}
}