namespace TikTokLiveSharpTestApplication
{
    public partial class MQTT
    {
        public const string OnConnected = "/OnConnected";
        public const string OnDisconnected = "/OnDisconnected";
        public const string OnViewerData = "/OnViewerData";
        public const string OnLiveEnded = "/OnLiveEnded";
        public const string OnJoin = "/OnJoin";
        public const string OnComment = "/OnComment";
        public const string OnFollow = "/OnFollow";
        public const string OnShare = "/OnShare";
        public const string OnSubscribe = "/OnSubscribe";
        public const string OnLike = "/OnLike";
        public const string OnGiftMessage = "/OnGiftMessage";
        public const string OnEmote = "/OnEmote";

        List<string> topicsList = new List<string> {
            OnConnected,
            OnDisconnected,
            OnViewerData,
            OnLiveEnded,
            OnJoin,
            OnComment,
            OnFollow,
            OnShare,
            OnSubscribe,
            OnLike,
            OnGiftMessage,
            OnEmote
        };
    }
}
