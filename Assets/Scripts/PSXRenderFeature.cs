using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PSXRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material ditheringMaterial;
        public int targetWidth = 320;
        public int targetHeight = 240;
    }

    public Settings settings = new Settings();
    private PSXRenderPass renderPass;

    public override void Create()
    {
        renderPass = new PSXRenderPass(settings);
        renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.ditheringMaterial == null)
        {
            Debug.LogWarning("PSX Dithering material is not assigned!");
            return;
        }

        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        renderPass?.Dispose();
    }

    class PSXRenderPass : ScriptableRenderPass
    {
        private Settings settings;
        private Material material;
        private RTHandle lowResHandle;
        private RTHandle tempHandle;

        public PSXRenderPass(Settings settings)
        {
            this.settings = settings;
            this.material = settings.ditheringMaterial;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            var lowResDescriptor = descriptor;
            lowResDescriptor.width = settings.targetWidth;
            lowResDescriptor.height = settings.targetHeight;

            RenderingUtils.ReAllocateIfNeeded(ref lowResHandle, lowResDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_LowResTexture");
            RenderingUtils.ReAllocateIfNeeded(ref tempHandle, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_TempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("PSX Post Process");

            // Blit using RTHandle's implicit conversion to RenderTargetIdentifier
            RenderTargetIdentifier cameraRT = cameraColorTarget;
            RenderTargetIdentifier lowResRT = lowResHandle;
            RenderTargetIdentifier tempRT = tempHandle;

            // Downscale
            cmd.Blit(cameraRT, lowResRT);
            
            // Apply dithering
            cmd.Blit(lowResRT, tempRT, material);
            
            // Copy back
            cmd.Blit(tempRT, cameraRT);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose()
        {
            lowResHandle?.Release();
            tempHandle?.Release();
        }
    }
}