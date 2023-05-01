using UnityEngine.Rendering.Universal;

public class CloudsBlitFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CloudsBlitSettings
    {
        public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
        public CloudsSettings CloudsSettings;
    }

    // MUST be named "settings" (lowercase) to be shown in the Render Features inspector
    public CloudsBlitSettings settings = new CloudsBlitSettings();

    private CloudsBlitPass _myRenderPass;

    public override void Create()
    {
        _myRenderPass = new CloudsBlitPass(
          "Atmosphere Pass",
          settings.WhenToInsert,
          settings.CloudsSettings
        );
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _myRenderPass.Setup(renderer);
        renderer.EnqueuePass(_myRenderPass);
    }
}