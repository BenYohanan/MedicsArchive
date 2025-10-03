using Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class ApplicationUserViewModel
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateRegistered { get; set; }
        public PassWordType PassWordType { get; set; }

        public string PassWordTypeDescription
        {
            get
            {
                return GetEnumDescription(PassWordType);
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            DescriptionAttribute? attribute = field
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault();

            return attribute?.Description ?? value.ToString();
        }
    }
}