using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

namespace WebService1
{
    [WebService(Namespace = "http://www.nlog-project.org/example")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class Service1 : System.Web.Services.WebService
    {
        [WebMethod]
        public void HelloWorld(string n1, string n2, string n3)
        {
            HttpContext.Current.Trace.Write("n1 " + n1);
            HttpContext.Current.Trace.Write("n2 " + n2);
            HttpContext.Current.Trace.Write("n3 " + n3);
        }
    }
}
