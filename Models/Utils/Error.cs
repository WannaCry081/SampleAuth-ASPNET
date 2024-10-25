namespace sample_auth_aspnet.Models.Utils;

public static class Error
{
    public enum ErrorType
    {
        ValidationError,
        BadRequest,
        Unauthorized,
        NotFound,
        InternalServer
    }

    public const string NotFound = "Resource not found";
    public const string ValidationError = "Validation failed";
    public const string AlreadyExists = "Already exists";
    public const string Unauthorized = "Access denied";

    public static string FIELD_IS_REQUIRED(string field)
    {
        return $"{field} is required";
    }
}
