﻿using System.ComponentModel.DataAnnotations;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class Priority : ValidationAttribute
    {
        private readonly bool _isCategory;
        private readonly string _emptyPriorityMessage;
        private readonly string _incorrectPriorityMessage;
        private readonly string _incorrectPriorityRangeMessage;
        private int _priority;
        private int _minPriority;
        private int _maxPriority;

        public Priority(int minPriority, int maxPriority, bool isCategory = false)
        {
            _isCategory = isCategory;
            _minPriority = minPriority;
            _maxPriority = maxPriority;
            _emptyPriorityMessage = Attribute.Priority_EmptyPriority;
            _incorrectPriorityMessage = Attribute.Priority_IncorrectPriority;
            _incorrectPriorityRangeMessage = Attribute.Priority_IncorrectPriorityRanged;

            if (minPriority == 0 && maxPriority == 0)
                SetDefaultPriority();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyPriorityMessage);
           
            bool success = int.TryParse(value.ToString(), out _priority);

            if (!success)
                return new ValidationResult(_incorrectPriorityMessage);
            
            if (IsPriorityValid())
                return ValidationResult.Success;
            else
                return new ValidationResult(_incorrectPriorityRangeMessage);
        }

        private bool IsPriorityValid()
        {
            return _priority <= _maxPriority &&
                   _priority >= _minPriority;
        }
        private void SetDefaultPriority()
        {
            _maxPriority = 100;
            _minPriority = _isCategory ? 1 : 0;
        }
    }
}