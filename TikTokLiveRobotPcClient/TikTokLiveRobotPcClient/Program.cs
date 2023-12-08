using System.Collections.Concurrent;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events.MessageData.Messages;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TikTokLiveSharpTestApplication
{
    internal class Program
    {

        //If something bad happens you can add admins and super admins to take over the robot
        static string[] admins =
        {
            "tiktokusername",
            "thatdetroitandy"
        };

        static string[] superAdmins =
        {
            "tiktokusername",
            "thatdetroitandy"
        };

        static string hold = "h";
        static string go = "g";
        static string forward = "f";
        static string backward = "b";
        static string left = "l";
        static string right = "r";
        static string superForward = "w";
        static string superRight = "d";
        static string superLeft = "a";
        static string superBackward = "s";
        static string aux = "x";
        static string shoot = "z";
        static string turnaround = "t";
        
        static string actionTopic = "action";
        static string soundTopic = "sound";
        static string adminTopic = "admin";

        static void Main(string[] args)
        {
            Console.WriteLine("Enter a username:");
            var client = new TikTokLiveClient(Console.ReadLine());

            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnViewerData += Client_OnViewerData;
            client.OnLiveEnded += Client_OnLiveEnded;
            client.OnJoin += Client_OnJoin;
            client.OnComment += Client_OnComment;
            client.OnFollow += Client_OnFollow;
            client.OnShare += Client_OnShare;
            client.OnSubscribe += Client_OnSubscribe;
            client.OnLike += Client_OnLike;
            client.OnGiftMessage += Client_OnGiftMessage;
            client.OnEmote += Client_OnEmote;
            client.Run(new System.Threading.CancellationToken());
        }

        private static void Client_OnConnected(TikTokLiveClient sender, bool e)
        {
            TikTokStatus(false, ConsoleColor.White, $"Connected to Room! [Connected:{e}]");
        }

        private static void Client_OnDisconnected(TikTokLiveClient sender, bool e)
        {
            TikTokStatus(false, ConsoleColor.White, $"Disconnected from Room! [Connected:{e}]");
        }

        private static void Client_OnViewerData(TikTokLiveClient sender, RoomViewerData e)
        {
            TikTokStatus(false, ConsoleColor.Cyan, $"Viewer count is: {e.ViewerCount}");

        }

        private static void Client_OnLiveEnded(TikTokLiveClient sender, EventArgs e)
        {
            TikTokStatus(false, ConsoleColor.White, "Host ended Stream!");
        }

        private static void Client_OnJoin(TikTokLiveClient sender, Join e)
        {
            TikTokStatus(false, ConsoleColor.Green, $"{e.User.UniqueId} joined!");

        }

        private static void Client_OnComment(TikTokLiveClient sender, Comment e)
        {
            if (superAdmins.Contains(e.User.UniqueId))
            {
                string comment = e.Text.ToLowerInvariant();
                Dictionary<string, string> superCommands = SuperCommandDictionary();
                if (superCommands.ContainsKey(comment))
                {

                    var instance = new MQTT();   
                    instance.SendCommand(superCommands[comment], adminTopic);
                    Console.WriteLine($" SuperAdmin send admin command: {superCommands[comment]}");

                }
            }
            else if (admins.Contains(e.User.UniqueId))
            {
                string comment = e.Text.ToLowerInvariant();
                Dictionary<string, string> commands = CommandDictionary();
                if (commands.ContainsKey(comment))
                {
                    var instance = new MQTT();
                    instance.SendCommand(commands[comment], adminTopic);
                    Console.WriteLine($" Admin send admin command: {commands[comment]}");
                }
            }
            else
            {
                TikTokStatus(false, ConsoleColor.Yellow, $"{e.User.UniqueId}: {e.Text}");
            }
        }

        private static void Client_OnFollow(TikTokLiveClient sender, Follow e)
        {
            TikTokStatus(false, ConsoleColor.DarkRed, $"{e.NewFollower?.UniqueId} followed!");

        }

        private static void Client_OnShare(TikTokLiveClient sender, Share e)
        {
            TikTokStatus(false, ConsoleColor.Blue, $"{e.User?.UniqueId} shared!");

        }

        private static void Client_OnSubscribe(TikTokLiveClient sender, Subscribe e)
        {
            TikTokStatus(false, ConsoleColor.DarkCyan, $"{e.NewSubscriber.UniqueId} subscribed!");

        }

        private static void Client_OnLike(TikTokLiveClient sender, Like e)
        {
            TikTokStatus(false, ConsoleColor.Red, $"{e.Sender.UniqueId} liked!");

        }

        private static void Client_OnGiftMessage(TikTokLiveClient sender, GiftMessage e)
        {

            TikTokStatus(true, ConsoleColor.Magenta, $"{e.Sender.UniqueId} sent {e.Amount}x {e.Gift.Name}! ID: {e.Gift.Id}");
            TikTokGift(e);

        }

        private static void Client_OnEmote(TikTokLiveClient sender, Emote e)
        {
            TikTokStatus(false, ConsoleColor.DarkGreen, $"{e.User.UniqueId} sent {e.EmoteId}!");

        }

        private static void TikTokStatus(bool show, ConsoleColor color, string status)
        {
            if (show)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(status);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void TikTokGift(GiftMessage giftItem)
        {
            uint cost = giftItem.Gift.DiamondCost;
            ulong id = giftItem.Gift.Id;
            string giftName = giftItem.Gift.Name.ToLower();
            bool send = false;
            string command = "";
            string topic = "";

            Dictionary<string, string> actionsByName = GiftNameActionDictionary();
            Dictionary<ulong, string> actionsByID = GiftIDActionDictionary();
            Dictionary<string, string> soundsByName = GiftNameSoundsDictionary();

            if (actionsByID.ContainsKey(id))
            {
                command = actionsByID[id];
                send = true;
                topic = actionTopic;
                Console.WriteLine($" Aaction command: {command}");
            }
            else if (soundsByName.ContainsKey(giftName))
            {
                command = soundsByName[giftName];
                send = true;
                topic = soundTopic;
                Console.WriteLine($" Aaction command: {command}");
            }
            else if (actionsByName.ContainsKey(giftName))
            {
                command = actionsByName[giftName];
                send = true;
                topic = actionTopic;
                Console.WriteLine($" Aaction command: {command}");
            }
            

            if (send)
            {
                var instance = new MQTT();
                instance.SendCommand(command, topic);     
            }

            send = false;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Got gift: name {giftName} id {id}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        enum Interaction
        {
            Like,
            Gift,
            Emote,
            Other
        }

        public static Dictionary<string, string> GiftNameActionDictionary()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>();
            commands.Add("rose", forward);
            commands.Add("weights", forward);
            commands.Add("ice cream cone", backward);
            commands.Add("chili", left);
            commands.Add("lightning bolt", right);
            commands.Add("doughnut", superForward);
            commands.Add("mirror", superRight);
            commands.Add("perfume", superLeft);
            commands.Add("tiny diny", superBackward);
            commands.Add("hat and mustache", turnaround);

            return commands;
        }

        public static Dictionary<ulong, string> GiftIDActionDictionary()
        {
            Dictionary<ulong, string> commands = new Dictionary<ulong, string>();
            commands.Add(5655, forward);    // rose
            commands.Add(5760, forward);    // weights
            commands.Add(5827, backward);   // ice cream cone
            commands.Add(7086, left);       // chili
            commands.Add(6652, right);      // lightning bolt
            commands.Add(5879, superForward);
            commands.Add(6070, superRight);
            commands.Add(5658, superLeft);
            commands.Add(6560, superBackward);
            return commands;
        }

        public static Dictionary<string, string> CommandDictionary()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>();
            commands.Add("hold", hold);
            commands.Add("forward", forward);
            commands.Add("backward", backward);
            commands.Add("left", left);
            commands.Add("right", right);
            commands.Add("aux", aux);
            commands.Add("shoot", shoot);
            commands.Add("turnaround", turnaround);
            return commands;
        }

        public static Dictionary<string, string> SuperCommandDictionary()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>();
            commands.Add("hold", hold);
            commands.Add("go", go);
            commands.Add("forward", forward);
            commands.Add("backward", backward);
            commands.Add("left", left);
            commands.Add("right", right);
            commands.Add("sforward", superForward);
            commands.Add("sright", superRight);
            commands.Add("sleft", superLeft);
            commands.Add("sbackwards", superBackward);
            commands.Add("aux", aux);
            commands.Add("shoot", shoot);
            commands.Add("turnaround", turnaround);
            return commands;
        }

        public static Dictionary<string, string> GiftNameSoundsDictionary()
        {
            // Create a dictionary with string key and char value pair();
            Dictionary<string, string> commands = new Dictionary<string, string>();
            commands.Add("gamepad", "0");
            commands.Add("golden trumpet", "1");
            commands.Add("chicken leg", "2");
            commands.Add("confetti", "3");
            commands.Add("sound4", "4");
            commands.Add("sound5", "5");
            commands.Add("sound6", "6");
            commands.Add("chill", "7");
            commands.Add("lollipop", "8");
            commands.Add("hearts", "9");
            return commands;
        }
    }
}