using System.Net;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.Models;

public class ApiResult<T>
{
    public ResultEnum ResultOutcome { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; } = string.Empty;
    
    public ApiResult(ResultEnum resultOutcomeEnum, T? data, string? errorMessage = "")
    {
        Data = data;
        ErrorMessage = errorMessage;
        ResultOutcome = resultOutcomeEnum;
    }
    
    public static ApiResult<T> AsSuccess(T data)
    {
        return new ApiResult<T>(ResultEnum.Success ,  data);
    }
    
    public static ApiResult<T> AsError(string errorMessage)
    {
        return new ApiResult<T>(ResultEnum.Error, default, errorMessage);
    }
    
    public static ApiResult<T> AsFailure(string errorMessage)
    {
        return new ApiResult<T>(ResultEnum.Failure, default, errorMessage);
    }
    
    
}