using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Internal;
using System.ComponentModel;
using System.Text;


namespace LivestreamClient
{
    public partial class MQTT
    {
        //MQTT settings
        public const string mqtt_user = "";
        public const string mqtt_password = "";
        public const string mqtt_server = "";
        public const int mqtt_port = 1883; //Default port
        IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();
        //public event PropertyChangedEventHandler PropertyChanged;

        public void StartMqttSubscriber()
        {
            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(
                        new MqttClientOptionsBuilder()
                        .WithClientId($"Display-{Guid.NewGuid()}")
                        .WithTcpServer(mqtt_server, mqtt_port)
                        .WithCredentials(mqtt_user, mqtt_password)
                        .WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
                        .Build()).Build();

            _mqttClient.ConnectedAsync += _mqttClient_ConnectedAsync;
            _mqttClient.DisconnectedAsync += _mqttClient_DisconnectedAsync;
            _mqttClient.ConnectingFailedAsync += _mqttClient_ConnectingFailedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
            _mqttClient.StartAsync(options);

            foreach(string topic in topicsList)
            {
                _mqttClient.SubscribeAsync(topic);
            }
        }

        Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("Connected");
            return Task.CompletedTask;
        }
        Task _mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            Console.WriteLine("Disconnected");
            return Task.CompletedTask;
        }
        Task _mqttClient_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            Console.WriteLine("Connection failed check network or broker!");
            return Task.CompletedTask;
        }

        public async Task SendCommand(string messagePaylaod, string topic)
        {

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT.mqtt_server, MQTT.mqtt_port)
                    .WithCredentials(MQTT.mqtt_user, MQTT.mqtt_password)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                                .WithTopic(topic)
                                .WithPayload(messagePaylaod)
                                .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                //Console.WriteLine("MQTT application message is published.");
            }
        }

        Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {

            string payload = Encoding.ASCII.GetString(e.ApplicationMessage.PayloadSegment);

            try
            {
                switch (e.ApplicationMessage.Topic)
                {

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }

            return CompletedTask.Instance;
        }
    }
}
