using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityExplorer.SMA
{
    public class SMAListItem
    {
        public readonly System.Object obj;
        public readonly string key;
        public readonly string name;
        public readonly string translation;
        public readonly Type type;
        public readonly System.Reflection.PropertyInfo propValue;
        public SMAListItem(System.Object obj, string key = "valor")
        {
            this.obj = obj;
            this.key = key;
            this.type = obj.GetType();
            var propName = this.type.GetProperty("nombre");
            var name = propName.GetValue(obj, null).ToString();
            this.name = key == "valor" ? name : name + "_" + key;
            this.propValue = this.type.GetProperty(this.key);
            this.translation = string.Join(" ", this.name.Split('_').Select(s => SMATranslations.translate(s)).ToArray()); ;
        }

        public bool Contains(string filter)
        {
            return this.name.ToLower().Contains(filter.ToLower()) || this.translation.ToLower().Contains(filter.ToLower());
        }
        public System.Object GetValue()
        {
            return this.propValue.GetValue(this.obj, null);
        }
        public void SetValue(System.Object value)
        {
            this.propValue.SetValue(this.obj, value, null);
        }
    }
}
