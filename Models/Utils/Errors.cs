namespace sample_auth_aspnet.Models.Utils;

public static class Errors
{
    public enum ErrorType
    {
        ValidationError,
        BadRequest,
        Unauthorized,
        NotFound,
        InternalServerError,
    }

    public const string NotFound = "Not found";
    public const string Invalid = "Invalid";
    public const string AlreadyExists = "Already exists";
    public const string Unauthorized = "Unauthorized";
    public const string Required = "Required";

    public static string FIELD_IS_REQUIRED(string field)
    {
        return $"{field} is required";
    }

    public static string ERROR_GETTING_RESOURCE(string resource)
    {
        return $"Error getting {resource}";
    }

    public static string ERROR_CREATING_RESOURCE(string resource)
    {
        return $"Error creating {resource}";
    }

    public static string ERROR_UPDATING_RESOURCE(string resource)
    {
        return $"Error updating {resource}";
    }

    public static string ERROR_DELETING_RESOURCE(string resource)
    {
        return $"Error deleting {resource}";
    }

    public static string ERROR_LOGGING_RESOURCE(string resource)
    {
        return "Error logging";
    }
}
