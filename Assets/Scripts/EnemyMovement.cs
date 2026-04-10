using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 3f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Quaternion rot = Quaternion.LookRotation(target.position - transform.position);
        rb.MoveRotation(rot);
        Vector3 forward = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forward);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var p = collision.gameObject.GetComponent<PlayerMover>();
        if (p != null)
        {
            Vector3 dir = collision.contacts[0].point - transform.position;
            dir.y = 0;
            dir.Normalize();
            dir.y = 1f;
            p.ApplyImpulse(dir * 17f);
        }
    }

    public void PushbackSelf(Vector3 dir)
    {
        rb.AddForce(dir, ForceMode.Impulse);
    }
}
