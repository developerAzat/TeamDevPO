using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using SignalRChat.DAL;

namespace SignalRChat
{
    // Core class that intercepts all messages from all clients
    public class SiemplifyChatHub : Hub
    {
        static ConcurrentDictionary<string, ChatData.User> _userConnections = new ConcurrentDictionary<string, ChatData.User>();

        public static void ActivateChat(string userName, string contactName)
        {


            //var user = GetContactByName(userName);
            //var contact = GetContactByName(contactName);

            //if (user != null)
            //    user.ActiveContact = contact;

            //if (contact != null)
            //    contact.ActiveContact = user;
        }
        //public override addNewMessageToPage(string contact, string name, string message, string time, string messagePosition)
        //{

        //}


        private static ChatData.User GetContactByName(string name = "String")
        {
            name = name.ToLower().Trim();
            return _userConnections.Where(u => u.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;            
        }

        public void Send(string name, string contact, string message)
        {
            string time = DateTime.Now.ToString();

          
                var contactUser = GetContactByName(contact);
               

           
            Clients.Caller.addNewMessageToPage(name, contact, message, time);
            // save to history            
            ChatData.AddMessageToHistory(contact, message, name);

            
            message = PythonConnection(message);

            Clients.Caller.addNewMessageToPage(contact, name, message, time);
            //}
            ChatData.AddMessageToHistory(name, message,contact);

        }

        public override Task OnConnected()
        {           
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {            
            return base.OnDisconnected();
        }
        public string PythonConnection(string userQuestion )
        {
            PythonProcces(userQuestion);

            string r = "ошибка";
            using(StreamReader read = new StreamReader("C:\\Users\\salim\\Desktop\\kernel\\result.txt"))
            {
                r = read.ReadToEnd();

            }
            File.WriteAllText("C:\\Users\\salim\\Desktop\\kernel\\result.txt", string.Empty);
            return r;

            
        }

        public void PythonProcces(string userQuestion)
        {
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo("CMD.exe", "/C  py C:\\Users\\salim\\Desktop\\kernel\\kernel.py  \"" + userQuestion + "\"");
                p.StartInfo.WorkingDirectory = "C:\\Users\\salim\\Desktop\\kernel";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
            }
        }

        public void Connect(string name, string contact)
        {
            var pair = _userConnections.FirstOrDefault(u => u.Value.Name == name);
            if (pair.Value != null)
            {
                if (!_userConnections.TryRemove(pair.Key, out ChatData.User value))
                    return;
            }

            var newUser = new ChatData.User()
            {
                Name = name,
                ConnectionId = Context.ConnectionId,
            };

            if (!_userConnections.TryAdd(name, newUser))
                return;

            ActivateChat(name, contact);

            // Update contacts list of other clients
            Clients.Others.clearContacts();

            // add All contact, which will broadcast all messsages
            Clients.All.addContact("All", "DEP");
            Clients.Others.addContact(name, "DEP");        // front-end won't add if it is already in list            

            // and update Caller contact list
            foreach (var u in _userConnections.Values)
            {
                if (u.Name != name)
                    Clients.Caller.addContact(u.Name, "DEP");
            }

        }
    }
}