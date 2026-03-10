using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScaleUpdater : MonoBehaviour
{
    private Vector2 currentScreenResolution;
    public float scalingAdjustment = 10.0f;

    [Range(0f, 1f)]
    public float screenSpaceAlpha = 1.0f; //slider (0–1)

    private Renderer[] childRenderers;
    private Dictionary<Renderer, Vector4> baseAlphaVectors = new Dictionary<Renderer, Vector4>();

    void Start()
    {
        currentScreenResolution = new Vector2(Screen.width, Screen.height);
        AdjustScaleToFitScreen();

        // Cache all renderers under this object
        childRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // Store original _AlphaOverrideChannel values (so we don't compound)
        CacheBaseAlphaVectors();
    }

    void Update()
    {
        // Adjust scale if screen size changes
        if (currentScreenResolution.x != Screen.width || currentScreenResolution.y != Screen.height)
        {
            AdjustScaleToFitScreen();
            currentScreenResolution.x = Screen.width;
            currentScreenResolution.y = Screen.height;
        }

        // Update shader property every frame
        UpdateAlphaOverride();
    }

    private void AdjustScaleToFitScreen()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Ensure there is a camera tagged as 'MainCamera' in the scene.");
            return;
        }

        float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
        float heightBasedOnScreen = (2.0f * Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * distanceToCamera) * scalingAdjustment;
        float widthBasedOnAspect = heightBasedOnScreen * Camera.main.aspect;
        transform.localScale = new Vector3(widthBasedOnAspect, heightBasedOnScreen, 1);
    }

    private void CacheBaseAlphaVectors()
    {
        baseAlphaVectors.Clear();

        foreach (var r in childRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;

            Material mat = r.sharedMaterial;
            if (!mat.HasProperty("_AlphaOverrideChannel")) continue;

            Vector4 originalValue = mat.GetVector("_AlphaOverrideChannel");
            baseAlphaVectors[r] = originalValue;
        }
    }

    private void UpdateAlphaOverride()
    {
        foreach (var r in childRenderers)
        {
            if (r == null || !baseAlphaVectors.ContainsKey(r)) continue;

            Vector4 baseVector = baseAlphaVectors[r];
            Vector4 modifiedVector = baseVector * screenSpaceAlpha;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetVector("_AlphaOverrideChannel", modifiedVector);
            r.SetPropertyBlock(block);
        }
    }
}
