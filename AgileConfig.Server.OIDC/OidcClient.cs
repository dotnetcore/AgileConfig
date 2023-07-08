using System.Dynamic;

namespace AgileConfig.Server.OIDC
{
    public class OidcClient
    {
        public dynamic Validate(string code)
        {
            dynamic obj = new ExpandoObject { 
            };
            obj.IdToken = "123";
            obj.AccessToken = "321";

            return obj;
        }

        public dynamic UnboxIdToken(string idToken)
        {
            dynamic obj = new ExpandoObject
            {
            };
            obj.Id = "super_admin";
            obj.UserName = "admin";
            return obj;
        }
    }
}