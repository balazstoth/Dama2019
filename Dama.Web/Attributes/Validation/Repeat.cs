using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class Repeat : ValidationAttribute
    {
        private string _day;
        private DateTime _end;
        private readonly string _emptyRepeatMessage;
        private readonly string _incorrectRepeatMessage;
        private readonly string _invalidRepeatMessage;

        public Repeat(string day)
        {
            if (string.IsNullOrEmpty(day))
                throw new ArgumentException(nameof(day));

            _day = day;
            _emptyRepeatMessage = Attribute.End_EmptyEnd;
            _incorrectRepeatMessage = Attribute.End_IncorrectEndValue;
            _invalidRepeatMessage = Attribute.End_InvalidEndValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyRepeatMessage);

            bool success = DateTime.TryParse(value.ToString(), out _end);

            if (!success)
                return new ValidationResult(_incorrectRepeatMessage);

            PropertyInfo property;

            try
            {
                property = validationContext.ObjectType.GetProperty(_day);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(Attribute.Attribute_NotFound, ex.Message);
                return new ValidationResult(errorMessage);
            }

            DateTime day = (DateTime) property.GetValue(validationContext.ObjectInstance);

            if (day < _end)
                return ValidationResult.Success;

            return new ValidationResult(_invalidRepeatMessage);
        }
    }
}