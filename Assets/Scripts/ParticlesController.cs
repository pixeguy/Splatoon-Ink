using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesController: MonoBehaviour{
    public Color paintColor;
    
    public float minRadius = 0.05f;
    public float maxRadius = 0.2f;
    public float strength = 1;
    public float hardness = 1;
    [Space]
    ParticleSystem part;
    List<ParticleCollisionEvent> collisionEvents;

    [Header("UV Raycast")]
    [SerializeField] private float rayOffset = 0.03f;
    [SerializeField] private float rayDistance = 0.1f;
    void Start(){
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        //var pr = part.GetComponent<ParticleSystemRenderer>();
        //Color c = new Color(pr.material.color.r, pr.material.color.g, pr.material.color.b, .8f);
        //paintColor = c;
    }

    void OnParticleCollision(GameObject other) {
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

        Paintable p = other.GetComponent<Paintable>();
        if(p != null)
        {
            MeshCollider meshCollider = other.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                return;
            }

            for (int i = 0; i < numCollisionEvents; i++)
            {

                ParticleCollisionEvent ev = collisionEvents[i];

                Vector3 hitPoint = ev.intersection;

                // Use incoming particle direction, then ray back into the surface
                Vector3 incoming = ev.velocity.normalized;
                Vector3 rayDir = incoming;

                // start a bit before the contact point
                Vector3 origin = hitPoint - rayDir * rayOffset;

                if (meshCollider.Raycast(new Ray(origin, rayDir), out RaycastHit hit, rayDistance))
                {
                    Vector2 uv = hit.textureCoord;
                    float radius = Random.Range(minRadius, maxRadius);

                    Debug.Log($"HIT UV: {uv}");

                    PaintManager.instance.paint(p, uv, radius, hardness, strength, paintColor);
                }
            }
        }
    }
}