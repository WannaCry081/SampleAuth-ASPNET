namespace sample_auth_aspnet.Models.Utils;

public static class Success
{
    public static string IS_AUTHENTICATED()
    {
        return "User has been successfully authenticated";
    }

    public static string ENTITY_CREATED(string entity)
    {
        return $"{entity} has been successfully created";
    }

    public static string ENTITY_UPDATED(string entity)
    {
        return $"{entity} has been successfully updated";
    }

    public static string ENTITY_DELETED(string entity)
    {
        return $"{entity} has been successfully deleted";
    }

    public static string ENTITY_RETRIEVED(string entity)
    {
        return $"{entity} has been successfully retrieved";
    }
}
