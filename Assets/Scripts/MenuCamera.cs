using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
	public float rotateSpeed = 10;
    void Update()
    {
		transform.Rotate(0, Time.deltaTime * rotateSpeed, 0,Space.World);
    }
}
