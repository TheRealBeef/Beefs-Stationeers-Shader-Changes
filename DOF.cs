using UnityEngine;

namespace BeefsShaderChanges
{
    public class DepthOfFieldEffect : MonoBehaviour
    {
        public enum BlurSampleCount
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        private Shader _dofHdrShader;
        private Material _dofHdrMaterial;
        private Camera _camera;

        public float focalLength = 10.0f;
        public float focalSize = 0.05f;
        public float aperture = 0.5f;
        public float maxBlurSize = 2.0f;
        public bool highResolution = false;
        public BlurSampleCount blurSampleCount = BlurSampleCount.High;
        public bool nearBlur = false;
        public float foregroundOverlap = 1.0f;

        private float focalDistance01 = 10.0f;
        private float internalBlurWidth = 1.0f;

        public bool InitializeShader()
        {
            _dofHdrShader = AssetBundleLoader.GetShader("Hidden/Dof/DepthOfFieldHdr");
            if (_dofHdrShader == null)
            {
                BeefsShaderChangesPlugin.Log.LogError("DOF shader not found in bundle");
                return false;
            }

            _dofHdrMaterial = new Material(_dofHdrShader);
            _dofHdrMaterial.hideFlags = HideFlags.DontSave;

            _camera = GetComponent<Camera>();
            if (_camera != null)
            {
                _camera.depthTextureMode |= DepthTextureMode.Depth;
            }

            return true;
        }

        private float FocalDistance01(float worldDist)
        {
            if (_camera == null) return 0.1f;
            return _camera.WorldToViewportPoint((worldDist - _camera.nearClipPlane) * _camera.transform.forward + _camera.transform.position).z /
                   (_camera.farClipPlane - _camera.nearClipPlane);
        }

        private void WriteCoc(RenderTexture fromTo, bool fgDilate)
        {
            if (_dofHdrMaterial == null) return;

            _dofHdrMaterial.SetTexture("_FgOverlap", null);

            if (nearBlur && fgDilate)
            {
                int rtW = fromTo.width / 2;
                int rtH = fromTo.height / 2;

                RenderTexture temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(fromTo, temp2, _dofHdrMaterial, 4);

                float fgAdjustment = internalBlurWidth * foregroundOverlap;

                _dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, fgAdjustment, 0.0f, fgAdjustment));
                RenderTexture temp1 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(temp2, temp1, _dofHdrMaterial, 2);
                RenderTexture.ReleaseTemporary(temp2);

                _dofHdrMaterial.SetVector("_Offsets", new Vector4(fgAdjustment, 0.0f, 0.0f, fgAdjustment));
                temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(temp1, temp2, _dofHdrMaterial, 2);
                RenderTexture.ReleaseTemporary(temp1);

                _dofHdrMaterial.SetTexture("_FgOverlap", temp2);
                Graphics.Blit(fromTo, fromTo, _dofHdrMaterial, 13);
                RenderTexture.ReleaseTemporary(temp2);
            }
            else
            {
                Graphics.Blit(fromTo, fromTo, _dofHdrMaterial, 0);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_dofHdrMaterial == null || !enabled || _camera == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (aperture < 0.0f) aperture = 0.0f;
            if (maxBlurSize < 0.1f) maxBlurSize = 0.1f;
            focalSize = Mathf.Clamp(focalSize, 0.0f, 2.0f);
            internalBlurWidth = Mathf.Max(maxBlurSize, 0.0f);

            focalDistance01 = FocalDistance01(focalLength);
            _dofHdrMaterial.SetVector("_CurveParams", new Vector4(1.0f, focalSize, (1.0f / (1.0f - aperture) - 1.0f), focalDistance01));

            RenderTexture rtLow = null;
            RenderTexture rtLow2 = null;

            source.filterMode = FilterMode.Bilinear;

            if (highResolution) internalBlurWidth *= 2.0f;

            WriteCoc(source, true);

            rtLow = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
            rtLow2 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);

            int blurPass = (blurSampleCount == BlurSampleCount.High || blurSampleCount == BlurSampleCount.Medium) ? 17 : 11;

            if (highResolution)
            {
                _dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, internalBlurWidth, 0.025f, internalBlurWidth));
                Graphics.Blit(source, destination, _dofHdrMaterial, blurPass);
            }
            else
            {
                _dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, internalBlurWidth, 0.1f, internalBlurWidth));

                Graphics.Blit(source, rtLow, _dofHdrMaterial, 6);
                Graphics.Blit(rtLow, rtLow2, _dofHdrMaterial, blurPass);

                _dofHdrMaterial.SetTexture("_LowRez", rtLow2);
                _dofHdrMaterial.SetTexture("_FgOverlap", null);
                _dofHdrMaterial.SetVector("_Offsets", Vector4.one * ((1.0f * source.width) / (1.0f * rtLow2.width)) * internalBlurWidth);
                Graphics.Blit(source, destination, _dofHdrMaterial, blurSampleCount == BlurSampleCount.High ? 18 : 12);
            }

            if (rtLow) RenderTexture.ReleaseTemporary(rtLow);
            if (rtLow2) RenderTexture.ReleaseTemporary(rtLow2);
        }

        private void OnDestroy()
        {
            if (_dofHdrMaterial != null)
                DestroyImmediate(_dofHdrMaterial);
        }
    }
}