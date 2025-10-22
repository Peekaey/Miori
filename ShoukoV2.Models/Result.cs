using System.Net;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.Models;

public class Result<T>
{
    public ResultEnum ResultOutcome { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; } = string.Empty;
    
    public Result(ResultEnum resultOutcomeEnum, T? data, string? errorMessage = "")
    {
        Data = data;
        ErrorMessage = errorMessage;
        ResultOutcome = resultOutcomeEnum;
    }
    
    public static Result<T> AsSuccess(T data)
    {
        return new Result<T>(ResultEnum.Success ,  data);
    }
    
    // Execeptions, Errors, etc
    public static Result<T> AsError(string errorMessage)
    {
        return new Result<T>(ResultEnum.Error, default, errorMessage);
    }
    
    // When missing prerequired info such as tokens before executing, etc
    public static Result<T> AsFailure(string errorMessage)
    {
        return new Result<T>(ResultEnum.Failure, default, errorMessage);
    }
    
    
}