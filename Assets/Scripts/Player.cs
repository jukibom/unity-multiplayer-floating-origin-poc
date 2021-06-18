using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkBehaviour {
    private Transform _transform;
    private Rigidbody _rigidbody;
    
    private FloatingOrigin _floatingOrigin;
    public Vector3 Origin => _floatingOrigin ? _floatingOrigin.Origin : Vector3.zero;

    [SerializeField] private float thrust = 100;

    void Start() {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        
        // assign a random color, not synced or anything but just enough to tell them apart on each local machine
        GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Random.ColorHSV());

        // non-local initialisation
        if (!isLocalPlayer) {
            // TODO: a better solution for this? 
            // Effectively make the non-local player a static object from a physics perspective
            _rigidbody.isKinematic = true;
            
            // Disable the non-local camera (is there a better way to handle this?)
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }

    }

    void Update()
    {
        // poll for floating origin component
        // TODO: is this necessary here? It definitely feels wrong
        if (!_floatingOrigin) {
            _floatingOrigin = FindObjectOfType<FloatingOrigin>();
        }
        
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

            CmdSetPosition(Origin, transform.localPosition, transform.rotation);
        }
    }

    [Command]
    void CmdSetPosition(Vector3 origin, Vector3 position, Quaternion rotation) {
        RpcUpdate(origin, position, rotation);
    }

    [ClientRpc]
    void RpcUpdate(Vector3 remoteOrigin, Vector3 position, Quaternion rotation) {
        if (!isLocalPlayer) {
            // Calculate the local difference to position based on the local clients' floating origin.
            // If these values are gigantic, the doesn't really matter as they only update at fixed distances.
            // We'll lose precision here but we add our position on top after-the-fact, so we always have
            // local-level precision.
            var offset = remoteOrigin - Origin;
            var localPosition = offset + position;
            
            // Debug.Log("remote player position: remoteOrigin: " + remoteOrigin + ", localOrigin: " + Origin +
            //           ", offset: " + offset + ", position: " + position + ", calculated: " + localPosition);
            
            transform.localPosition = localPosition;
            transform.localRotation = rotation;
        }
    }
}
