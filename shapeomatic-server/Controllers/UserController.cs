using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using shapeomatic_server.Models;

namespace shapeomatic_server.Controllers
{
    public class UserController : ApiController
    {
        // GET: api/User
        public IEnumerable<User> GetAllUsers()
        {
            return UserRepository.GetInstance().GetAll();
        }

        // GET: api/User/5
        public User GetUserById(long id)
        {
            User returnedVal = UserRepository.GetInstance().Get(id);
            if (returnedVal == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            return returnedVal;
        }

        public void DeleteProduct(long id)
        {
            var returnVal = UserRepository.GetInstance().Remove(id);
            if (returnVal == false)
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // POST: api/User
        public HttpResponseMessage PostUser(User user)
        {
            if (user == null || user.name == null ||
                user.pic == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            User returnedVal = UserRepository.GetInstance().Add(user);
            var response = Request.CreateResponse(HttpStatusCode.Created, returnedVal);

            string uri = Url.Link("DefaultApi", new { id = returnedVal.facebookId});
            response.Headers.Location = new Uri(uri);
            return response;
        }

        public void PutUser(long id, User user)
        {
            if (user == null || user.name == null ||
                user.pic == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            user.facebookId = id;
            if (!UserRepository.GetInstance().Update(user))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
