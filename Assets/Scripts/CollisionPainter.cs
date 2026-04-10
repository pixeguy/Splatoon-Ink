using UnityEngine;

public class CollisionPainter : MonoBehaviour{
    public Color paintColor;
    
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    private void OnCollisionStay(Collision other) {
        Paintable p = other.collider.GetComponent<Paintable>();
        if(p != null){
            Vector3 pos = other.contacts[0].point;
            PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Ray ray = new Ray(transform.position, -transform.up);
        Debug.DrawRay(transform.position, -transform.up * 2f, Color.red);

        if (!other.Raycast(ray, out RaycastHit hit, 2f))
            return;

        Paintable p = hit.collider.GetComponent<Paintable>();
        if (p == null)
            return;

        Transform basis = transform; // use the brush/tool orientation, not the mesh/root

        // Direction you want to count as "behind"
        Vector3 desiredBack = -basis.forward;

        // Remove any component that goes into the surface normal,
        // leaving only motion along the surface.
        Vector3 surfaceBack = Vector3.ProjectOnPlane(desiredBack, hit.normal).normalized;

        if (surfaceBack.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("Surface back direction is too small. Brush forward may be almost parallel to the normal.");
            return;
        }

        float offsetAmount = 0.5f;
        Vector3 offsetPoint = hit.point + surfaceBack * offsetAmount;

        // Reproject onto the same surface by casting along the normal
        Vector3 rayOrigin = offsetPoint + hit.normal * 0.1f;
        Vector3 rayDir = -hit.normal;

        Debug.DrawRay(hit.point, desiredBack * 0.5f, Color.blue);      // raw back
        Debug.DrawRay(hit.point, surfaceBack * 0.5f, Color.yellow);    // back along surface
        Debug.DrawRay(rayOrigin, rayDir * 0.3f, Color.green);          // reprojection ray

        if (hit.collider.Raycast(new Ray(rayOrigin, rayDir), out RaycastHit shiftedHit, 1f))
        {
            PaintManager.instance.paint(
                p,
                shiftedHit.textureCoord,
                radius,
                hardness,
                strength,
                paintColor
            );
        }
    }
}
