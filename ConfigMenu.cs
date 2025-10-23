using UnityEngine;
using Assets.Scripts;

namespace BeefsShaderChanges
{
    public class ConfigMenu : MonoBehaviour
    {
        private bool _showConfig = false;
        private Vector2 _scrollPosition = Vector2.zero;
        private Rect _windowRect;
        private bool _windowRectInitialized = false;
        private int _lastScreenHeight = 0;
        private int _lastScreenWidth = 0;
        private float _guiScale = 1.0f;
        private GUIStyle _blueBoxStyle;
        private GUIStyle _orangeBoxStyle;
        private GUIStyle _greenBoxStyle;
        private GUIStyle _cyanBoxStyle;
        private bool _stylesInitialized = false;

        private void Update()
        {
            bool inGameWorld = IsInGameWorldExclMainMenu();

            if (_showConfig && !inGameWorld)
            {
                _showConfig = false;
                return;
            }

            if (!inGameWorld) return;

            if (_showConfig && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                _showConfig = false;
                return;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F10))
            {
                _showConfig = !_showConfig;
                if (_showConfig)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            if (_windowRectInitialized && (Screen.height != _lastScreenHeight || Screen.width != _lastScreenWidth))
            {
                _windowRectInitialized = false;
                _lastScreenHeight = Screen.height;
                _lastScreenWidth = Screen.width;
            }
        }

        private void OnGUI()
        {
            if (_showConfig)
            {
                if (!_windowRectInitialized)
                    InitializeWindowRect();

                if (!_stylesInitialized)
                    InitializeStyles();

                Matrix4x4 oldMatrix = GUI.matrix;
                GUI.matrix = Matrix4x4.Scale(new Vector3(_guiScale, _guiScale, 1.0f));

                Rect scaledWindowRect = new Rect(
                    _windowRect.x / _guiScale,
                    _windowRect.y / _guiScale,
                    _windowRect.width / _guiScale,
                    _windowRect.height / _guiScale
                );

                Color oldColor = GUI.color;
                GUI.color = new Color(0.05f, 0.05f, 0.05f, 1f);
                GUI.Box(new Rect(scaledWindowRect.x - 2, scaledWindowRect.y - 2, scaledWindowRect.width + 4, scaledWindowRect.height + 4), "");
                GUI.color = oldColor;

                scaledWindowRect = GUILayout.Window(12345, scaledWindowRect, ConfigWindow, "Beef's Shader Changes (F10 to close)");
                _windowRect = new Rect(
                    scaledWindowRect.x * _guiScale,
                    scaledWindowRect.y * _guiScale,
                    scaledWindowRect.width * _guiScale,
                    scaledWindowRect.height * _guiScale
                );

                GUI.matrix = oldMatrix;
            }
        }

        private bool IsInGameWorldExclMainMenu()
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

        private void InitializeWindowRect()
        {
            if (_windowRectInitialized) return;
            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            float baseHeight = 1440f;
            float baseWindowHeight = 800f;
            float baseWindowWidth = 750f;
            _guiScale = Mathf.Max(1.0f, screenHeight / baseHeight);
            float scaledWidth = baseWindowWidth * _guiScale;
            float scaledHeight = baseWindowHeight * _guiScale;
            float maxHeight = screenHeight * 0.9f;
            float maxWidth = screenWidth * 0.65f;
            float windowHeight = Mathf.Min(scaledHeight, maxHeight);
            float windowWidth = Mathf.Min(scaledWidth, maxWidth);
            _windowRect = new Rect(20, 20, windowWidth, windowHeight);
            _windowRectInitialized = true;
        }

        private void ConfigWindow(int windowID)
        {
            GUILayout.BeginVertical();

            float scrollViewHeight = (_windowRect.height / _guiScale) - 50f;
            float scrollViewWidth = (_windowRect.width / _guiScale) - 20f;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,
                GUILayout.Width(scrollViewWidth),
                GUILayout.Height(scrollViewHeight));

