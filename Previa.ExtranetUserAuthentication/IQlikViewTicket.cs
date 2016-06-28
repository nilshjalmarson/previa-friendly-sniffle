namespace Previa.ExtranetUserAuthentication
{
    public interface IQlikViewTicket
    {
        string GetWebTicket(string username);
    }
}