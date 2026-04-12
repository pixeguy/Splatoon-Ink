using System.Collections.Generic;
using UnityEngine;

public class EraseParticleGun : MonoBehaviour
{
    [SerializeField] private ParticleSystem part;
    [SerializeField] private float rayOffset = 0.01f;
    [SerializeField] private float rayDistance = 0.1f;

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        //var pr = part.GetComponent<ParticleSystemRenderer>();
        //Color c = new Color(pr.material.color.r, pr.material.color.g, pr.material.color.b, .8f);
        //paintColor = c;
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        EnemyMovement enemy = other.GetComponent<EnemyMovement>();
        if (enemy != null)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                ParticleCollisionEvent ev = collisionEvents[i];
                Vector3 incoming = ev.velocity.normalized;
                incoming += new Vector3(0, 0.5f, 0);
                enemy.PushbackSelf(incoming);
            }
        }

        EraseGun erase = other.GetComponent<EraseGun>();
        if (erase == null || erase.Cleaned)
            return;

        MeshCollider meshCollider = other.GetComponent<MeshCollider>();
        if (meshCollider == null)
            return;

        for (int i = 0; i < numCollisionEvents; i++)
        {
            ParticleCollisionEvent ev = collisionEvents[i];

            Vector3 hitPoint = ev.intersection;

            // Same idea as your paint code
            Vector3 incoming = ev.velocity.normalized;
            Vector3 rayDir = incoming;
            Vector3 origin = hitPoint - rayDir * rayOffset;

            if (meshCollider.Raycast(new Ray(origin, rayDir), out RaycastHit hit, rayDistance))
            {
                Vector2 uv = hit.textureCoord;
                Debug.Log("Erase UV: " + uv);

                erase.ErazeAtUV(uv);
                //erase.percentChecked = false;
                //erase.StopEraze();
                //erase.percentChecked = false;
            }
        }
    }
}