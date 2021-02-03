using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class EnumExtenstion
    {
        public static IEnumItem[] GetEnumItems<T>()
        {
            return GetEnumItems(typeof(T));
        }
        public static string GetDisplayText(this Enum item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            var enumItem = GetEnumItem(item.GetType(), item);
            if (enumItem != null)
                return enumItem.Text;
            else
                return string.Empty;
        }

        public static IEnumItem[] GetEnumItems(Type enumType)

        {
            var items = Enum.GetValues(enumType);
            var result = new List<IEnumItem>();
            foreach (var item in items)
            {
                var enumItem = GetEnumItem(enumType, item as Enum);
                if (enumItem != null)
                    result.Add(enumItem);
            }
            return result.ToArray();
        }
        public static IEnumItem[] GetStockOutEnumItems(Type enumType)
        {
            var items = Enum.GetValues(enumType);
            var result = new List<IEnumItem>();
            foreach (var item in items)
            {
                var enumItem = GetEnumItem(enumType, item as Enum);
                if (enumItem != null && enumItem.Value <= 0)
                {
                    result.Add(enumItem);
                }

            }
            return result.ToArray();
        }
        public static IEnumItem[] GetStockInEnumItems(Type enumType)
        {
            var items = Enum.GetValues(enumType);
            var result = new List<IEnumItem>();
            foreach (var item in items)
            {
                var enumItem = GetEnumItem(enumType, item as Enum);
                if (enumItem != null && enumItem.Value > 0)
                {
                    result.Add(enumItem);
                }

            }
            return result.ToArray();
        }
        public static IEnumItem[] GetApprovleTypeItems(Type enumType)
        {
            var items = Enum.GetValues(enumType);
            var result = new List<IEnumItem>();
            foreach (var item in items)
            {
                var enumItem = GetEnumItem(enumType, item as Enum);
                if (enumItem != null && enumItem.Value != 5)
                {
                    result.Add(enumItem);
                }

            }
            return result.ToArray();
        }

        public static IEnumItem GetEnumItem(Type enumType, Enum item)
        {
            var name = item.ToString();
            var text = name;
            var mi = enumType.GetMember(name).FirstOrDefault();
            if (mi != null)
            {
                var ign = mi.GetCustomAttributes(typeof(Utility.IgnoreAttribute), false);
                if (ign != null && ign.Length > 0)
                {
                    return null;
                }
                var dtAttribute = mi.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                if (dtAttribute != null)
                {
                    text = (dtAttribute as DescriptionAttribute).Description;
                }
            }
            var enumItem = new EnumItem { Value = Convert.ToInt32(item), Name = item.ToString(), Text = text };
            return enumItem;
        }

        public static bool OwnElement<TEnum>(object element) where TEnum : struct
        {
            return Enum.IsDefined(typeof(TEnum), element);
        }
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }
    }
}
