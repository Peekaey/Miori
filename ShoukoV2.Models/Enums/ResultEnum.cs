namespace ShoukoV2.Models.Enums;

public enum ResultEnum
{
    // Known Failures, Invalid Inputs, Missing Secrets, etcc
    AsFailure = 0,
    // Unexpected Errors, Try Catches, etc
    AsError = 1,
    AsSuccess = 2
}