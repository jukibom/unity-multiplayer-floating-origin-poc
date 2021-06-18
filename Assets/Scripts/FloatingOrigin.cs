using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingOrigin : MonoBehaviour {

    [SerializeField] public Transform focalTransform; // this should be the client player

    // Distance required to perform a correction. If 0, will occur every frame.
    [SerializeField] public float correctionDistance = 0.0f; 
    
    // The target world transform to manipulate
    private Transform _worldTransform;

    public Vector3 Origin => _worldTransform.position * -1;
    public Vector3 FocalObjectPosition => focalTransform.position + Origin;

    void OnEnable() {
        _worldTransform = GameObject.Find("World")?.transform;
        if (!_worldTransform) {
            Debug.LogWarning("Floating Origin failed to find target World! Is one loaded?");
        }
    }

    private void OnDisable() {
        _worldTransform = null;
    }

    void Update() {
        // query for local player
        if (!focalTransform) {
            foreach (var player in FindObjectsOfType<Player>() as NetworkBehaviour[]) {
                if (player.isLocalPlayer) {
                    focalTransform = player.transform;
                }
            }
        }
        // if we have one, perform the floating origin fix
        else if (_worldTransform && focalTransform.position.magnitude > correctionDistance) {
            _worldTransform.position -= focalTransform.position;
            focalTransform.position = Vector3.zero;
        }
    }

}