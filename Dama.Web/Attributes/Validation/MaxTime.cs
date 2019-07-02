using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class MaxTime : ValidationAttribute
    {
        private readonly string _minTime;
        private int _maxTime;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly string _emptyMaxTimeMessage;
        private readonly string _incorrectMaxTimeMessage;
        private readonly string _incorrectDifferenceMessage;
        private readonly string _incorrectMaxTimeRangeMessage;
        private const int _difference = 5;

        public MaxTime(string minTime, int minValue = 5, int maxValue = 480)
        {
            if (string.IsNullOrEmpty(minTime))
                throw new ArgumentException(nameof(minTime));

            if (minValue < 0)
                throw new ArgumentException(nameof(minValue));

            if (maxValue < 0)
                throw new ArgumentException(nameof(maxValue));

            _minTime = minTime;
            _minValue = minValue;
            _maxValue = maxValue;
            _emptyMaxTimeMessage = Attribute.MaxTime_EmptyMaxTime;
            _incorrectMaxTimeMessage = Attribute.MaxTime_IncorrectMaxTime;
            _incorrectDifferenceMessage = string.Format(Attribute.MaxTime_IncorrectMaxTimeDifference, _difference);
            _incorrectMaxTimeRangeMessage = string.Format(Attribute.MaxTime_IncorrectMaxTimeRange, _minValue, _maxValue);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyMaxTimeMessage);
            
            bool success = int.TryParse(value.ToString(), out _maxTime);

            if (!success)
                return new ValidationResult(_incorrectMaxTimeMessage);

            if (IsMaxTimeValid())
            {
                PropertyInfo property;

                try
                {
                    property = validationContext.ObjectType.GetProperty(_minTime);
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format(Attribute.Attribute_NotFound, ex.Message);
                    return new ValidationResult(errorMessage);
                }

                int minTime = (int)property.GetValue(validationContext.ObjectInstance);

                if (minTime + _difference <= _maxTime)
                    return ValidationResult.Success;
                else
                    return new ValidationResult(_incorrectDifferenceMessage);
            }

            return new ValidationResult(_incorrectMaxTimeRangeMessage);
        }

        private bool IsMaxTimeValid()
        {
            return _maxTime <= _maxValue &&
                   _maxTime >= _minValue;
        }
    }
}