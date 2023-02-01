using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTesting : MonoBehaviour
{
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        new WaitForSeconds(5);
    }
    // Update is called once per frame
    void Update()
    {
        Invoke("Move", 5);
    }

    void Move() {
        rb.velocity = transform.forward * 10f;
    }
}
