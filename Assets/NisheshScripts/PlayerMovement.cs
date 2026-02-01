using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float rollSpeed = 3f;
    private bool isMoving;

    private void Update()
    {
        if (isMoving)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Assemble(Vector3.left);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Assemble(Vector3.back);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Assemble(Vector3.right);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Assemble(Vector3.forward);
        }
        
    }
    void Assemble(Vector3 dir)
    {
        var anchor = transform.position + (Vector3.down+dir) * 0.5f;
        var axis = Vector3.Cross(Vector3.up, dir);
        StartCoroutine(RollPlayer(anchor,axis));
    }

    IEnumerator RollPlayer(Vector3 anchor,Vector3 axis)
    {
        isMoving = true;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        for (int i = 0; i < 90/rollSpeed; i++)
        {
            transform.RotateAround(anchor,axis,rollSpeed);
            yield return new WaitForSeconds(0.01f);
        }
        transform.GetComponent<Rigidbody>().isKinematic = false;
        isMoving = false;
    }
}