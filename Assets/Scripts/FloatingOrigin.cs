using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour {

    public static FloatingOrigin instance { get; private set; }
    public delegate void FloatingOriginCorrectionAction(Vector3 offset);
    public static event FloatingOriginCorrectionAction OnFloatingOriginCorrection;
    
    // The object to track - this should be the local client player
    [SerializeField] public Transform focalTransform; 
    
    // Distance required to perform a correction. If 0, will occur every frame.
    [SerializeField] public float correctionDistance = 100.0f;

    public Vector3 Origin { get; private set; }
    public Vector3 FocalObjectPosition => focalTransform.position + Origin;

    void Awake() {
        // singleton shenanigans
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Update() {
        
        // if we have a focal object, perform the floating origin fix
        if (focalTransform && focalTransform.position.magnitude > correctionDistance) {
            var focalPosition = focalTransform.position;
            Origin += focalPosition;
            
            // update all non-local players too (positional updates from clients will do this but there may be a delay)
            // TODO: a better system for prefetching theses, this is slow but adequate for demonstration
            // foreach (var player in FindObjectsOfType<Player>() as NetworkBehaviour[]) {
            //     if (!player.isLocalPlayer) {
            //         player.transform.position -= focalTransform.position;
            //     }
            // }

            OnFloatingOriginCorrection?.Invoke(focalPosition);

            // reset focal object (local player) to 0,0,0
            focalTransform.position = Vector3.zero;
        }
    }
}