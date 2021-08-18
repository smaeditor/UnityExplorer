using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityExplorer.UI;
using UnityExplorer.UI.Widgets;

namespace UnityExplorer.SMA
{
    public class SMACell : ButtonCell
    {
        public Slider Slider;
        public Text Label;
        public InputFieldRef InputField;

        public Action<float, int, SMACell> OnSliderChange;
        public Action<String, int, SMACell> OnInputValueChange;

        private void SliderChange(float val)
        {
            OnSliderChange?.Invoke(val, CurrentDataIndex, this);
        }

        private void InputValueChange(String s)
        {
            OnInputValueChange?.Invoke(s, CurrentDataIndex, this);
        }

        public override GameObject CreateContent(GameObject parent)
        {
            // var root = base.CreateContent(parent);

            UIRoot = UIFactory.CreateVerticalGroup(parent, "ContentHolder", true, true, true, true, 0, default,
                new Color(0.1f, 0.1f, 0.1f));
            UIRoot.SetActive(false);
            Rect = UIRoot.GetComponent<RectTransform>();
            /*
            Rect.anchorMin = new Vector2(0, 1);
            Rect.anchorMax = new Vector2(0, 1);
            Rect.pivot = new Vector2(0.5f, 1);
            Rect.sizeDelta = new Vector2(25, 25);
            */
            UIFactory.SetLayoutElement(UIRoot, minWidth: 100, flexibleWidth: 9999, minHeight: 50, flexibleHeight: 9999);

            var filterRow = UIFactory.CreateHorizontalGroup(UIRoot, "SMAFilterGroup", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(filterRow, minHeight: 25, flexibleHeight: 0);


            Label = UIFactory.CreateLabel(filterRow, "SMALabel", "...", TextAnchor.MiddleLeft);
            InputField = UIFactory.CreateInputField(filterRow, "...", "");
            InputField.OnValueChanged += InputValueChange;
            UIFactory.SetLayoutElement(InputField.UIRoot, minWidth: 150, flexibleWidth: 0, minHeight: 25, flexibleHeight: 0);
            
            
            var sliderRef = UIFactory.CreateSlider(UIRoot, "SMASlider", out Slider);
            UIFactory.SetLayoutElement(sliderRef, minHeight: 25, flexibleWidth: 9999);


            Slider.onValueChanged.AddListener(SliderChange);
            Slider.minValue = 0;
            Slider.maxValue = 100;

            return UIRoot;
        }
    }
}
