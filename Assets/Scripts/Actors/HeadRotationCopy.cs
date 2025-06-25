using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadRotationCopy : MonoBehaviour
{
    public Transform Head;
    public float LockedHeight = 1.58f;
    // Update is called once per frame
    void Update()
    {
        if (Head == null || Head.gameObject == null)
        {
            this.enabled = false;
            Destroy(gameObject);
            return;
        }

        Vector3 pos = transform.position;
        pos.x = Head.position.x;
        pos.y = LockedHeight;
        pos.z = Head.position.z;
        transform.position = pos;
        transform.rotation = Head.rotation;
    }
}
