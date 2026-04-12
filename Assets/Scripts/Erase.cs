using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Video;

public class Erase : MonoBehaviour
{
    public RenderTexture cleanedAreaMask;
    public Texture2D cleanedArea;

    public Renderer rend;

    public Material brushMaterial;

    public float percentClean;

    public bool percentChecked;

    public bool Cleaned;


    // Start is called before the first frame update
    void Start()
    {
        cleanedAreaMask = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        cleanedAreaMask.Create();

        Graphics.Blit(Texture2D.blackTexture, cleanedAreaMask);

        rend = GetComponent<Renderer>();
        rend.material = new Material(rend.material);
        rend.material.SetTexture("_CleanedArea", cleanedAreaMask);

        percentChecked = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) // hold click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ErazeAction(ray);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopEraze();
        }
    }

    void ErazeAction(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<Erase>(out Erase erase))
            {
                if (!erase.Cleaned)
                {
                    erase.Eraze(hit);
                    erase.percentChecked = false;
                }
            }
        }
    }

    void StopEraze()
    {
        if (!this.percentChecked)
        {
            percentClean = this.CheckPercentage();
            if(percentClean == 100)
            {
                Cleaned = true;
            }
            this.percentChecked = true;
        }
    }

    void Eraze(RaycastHit hit)
    {
        Vector2 uv = hit.textureCoord;
        Debug.Log("UV: " + uv);

        DrawOnMask(uv);
    }

    float CheckPercentage()
    {
        RenderTexture currentRT = RenderTexture.active;

        RenderTexture.active = cleanedAreaMask;

        Texture2D tex = new Texture2D(cleanedAreaMask.width, cleanedAreaMask.height, TextureFormat.RGBA32, false);

        tex.ReadPixels(new Rect(0, 0, cleanedAreaMask.width, cleanedAreaMask.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        int width = tex.width;
        int height = tex.height;

        int total = width * height;
        float red = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (tex.GetPixel(i, j).r == 1.0f)
                {
                    red++;
                }
            }
        }

        cleanedArea = tex;

        return red / total * 100;
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
