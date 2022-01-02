// RenderPipelineHelper
// by Andy Miira (Andrei Müller), November 2020

using UnityEngine;
using UnityEngine.Rendering;

namespace MirrorMirai.Helpers
{
    public enum RenderPipelines
    {
        BuiltIn,
        URP,
        HDRP
    }
    
    public static class RenderPipelineHelper
    {
        public static RenderPipelines CheckRenderPipeline()
        {
            if (GraphicsSettings.renderPipelineAsset)
            {
                if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                {
                    // HDRP active
                    Debug.Log("HDRP active");
                    return RenderPipelines.HDRP;
                }
                else
                {
                    // URP active
                    Debug.Log("URP active");
                    return RenderPipelines.URP;
                }
            }
            else
            {
                // Built-in RP active
                Debug.Log("Built-in RP active");
                return RenderPipelines.BuiltIn;
            }
        }
    }
}
