using UnityEngine;

public class Wheel : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;
    public bool wheelTurn; // Czy ko�o skr�tne

    private float currentSteerAngle = 0f;
    private float wheelRotation = 0f;

    void Update()
    {
        UpdateWheelVisuals();
    }

    void UpdateWheelVisuals()
    {
        // Pozycja z WheelCollidera
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion _);
        wheelMesh.position = pos;

        // P�ynny skr�t
        float targetAngle = wheelTurn ? wheelCollider.steerAngle : 0f;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.deltaTime * 10f);

        // Oblicz obr�t tocz�cy si�
        float rotationThisFrame = wheelCollider.rpm / 60f * 360f * Time.deltaTime;
        wheelRotation += rotationThisFrame;
        wheelRotation %= 360f;

        // Ustaw rotacj�: skr�t na Y, toczenie na X
        wheelMesh.localRotation = Quaternion.Euler(wheelRotation, currentSteerAngle, 0f);
    }
}