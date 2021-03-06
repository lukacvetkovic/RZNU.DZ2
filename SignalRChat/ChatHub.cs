﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SignalRChat.Common;

namespace SignalRChat
{
    //https://www.codeproject.com/Articles/562023/Asp-Net-SignalR-Chat-Room  source
    public class ChatHub : Hub
    {
        #region Data Members

        static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        static List<MessageDetail> CurrentMessage = new List<MessageDetail>();

        private static readonly string[] Colors = new[]
        {
            "Red", "Orange", "Yellow", "Cyan", "Blue", "Black", "Chartreuse", "DarkGoldenRod", "DarkMagenta",
            "DarkSeaGreen", "DeepPink", "Gold", "MistyRose", "YellowGreen"
        };

        #endregion

            #region Methods

        public void Connect(string userName)
        {
            var id = Context.ConnectionId;

            Random random = new Random();

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetail { ConnectionId = id, UserName = userName, Color = Colors[random.Next(0,Colors.Length-1)] });

                // send to caller
                Clients.Caller.onConnected(id, userName, ConnectedUsers, CurrentMessage);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName);

            }

        }

        public void SendMessageToAll(string userName, string message)
        {

            // store last 100 messages in cache
            var mess=AddMessageinCache(userName, message);

            // Broad cast message
            Clients.All.messageReceived(mess.UserName,mess.Message,mess.Color);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {

            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser != null)
            {
                // send to 
                Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message);

                // send to caller user
                Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message);
            }

        }

        public void SendPrivatePicture(string toUserId, string picture)
        {

            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser != null)
            {
                // send to 
                Clients.Client(toUserId).sendPrivatePicture(fromUserId, fromUser.UserName, picture);

                // send to caller user
                Clients.Caller.sendPrivatePicture(toUserId, fromUser.UserName, picture);
            }

        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.UserName);

            }

            return base.OnDisconnected();
        }


        #endregion

        #region private Messages

        private MessageDetail AddMessageinCache(string userName, string message)
        {
            UserDetail user = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                var mess= new MessageDetail {UserName = userName, Message = message, Color = user.Color};
                CurrentMessage.Add(mess);

                if (CurrentMessage.Count > 100)
                    CurrentMessage.RemoveAt(0);
                return mess;
            }

            return null;
        }

        #endregion
    }

}