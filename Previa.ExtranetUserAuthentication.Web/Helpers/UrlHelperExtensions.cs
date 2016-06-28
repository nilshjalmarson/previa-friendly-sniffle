using System;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web;
using log4net;

namespace Previa.ExtranetUserAuthentication.Web.Helpers
{
    public static class UrlHelperExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Like Url.Content(...) but also appends a timestamp representing the last write date and time
        /// of the file to the querystring. Useful for cache-busting.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="contentPath">The virtual path of the content.</param>
        /// <returns></returns>
        public static string ContentWithTimestamp(this UrlHelper helper, string contentPath)
        {
            var contentUrl = helper.Content(contentPath);
            try
            {
                var fileLastModifiedTicks = File.GetLastWriteTime(HttpContext.Current.Server.MapPath(contentPath)).Ticks;
                return contentUrl.Contains("?") ? contentUrl + "&" + fileLastModifiedTicks : contentUrl + "?" + fileLastModifiedTicks;
            } 
            catch (Exception ex)
            {
                Log.Error("Failed to retrieve and/or append last-modified timestamp to content url.", ex);
                return contentUrl;
            }
        }
    }
}