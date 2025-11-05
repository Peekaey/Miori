using System.Net;
using Miori.Models.Enums;

namespace Miori.Models;

public class ApiResult<T>
{
    public ResultEnum ResultOutcome { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; } = "An unexpected error occurred on the server. Please try again later.";


    public ApiResult(ResultEnum resultOutcome, HttpStatusCode statusCode, T? data, string errorMessage)
    {
        ResultOutcome = resultOutcome;
        StatusCode = statusCode;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static ApiResult<T> AsSuccess(T data)
    {
        return new ApiResult<T>(ResultEnum.Success, HttpStatusCode.OK, data, "");
    }

    public static ApiResult<T> AsInternalError()
    {
        return new ApiResult<T>(ResultEnum.Error, HttpStatusCode.InternalServerError, default, "");
    }

    public static ApiResult<T> AsErrorDisplayFriendlyMessage(string errorMessage, HttpStatusCode statusCode)
    {
        return new ApiResult<T>(ResultEnum.Error, statusCode, default, errorMessage);
    }
}