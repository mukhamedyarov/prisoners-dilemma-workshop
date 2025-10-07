using System.ComponentModel.DataAnnotations;

namespace PrisonersDilemma.Api.Validation;

public class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is Guid guid)
        {
            return guid != Guid.Empty;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be a valid non-empty GUID.";
    }
}

public class PlayerNumberAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is int playerNumber)
        {
            return playerNumber is 1 or 2;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be either 1 or 2.";
    }
}