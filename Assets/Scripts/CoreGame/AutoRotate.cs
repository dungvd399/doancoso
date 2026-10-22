using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public Vector3 speed = new Vector3(0, 50f, 0);
    void Update()
    {
        transform.Rotate(speed * Time.deltaTime);
    }
}
