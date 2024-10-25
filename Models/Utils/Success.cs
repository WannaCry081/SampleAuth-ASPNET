namespace sample_auth_aspnet.Models.Utils;

public static class Success
{
    public static string USER_IS_AUTHENTICATED()
    {
        return "User has been successfully logged in";
    }

    public static string ENTITY_CREATED(string entity)
    {
        return $"{entity} has been successfully created";
    }
}
