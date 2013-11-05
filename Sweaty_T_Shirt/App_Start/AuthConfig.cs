using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using Sweaty_T_Shirt.Models;
using Sweaty_T_Shirt.DAL;

namespace Sweaty_T_Shirt
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //link to SweatyShirt facebook application.
            //https://developers.facebook.com/apps/1427057544185579

            //to post to Facebook wall need add that to scope on registration, no way to do that using MS object.
            //Dictionary<string, object> extraData = new Dictionary<string, object>();
            //extraData.Add("Icon", "~/Images/facebook.png");
            /*OAuthWebSecurity.RegisterFacebookClient(
                appId: "416517368461806",
                appSecret: "b15efe8b9c46dfbced7e6f38733d71e8",
                displayName: "Facebook",
                extraData: extraData);*/

            //http://stackoverflow.com/questions/12610402/oauthwebsecurity-with-facebook-not-using-email-permission-as-expected
            //http://social.msdn.microsoft.com/Forums/wpapps/en-US/7e84e9ac-516a-4b48-b8b7-b8215e0e02a6/facebookclient-posting-a-message-oauthexception-200-200-the-user-hasnt-authorized-the
            //http://www.c-sharpcorner.com/UploadFile/raj1979/post-on-facebook-users-wall-using-Asp-Net-C-Sharp/
            //this is approach I used:
            //http://stackoverflow.com/questions/14987868/net-mvc-app-facebook-oauth-with-defiend-scope

            OAuthWebSecurity.RegisterClient(FacebookRepository.GetFacebookExtendedClient());
                   
            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
