using System;
using System.ComponentModel.DataAnnotations;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class Duration : ValidationAttribute
    {
        private TimeSpan _time;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly string _emptyDurationMessage;
        private readonly string _incorrectDurationMessage;
        private readonly string _incorrectDurationRangeMessage;

        public Duration(int minValueInMinutes = 5, int maxValueInHours = 13)
        {
            if (minValueInMinutes < 0)
                throw new ArgumentException(nameof(minValueInMinutes));

            if (maxValueInHours > 23)
                throw new ArgumentException(nameof(maxValueInHours));

            _minValue = minValueInMinutes;
            _maxValue = maxValueInHours;
            _emptyDurationMessage = Attribute.Duration_EmptyDuration;
            _incorrectDurationMessage = Attribute.Duration_IncorrectFormat;
            _incorrectDurationRangeMessage = string.Format(Attribute.Duration_IncorrectRange, _minValue, _maxValue);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyDurationMessage);
            
            bool success = TimeSpan.TryParse(value.ToString(), out _time);

            if (!success)
                return new ValidationResult(_incorrectDurationMessage);
            
            if (IsDurationValid())
                return ValidationResult.Success;
            else
                return new ValidationResult(_incorrectDurationRangeMessage);
        }

        private bool IsDurationValid()
        {
            return _time.TotalHours <= _maxValue &&
                   _time.TotalMinutes >= _minValue;
        }
    }
}