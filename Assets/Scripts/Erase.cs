using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erase : MonoBehaviour
{
    public RenderTexture cleanedAreaMask;

    public Renderer rend;

    public Material brushMaterial;

    // Start is called before the first frame update
    void Start()
    {
        cleanedAreaMask = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        cleanedAreaMask.Create();

        //Texture2D blueTex = new Texture2D(1, 1);
        //blueTex.SetPixel(0, 0, Color.blue);
        //blueTex.Apply();

        //Graphics.Blit(blueTex, cleanedAreaMask);

        rend = GetComponent<Renderer>();
        rend.material.SetTexture("_CleanedArea", cleanedAreaMask);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) // hold click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Eraze(hit);
            }
        }
    }

    void Eraze(RaycastHit hit)
    {
        Vector2 uv = hit.textureCoord;
        Debug.Log("UV: " + uv);

        DrawOnMask(uv);
    }


    void DrawOnMask(Vector2 uv)
    {
        brushMaterial.SetVector("_BrushUV", new Vector4(uv.x, uv.y, 0, 0));
        brushMaterial.SetFloat("_BrushSize", 0.05f);
        brushMaterial.SetFloat("_Strength", 1.0f);

        RenderTexture temp = RenderTexture.GetTemporary(cleanedAreaMask.width, cleanedAreaMask.height);

        Graphics.Blit(cleanedAreaMask, temp);
        Graphics.Blit(temp, cleanedAreaMask, brushMaterial);

        RenderTexture.ReleaseTemporary(temp);
    }
}
