using System;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;
using Cinder.Core.Domain.Person;
using Cinder.Platform;
using Cinder.Platform.RestClients;
using Cinder.Platform.Security.AuthenticationProviders.DocFlock;
using Cinder.Platform.Security.AuthProviders;
using Cinder.Platform.Security.AuthProviders.DocFlock;
using Microsoft.WindowsAzure.MobileServices;

namespace Cinder.Platform.Security.AuthenticationProviders.AzureAD
{
    public class AzureAuthenticationProvider : IAuthenticationProvider
    {
        private MobileServiceUser _user;
//=======================================================================================================
/// <summary>
///  Ctor
/// </summary>
//=======================================================================================================
        //public AzureProvider()
        //{}
//=======================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="customerId"></param>
/// <returns></returns>
//=======================================================================================================
        private Organization GetOrganization(Guid customerId)
        {
            ////var request       = new HttpRequest("Customer/Get/" + customerId);
            ////var response      = this.Client.SendGetRequestAsync<Customer>(request).Result;
            ////var organization  = new  Organization();
            ////organization.Name = response.Name;
            ////organization.Id   = response.Id.ToString();

            ////return organization;
            var organization = new Organization();

            if (customerId.ToString() == "00000000-0000-0000-0000-000000000000")
            {
                organization.Id = "b163c78b-ca9c-41f3-92bb-8f52d0550541";
            }
            else
            {
                organization.Id = customerId.ToString();
            }


            return organization;
        }
//=======================================================================================================
/// <summary>
///  Create valid security principal and identity.
/// </summary>
//=======================================================================================================
        private CinderPrincipal CreateValidPrincipal(DocFlockAuthenticationResponse response)
        {
            // Send back a valid principal and identity with the Authentication requests results.

            var user = new User(true)
            {
                Organization = this.GetOrganization(response.CustomerId),
                Email = response.Email,
                Name = response.FirstName + " " + response.LastName,
                Id = response.UserId.ToString()
            };

            var newPrincipal = new CinderPrincipal(user);
            ApplicationContext.Principal = newPrincipal;

            return newPrincipal;
        }
//=======================================================================================================
/// <summary>
///  Create an empty security principal and identity.
/// </summary>
//=======================================================================================================
        private CinderPrincipal CreateEmptyPrincipal()
        {
            // Send back an  principal and identity which refelcts this state.

            var newIdentity = new CinderIdentity(String.Empty, string.Empty, string.Empty, false);
            var newPrincipal = new CinderPrincipal(newIdentity);

            return newPrincipal;
        }
//=======================================================================================================
/// <summary>
/// Method used to do the actual authentication..
/// </summary>
//=======================================================================================================
        public async Task<AuthenticationResponse> AuthenticateAsync(BaseCredentials credentials)
        {
            // When cast to this, represents Email and Password elements. The Docflock way yo. 

            var docFlockCredentials = credentials as CustomCredentials;
            if (docFlockCredentials == null)
                throw new InvalidOperationException(
                    "Must supply a valid set of credentials to Authenticate agsint this provider");

    return null;

    //while (_user == null)
    //{
    //    string message;
    //    try
    //    {
    //        // Change 'MobileService' to the name of your MobileServiceClient instance.
    //        // Sign-in using Facebook authentication.
    //        _user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
    //        message =
    //            string.Format("You are now signed in - {0}", _user.UserId);
    //    }
    //    catch (InvalidOperationException)
    //    {
    //        message = "You must log in. Login Required";
    //    }

    //    //var dialog = new MessageDialog(message);
    //    //dialog.Commands.Add(new UICommand("OK"));
    //    //await dialog.ShowAsync();
    //}









    // Send back either a valid or empty principal to reflect the Authentication outcome..

    //if (response != null)
    //{
    //    Client.Credentials = docFlockCredentials;
    //    return new AuthenticationResponse
    //    {
    //        Principal = this.CreateValidPrincipal(response)
    //    };
    //}
    //else
    //{
    //    return new AuthenticationResponse
    //    {
    //        Principal = this.CreateEmptyPrincipal()
    //    };
    //}
        }
    }
}