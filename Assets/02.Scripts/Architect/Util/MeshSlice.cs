using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshSlice : MonoBehaviour
{
    [SerializeField] private Transform[] slicePivots;

    void Start()
    {
        GameObject[] gameObjects = MeshCut.Cut(gameObject, slicePivots[0].position, Vector3.left, GetComponent<MeshRenderer>().material);

        Destroy(gameObjects[1]);

        gameObjects = MeshCut.Cut(gameObject, slicePivots[1].position, Vector3.right, GetComponent<MeshRenderer>().material);

        Destroy(gameObjects[1]);
    }
}
