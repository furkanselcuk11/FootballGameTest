using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // Takip edilecek hedef nesne (�rn. karakter)

    public float distance = 5.0f;   // Kameran�n hedef nesneye olan mesafesi
    public float height = 3.0f;     // Kameran�n hedef nesneye olan y�ksekli�i
    public float smoothSpeed = 0.125f; // Kamera yumu�ak takip h�z�

    private Vector3 offset;

    void Start()
    {
        CursorVisible();
        // Kameran�n ba�lang�� pozisyonunu hesapla
        offset = new Vector3(0, height, -distance);
    }

    void LateUpdate()
    {
        // Hedef nesnenin y�n�ne ve konumuna g�re kamera pozisyonunu g�ncelle
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Kamera hedef nesneyi s�rekli takip etsin diye, hedef nesnenin y�n�ne do�ru d�nmesini sa�la
        transform.LookAt(target);
    }
    void CursorVisible()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}