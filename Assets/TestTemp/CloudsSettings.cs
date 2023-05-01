using UnityEngine;

[CreateAssetMenu(menuName = "Clouds")]
public class CloudsSettings : ScriptableObject
{
    [Header("Main")]
    public Shader shader;
    public float innerShellRadius = 400;
    public float outerShellRadius = 500;
    public float animSpeed;
    public Vector3 cloudTestParams;

    [Header("March settings")]
    public float minMainStepSize = 0;
    public int numStepsMain = 5;
    public int numStepsLight = 8;
    public float rayOffsetStrength;

    [Header("Base Shape")]
    public float cloudScale = 1;
    public float densityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header("Detail")]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;


    [Header("Lighting")]
    public float lightAbsorptionThroughCloud = 1;
    public float lightAbsorptionTowardSun = 1;
    [Range(0, 1)]
    public float darknessThreshold = .2f;
    [Range(0, 1)]
    public float forwardScattering = .83f;
    [Range(0, 1)]
    public float backScattering = .3f;
    [Range(0, 1)]
    public float baseBrightness = .8f;
    [Range(0, 1)]
    public float phaseFactor = .15f;

    [Header("Noise")]
    public Texture3D shapeTexture;
    public Texture3D detailTexture;
    public ComputeShader copyShader;

    private RenderTexture _shapeTexture;
    private RenderTexture _detailTexture;

    bool settingsUpToDate;

    public void FlagForUpdate()
    {
        settingsUpToDate = false;
    }

    public void SetProperties(Material material, Vector3 directionToSun)
    {
        if (!settingsUpToDate || Application.isEditor)
        {
            // Validate inputs
            numStepsLight = Mathf.Max(1, numStepsLight);

            // Noise
            CreateTexture(ref _shapeTexture, 64, shapeTexture);
            CreateTexture(ref _detailTexture, 64, detailTexture);
            material.SetTexture("NoiseTex", _shapeTexture);
            material.SetTexture("DetailNoiseTex", _detailTexture);

            material.SetTexture("NoiseTex", shapeTexture);
            material.SetTexture("DetailNoiseTex", detailTexture);

            material.SetFloat("scale", cloudScale);
            material.SetFloat("densityMultiplier", densityMultiplier);
            material.SetFloat("densityOffset", densityOffset);
            material.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
            material.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
            material.SetFloat("darknessThreshold", darknessThreshold);
            material.SetVector("params", cloudTestParams);
            material.SetFloat("rayOffsetStrength", rayOffsetStrength);

            material.SetFloat("detailNoiseScale", detailNoiseScale);
            material.SetFloat("detailNoiseWeight", detailNoiseWeight);
            material.SetVector("shapeOffset", shapeOffset);
            material.SetVector("detailOffset", detailOffset);
            material.SetVector("detailWeights", detailNoiseWeights);
            material.SetVector("shapeNoiseWeights", shapeNoiseWeights);
            material.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));

            material.SetFloat("innerShellRadius", innerShellRadius);
            material.SetFloat("outerShellRadius", outerShellRadius);
            material.SetFloat("animSpeed", animSpeed);

            material.SetFloat("minMainStepSize", minMainStepSize);
            material.SetInt("numStepsLight", numStepsLight);
            material.SetInt("numStepsMain", numStepsMain);
            material.SetVector("dirToSun", directionToSun);

            // Set debug params
            SetDebugParams(material);

            settingsUpToDate = true;
        }
    }

    void CreateTexture(ref RenderTexture texture, int resolution, Texture3D source)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated() || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format)
        {
            //Debug.Log ("Create tex: update noise: " + updateNoise);
            if (texture != null)
            {
                texture.Release();
            }
            texture = new RenderTexture(resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.name = name;

            texture.Create();
            Load(source, texture);
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
    }

    public void Load(Texture3D source, RenderTexture target)
    {
        if (source != null && source.width == target.width)
        {
            copyShader.SetTexture(0, "tex", source);
            copyShader.SetTexture(0, "renderTex", target);
            int numThreadGroups = Mathf.CeilToInt(source.width / 8f);
            copyShader.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
        }
    }

    void OnValidate()
    {
        FlagForUpdate();
    }

    void SetDebugParams(Material material)
    {

        //var noise = FindObjectOfType<NoiseGenerator>();
        //var weatherMapGen = FindObjectOfType<WeatherMap>();

        //int debugModeIndex = 0;
        //if (noise.viewerEnabled)
        //{
        //    debugModeIndex = (noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        //}
        //if (weatherMapGen.viewerEnabled)
        //{
        //    debugModeIndex = 3;
        //}

        //material.SetInt("debugViewMode", debugModeIndex);
        //material.SetFloat("debugNoiseSliceDepth", noise.viewerSliceDepth);
        //material.SetFloat("debugTileAmount", noise.viewerTileAmount);
        //material.SetFloat("viewerSize", noise.viewerSize);
        //material.SetVector("debugChannelWeight", noise.ChannelMask);
        //material.SetInt("debugGreyscale", (noise.viewerGreyscale) ? 1 : 0);
        //material.SetInt("debugShowAllChannels", (noise.viewerShowAllChannels) ? 1 : 0);
    }
}
