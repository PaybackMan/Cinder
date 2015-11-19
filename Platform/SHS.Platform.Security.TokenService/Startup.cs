using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using SHS.Platform.Security.TokenService.Configuration;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace SHS.Platform.Security.TokenService
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var factory = InMemoryFactory.Create(
                scopes: Scopes.Get(),
                clients: Clients.Get());
                //users: Users.Get());


                var userService     = new UserService();
                factory.UserService = new Registration<IUserService>(resolver => userService);


                var options = new IdentityServerOptions
                {
                    Factory = factory
                };

           

                app.UseIdentityServer(options);
        }
    }
}
