using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NiTest : MonoBehaviour
{
    [SerializeField] private Vector3 anchor = new Vector3();
    private Vector3 axis = new Vector3(0,0,1);
    private float height;
    private void Update()
    {
        height = GetColliderHeight(transform.GetComponent<BoxCollider>());
        Debug.Log(height);
        if (Input.GetKeyDown(KeyCode.A))
        {
            AssembleOne(Vector3.left);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            AssembleOne(Vector3.right);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            AssembleOne(Vector3.back);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            AssembleOne(Vector3.forward);
        }
    }

    void AssembleOne(Vector3 dir)
    {
        AnchorCalculations(dir);
        axis = Vector3.Cross(Vector3.up, dir);
        transform.RotateAround(anchor,axis,90f);
    }

    void AnchorCalculations(Vector3 dir)
    {
        var scale = transform.localScale;
        var halfscale = new Vector3(scale.x * .5f, scale.y * .5f, scale.z * .5f);
        var ancscale = (Vector3.down + dir);
        if (height>1.5f)
        {
            anchor = transform.position + new Vector3(halfscale.x * ancscale.x, halfscale.y * ancscale.y, halfscale.z * ancscale.z);
        }
        else if (height<1.5f)
        {
            anchor = transform.position + new Vector3(halfscale.x * ancscale.x, 0f, halfscale.z * ancscale.z);
        }

    }
    
    float GetColliderHeight(BoxCollider bc)
    {
        // box size is in local space; convert to world extents
        Vector3 worldSize = Vector3.Scale(bc.size, bc.transform.lossyScale);
        // worldSize components are axis-aligned in object local axes; project onto world up
        Transform t = bc.transform;
        float h = Mathf.Abs(Vector3.Dot(t.right, Vector3.up)) * worldSize.x
                  + Mathf.Abs(Vector3.Dot(t.up, Vector3.up))    * worldSize.y
                  + Mathf.Abs(Vector3.Dot(t.forward, Vector3.up)) * worldSize.z;
        return h;
    }
}