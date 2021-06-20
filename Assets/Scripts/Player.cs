using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mirror;
using Mirror.Experimental;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkBehaviour {
    private Transform _transform;
    private Rigidbody _rigidbody;
    private bool IsReady => _transform && _rigidbody;
    
    [SerializeField] private float thrust = 100;

    void Start() {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();

        // assign a random color, not synced or anything but just enough to tell them apart on each local machine
        GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Random.ColorHSV());

        // non-local initialisation
        if (!isLocalPlayer) {
            // Disable the non-local camera (is there a better way to handle this?)
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
    }

    public override void OnStartLocalPlayer() {
        // register self as the focal object
        FloatingOrigin.instance.focalTransform = transform;
    }
    
    void OnEnable() {
        // perform positional correction like anything else in the world
        FloatingOrigin.OnFloatingOriginCorrection += PositionCorrection;
    }
    
    private void OnDisable() {
        FloatingOrigin.OnFloatingOriginCorrection -= PositionCorrection;
    }

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

            // Send the current floating origin along with the new position and rotation to the server
            CmdSetPosition(FloatingOrigin.instance.Origin, transform.localPosition, transform.rotation, _rigidbody.velocity);
        }
    }

    // This is server-side and should really validate the positions coming in before blindly firing to all the clients!
    [Command]
    void CmdSetPosition(Vector3 origin, Vector3 position, Quaternion rotation, Vector3 velocity) {
        RpcUpdate(origin, position, rotation, velocity);
    }
    
    // On each client, update the position of this object if it's not the local player.
    [ClientRpc]
    void RpcUpdate(Vector3 remoteOrigin, Vector3 position, Quaternion rotation, Vector3 velocity) {
        if (!isLocalPlayer && IsReady) {
            // Calculate the local difference to position based on the local clients' floating origin.
            // If these values are gigantic, the doesn't really matter as they only update at fixed distances.
            // We'll lose precision here but we add our position on top after-the-fact, so we always have
            // local-level precision.
            var offset = remoteOrigin - FloatingOrigin.instance.Origin;
            var localPosition = offset + position;

            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, velocity, 0.5f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, 0.5f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, 0.5f);
            
            // add velocity to position as position would have moved on server at that velocity
            transform.localPosition += velocity * Time.fixedDeltaTime;
        }
    }

    void PositionCorrection(Vector3 offset) {
        if (!isLocalPlayer) {
            transform.position -= offset;
        }
    }
}
