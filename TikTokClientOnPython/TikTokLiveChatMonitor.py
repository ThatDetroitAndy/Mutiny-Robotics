import random
import time
import asyncio

from paho.mqtt import client as mqtt_client
from TikTokLive.client.client import TikTokLiveClient
from TikTokLive.events import ConnectEvent, CommentEvent

tiktoker = "@"

broker = ""
port = 1883

# Generate a Client ID with the publish prefix.
client_id = f'publish-{random.randint(0, 1000)}'
username = ""
password = ""

action_commands = [ 
    "left", "lef", "lefft", "lft", "port", "sinister", "larboard", "leftward", "leftwards", "leftmost", 
    "right", "rite", "riht", "rgt", "starboard", "dexter", "correct", "rightward", "rightwards", "rightmost", 
    "backwards", "bkwds", "bckwrds", "bkwrds", "bwd", "bw", "retrograde", "reverse", "rearward", "rearwards", "aft", "sternwards", "sternward", "sternmost", 
    "forward", "frwd", "fwd", "forwd", "fw", "ahead", "onward", "forth", "forwards", "frontward", "frontwards", "foremost",
    "claw", "hand", "grab" 
]

action_command_map = {
    "left": "l",
    "port": "l",           
    "sinister": "l",          
    "larboard": "l",
    "leftwards": "l",
    "leftward": "l",
    "leftmost": "l",
    "lef": "l",
    "lefft": "l",
    "lft": "l",
    "right": "r",
    "riht": "r",
    "rgt": "r",
    "rite": "r",
    "starboard": "r",
    "dexter": "r",
    "correct": "r",
    "rightward": "r",
    "rightwards": "r",
    "rightmost": "r",
    "backwards": "b",        
    "sternwards": "b",
    "sternward": "b",
    "sternmost": "b",
    "rearward": "b",
    "rearwards": "b",
    "retrograde": "b",
    "reverse": "b",
    "aft": "b",
    "bkwrds": "b",
    "bckwrds": "b",
    "bkwds": "b",
    "bwd": "b",
    "bw": "b",
    "forward": "f",
    "frontward": "f",
    "frontwards": "f",
    "foremost": "f",
    "forwards": "f",
    "forth": "f",
    "onward": "f",
    "ahead": "f",
    "fw": "f",
    "forwd": "f",
    "fwd": "f",
    "frwd": "f",
    "claw": "z",
    "hand": "z",
    "grab": "z"

}

sound_commands = [ "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" ]

sound_command_map = {
    "0": "0",
    "1": "1",
    "2": "2",
    "3": "3",
    "4": "4",
    "5": "5",
    "6": "6",
    "7": "7",
    "8": "8",
    "9": "9"
}

def is_it_an_action_command(message):
    # search in the message for a command
    for command in action_commands:
        if command in message:
            print("Command found: " + command)
            upload(action_command_map.get(command), "action")

def is_it_a_sound_command(message):
    # search in the message for a command
    for command in sound_commands:
        if command in message:
            print("Command found: " + command)
            upload(sound_command_map.get(command), "sound")
            


TikTokClient: TikTokLiveClient = TikTokLiveClient(
    unique_id=tiktoker
)
 
 
@TikTokClient.on(ConnectEvent)
async def on_connect(event: ConnectEvent):
    print(f"Connected to @{event.unique_id}!")
 
 
@TikTokClient.on(CommentEvent)
async def on_comment(event: CommentEvent):
    print(f"Received a comment: {event.comment}")
    is_it_an_action_command(event.comment.lower())
    #is_it_a_sound_command(event.comment.lower())
    

def connect_mqtt():
    print("connect_mqtt")
    def on_connect(client, userdata, flags, rc):
        if rc == 0:
            print("Connected to MQTT Broker!")
        else:
            print(":(")
            print("Failed to connect, return code %d\n", rc)

    client = mqtt_client.Client(mqtt_client.CallbackAPIVersion.VERSION1, client_id)
    client.username_pw_set(username, password)
    client.on_connect = on_connect
    client.connect(broker, port)
    return client


def publish(client, message, topic):
    
    print("publish")
    result = client.publish(topic, message)
    # result: [0, 1]
    status = result[0]
    if status == 0:
        print(f"Send `{message}` to topic `{topic}`")
    else:
        print(f"Failed to send message to topic {topic}")

    result = client.publish("TikTok", "Live")
    # result: [0, 1]
    status = result[0]
    if status == 0:
        print(f"Alive!")
    else:
        print(f"Dead")


def upload(message, topic):
    print(f"Connecting to MQTT Broker {broker}")
    client = connect_mqtt()
    print(f"Starting loop")
    client.loop_start()
    print(f"Publishing to topic `{topic}`")
    publish(client, message, topic)
    client.loop_stop()


if __name__ == '__main__':
    TikTokClient.run(process_connect_events=False)




