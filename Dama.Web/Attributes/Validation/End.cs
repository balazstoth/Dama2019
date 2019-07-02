using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class End : ValidationAttribute
    {
        private readonly string _start;
        private DateTime _end;
        private TimeSpan _difference;
        private readonly string _emptyEndMessage;
        private readonly string _incorrectEndMessage;
        private readonly string _invalidEndMessage;

        public End(string start, int differenceInHours = 5)
        {
            if (differenceInHours < 1)
                throw new ArgumentException(nameof(differenceInHours));

            if (string.IsNullOrEmpty(start))
                throw new ArgumentException(nameof(start));

            _start = start;
            _difference = new TimeSpan(0, differenceInHours, 0);
            _emptyEndMessage = Attribute.End_EmptyEnd;
            _incorrectEndMessage = Attribute.End_IncorrectEndValue;
            _invalidEndMessage = Attribute.End_InvalidEndValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyEndMessage);

            bool success = DateTime.TryParse(value.ToString(), out _end);

            if (!success)
                return new ValidationResult(_incorrectEndMessage);

            PropertyInfo property;

            try
            {
                property = validationContext.ObjectType.GetProperty(_start);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(Attribute.Attribute_NotFound, ex.Message);
                return new ValidationResult(errorMessage);
            }
            
            DateTime startDateTime = (DateTime)property.GetValue(validationContext.ObjectInstance);

            if (startDateTime.TimeOfDay + _difference <= _end.TimeOfDay)
                return ValidationResult.Success;

            return new ValidationResult(_invalidEndMessage);
        }
    }
}