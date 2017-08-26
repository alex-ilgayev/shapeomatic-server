using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using shapeomatic_server.DAL;
using shapeomatic_server.Models;

namespace shapeomatic_server
{
    public class UserRepository
    {
        private static UserRepository _ins;
        private DALDataContext dc;

        private UserRepository()
        {
            dc = new DALDataContext();
            
        }

        public static UserRepository GetInstance() {
            if(_ins == null)
                _ins = new UserRepository();
            return _ins;
        }

        // returns all items.
        public IEnumerable<User> GetAll()
        {
            return dc.users.Select(row => Get(row.facebookId));
        }

        // returns plan with specified id.
        // returns null if not found.
        public User Get(long id)
        {
            var elem = dc.users.SingleOrDefault(x => x.facebookId == id);
            if (elem == null)
                return null;
            var friendList = new LinkedList<User>();
            foreach (var e in elem.friends)
            {
                var friend = dc.users.SingleOrDefault(x => x.facebookId == e.facebookId2);
                if (friend == null)
                    continue;
                friendList.AddFirst(DALConverter.UserTableToObject(friend, dc));
            }
            
            var convertedElem = DALConverter.UserTableToObject(elem, dc);
            convertedElem.friends = friendList;
            return convertedElem;
        }
        
        // adding the item and returning him.
        // receiving item without id, and returning with the new id.
        public User Add(User item)
        {
            if (Get(item.facebookId) != null)
            {
                if (Update(item))
                    return item;
                return null;   
            }

            user plan = new user()
            {
                facebookId = item.facebookId,
                name = item.name,
                pic = item.pic,
                score = item.score,
            };
            using (var ctx = new DALDataContext())
            {
                ctx.users.InsertOnSubmit(plan);
                ctx.SubmitChanges();   
            }

            if (item.friends == null)
                return item;

            foreach (User friend in item.friends)
            {
                if (Get(friend.facebookId) == null)
                {
                    friend.score = (-1);
                    Add(friend);
                }
                using (var ctx = new DALDataContext())
                {
                    var friendRow = new friend()
                    {
                        facebookId1 = item.facebookId,
                        facebookId2 = friend.facebookId
                    };
                    ctx.friends.InsertOnSubmit(friendRow);
                    friendRow = new friend()
                    {
                        facebookId2 = item.facebookId,
                        facebookId1 = friend.facebookId
                    };
                    ctx.friends.InsertOnSubmit(friendRow);
                    ctx.SubmitChanges();
                }
            }
            return item;
        }

        // if no item with such id exists, returns false,
        // otherwise removing the item and returning true.
        public bool Remove(long id)
        {
            using (var ctx = new DALDataContext())
            {
                var items = from e in ctx.users
                            where e.facebookId == id
                            select e;

                if (items.Count() != 1)
                    return false;

                var item = items.Single();

                foreach (var friendRow in item.friends)
                {
                    ctx.friends.DeleteOnSubmit(friendRow);
                }

                foreach (var friendRow in item.friends1)
                {
                    ctx.friends.DeleteOnSubmit(friendRow);
                }
                ctx.SubmitChanges();

                ctx.users.DeleteOnSubmit(item);
                ctx.SubmitChanges();    
            }
            return true;
        }

        public bool Update(User item)
        {
            using (var ctx = new DALDataContext())
            {
                var items = from e in ctx.users
                            where e.facebookId == item.facebookId
                            select e;

                if (items.Count() != 1)
                    return false;

                var singleItem = items.Single();
                singleItem.name = item.name;
                singleItem.pic = item.pic;
                singleItem.score = item.score != (-1) ? (item.score > singleItem.score ? item.score : singleItem.score ) : singleItem.score;

                ctx.SubmitChanges();

                var newFriends = item.friends;
                var currentFriendship = ctx.friends.Where(x => x.facebookId1 == item.facebookId);
                var currentFriends = currentFriendship.Select(x => Get(x.facebookId2)).ToList();

                foreach (var someFriendship in currentFriendship)
                {
                    var currUser = Get(someFriendship.facebookId2);
                    if (!newFriends.Contains(currUser))
                    {
                        ctx.friends.DeleteOnSubmit(someFriendship);
                        var reversed = ctx.friends.SingleOrDefault(x => x.facebookId1 == someFriendship.facebookId2 &&
                                                                        x.facebookId2 == someFriendship.facebookId1);
                        if(reversed != null)
                            ctx.friends.DeleteOnSubmit(reversed);
                    }
                }

                foreach (var newFriend in newFriends)
                {
                    if (!currentFriends.Contains(newFriend))
                    {
                        if (Get(newFriend.facebookId) == null)
                        {
                            newFriend.score = (-1);
                            Add(newFriend);
                        }

                        var entry = new friend()
                        {
                            facebookId1 = item.facebookId,
                            facebookId2 = newFriend.facebookId
                        };
                        ctx.friends.InsertOnSubmit(entry);
                        entry = new friend()
                        {
                            facebookId2 = item.facebookId,
                            facebookId1 = newFriend.facebookId
                        };
                        ctx.friends.InsertOnSubmit(entry);
                    }
                }
                ctx.SubmitChanges();
            }
            return true;
        }

        public void Clear()
        {
            using (var ctx = new DALDataContext())
            {
                foreach (var plan in ctx.users)
                {
                    ctx.users.DeleteOnSubmit(plan);
                }
                ctx.SubmitChanges();
            }
        }
    }
}