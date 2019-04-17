using Saml;
using System;
using System.Security.Principal;
using System.Threading;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace mvcoktasamlviernes.Controllers
{
    public class AccountController : Controller
    {
        // GET: LogOn
        public ActionResult LogOn()
        {
            var samlEndpoint = WebConfigurationManager.AppSettings["samlEndpoint"];

            var request = new AuthRequest("", "");
            //generate the provider URL
            string url = request.GetRedirectUrl(samlEndpoint);

            //then redirect your user to the above "url" var
            //for example, like this:

            Response.Redirect(url);
            return View();
        }

        [HttpPost]
        public ActionResult SamlConsume()
        {
            //specify the certificate that your SAML provider has given to you
            string samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIDpDCCAoygAwIBAgIGAWnemqZjMA0GCSqGSIb3DQEBCwUAMIGSMQswCQYDVQQGEwJVUzETMBEG
A1UECAwKQ2FsaWZvcm5pYTEWMBQGA1UEBwwNU2FuIEZyYW5jaXNjbzENMAsGA1UECgwET2t0YTEU
MBIGA1UECwwLU1NPUHJvdmlkZXIxEzARBgNVBAMMCmRldi02NDQ2MDYxHDAaBgkqhkiG9w0BCQEW
DWluZm9Ab2t0YS5jb20wHhcNMTkwNDAyMTUxMDEwWhcNMjkwNDAyMTUxMTEwWjCBkjELMAkGA1UE
BhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExFjAUBgNVBAcMDVNhbiBGcmFuY2lzY28xDTALBgNV
BAoMBE9rdGExFDASBgNVBAsMC1NTT1Byb3ZpZGVyMRMwEQYDVQQDDApkZXYtNjQ0NjA2MRwwGgYJ
KoZIhvcNAQkBFg1pbmZvQG9rdGEuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA
xfXuUxEheUJDZ3IHK9mWlAa3/zC8quHlLVRQixLbpkkVOHVHsjdn6WuyKWKxWr6sfK3ZNodqpMXs
rp+xukhxrsxJns7nMhigH7PcWMvr2uBkLT19kxupHwiExjf2/IvN08sk/5IGWm8J54va7Ft5q/+l
wnPOnegfmb+INlCGdd41FsMNn/yhXHVGxUiNypAZuII5A1qzCmXVKQEebm8iTqJDXPBRfKiHxs3D
YiUzHFq6Uk7Y9qRsyyttehgo59ovfTkY3mSy/du30n2TEwWcf0GrPtjQA0jgnxQdE0YJQB3TgXqm
L/7YK8tFuLEbWd7SWOEszRiEUfETWdrS9BMi3QIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQAFYHSX
LPeDGtkxZxnbxjCWyQ5aMR+YbEAP/zjC0rKytDnmZFQKNU6CVbkOmmatU24VCiUcp//fdNk9qMFJ
0855m1d6BH1HdPKT5pWmAskRxKCGdhfbTR+fsCAiGaORUk41SHaO1iCIHsm3f/UQZrU9BFxZFmLK
Ehs3Lup93f/L/ZH+DpJnyAZASujkK8eMJB+UhjSHxfBbM/0kvDzJcDm2Z9JUXcqmvcAcfSgbxLY3
QYBYAdHsssO6b099nu93MaOMRMyLAvkcZCfmo5w4vk+Wm/em3I0VW94sFnIzWyipnECQQtWCis3n
YsNIn9+JcNllBHc0PHL0htqAV3wMmrZ6
-----END CERTIFICATE-----";

            var samlResponse = new Response(samlCertificate);
            samlResponse.LoadXmlFromBase64(Request.Form["SAMLResponse"]); //SAML providers usually POST the data into this var

            if (samlResponse.IsValid())
            {
                //WOOHOO!!! user is logged in
                //YAY!

                //Some more optional stuff for you
                //lets extract username/firstname etc
                string username, email, firstname, lastname;
                try
                {
                    username = samlResponse.GetNameID();
                    email = samlResponse.GetEmail();
                    firstname = samlResponse.GetFirstName();
                    lastname = samlResponse.GetLastName();
                }
                catch (Exception ex)
                {
                    //insert error handling code
                    //no, really, please do
                    return null;
                }

                var identity = new GenericIdentity(username);


                SetPrincipal(new GenericPrincipal(identity, null));

                Session.Add("username", username);
                FormsAuthentication.SetAuthCookie(username, true);
                return RedirectToAction("About", "home");
            }

            else
            {
                ViewBag.Message = samlResponse._error ?? "OKerror";
                return View(); //Error with the certificate or something else. Handle this.
            }
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.User = principal;
            }
        }
        public ActionResult Logout()
        {

            FormsAuthentication.SignOut();
            Session.Clear(); //Clear user sesion state
            return RedirectToAction("Index", "Home");
        }


    }
}