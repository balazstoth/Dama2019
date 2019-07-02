using System;
using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Attributes
{
    public class MinTime : ValidationAttribute
    {
        private int _minTime;
        private readonly int _minValue;
        private readonly int _maxValue;
        private string _emptyMinTimeMessage;
        private string _incorrectMinTimeMessage;
        private string _incorrectMinTimeRangeMessage;

        public MinTime(int minValue = 5, int maxValue = 480)
        {
            if (minValue < 0)
                throw new ArgumentException(nameof(minValue));

            if (maxValue < 0)
                throw new ArgumentException(nameof(maxValue));

            _minValue = minValue;
            _maxValue = maxValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyMinTimeMessage);
            
            bool success = int.TryParse(value.ToString(), out _minTime);

            if (!success)
                return new ValidationResult(_incorrectMinTimeMessage);
            
            if (isMinTimeValid())
                return ValidationResult.Success;
            else
                return new ValidationResult(_incorrectMinTimeRangeMessage);
        }

        private bool isMinTimeValid()
        {
            return _minTime <= _maxValue &&
                   _minTime >= _minValue;
        }
    }
}