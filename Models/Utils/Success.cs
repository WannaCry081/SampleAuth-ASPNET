namespace sample_auth_aspnet.Models.Utils;

public static class Success
{
    public const string IS_AUTHENTICATED = "User is successfully authenticated.";
    public const string EMAILED_SUCCESSFULLY = "Email sent successfully";

    public static string ENTITY_CREATED(string entity)
    {
        return $"{entity} is successfully created.";
    }

    public static string ENTITY_UPDATED(string entity)
    {
        return $"{entity} is successfully updated.";
    }

    public static string ENTITY_DELETED(string entity)
    {
        return $"{entity} is successfully deleted.";
    }

    public static string ENTITY_RETRIEVED(string entity)
    {
        return $"{entity} is successfully retrieved.";
    }
}
