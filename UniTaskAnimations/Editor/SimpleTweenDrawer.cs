#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.Editor
{
    [CustomPropertyDrawer(typeof(SimpleTween), true)]
    public class SimpleTweenDrawer : BaseTweenDrawer
    {
        #region Consts

        private const int ButtonsCount = 6;
        private static string _cachedName = string.Empty;

        #endregion

        protected float PropertyHeight;
        protected GameObject TweenObject;
        protected SimpleTween TargetTween;

        private bool _initialized;
        private readonly PopupTypes _inheredTypes = new();
        private int _popupTweenIndex;
        private string _changedType;
        private IBaseTween _changedTween;
        private float _currentSliderValue = -1f;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            Initialize();

            _changedType = null;
            _changedTween = null;

            MakeTweenLabel(property, label);

            var propertyRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            var foldoutRect = new Rect(propertyRect.x, propertyRect.y, propertyRect.width, LineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            var startHeight = propertyRect.y;
            propertyRect.y += LineHeight;

            if (property.isExpanded)
            {
                FillTweenObject(property);
                propertyRect.y += DrawChooseTween(propertyRect);
                propertyRect.y += DrawSetTweenButtons(propertyRect, property);
                propertyRect.y += DrawProgress(propertyRect, property);
                propertyRect.y += DrawMainProperties(propertyRect, property);
                propertyRect.y += DrawTweenProperties(propertyRect, property, label);
            }

            PropertyHeight = propertyRect.y - startHeight;
            if (GUI.changed && property.managedReferenceValue is IBaseTween baseTween) OnGuiChange(baseTween).Forget();

            if (_changedType != null) ChangeType(_changedType, property);
            if (_changedTween != null) ChangeTween(_changedTween, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => PropertyHeight;

        private void MakeTweenLabel(SerializedProperty property, GUIContent label)
        {
            string tweenName;
            if (property.managedReferenceId == -1 ||
                property.managedReferenceValue == null)
            {
                property.isExpanded = true;
                tweenName = "Null";
            }
            else
            {
                tweenName = property.managedReferenceValue.GetType().ToString();
            }

            var lastIndexOfPoint = tweenName.LastIndexOf('.');
            var shortTweenNameLength = tweenName.Length - lastIndexOfPoint - 1;
            var shortTweenName = tweenName.Substring(lastIndexOfPoint + 1, shortTweenNameLength);

            if (string.Equals(_cachedName, shortTweenName, StringComparison.Ordinal))
            {
                label.text = _cachedName;
            }
            else
            {
                label.text += $" {shortTweenName}";
                _cachedName = label.text;
            }
        }

        private float DrawSetTweenButtons(Rect propertyRect, SerializedProperty property)
        {
            var buttonWidth = propertyRect.width / ButtonsCount;
            var x = propertyRect.x;
            var y = propertyRect.yMin;

            var buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Copy")) CachedTween = property.managedReferenceValue as IBaseTween;

            x += buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Paste"))
            {
                var currentTween = property.managedReferenceValue as SimpleTween;
                var targetGo = currentTween?.TweenObject;
                if (targetGo == null)
                {
                    var component = property.serializedObject?.targetObject as Component;
                    if (component != null) targetGo = component.gameObject;
                }

                _changedTween = IBaseTween.Clone(CachedTween, targetGo);
            }

            x += buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Play"))
            {
                TargetTween?.StartAnimation().Forget();
            }

            x += buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Stop"))
            {
                TargetTween?.StopAnimation().Forget();
            }

            x += buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Reset"))
            {
                TargetTween?.ResetValues();
            }

            x += buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "End"))
            {
                TargetTween?.EndValues();
            }

            return LineHeight;
        }

        private float DrawProgress(Rect propertyRect, SerializedProperty property)
        {
            if (_currentSliderValue < 0f)
            {
                //TODO: get current time value
                _currentSliderValue = 0f;
            }
            
            var x = propertyRect.x;
            var y = propertyRect.yMin;
            var progressWidth = propertyRect.width;
            var progressRect = new Rect(x, y, progressWidth, LineHeight);
            var sliderValue = EditorGUI.Slider(progressRect, _currentSliderValue, 0f, 1f);

            if (Math.Abs(_currentSliderValue - sliderValue) > 0.0001f)
            {
                _currentSliderValue = sliderValue;
                TargetTween?.SetTimeValue(_currentSliderValue);
            }

            return LineHeight;
        }

        private class PopupTypes
        {
            private readonly List<Type> _types = new();
            public string[] Names;

            public void Clear()
            {
                _types.Clear();
                Names = null;
            }

            public void Add(Type type)
            {
                _types.Add(type);
            }

            public void Compile()
            {
                Names = new string[_types.Count + 1];
                Names[0] = "Null";
                for (var i = 0; i < _types.Count; i++)
                {
                    var type = _types[i];
                    Names[i + 1] = type.Name;
                }
            }

            public string this[int index] => index < 1 ? "Null" : _types[index - 1].Name;
        }

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _inheredTypes.Clear();
            foreach (var type in
                     Assembly.GetAssembly(typeof(SimpleTween)).GetTypes()
                         .Where(myType => myType.IsClass &&
                                          !myType.IsAbstract &&
                                          myType.IsSubclassOf(typeof(SimpleTween))))
            {
                _inheredTypes.Add(type);
            }

            _inheredTypes.Compile();
        }

        private float DrawChooseTween(Rect propertyRect)
        {
            var x = propertyRect.x;
            var y = propertyRect.yMin;

            var labelWidth = propertyRect.width * 7 / 32;
            var labelRect = new Rect(x, y, labelWidth, LineHeight);
            EditorGUI.LabelField(labelRect, "Tween:");

            var popupX = x + labelWidth;
            var popupWidth = propertyRect.width * 3 / 8;
            var popupRect = new Rect(popupX, y, popupWidth, LineHeight);
            _popupTweenIndex = EditorGUI.Popup(popupRect, _popupTweenIndex, _inheredTypes.Names);

            var spaceX = popupX + popupWidth;
            var spaceWidth = propertyRect.width / 32;
            var spaceRect = new Rect(spaceX, y, spaceWidth, LineHeight);
            EditorGUI.LabelField(spaceRect, " ");

            var buttonX = spaceX + spaceWidth;
            var buttonWidth = _popupTweenIndex == 0 ? propertyRect.width * 3 / 8 : propertyRect.width * 3 / 16;
            var buttonRect = new Rect(buttonX, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Set")) _changedType = _inheredTypes[_popupTweenIndex];

            var helpBoxX = buttonX + buttonWidth;
            var helpBoxWidth = _popupTweenIndex == 0 ? 0f : propertyRect.width * 3 / 16;
            var helpBoxRect = new Rect(helpBoxX, y, helpBoxWidth, LineHeight);
            EditorGUI.HelpBox(helpBoxRect, "Set Type", MessageType.Warning);

            return LineHeight;
        }

        private void ChangeType(string typeName, SerializedProperty property)
        {
            var tween = TweenFactory.CreateSimpleTween(typeName, TweenObject);

            if (tween != null)
            {
                if (property.managedReferenceId == -1)
                    Debug.Log("Shoud Be SerializeReference");
                else
                    property.managedReferenceValue = tween;
            }
            else
            {
                property.managedReferenceValue = null;
            }
        }

        private void ChangeTween(IBaseTween baseTween, SerializedProperty property)
        {
            property.managedReferenceValue = baseTween;
        }

        private void FillTweenObject(SerializedProperty property)
        {
            TargetTween = property.managedReferenceValue as SimpleTween;
            TweenObject = TargetTween?.TweenObject;
            if (TweenObject == null)
            {
                TweenObject = property.serializedObject.targetObject switch
                {
                    GameObject go => go,
                    Component component => component.gameObject,
                    _ => TweenObject
                };

                if (TargetTween != null) property.managedReferenceValue = SimpleTween.Clone(TargetTween, TweenObject);
            }
        }

        private float DrawMainProperties(Rect propertyRect, SerializedProperty property)
        {
            var x = propertyRect.x;
            var y = propertyRect.y;
            var width = propertyRect.width;
            var height = LineHeight;

            var labelRect = new Rect(x, y, width, height);

            EditorGUI.LabelField(labelRect, "Main Tween", EditorStyles.boldLabel);
            y += height;

            var tweenObjectRect = new Rect(x, y, width, height);
            var tweenObjectProperty = property.FindPropertyRelative("tweenObject");
            EditorGUI.PropertyField(tweenObjectRect, tweenObjectProperty);
            y += height;

            var startDelayRect = new Rect(x, y, width, height);
            var startDelayProperty = property.FindPropertyRelative("startDelay");
            EditorGUI.PropertyField(startDelayRect, startDelayProperty);
            y += height;

            var tweenTimeRect = new Rect(x, y, width, height);
            var tweenTimeProperty = property.FindPropertyRelative("tweenTime");
            EditorGUI.PropertyField(tweenTimeRect, tweenTimeProperty);
            y += height;

            var loopRect = new Rect(x, y, width, height);
            var loopProperty = property.FindPropertyRelative("loop");
            EditorGUI.PropertyField(loopRect, loopProperty);
            y += height;

            var animationCurveRect = new Rect(x, y, width, height);
            var animationCurveProperty = property.FindPropertyRelative("animationCurve");
            EditorGUI.PropertyField(animationCurveRect, animationCurveProperty);
            y += height;

            return y - propertyRect.y;
        }

        protected virtual float DrawTweenProperties(
            Rect propertyRect,
            SerializedProperty property,
            GUIContent label) =>
            0f;
    }
}
#endif