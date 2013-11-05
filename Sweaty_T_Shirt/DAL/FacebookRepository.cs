using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Facebook;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace Sweaty_T_Shirt.DAL
{
    public class FacebookRepository
    {
        public const string FB_ACCESS_TOKEN = "accesstoken";
        public const string FB_ID = "id";
        public const string IS_FB_AUTHENTICATED = "is_fb_authenticated";

        public static bool IsFacebookAuthenticated()
        {
            return HttpContext.Current.Session[IS_FB_AUTHENTICATED] != null
                && (bool)HttpContext.Current.Session[IS_FB_AUTHENTICATED] == true;
        }
        public static string GetFacebookPictureUrl()
        {
            if (IsFacebookAuthenticated())
            {
                IDictionary<string, string> userData = (Microsoft.Web.WebPages.OAuth.OAuthWebSecurity.GetOAuthClientData("facebook").AuthenticationClient as FacebookExtendedClient).UserData;
                return userData["picture"];
            }
            else
            {
                return null;
            }
        }

        public static void PostToFacebook(Sweaty_T_Shirt.Models.SweatyTShirt sweatyTShirt)
        {
            string message = string.Format("{0} has posted {1} sweaty-t-shirt{2} on http://SweatyTShirt.com: {3}", sweatyTShirt.UserProfile.FullName, sweatyTShirt.Amount, sweatyTShirt.Amount > 1 ? "s" : string.Empty, sweatyTShirt.Description);

            try
            {
                var client = new FacebookClient(HttpContext.Current.Session[FB_ACCESS_TOKEN].ToString());
                dynamic response = client.Post("/me/feed", new { message = message });
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Elmah.Error(ex));
            }
        }

        public static FacebookExtendedClient GetFacebookExtendedClient()
        {
            string FacebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
            if(string.IsNullOrEmpty(FacebookAppId))
            {
                throw new ApplicationException("Missing required Appsetting FacebookAppId");
            }

            string FacebookAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
            if (string.IsNullOrEmpty(FacebookAppSecret))
            {
                throw new ApplicationException("Missing required Appsetting FacebookAppSecret");
            }

            string FacebookScope = ConfigurationManager.AppSettings["FacebookScope"];
            if (string.IsNullOrEmpty(FacebookScope))
            {
                throw new ApplicationException("Missing required Appsetting FacebookScope");
            }

            string FacebookFields = ConfigurationManager.AppSettings["FacebookFields"];
            if (string.IsNullOrEmpty(FacebookFields))
            {
                throw new ApplicationException("Missing required Appsetting FacebookFields");
            }
          
            return new FacebookExtendedClient(
               FacebookAppId,
               FacebookAppSecret,
               FacebookFields,
               new Func<string, object, string>(fieldsTransformer),
               FacebookScope);
        }

        private static string fieldsTransformer(string key, object value)
        {
            switch (key)
            {
                case "picture":
                    var data = (value as IDictionary<string, object>)["data"] as IDictionary<string, object>;
                    return data["url"].ToString();
                case "age_range":
                    var min = (value as IDictionary<string, object>)["min"];
                    return min.ToString();
                default:
                    return value.ToString();
            }
        }
    }

    public class FacebookExtendedClient : DotNetOpenAuth.AspNet.Clients.OAuth2Client
    {
        protected FacebookClient facebookClient;
        protected string fields;
        protected string scope;
        protected Func<string, object, string> fieldTransformer;
        protected bool emailAsUsername;
        protected IDictionary<string, string> userData;

        private string[] splittedFields;
        private string[] splittedScope;

        protected const string serviceLoginBaseUrl = "https://www.facebook.com/dialog/oauth";
        protected const string serviceMeBaseUrl = "https://graph.facebook.com/me";
        protected const string serviceAccessTokenBaseUrl = "https://graph.facebook.com/oauth/access_token";

        /// <summary>
        /// Create an instrance of the class.
        /// </summary>
        /// <param name="appId">The App ID of the application used to connect to Facebook service.</param>
        /// <param name="appSecret">The App Secret of the application used to connect to Facebook service.</param>
        /// <param name="fields">
        /// String containing comma separated fields to add to the request.
        /// If empty the request will retrieve the default fields based of the specified scope.
        /// </param>
        /// <param name="fieldTransformer">
        /// Function to be applied to the values retrived from facebook.
        /// If null provided the method will try to cast values from object to string explicitly,
        /// an InvalidCastException will be thrown if the cast will not be possible.
        /// </param>
        /// <param name="scope">
        /// String containing comma separated permissions to add to the request.
        /// If empty the request will have the basic scope.
        /// </param>
        /// <param name="emailAsUsername">Makes the email of the facebook user used as authentication username.</param>
        public FacebookExtendedClient(string appId, string appSecret, string fields = "", Func<string, object, string> fieldTransformer = null, string scope = "", bool emailAsUsername = false)
            : base("facebook")
        {
            if (string.IsNullOrEmpty(appId))
                throw new ArgumentException("The appId argument can not be null or empty.", "appId");
            if (string.IsNullOrEmpty(appSecret))
                throw new ArgumentException("The appSecret argument can not be null or empty.", "appSecret");

            fields = fields.Replace(" ", "");
            scope = scope.Replace(" ", "");
            this.splittedFields = fields.Split(',');
            this.splittedScope = scope.Split(',');

            if (emailAsUsername == true && !this.splittedFields.Contains("email") && !this.splittedScope.Contains("email"))
                throw new ArgumentException("The scope argument must contain the 'email' permission and the 'email' field to allow emailAsUsername to true.", "scope");

            this.facebookClient = new FacebookClient();
            this.facebookClient.AppId = appId;
            this.facebookClient.AppSecret = appSecret;
            this.fields = fields;
            this.fieldTransformer = fieldTransformer;
            this.scope = scope;
            this.emailAsUsername = emailAsUsername;
        }

        public FacebookClient FacebookClient
        {
            get
            {
                return this.facebookClient;
            }
        }

        public IDictionary<string, string> UserData
        {
            get
            {
                return this.userData;
            }
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("redirect_uri", returnUrl.AbsoluteUri);

            if (!string.IsNullOrEmpty(this.scope))
                parameters.Add("scope", this.scope);

            return this.facebookClient.GetLoginUrl(parameters);
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            // This method makes the AuthenticationResult's UserName property be the facebook username of the logged user,
            // but if the facebook username is missing the facebook id will be used.
            // If emailAsUsername is true then AuthenticationResult's UserName property is the email retrieved from facebook
            // and the facebook username can be retrieved by the key "fb_username" in this.userData

            FacebookClient facebookClient = new FacebookClient(accessToken);

            var getResult = facebookClient.Get<IDictionary<string, object>>("me", new { fields = this.fields });
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (this.fieldTransformer != null)
            {
                foreach (var pair in getResult)
                    result.Add(pair.Key, this.fieldTransformer(pair.Key, pair.Value));
            }
            else
            {
                foreach (var pair in getResult)
                {
                    string value = pair.Value.ToString();

                    if (value == null)
                        throw new InvalidCastException("Cast not possible for the object associate to the key '" + pair.Key + "'.");

                    result.Add(pair.Key, value);
                }
            }

            if (this.splittedFields.Contains("username"))
                result["fb_username"] = result["username"];

            if (this.emailAsUsername)
                result["username"] = result["email"];

            this.userData = result;

            return result;
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            UriBuilder builder = new UriBuilder(serviceAccessTokenBaseUrl);
            builder.Query = string.Format("client_id={0}&client_secret={1}&redirect_uri={2}&code={3}",
                this.facebookClient.AppId, this.facebookClient.AppSecret, HttpUtility.UrlEncode(Encoding.ASCII.GetBytes(returnUrl.AbsoluteUri)), authorizationCode);

            using (WebClient client = new WebClient())
            {
                string str = client.DownloadString(builder.Uri);

                if (string.IsNullOrEmpty(str))
                    return null;

                return HttpUtility.ParseQueryString(str)["access_token"];
            }
        }
    }
}