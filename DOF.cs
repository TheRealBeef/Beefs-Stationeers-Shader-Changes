using Assets.Scripts;
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

        public enum AutoFocusMode
        {
            SinglePoint = 0,
            NinePointAverage = 1,
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

        public bool autoFocus = false;
        public AutoFocusMode autoFocusMode = AutoFocusMode.SinglePoint;
        public float autoFocusSampleRadius = 0.05f; // Percentage of screen size
        public float autoFocusOffset = 0f;
        public float autoFocusSmoothTime = 0.15f;
        public float autoFocusMinDistance = 0.5f;
        public float autoFocusMaxDistance = 100f;

        private float focalDistance01 = 10.0f;
        private float internalBlurWidth = 1.0f;
        private float _currentFocalLength;
        private float _targetFocalLength;
        private float _focusVelocity;
        private RenderTexture _depthReadbackRT;

        public bool showFocusPoint = false;
        private float _lastMeasuredDepth = 0f;

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

            _currentFocalLength = focalLength;
            _targetFocalLength = focalLength;

            return true;
        }

        private bool IsWorldReady()
        {
            if (!(BeefsShaderChangesPlugin.Instance?.IsInGameWorld() ?? false))
                return false;

            try
            {
                Light worldSun = WorldManager.Instance?.WorldSun?.TargetLight;
                return worldSun != null;
            }
            catch
            {
                return false;
            }
        }

        private void Update()
        {
            if (!enabled || !autoFocus || _camera == null)
                return;

            if (!IsWorldReady())
                return;

            _targetFocalLength = CalculateAutoFocusDistance();

            if (autoFocusSmoothTime > 0.001f)
            {
                _currentFocalLength = Mathf.SmoothDamp(
                    _currentFocalLength,
                    _targetFocalLength,
                    ref _focusVelocity,
                    autoFocusSmoothTime
                );
            }
            else
            {
                _currentFocalLength = _targetFocalLength;
            }
        }

        private float CalculateAutoFocusDistance()
        {
            if (_camera == null)
                return focalLength;

            float totalDepth = 0f;
            int sampleCount = 0;

            if (autoFocusMode == AutoFocusMode.SinglePoint)
            {
                float depth = SampleDepthAtScreenPosition(0.5f, 0.5f);
                if (depth > 0)
                {
                    totalDepth = depth;
                    sampleCount = 1;
                }
            }
            else
            {
                float radius = autoFocusSampleRadius;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float screenX = 0.5f + x * radius;
                        float screenY = 0.5f + y * radius;

                        float depth = SampleDepthAtScreenPosition(screenX, screenY);
                        if (depth > 0)
                        {
                            totalDepth += depth;
                            sampleCount++;
                        }
                    }
                }
            }

            if (sampleCount == 0)
                return _currentFocalLength;

            float averageDepth = totalDepth / sampleCount;
            _lastMeasuredDepth = averageDepth;

            float finalDistance = averageDepth + autoFocusOffset;
            finalDistance = Mathf.Clamp(finalDistance, autoFocusMinDistance, autoFocusMaxDistance);

            return finalDistance;
        }

        private float SampleDepthAtScreenPosition(float screenX, float screenY)
        {
            if (_camera == null)
                return 0f;

            Ray ray = _camera.ViewportPointToRay(new Vector3(screenX, screenY, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, autoFocusMaxDistance))
            {
                return hit.distance;
            }
            return autoFocusMaxDistance;
        }

        private float GetEffectiveFocalLength()
        {
            if (autoFocus)
            {
                return _currentFocalLength;
            }
            return focalLength;
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

            if (!IsWorldReady())
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (aperture < 0.0f) aperture = 0.0f;
            if (maxBlurSize < 0.1f) maxBlurSize = 0.1f;
            focalSize = Mathf.Clamp(focalSize, 0.0f, 2.0f);
            internalBlurWidth = Mathf.Max(maxBlurSize, 0.0f);

            float effectiveFocalLength = GetEffectiveFocalLength();
            focalDistance01 = FocalDistance01(effectiveFocalLength);
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

        private void OnGUI()
        {
            if (!enabled || !autoFocus || !showFocusPoint)
                return;

            if (!IsWorldReady())
                return;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float crosshairSize = 10f;

            GUI.color = Color.green;

            GUI.DrawTexture(new Rect(centerX - crosshairSize, centerY - 1, crosshairSize * 2, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - 1, centerY - crosshairSize, 2, crosshairSize * 2), Texture2D.whiteTexture);

            if (autoFocusMode == AutoFocusMode.NinePointAverage)
            {
                GUI.color = new Color(1f, 1f, 0f, 0.7f);
                float radiusPixels = autoFocusSampleRadius * Screen.width;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        float px = centerX + x * radiusPixels;
                        float py = centerY - y * radiusPixels;

                        GUI.DrawTexture(new Rect(px - 2, py - 2, 4, 4), Texture2D.whiteTexture);
                    }
                }
            }

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.green;

            string focusInfo = $"Focus: {_currentFocalLength:F1}m (Target: {_targetFocalLength:F1}m)";
            GUI.Label(new Rect(centerX - 100, centerY + 20, 200, 30), focusInfo, style);
        }

        public float GetCurrentFocalDistance()
        {
            return autoFocus ? _currentFocalLength : focalLength;
        }

        public float GetTargetFocalDistance()
        {
            return autoFocus ? _targetFocalLength : focalLength;
        }

        private void OnDestroy()
        {
            if (_dofHdrMaterial != null)
                DestroyImmediate(_dofHdrMaterial);
            if (_depthReadbackRT != null)
                RenderTexture.ReleaseTemporary(_depthReadbackRT);
        }
    }
}