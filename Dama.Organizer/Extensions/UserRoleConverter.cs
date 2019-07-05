using Dama.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Organizer.Extensions
{
    public static class UserRoleConverter
    {
        public static IEnumerable<UserRole> ToUserRole(this IEnumerable<string> roleCollection)
        {
           return roleCollection
                        .Select(role => 
                                (UserRole) Enum.Parse(typeof(UserRole), role));
        }
    }
}
