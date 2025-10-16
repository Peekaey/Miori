using System.Net;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.Models;

public class ApiResult<T>
{
    public ResultEnum Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; } = string.Empty;
    
    public ApiResult(ResultEnum resultEnum, HttpStatusCode statusCode, T? data, string? errorMessage = "")
    {
        StatusCode = statusCode;
        Data = data;
        ErrorMessage = errorMessage;
        Result = resultEnum;
    }
    
    public static ApiResult<T> AsSuccess(T data)
    {
        return new ApiResult<T>(ResultEnum.Success , HttpStatusCode.OK, data);
    }
    
    public static ApiResult<T> AsError(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new ApiResult<T>(ResultEnum.Error, statusCode, default, errorMessage);
    }
    
    public static ApiResult<T> AsFailure(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new ApiResult<T>(ResultEnum.Failure, statusCode, default, errorMessage);
    }
    
    
}