            DrawSSAOSettings();
            GUILayout.Space(5);

            DrawBloomSettings();
            GUILayout.Space(5);

            DrawHelmetVisorSettings();
            GUILayout.Space(5);

            DrawDepthOfFieldSettings();
            GUILayout.Space(10);

            GUILayout.Space(10);
            GUILayout.Label("Press F10 to toggle this menu | ESC to close", GUILayout.ExpandWidth(true));
            GUILayout.Label("All changes are saved and applied immediately", GUILayout.ExpandWidth(true));

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawSSAOSettings()
        {
            GUILayout.BeginVertical(_blueBoxStyle);
            GUILayout.Label("=== SSAO (Ambient Occlusion) ===", GUI.skin.box, GUILayout.ExpandWidth(true));

            var currentEnabled = BeefsShaderChangesPlugin.EnableSSAO.Value;
            var newEnabled = GUILayout.Toggle(currentEnabled, $"Enable SSAO ({currentEnabled})");
            if (newEnabled != currentEnabled)
            {
                BeefsShaderChangesPlugin.EnableSSAO.Value = newEnabled;
                CameraController.SetAmbientOcclusion();
            }

            if (BeefsShaderChangesPlugin.EnableSSAO.Value)
            {
                GUILayout.Space(3);

                string currentPreset = BeefsShaderChangesPlugin.QualityPreset.Value;
                GUILayout.Label($"Preset: {SSAOPresets.GetPresetName(currentPreset)}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Low", currentPreset == "Low" ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.QualityPreset.Value = "Low";
                    CameraController.SetAmbientOcclusion();
                }
                if (GUILayout.Button("Medium", currentPreset == "Medium" ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.QualityPreset.Value = "Medium";
                    CameraController.SetAmbientOcclusion();
                }
                if (GUILayout.Button("High", currentPreset == "High" ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.QualityPreset.Value = "High";
                    CameraController.SetAmbientOcclusion();
                }
                if (GUILayout.Button("Custom", currentPreset == "Custom" ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.QualityPreset.Value = "Custom";
                    CameraController.SetAmbientOcclusion();
                }
                GUILayout.EndHorizontal();

                var currentNoise = BeefsShaderChangesPlugin.EnableCustomNoise.Value;
                var newNoise = GUILayout.Toggle(currentNoise, $"Custom Noise ({currentNoise})");
                if (newNoise != currentNoise)
                {
                    BeefsShaderChangesPlugin.EnableCustomNoise.Value = newNoise;
                    NoiseGenerator.ForceRegenerate(true);
                }

                if (BeefsShaderChangesPlugin.QualityPreset.Value == "Custom")
                {
                    GUILayout.Space(3);
                    GUILayout.Label("SSAO Settings", GUILayout.ExpandWidth(true));

                    int currentSamples = BeefsShaderChangesPlugin.CustomSamples.Value;
                    GUILayout.Label($"Sample Count: {SSAOPresets.GetSampleCountName(currentSamples)} ({currentSamples})");
                    int newSamples = Mathf.RoundToInt(GUILayout.HorizontalSlider(currentSamples, 0, 4));
                    if (newSamples != currentSamples)
                    {
                        BeefsShaderChangesPlugin.CustomSamples.Value = newSamples;
                        CameraController.SetAmbientOcclusion();
                    }

                    int currentDownsampling = BeefsShaderChangesPlugin.CustomDownsampling.Value;
                    GUILayout.Label($"Downsampling: {currentDownsampling}x");
                    int newDownsampling = Mathf.RoundToInt(GUILayout.HorizontalSlider(currentDownsampling, 1, 2));
                    if (newDownsampling != currentDownsampling)
                    {
                        BeefsShaderChangesPlugin.CustomDownsampling.Value = newDownsampling;
                        CameraController.SetAmbientOcclusion();
                    }

                    float currentRadius = BeefsShaderChangesPlugin.CustomRadius.Value;
                    GUILayout.Label($"Radius: {currentRadius:F3}");
                    float newRadius = GUILayout.HorizontalSlider(currentRadius, 0.001f, 0.5f);
                    if (!Mathf.Approximately(newRadius, currentRadius))
                    {
                        BeefsShaderChangesPlugin.CustomRadius.Value = newRadius;
                        CameraController.SetAmbientOcclusion();
                    }

                    float currentIntensity = BeefsShaderChangesPlugin.CustomIntensity.Value;
                    GUILayout.Label($"Intensity: {currentIntensity:F2}");
                    float newIntensity = GUILayout.HorizontalSlider(currentIntensity, 0f, 16f);
                    if (!Mathf.Approximately(newIntensity, currentIntensity))
                    {
                        BeefsShaderChangesPlugin.CustomIntensity.Value = newIntensity;
                        CameraController.SetAmbientOcclusion();
                    }

                    float currentBias = BeefsShaderChangesPlugin.CustomBias.Value;
                    GUILayout.Label($"Bias: {currentBias:F2}");
                    float newBias = GUILayout.HorizontalSlider(currentBias, 0f, 1f);
                    if (!Mathf.Approximately(newBias, currentBias))
                    {
                        BeefsShaderChangesPlugin.CustomBias.Value = newBias;
                        CameraController.SetAmbientOcclusion();
                    }

                    float currentDistance = BeefsShaderChangesPlugin.CustomDistance.Value;
                    GUILayout.Label($"Distance: {currentDistance:F1}");
                    float newDistance = GUILayout.HorizontalSlider(currentDistance, 0f, 10f);
                    if (!Mathf.Approximately(newDistance, currentDistance))
                    {
                        BeefsShaderChangesPlugin.CustomDistance.Value = newDistance;
                        CameraController.SetAmbientOcclusion();
                    }

                    float currentLum = BeefsShaderChangesPlugin.CustomLumContribution.Value;
                    GUILayout.Label($"Luminance Contribution: {currentLum:F2}");
                    float newLum = GUILayout.HorizontalSlider(currentLum, 0f, 1f);
                    if (!Mathf.Approximately(newLum, currentLum))
                    {
                        BeefsShaderChangesPlugin.CustomLumContribution.Value = newLum;
                        CameraController.SetAmbientOcclusion();
                    }

                    int currentBlur = BeefsShaderChangesPlugin.CustomBlurPasses.Value;
                    GUILayout.Label($"Blur Passes: {currentBlur}");
                    int newBlur = Mathf.RoundToInt(GUILayout.HorizontalSlider(currentBlur, 1, 4));
                    if (newBlur != currentBlur)
                    {
                        BeefsShaderChangesPlugin.CustomBlurPasses.Value = newBlur;
                        CameraController.SetAmbientOcclusion();
                    }

                    float currentCutoff = BeefsShaderChangesPlugin.CustomCutoffDistance.Value;
                    GUILayout.Label($"Cutoff Distance: {currentCutoff:F0}");
                    float newCutoff = GUILayout.HorizontalSlider(currentCutoff, 10f, 100f);
                    if (!Mathf.Approximately(newCutoff, currentCutoff))
                    {
                        BeefsShaderChangesPlugin.CustomCutoffDistance.Value = newCutoff;
                        CameraController.SetAmbientOcclusion();
                    }
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawBloomSettings()
        {
            GUILayout.BeginVertical(_greenBoxStyle);
            GUILayout.Label("=== Bloom ===", GUI.skin.box, GUILayout.ExpandWidth(true));

            var currentEnabled = BeefsShaderChangesPlugin.EnableBloomTweaks.Value;
            var newEnabled = GUILayout.Toggle(currentEnabled, $"Enable Bloom Adjustments ({currentEnabled})");
            if (newEnabled != currentEnabled)
            {
                BeefsShaderChangesPlugin.EnableBloomTweaks.Value = newEnabled;
                BloomManager.UpdateBloom();
            }

            if (BeefsShaderChangesPlugin.EnableBloomTweaks.Value)
            {
                float currentInt = BeefsShaderChangesPlugin.BloomIntensity.Value;
                GUILayout.Label($"Intensity: {currentInt:F3}");
                float newInt = GUILayout.HorizontalSlider(currentInt, 0f, 1f);
                if (!Mathf.Approximately(newInt, currentInt))
                {
                    BeefsShaderChangesPlugin.BloomIntensity.Value = newInt;
                    BloomManager.UpdateBloom();
                }

                float currentThresh = BeefsShaderChangesPlugin.BloomThreshold.Value;
                GUILayout.Label($"Threshold: {currentThresh:F2}");
                float newThresh = GUILayout.HorizontalSlider(currentThresh, 0f, 5f);
                if (!Mathf.Approximately(newThresh, currentThresh))
                {
                    BeefsShaderChangesPlugin.BloomThreshold.Value = newThresh;
                    BloomManager.UpdateBloom();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawHelmetVisorSettings()
        {
            GUIStyle boxStyle = _cyanBoxStyle ?? GUI.skin.box;

            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("=== Helmet Visor ===", GUI.skin.box, GUILayout.ExpandWidth(true));

            var currentEnabled = BeefsShaderChangesPlugin.EnableHelmetVisor.Value;
            var newEnabled = GUILayout.Toggle(currentEnabled, $"Enable Helmet Effects ({currentEnabled})");
            if (newEnabled != currentEnabled)
            {
                BeefsShaderChangesPlugin.EnableHelmetVisor.Value = newEnabled;
                HelmetStateManager.ForceRefresh();
            }

            if (BeefsShaderChangesPlugin.EnableHelmetVisor.Value)
            {
                int currentSamples = BeefsShaderChangesPlugin.VisorReflectionSamples.Value;
                GUILayout.Label($"Reflection Quality: {currentSamples} samples (higher = better but slower)");
                int newSamples = Mathf.RoundToInt(GUILayout.HorizontalSlider(currentSamples, 8, 64));
                if (newSamples != currentSamples)
                {
                    BeefsShaderChangesPlugin.VisorReflectionSamples.Value = newSamples;
                    HelmetStateManager.ForceRefresh();
                }

                float current = BeefsShaderChangesPlugin.VisorReflectionIntensity.Value;
                GUILayout.Label($"Reflection Intensity: {current:F2}");
                float newVal = GUILayout.HorizontalSlider(current, 0f, 2f);
                if (!Mathf.Approximately(newVal, current))
                {
                    BeefsShaderChangesPlugin.VisorReflectionIntensity.Value = newVal;
                    HelmetStateManager.ForceRefresh();
                }

                GUILayout.Space(5);
            }

            GUILayout.EndVertical();
        }

        private void DrawDepthOfFieldSettings()
        {
            GUILayout.BeginVertical(_orangeBoxStyle);
            GUILayout.Label("=== Depth of Field ===", GUI.skin.box, GUILayout.ExpandWidth(true));

            var currentEnabled = BeefsShaderChangesPlugin.EnableDepthOfField.Value;
            var newEnabled = GUILayout.Toggle(currentEnabled, $"Enable DOF ({currentEnabled})");
            if (newEnabled != currentEnabled)
            {
                BeefsShaderChangesPlugin.EnableDepthOfField.Value = newEnabled;
                ShaderChangesManager.UpdateDepthOfField();
            }

            if (BeefsShaderChangesPlugin.EnableDepthOfField.Value)
            {
                float current = BeefsShaderChangesPlugin.DOFFocalLength.Value;
                GUILayout.Label($"Focal Distance: {current:F1}m");
                float newVal = GUILayout.HorizontalSlider(current, 0.1f, 100f);
                if (!Mathf.Approximately(newVal, current))
                {
                    BeefsShaderChangesPlugin.DOFFocalLength.Value = newVal;
                    ShaderChangesManager.UpdateDepthOfField();
                }

                current = BeefsShaderChangesPlugin.DOFFocalSize.Value;
                GUILayout.Label($"Focal Size: {current:F2}");
                newVal = GUILayout.HorizontalSlider(current, 0f, 2f);
                if (!Mathf.Approximately(newVal, current))
                {
                    BeefsShaderChangesPlugin.DOFFocalSize.Value = newVal;
                    ShaderChangesManager.UpdateDepthOfField();
                }

                current = BeefsShaderChangesPlugin.DOFAperture.Value;
                GUILayout.Label($"Aperture: {current:F2}");
                newVal = GUILayout.HorizontalSlider(current, 0f, 1f);
                if (!Mathf.Approximately(newVal, current))
                {
                    BeefsShaderChangesPlugin.DOFAperture.Value = newVal;
                    ShaderChangesManager.UpdateDepthOfField();
                }

                current = BeefsShaderChangesPlugin.DOFMaxBlurSize.Value;
                GUILayout.Label($"Max Blur: {current:F1}");
                newVal = GUILayout.HorizontalSlider(current, 0.1f, 10f);
                if (!Mathf.Approximately(newVal, current))
                {
                    BeefsShaderChangesPlugin.DOFMaxBlurSize.Value = newVal;
                    ShaderChangesManager.UpdateDepthOfField();
                }

                int currentSamples = BeefsShaderChangesPlugin.DOFSampleCount.Value;
                string[] sampleNames = new string[] { "Low", "Medium", "High" };
                GUILayout.Label($"Sample Quality: {sampleNames[currentSamples]}");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Low", currentSamples == 0 ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.DOFSampleCount.Value = 0;
                    ShaderChangesManager.UpdateDepthOfField();
                }
                if (GUILayout.Button("Medium", currentSamples == 1 ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.DOFSampleCount.Value = 1;
                    ShaderChangesManager.UpdateDepthOfField();
                }
                if (GUILayout.Button("High", currentSamples == 2 ? GUI.skin.box : GUI.skin.button))
                {
                    BeefsShaderChangesPlugin.DOFSampleCount.Value = 2;
                    ShaderChangesManager.UpdateDepthOfField();
                }
                GUILayout.EndHorizontal();

                var currentHR = BeefsShaderChangesPlugin.DOFHighResolution.Value;
                var newHR = GUILayout.Toggle(currentHR, $"High Resolution ({currentHR})");
                if (newHR != currentHR)
                {
                    BeefsShaderChangesPlugin.DOFHighResolution.Value = newHR;
                    ShaderChangesManager.UpdateDepthOfField();
                }
            }

            GUILayout.EndVertical();
        }

        private void OnDestroy()
        {
            if (_showConfig && (BeefsShaderChangesPlugin.Instance?.IsInGameWorld() ?? false))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _blueBoxStyle = MakeStyle(new Color(0f, 0.18f, 0.6f, 0.8f), Color.white);
            _orangeBoxStyle = MakeStyle(new Color(0.65f, 0.27f, 0f, 0.8f), Color.white);
            _greenBoxStyle = MakeStyle(new Color(0f, 0.5f, 0.05f, 0.8f), Color.white);
            _cyanBoxStyle = MakeStyle(new Color(0f, 0.4f, 0.5f, 0.8f), Color.white);

            _stylesInitialized = true;
        }

        private GUIStyle MakeStyle(Color backgroundColor, Color textColor)
        {
            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = CreateTexture(backgroundColor);
            style.normal.textColor = textColor;
            style.fontStyle = UnityEngine.FontStyle.Bold;
            style.alignment = UnityEngine.TextAnchor.MiddleCenter;
            return style;
        }

        private Texture2D CreateTexture(Color color)
        {
            Color[] pixels = new Color[4];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D texture = new Texture2D(2, 2);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}