using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class Deadline : ValidationAttribute
    {
        private readonly string _startDate;
        private readonly string _startTime;
        private readonly string _endDate;
        private DateTime _endTime;
        private TimeSpan _difference;
        private readonly string _emptyDeadlineMessage;
        private readonly string _incorrectDeadlineMessage;
        private readonly string _invalidDeadlineMessage;

        public Deadline(string startDate, string startTime, string endDate)
        {
            if (string.IsNullOrEmpty(startDate))
                throw new ArgumentException(nameof(startDate));

            if (string.IsNullOrEmpty(startTime))
                throw new ArgumentException(nameof(startTime));

            if (string.IsNullOrEmpty(endDate))
                throw new ArgumentException(nameof(endDate));

            _startDate = startDate;
            _startTime = startTime;
            _endDate = endDate;
            _emptyDeadlineMessage = Attribute.Deadline_EmptyDeadline;
            _incorrectDeadlineMessage = Attribute.Deadline_IncorrectDeadline;
            _invalidDeadlineMessage = Attribute.Deadline_InvalidDeadline;
            _difference = new TimeSpan(1, 0, 0);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(_emptyDeadlineMessage);

            bool success = DateTime.TryParse(value.ToString(), out _endTime);

            if (!success)
                return new ValidationResult(_invalidDeadlineMessage);

            PropertyInfo startDateProperty;
            PropertyInfo startTimeProperty;
            PropertyInfo endDateProperty;

            try
            {
                startDateProperty = validationContext.ObjectType.GetProperty(_startDate);
                startTimeProperty = validationContext.ObjectType.GetProperty(_startTime);
                endDateProperty = validationContext.ObjectType.GetProperty(_endDate);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(Attribute.Attribute_NotFound, ex.Message);
                return new ValidationResult(errorMessage);
            }

            DateTime startDateVar = (DateTime) startDateProperty.GetValue(validationContext.ObjectInstance);
            DateTime startTimeVar = (DateTime) startTimeProperty.GetValue(validationContext.ObjectInstance);
            DateTime endDateVar = (DateTime) endDateProperty.GetValue(validationContext.ObjectInstance);

            if (startDateVar.Date < endDateVar.Date)
                return ValidationResult.Success;
            
            if (startDateVar.Date > endDateVar.Date)
                return new ValidationResult(_incorrectDeadlineMessage);
            
            if (startTimeVar + _difference <= _endTime)
                return ValidationResult.Success;

            return new ValidationResult(_invalidDeadlineMessage);
        }
    }
}