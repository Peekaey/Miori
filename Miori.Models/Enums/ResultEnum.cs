namespace Miori.Models.Enums;

public enum ResultEnum
{
    // Known Failures, Invalid Inputs, Missing Secrets, etcc
    Failure = 0,
    // Unexpected Errors, Try Catches, etc
    Error = 1,
    Success = 2
}