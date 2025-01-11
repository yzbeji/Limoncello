using System;
using System.ComponentModel.DataAnnotations;
using Limoncello.Models;
public class DueDateAfterStartDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var instance = (ProjectTask)validationContext.ObjectInstance;

        if (instance.StartDate != null && instance.DueDate != null)
        {
            if (instance.DueDate < instance.StartDate)
            {
                return new ValidationResult("Due date must be greater than starting date.");
            }
        }
        return ValidationResult.Success;
    }
}
