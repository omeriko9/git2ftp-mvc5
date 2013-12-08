using System;
using System.Web;

public class GitHubWebHookHandler : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        var p = new Uri(HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.AbsolutePath, ""));
        System.Net.WebRequest wr = System.Net.WebRequest.Create(new Uri(p, "Github/WebhookEvent"));
        wr.Headers.Add("texttodisplay", new System.IO.StreamReader(context.Request.InputStream).ReadToEnd());
        wr.Method = "POST";
        wr.ContentLength = 0;
        wr.GetResponse();
        context.Response.ContentType = "text/plain";
        context.Response.Write("Hello World");
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}