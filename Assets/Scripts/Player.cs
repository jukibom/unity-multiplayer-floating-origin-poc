using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkBehaviour {
    private Transform _transform;
    private Rigidbody _rigidbody;

    [SerializeField] private float thrust = 100;

    void Start() {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        if (!isLocalPlayer) {
            _rigidbody.isKinematic = true;
        }

        GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
    }
    // Update is called once per frame
    void Update()
    {
        // only apply to the local client
        if (isLocalPlayer) {
            // very basic movement mechanics
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

            CmdSetPosition(transform.position, transform.rotation);
        }
    }

    [Command]
    void CmdSetPosition(Vector3 position, Quaternion rotation) {
        RpcUpdate(position, rotation);
    }

    [ClientRpc]
    void RpcUpdate(Vector3 position, Quaternion rotation) {
        if (!isLocalPlayer) {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
