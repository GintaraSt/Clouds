using UnityEngine;

[CreateAssetMenu(menuName = "Clouds")]
public class CloudsSettings : ScriptableObject
{
    const string headerDecoration = " --- ";
    [Header(headerDecoration + "Main" + headerDecoration)]
    public Shader shader;
    public int width;
    public int height;
    public int depth;
    public Vector3 cloudTestParams;

    [Header("March settings" + headerDecoration)]
    public int numStepsLight = 8;
    public float rayOffsetStrength;

    [Header(headerDecoration + "Base Shape" + headerDecoration)]
    public float cloudScale = 1;
    public float densityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header(headerDecoration + "Detail" + headerDecoration)]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;


    [Header(headerDecoration + "Lighting" + headerDecoration)]
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

    [Header(headerDecoration + "Animation" + headerDecoration)]
    public float timeScale = 1;
    public float baseSpeed = 1;
    public float detailSpeed = 2;

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
        //if (!settingsUpToDate || Application.isEditor)
        //{
            // Validate inputs
            numStepsLight = Mathf.Max(1, numStepsLight);

            // Noise
            CreateTexture(ref _shapeTexture, 1, shapeTexture);
            CreateTexture(ref _detailTexture, 1, detailTexture);
            material.SetTexture("NoiseTex", _shapeTexture);
            material.SetTexture("DetailNoiseTex", _detailTexture);

            //// Weathermap
            //var weatherMapGen = FindObjectOfType<WeatherMap>();
            //if (!Application.isPlaying)
            //{
            //    weatherMapGen.UpdateMap();
            //}
            //material.SetTexture("WeatherMap", weatherMapGen.weatherMap);

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

            var containerVolume = new Vector3(width, height, depth);
            material.SetVector("boundsMin", -containerVolume / 2);
            material.SetVector("boundsMax", containerVolume / 2);

            material.SetInt("numStepsLight", numStepsLight);

            material.SetVector("mapSize", new Vector4(width, height, depth, 0));

            material.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
            material.SetFloat("baseSpeed", baseSpeed);
            material.SetFloat("detailSpeed", detailSpeed);

            // Set debug params
            SetDebugParams(material);

            settingsUpToDate = true;
        //}
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
