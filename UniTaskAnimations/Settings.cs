#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UniTaskAnimations
{
    public class Settings : ScriptableObject
    {
        internal const string SettingsPath = "Assets/Editor/UnitaskAnimationsSettings.asset";

        [SerializeField]
        private float gizmosSize;

        [SerializeField]
        private float gizmosUpdateInterval;

        public float GizmosSize
        {
            get => gizmosSize;
            internal set => gizmosSize = value;
        }

        public float GizmosUpdateInterval
        {
            get => gizmosUpdateInterval;
            internal set => gizmosUpdateInterval = value;
        }


        internal static Settings Instance
        {
            get
            {
                var settings = AssetDatabase.LoadAssetAtPath<Settings>(SettingsPath);
                if (settings == null)
                {
                    settings = CreateInstance<Settings>();
                    settings.gizmosSize = 10f;
                    settings.gizmosUpdateInterval = 1f;
                    if (!Directory.Exists("Assets/Editor")) Directory.CreateDirectory("Assets/Editor");
                    AssetDatabase.CreateAsset(settings, SettingsPath);
                    AssetDatabase.SaveAssets();
                }

                return settings;
            }
        }
    }

    public class SettingsRegister : SettingsProvider
    {
        private Settings _settings;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = Settings.Instance;
        }


        public override void OnGUI(string searchContext)
        {
            _settings.GizmosSize = EditorGUILayout.FloatField("Gizmos Size", _settings.GizmosSize);
            _settings.GizmosUpdateInterval =
                EditorGUILayout.FloatField("Gizmos Update Interval", _settings.GizmosUpdateInterval);
        }

        public SettingsRegister(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var settings = Settings.Instance;
            var provider = new SettingsRegister(
                "Project/Unitask Animations Settings",
                SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };

            return provider;
        }
    }
}
#endif