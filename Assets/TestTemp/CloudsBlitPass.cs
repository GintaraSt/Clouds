using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#pragma warning disable CS0618 // Type or member is obsolete
class CloudsBlitPass : ScriptableRenderPass
{
    public static Vector3 _sunDirection = Vector3.forward;

    // used to label this pass in Unity's Frame Debug utility
    private string _profilerTag;
    private ScriptableRenderer _renderer;
    private Material _cloudMat;
    private CloudsSettings _cloudsSettings;

    public CloudsBlitPass(
        string profilerTag,
        RenderPassEvent passEvent,
        CloudsSettings cloudsSettings)
    {
        _profilerTag = profilerTag;
        renderPassEvent = passEvent;
        _cloudsSettings = cloudsSettings;
    }

    public void Setup(ScriptableRenderer renderer)
    {
        _renderer = renderer;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (_cloudMat == null || _cloudMat.shader != _cloudsSettings.shader)
        {
            _cloudMat = new Material(_cloudsSettings.shader);
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(_profilerTag);
        cmd.Clear();

        _cloudsSettings.SetProperties(_cloudMat, -_sunDirection);

        cmd.Blit(_renderer.cameraColorTarget, _renderer.cameraColorTarget, _cloudMat, 0);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}