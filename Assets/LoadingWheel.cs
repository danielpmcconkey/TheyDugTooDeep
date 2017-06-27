using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWheel : MonoBehaviour {

    private bool _rotate = false;
    
    // Update is called once per frame
    void Update () {

        if (_rotate)
        {
            transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z - 20 * Time.deltaTime));
            if (transform.rotation.z < -360) transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z +360));
        }
	}
    public void setRotatation(bool isRotating)
    {
        _rotate = isRotating;
    }
}
