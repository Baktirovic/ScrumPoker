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
         
        public async Task SetTopicAsync(string message)
        {
            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && room.Admin == user.Id)
            {
                await Clients.Group(room.Name).SendAsync("setTopic", message);
            } 
        }

        public async Task ShowVotesAsync()
        {

            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && room.Admin == user.Id)
            {
                int mode =
                     room.Votes
                     .GroupBy(x => x.Score)
                     .OrderByDescending(x => x.Count()).ThenBy(x => x.Key)
                     .Select(x => (int?)x.Key)
                     .FirstOrDefault()?? 0;
                int? median = Convert.ToInt32(Median(room.Votes.Select(q => q.Score).ToArray()));
                await Clients.Group(room.Name).SendAsync("showVotes", room.Votes, median, mode);   
            }
            else
            {
                if (room != null)
                    await Clients.Group(room.Name).SendAsync("receive", "User can not show votes", user.Name);
            } 
        }

        private static decimal Median(int[] votes)
        {
            var sortedList = votes.OrderBy(x => x).ToList();
            var mid = (sortedList.Count - 1) / 2.0;
            return (sortedList[(int)(mid)] + sortedList[(int)(mid + 0.5)]) / 2;
        }
        public async Task NewRoundAsync()
        {            
            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default && room.Admin == user.Id)
            {
                room.Votes.Clear();
                await Clients.Group(room.Name).SendAsync("newRound");
            } 
        }

        public async Task VoteAsync(string vote)
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
 
        private async Task JoinRoomAsync(string roomName)
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
        public async Task JoinChatAsync(string userName, string roomName)
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
            if(!CheckIfRoomExists(roomName))
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
            await JoinRoomAsync(roomName); 

        }
        public async Task ValidateUserNameAndRoomNameAsync(string roomName, string userName)
        {

            await Clients.Client(Context.ConnectionId).SendAsync("ValidateUserNameAndRoomName",
                            CheckIfRoomExists(roomName) ? roomName : "",
                            !CheckIfUserNameExists(userName)? userName : "");
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            var room = GetUsersCurrentRoom();
            var user = GetUser();
            if (room != default)
            {
                room.Users.Remove(user);
                if (room.Users.Count > 0)
                {
                    if (room.Admin == user.Id)
                    {
                        room.Admin = room.Users.FirstOrDefault().Id;
                    }
                    room.Votes.RemoveAll(q => q.UserId == user.Id);
                }
                else
                {
                    Rooms.Remove(room);
                }

                await Clients.Group(room.Name).SendAsync("disconnected", user.Id);
            }

            Users.Remove(user);
            await base.OnDisconnectedAsync(exception);
        }

        private static bool CheckIfRoomExists(string roomName)
        {
            return Rooms.Any(q=> q.Name.Contains(roomName));
        }

        private static bool CheckIfUserNameExists(string userName)
        {
            return Users.Any(q => q.Name.Contains(userName));
        }

        private User GetUser()
        {
            return Users.FirstOrDefault(q => q.Id == Context.ConnectionId);
        }

        private Room GetUsersCurrentRoom()
        {
            return Rooms.FirstOrDefault(q => q.Users.Any(us => us.Id == Context.ConnectionId));
        }


    }
}
