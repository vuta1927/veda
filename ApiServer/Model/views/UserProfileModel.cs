using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class UserProfileModel
    {
        public class UserProfile
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
        }
    }
}
