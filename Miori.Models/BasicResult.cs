using System.Runtime.InteropServices.JavaScript;
using Miori.Models.Enums;

namespace Miori.Models;

public class BasicResult
{
    public ResultEnum ResultOutcome { get; set; }
    public string? ErrorMessage { get; set; } = string.Empty;

    public BasicResult(ResultEnum resultOutcome, string? errorMessage = null)
    {
        ResultOutcome = resultOutcome;
        ErrorMessage = errorMessage;
    }

    public static BasicResult AsSuccess()
    {
        return new BasicResult(ResultEnum.Success);
    }

    // Exceptions, errors, etc
    public static BasicResult AsError(string errorMessage)
    {
        return new BasicResult(ResultEnum.Error, errorMessage);
    }

    // When missing prerequired info such as tokens before executing, etc
    public static BasicResult AsFailure(string errorMessage)
    {
        return new BasicResult(ResultEnum.Failure, errorMessage);
    }
}