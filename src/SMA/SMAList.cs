using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityExplorer.UI.Widgets;

namespace UnityExplorer.SMA
{
    public class SMAList : ButtonListHandler<SMAListItem, SMACell>
    {
        public SMAList(ScrollPool<SMACell> scrollPool, Func<List<SMAListItem>> getEntriesMethod)
    : base(scrollPool, getEntriesMethod, null, null, null)
        {
            base.SetICell = SetSMACell;
            base.ShouldDisplay = CheckShouldDisplay;
            base.OnCellClicked = OnComponentClicked;
        }

        private bool CheckShouldDisplay(SMAListItem obj, string filter) {
            if (filter != null && filter.Length > 0) {
                return obj.Contains(filter);
            }
            return true;
        }

        public override void OnCellBorrowed(SMACell cell)
        {
            base.OnCellBorrowed(cell);
            cell.OnSliderChange += OnSliderChange;
            cell.OnInputValueChange += OnInputValueChange;
        }

        private void OnSliderChange(float value, int index, SMACell cell)
        {
            try
            {
                var entries = currentEntries;
                var item = entries[index];
                item.SetValue(Convert.ToSingle(value));
                cell.InputField.Text = value.ToString();
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning($"Exception gettin Slider.enabled: {ex.ReflectionExToString()}");
            }
        }

        private void OnInputValueChange(String s, int index, SMACell cell)
        {
            try
            {
                var entries = currentEntries;
                var item = entries[index];
               
                item.SetValue(Convert.ToSingle(s));
                cell.Slider.value = float.Parse(s);
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning($"Exception gettin Slider.enabled: {ex.ReflectionExToString()}");
            }
        }

        public override void SetCell(SMACell cell, int index)
        {
            base.SetCell(cell, index);
        }

        private void OnComponentClicked(int index)
        {
                return;
        }

        // Called from ButtonListHandler.SetCell, will be valid
        private void SetSMACell(SMACell cell, int index)
        {
            var entries = currentEntries;
            cell.Enable();
            var item = entries[index];
            cell.Label.text = item.translation;
            if (item.propValue != null)
            {
                var value = item.GetValue();
                cell.InputField.SetActive(true);
                cell.InputField.Text = value.ToString();
                cell.Slider.gameObject.SetActive(true);
                cell.Slider.value = float.Parse(value.ToString());
                
            } else {
                cell.InputField.SetActive(false);
                cell.Slider.gameObject.SetActive(false);
                cell.Label.text += " [NOT SUPPORTED]";
            }
        }
    }
}
