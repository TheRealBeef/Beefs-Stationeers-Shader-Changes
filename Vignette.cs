using UnityEngine;

namespace BeefsShaderChanges
{
    public class VignetteEffect : MonoBehaviour
    {
        private Shader _vignetteShader;
        private Shader _separableBlurShader;

        private Material _vignetteMaterial;
        private Material _separableBlurMaterial;

        public float intensity = 0.036f;
        public float blur = 0.0f;
        public float blurSpread = 0.75f;

        public bool InitializeShaders()
        {
            _vignetteShader = AssetBundleLoader.GetShader("Hidden/Vignetting");
            _separableBlurShader = AssetBundleLoader.GetShader("Hidden/SeparableBlur");

            if (_vignetteShader == null)
            {
                BeefsShaderChangesPlugin.Log.LogError("Vignette shader not found in bundle");
                return false;
            }

            if (_separableBlurShader == null)
            {
                BeefsShaderChangesPlugin.Log.LogError("SeparableBlur shader not found in bundle");
                return false;
            }

            _vignetteMaterial = new Material(_vignetteShader);
            _vignetteMaterial.hideFlags = HideFlags.DontSave;

            _separableBlurMaterial = new Material(_separableBlurShader);
            _separableBlurMaterial.hideFlags = HideFlags.DontSave;

            return true;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_vignetteMaterial == null || !enabled)
            {
                Graphics.Blit(source, destination);
                return;
            }

            int rtW = source.width;
            int rtH = source.height;

            bool doPrepass = (Mathf.Abs(blur) > 0.0f || Mathf.Abs(intensity) > 0.0f);

            float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
            const float oneOverBaseSize = 1.0f / 512.0f;

            RenderTexture color = null;
            RenderTexture color2A = null;

            if (doPrepass)
            {
                color = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

                if (Mathf.Abs(blur) > 0.0f && _separableBlurMaterial != null)
                {
                    color2A = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                    Graphics.Blit(source, color2A);

                    for (int i = 0; i < 2; i++)
                    {
                        _separableBlurMaterial.SetVector("offsets", new Vector4(0.0f, blurSpread * oneOverBaseSize, 0.0f, 0.0f));
                        RenderTexture color2B = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                        Graphics.Blit(color2A, color2B, _separableBlurMaterial);
                        RenderTexture.ReleaseTemporary(color2A);

                        _separableBlurMaterial.SetVector("offsets", new Vector4(blurSpread * oneOverBaseSize / widthOverHeight, 0.0f, 0.0f, 0.0f));
                        color2A = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                        Graphics.Blit(color2B, color2A, _separableBlurMaterial);
                        RenderTexture.ReleaseTemporary(color2B);
                    }
                }

                _vignetteMaterial.SetFloat("_Intensity", (1.0f / (1.0f - intensity) - 1.0f));
                _vignetteMaterial.SetFloat("_Blur", (1.0f / (1.0f - blur)) - 1.0f);
                _vignetteMaterial.SetTexture("_VignetteTex", color2A);

                Graphics.Blit(source, color, _vignetteMaterial, 0);
                Graphics.Blit(color, destination);
            }
            else
            {
                Graphics.Blit(source, destination);
            }

            if (color != null)
                RenderTexture.ReleaseTemporary(color);
            if (color2A != null)
                RenderTexture.ReleaseTemporary(color2A);
        }

        private void OnDestroy()
        {
            if (_vignetteMaterial != null)
                DestroyImmediate(_vignetteMaterial);
            if (_separableBlurMaterial != null)
                DestroyImmediate(_separableBlurMaterial);
        }
    }
}