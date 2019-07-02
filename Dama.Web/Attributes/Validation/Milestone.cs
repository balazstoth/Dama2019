using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Attribute = Dama.Organizer.Resources.Attribute;

namespace Dama.Web.Attributes
{
    public class Milestone : ValidationAttribute
    {
        private string _startDate;
        private string _startTime;
        private string _endDate;
        private string _endTime;
        private readonly string _emptyMilestoneMessage;
        private readonly string _incorrectMilestoneMessage;
        private readonly string _invalidMilestoneRangeMessage;
        private readonly int _minNameLength;
        private readonly int _maxNameLength;
        private readonly int _dateLength;

        public Milestone(string startDate, string startTime, string endDate, string endTime)
        {
            _startDate = startDate;
            _startTime = startTime;
            _endDate = endDate;
            _endTime = endTime;
            _emptyMilestoneMessage = Attribute.Milestone_EmptyMilestone;
            _incorrectMilestoneMessage = Attribute.Milestone_IncorrectMilestoneName;
            _invalidMilestoneRangeMessage = Attribute.Milestone_IncorrectRange;
            _minNameLength = 3;
            _maxNameLength = 20;
            _dateLength = 16;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //Is it a typo??
            if (value == null)
                return ValidationResult.Success;

            PropertyInfo startDateproperty;
            PropertyInfo startTimeproperty;
            PropertyInfo endDateproperty;
            PropertyInfo endTimeProperty;

            try
            {
                startDateproperty = validationContext.ObjectType.GetProperty(_startDate);
                startTimeproperty = validationContext.ObjectType.GetProperty(_startTime);
                endDateproperty = validationContext.ObjectType.GetProperty(_endDate);
                endTimeProperty = validationContext.ObjectType.GetProperty(_endTime);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(Attribute.Attribute_NotFound, ex.Message);
                return new ValidationResult(errorMessage);
            }

            DateTime startDate = (DateTime)startDateproperty.GetValue(validationContext.ObjectInstance);
            DateTime startTime = (DateTime)startTimeproperty.GetValue(validationContext.ObjectInstance);
            DateTime endDate = (DateTime)endDateproperty.GetValue(validationContext.ObjectInstance);
            DateTime endTime = (DateTime)endTimeProperty.GetValue(validationContext.ObjectInstance);

            DateTime deadLineStart = startDate.Date + startTime.TimeOfDay;
            DateTime deadlineEnd = endDate.Date + endTime.TimeOfDay;

            string valueString = value.ToString();
            List<string> milestones = valueString.Split('|').ToList();

            foreach (string milestone in milestones)
            {
                if (milestone != string.Empty)
                {
                    string name = milestone.Substring(0, milestone.IndexOf(';'));
                    string date = milestone.Substring(milestone.IndexOf(';') + 1);

                    if (IsNameValid(name))
                        return new ValidationResult(_incorrectMilestoneMessage);

                    if (IsDateValid(date))
                        return new ValidationResult(_emptyMilestoneMessage);

                    DateTime milestoneDate = DateTime.Parse(date);

                    if (!IsMilestoneValid(milestoneDate, deadLineStart, deadlineEnd))
                        return new ValidationResult(_invalidMilestoneRangeMessage);
                }
            }

            return ValidationResult.Success;
        }

        private bool IsNameValid(string name)
        {
            return _minNameLength <= name.Length &&
                   _maxNameLength >= name.Length;
        }
        private bool IsDateValid(string date)
        {
            //Regex?
            return !string.IsNullOrEmpty(date) && date.Length == _dateLength;
        }
        private bool IsMilestoneValid(DateTime milestone, DateTime deadlineStart, DateTime deadlineEnd)
        {
            return milestone < deadlineEnd && milestone > deadlineStart;
        }
    }
}