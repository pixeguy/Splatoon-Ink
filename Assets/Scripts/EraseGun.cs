using UnityEngine;

public class EraseGun : MonoBehaviour
{
    [Header("Visual Mask")]
    public RenderTexture cleanedAreaMask;
    public Renderer rend;
    public Material brushMaterial;

    [Header("Progress")]
    public float percentClean;
    public bool percentChecked;
    public bool Cleaned;

    [Header("Brush")]
    [SerializeField] private float brushSize = 0.05f;
    [SerializeField] private float brushStrength = 1.0f;

    [Header("Grid Settings")]
    [SerializeField] private int gridResolution = 128;

    private bool[,] cleanedGrid;
    private int cleanedCellCount;
    private int totalCellCount;

    private bool initialized = false;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (initialized)
            return;

        cleanedAreaMask = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        cleanedAreaMask.Create();

        Graphics.Blit(Texture2D.blackTexture, cleanedAreaMask);

        if (rend == null)
            rend = GetComponent<Renderer>();

        if (rend != null)
        {
            rend.material = new Material(rend.material);
            rend.material.SetTexture("_CleanedArea", cleanedAreaMask);
        }
        else
        {
            Debug.LogError($"No Renderer found on {gameObject.name}", this);
        }

        cleanedGrid = new bool[gridResolution, gridResolution];
        cleanedCellCount = 0;
        totalCellCount = gridResolution * gridResolution;

        percentClean = 0f;
        percentChecked = true;
        Cleaned = false;

        initialized = true;
    }

    public void ErazeAtUV(Vector2 uv)
    {
        Initialize();

        Debug.Log("UV: " + uv);

        DrawOnMask(uv);
        MarkGrid(uv);

        percentChecked = false;
    }

    public void StopEraze()
    {
        if (!percentChecked)
        {
            percentClean = (float)cleanedCellCount / totalCellCount * 100f;

            if (percentClean >= 99.9f)
            {
                percentClean = 100f;
                Cleaned = true;
            }

            percentChecked = true;
        }
    }

    void DrawOnMask(Vector2 uv)
    {
        if (brushMaterial == null || cleanedAreaMask == null)
            return;

        brushMaterial.SetVector("_BrushUV", new Vector4(uv.x, uv.y, 0, 0));
        brushMaterial.SetFloat("_BrushSize", brushSize);
        brushMaterial.SetFloat("_Strength", brushStrength);

        RenderTexture temp = RenderTexture.GetTemporary(cleanedAreaMask.width, cleanedAreaMask.height);

        Graphics.Blit(cleanedAreaMask, temp);
        Graphics.Blit(temp, cleanedAreaMask, brushMaterial);

        RenderTexture.ReleaseTemporary(temp);
    }

    void MarkGrid(Vector2 uv)
    {
        // Clamp UV so it stays inside the grid
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int centerX = Mathf.RoundToInt(uv.x * (gridResolution - 1));
        int centerY = Mathf.RoundToInt(uv.y * (gridResolution - 1));

        // Convert brush size from UV space into grid radius
        float radius = brushSize * gridResolution;

        int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - radius));
        int maxX = Mathf.Min(gridResolution - 1, Mathf.CeilToInt(centerX + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - radius));
        int maxY = Mathf.Min(gridResolution - 1, Mathf.CeilToInt(centerY + radius));

        float radiusSqr = radius * radius;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                float dx = x - centerX;
                float dy = y - centerY;

                if (dx * dx + dy * dy <= radiusSqr)
                {
                    if (!cleanedGrid[x, y])
                    {
                        cleanedGrid[x, y] = true;
                        cleanedCellCount++;
                    }
                }
            }
        }

        percentClean = (float)cleanedCellCount / totalCellCount * 100f;

        if (percentClean >= 99.9f)
        {
            percentClean = 100f;
            Cleaned = true;
        }
    }
}