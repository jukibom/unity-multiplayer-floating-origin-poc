using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {
    private Transform _transform;
    private Rigidbody _rigidbody;

    [SerializeField] private float thrust = 100;

    void Start() {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) {
            _rigidbody.AddForce(0, 0, thrust);
        }
        if (Input.GetKey(KeyCode.A)) {
            _rigidbody.AddForce(-thrust, 0, 0);
        }
        if (Input.GetKey(KeyCode.S)) {
            _rigidbody.AddForce(0, 0, -thrust);
        }
        if (Input.GetKey(KeyCode.D)) {
            _rigidbody.AddForce(thrust, 0, 0);
        }
        
        // clamp Y and prevent rotation 
        _transform.localPosition = new Vector3(_transform.localPosition.x, 0, _transform.localPosition.z);
        _transform.localRotation = Quaternion.identity;
    }
}
