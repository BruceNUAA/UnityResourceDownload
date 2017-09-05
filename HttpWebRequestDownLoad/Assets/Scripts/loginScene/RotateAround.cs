using UnityEngine;
using System.Collections;

public class RotateAround : MonoBehaviour
{
    public float rotateSpeed;
    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.up, Time.deltaTime * rotateSpeed);
    }
}
