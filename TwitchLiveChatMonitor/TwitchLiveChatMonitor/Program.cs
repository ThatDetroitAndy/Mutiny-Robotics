using System;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace LivestreamClient
{
    class Program
    {
        public static string UserName = "";
        public static string AccessToken = "";
        public static string Channel = "";
        static void Main(string[] args)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(UserName, AccessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            TwitchClient client = new TwitchClient(customClient);
            client.Initialize(credentials, Channel);

            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();

            Console.ReadLine();
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string message = e.ChatMessage.Message;
            Console.WriteLine($"Message received from {e.ChatMessage.Username}: {message}");
            Console.WriteLine("Jumping into action");
            isItAnActionCommand(message.ToLower());
            Console.WriteLine("Jumping into sound");
            isItASoundCommand(message.ToLower());
            Console.WriteLine("here");
        }


        static string actionTopic = "action";
        static string soundTopic = "sound";


        public static List<string> actionCommands = new List<string> {
            "left", "lef", "lefft", "lft", "port", "sinister", "larboard", "leftward", "leftwards", "leftmost",
            "right", "rite", "riht", "rgt", "starboard", "dexter", "correct", "rightward", "rightwards", "rightmost",
            "backwards", "bkwds", "bckwrds", "bkwrds", "bwd", "bw", "retrograde", "reverse", "rearward", "rearwards", "aft", "sternwards", "sternward", "sternmost",
            "forward", "frwd", "fwd", "forwd", "fw", "ahead", "onward", "forth", "forwards", "frontward", "frontwards", "foremost", "claw"};

        public static Dictionary<string, string> actionCommandMap = new Dictionary<string, string>
        {
            { "left", "l" },
            { "port", "l" },
            { "sinister", "l" },
            { "larboard", "l" },
            { "leftwards", "l" },
            { "leftward", "l" },
            { "leftmost", "l" },
            { "lef", "l" },
            { "lefft", "l" },
            { "lft", "l" },
            { "right", "r" },
            { "riht", "r" },
            { "rgt", "r" },
            { "rite", "r" },
            { "starboard", "r" },
            { "dexter", "r" },
            { "correct", "r" },
            { "rightward", "r" },
            { "rightwards", "r" },
            { "rightmost", "r" },
            { "backwards", "b" },
            { "sternwards", "b" },
            { "sternward", "b" },
            { "sternmost", "b" },
            { "rearward", "b" },
            { "rearwards", "b" },
            { "retrograde", "b" },
            { "reverse", "b" },
            { "aft", "b" },
            { "bkwrds", "b" },
            { "bckwrds", "b" },
            { "bkwds", "b" },
            { "bwd", "b" },
            { "bw", "b" },
            { "forward", "f" },
            { "frontward", "f" },
            { "frontwards", "f" },
            { "foremost", "f" },
            { "forwards", "f" },
            { "forth", "f" },
            { "onward", "f" },
            { "ahead", "f" },
            { "fw", "f" },
            { "forwd", "f" },
            { "fwd", "f" },
            { "frwd", "f" },
            { "claw", "z" },
            { "hand", "z" }
        };

        public static List<string> soundCommands = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static Dictionary<string, string> soundCommandMap = new Dictionary<string, string>
        {
            { "0", "0" },
            { "1", "1" },
            { "2", "2" },
            { "3", "3" },
            { "4", "4" },
            { "5", "5" },
            { "6", "6" },
            { "7", "7" },
            { "8", "8" },
            { "9", "9" }
        };

        public static void isItAnActionCommand(string message)
        {
            try
            {
                //Search in the message for a _actionCommand
                foreach (string _actionCommand in actionCommands)
                {
                    if (message.Contains(_actionCommand))
                    {
                        Console.WriteLine("Command found: " + _actionCommand);

                        //Send the _actionCommand to the robot
                        var instance = new MQTT();
                        _ = instance.SendCommand(actionCommandMap[_actionCommand], actionTopic);
                        _ = instance.SendCommand("Live", "Twitch");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }


        }

        public static void isItASoundCommand(string message)
        {
            try
            {
                //search in the message for a _soundCommand
                foreach (string _soundCommand in soundCommands)
                {
                    if (message.Contains(_soundCommand))
                    {
                        Console.WriteLine("Command found: " + _soundCommand);

                        //send the _soundCommand to the robot
                        var instance = new MQTT();
                        _ = instance.SendCommand(soundCommandMap[_soundCommand], soundTopic);
                        _ = instance.SendCommand("Live", "Twitch");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }


    }
}