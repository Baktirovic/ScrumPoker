using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ScrumPoker.Helpers;
using ScrumPoker.Models;

namespace ScrumPoker.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly IList<Room> Rooms = new List<Room>();
        private static readonly IList<User> Users = new List<User>();


        private User GetUser()
        {
            return Users.FirstOrDefault(q => q.Id == Context.ConnectionId);
        }

        private Room GetUsersCurrentRoom()
        {
            return Rooms.FirstOrDefault(q => q.Users.Any(us => us.Id == Context.ConnectionId));
        }

        public async Task SetTopic(string message)
        {
            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && room.Admin == user.Id)
            {
                await Clients.Group(room.Name).SendAsync("setTopic", message);
            } 
        }

        public async Task ShowVotes()
        {

            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && room.Admin == user.Id)
            {
                await Clients.Group(room.Name).SendAsync("showVotes", room.Votes);   
            }else
            {
                await Clients.Group(room.Name).SendAsync("receive", "User can not show votes", user.Name);
            } 
        }

        public async Task NewRound()
        {            
            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && room.Admin == user.Id)
            {
                room.Votes.Clear();
                await Clients.Group(room.Name).SendAsync("newRound");
            } 
        }

        public async Task Vote(string vote)
        {

            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && user != default)
            {
                var oldVote = room.Votes.FirstOrDefault(q => q.UserId == user.Id);
                if (oldVote != null)
                {
                    oldVote.Score = int.Parse(vote); 
                }
                else
                {
                    room.Votes.Add(new Vote
                    { 
                        Score = int.Parse(vote),
                        UserId = user.Id
                    });

                    await Clients.Group(room.Name).SendAsync("voteMessage", user.Name, " Voted ");
                    await Clients.Group(room.Name).SendAsync("voted", user.Id);
                }
            }
            else
            {
               await Clients.Client(Context.ConnectionId).SendAsync("receive", "Unable to find user or room", "");
            }
          
        }
 
        private async Task JoinRoom(string roomName)
        {
            var room = Rooms.FirstOrDefault(q => q.Name == roomName);
            var user = GetUser();
            if(room != default && user != default)
            { 
                if (!room.Users.Contains(user))
                {
                    room.Users.Add(user);

                }
                //TODO remove user from other rooms
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

                await Clients.Client(user.Id).SendAsync("enter", roomName);
                await Clients.Group(roomName).SendAsync("joined", room.Users);
                if(room.Admin == user.Id)
                {
                    await Clients.Client(user.Id).SendAsync("admin");
                }
               
            } 
        }
        public async Task Join(string userName, string roomName)
        {
            if(userName.Length > 10 || userName.Length< 3)
            {
                return;
            }

            if (string.IsNullOrEmpty(roomName))
            { 
                roomName = SessionHelper.RandomString(6, true); 
            }
          
            var user = Users.FirstOrDefault(q => q.Id == Context.ConnectionId);
            if (user != default)
            {
                user.Name = userName;
            }
            else
            {
                user = new User { Id = Context.ConnectionId, Name = userName, TimeStamp = DateTime.Now};
                Users.Add(user);
            } 
            if(!CheckIfRoomExists(roomName) && user != default)
            {
                Rooms.Add(new Room
                {
                    Admin = Context.ConnectionId,
                    Name = roomName,
                    Users = new HashSet<User> { user },
                    Votes = new List<Vote>(),
                    TimeStamp = DateTime.Now
                });
            }
            await JoinRoom(roomName); 

        }
        public async Task RoomExists(string roomName)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("RoomExists", CheckIfRoomExists(roomName) ? roomName : "");
        }
        public Task LeaveRoom(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        private static bool CheckIfRoomExists(string roomName)
        {
            return Rooms.Any(q=> q.Name.Contains(roomName));
        }
  
    }
}
