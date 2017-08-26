using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using shapeomatic_server.Models;

namespace shapeomatic_server.DAL
{
    public class DALConverter
    {
        public static User UserTableToObject(user obj, DALDataContext ctx)
        {
            return new User
            {
                facebookId = obj.facebookId,
                friends = null,
                name = obj.name,
                pic = obj.pic,
                score = obj.score ?? (-1),
            };
        }
    }
}