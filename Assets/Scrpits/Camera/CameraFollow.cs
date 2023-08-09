using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // Takip edilecek hedef nesne (örn. karakter)

    public float distance = 5.0f;   // Kameranýn hedef nesneye olan mesafesi
    public float height = 3.0f;     // Kameranýn hedef nesneye olan yüksekliði
    public float smoothSpeed = 0.125f; // Kamera yumuþak takip hýzý

    private Vector3 offset;

    void Start()
    {
        CursorVisible();
        // Kameranýn baþlangýç pozisyonunu hesapla
        offset = new Vector3(0, height, -distance);
    }

    void LateUpdate()
    {
        // Hedef nesnenin yönüne ve konumuna göre kamera pozisyonunu güncelle
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Kamera hedef nesneyi sürekli takip etsin diye, hedef nesnenin yönüne doðru dönmesini saðla
        transform.LookAt(target);
    }
    void CursorVisible()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}