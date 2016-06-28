using System;
namespace Previa.ExtranetUserAuthentication.Web.Tests
{
    public static class Do
    {
        /// <summary>
        /// Performs an action and swallows any exceptions that are thrown.
        /// </summary>
        /// <param name="whatever"></param>
        public static void Suppress(Action whatever)
        {
            try
            {
                whatever();
            }
            catch
            {}
        }
    }
}