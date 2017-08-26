using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Configuration;

namespace shapeomatic_server.Models
{
    [DataContract(Namespace="")]
    public class User
    {
        [DataMember(Name = "facebookId", Order = 0)]
        public long facebookId;
        [DataMember(Name = "friends", Order = 1)]
        public IEnumerable<User> friends;
        [DataMember(Name = "name", Order = 2)]
        public string name;
        [DataMember(Name = "pic", Order = 3)]
        public string pic;
        [DataMember(Name = "score", Order = 4)] 
        public int score;

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            User that = obj as User;
            return facebookId == that.facebookId;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Convert.ToInt32(facebookId);
        }
    }
}