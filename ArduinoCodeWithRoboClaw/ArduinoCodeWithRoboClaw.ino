#include <ESP32Servo.h>
#include <PubSubClient.h>
#include <WiFi.h>
#include "RoboClaw.h"

//Pin Settings
#define EStopPin 21  //if the Estop is HIGH then the robot won't move
#define IO1Pin 4     // Relay 1
#define IO2Pin 18    // Relay 2
#define IO3Pin 19    // Relay 3
#define IO4Pin 5     // Relay 4

#define AccessoryServo1 33  // Servo for doing shenanigans
#define AccessoryServo2 15  // Servo for doing more shenanigans

#define minUs 500
#define maxUs 2500
#define ShortActionDelay 200
#define ActionDelay 250
#define LongActionDelay 300
#define InertiaDelay 50
#define StopDelay 551

#define StraightSpeed 32
#define StraightSuperSpeed 38
#define TurnSpeed 42

Servo accessoryServo1;
Servo accessoryServo2;

// Wifi Settings
const char* ssid = "the wifi name";  // Remember that both the name and password need to be inside ""
const char* password = "the wifi password";
WiFiClient espClient;
PubSubClient client(espClient);
long lastMsg = 0;
char msg[50];
int value = 0;


// MQTT Settings
const char* mqtt_user = "User";
const char* mqtt_password = "PasswordPleaseChancgeThis";
const char* mqtt_server = "256.256.256.256";
#define mqtt_port 1883  // This is the defaault

const char* ActionTag = "action";
const char* AdminTag = "admin";
const char* SoundTag = "sound";

// Timers
volatile unsigned long stopTimer;
volatile unsigned long reportStatusTimer;


// Global Variables
const bool RelayOff = false;
const bool RelayOn = !RelayOff;
volatile bool estop = false;
bool hold = true;
bool movementEnabled = true;
bool markerEnabled = false;
bool auxEnabled = false;
const int servo1NeutralPos = 135;
const int servo2NeutralPos = 180;
int servo1ActionPos = 0;
int servo2ActionPos = 0;


//Roboclaw
RoboClaw roboclaw(&Serial1, 10000);
#define address 0x80

void setup() {

  Serial.begin(115200);  // initialize serial:
  Serial.println("Initializing...");
  pinMode(EStopPin, INPUT_PULLUP);
  //attachInterrupt(digitalPinToInterrupt(EStopPin), EStop, FALLING);

  // Starting Servos
  ESP32PWM::allocateTimer(0);
  ESP32PWM::allocateTimer(1);
  // ESP32PWM::allocateTimer(2);
  // ESP32PWM::allocateTimer(3);
  // Standard 50hz servo
  accessoryServo1.setPeriodHertz(50);
  accessoryServo2.setPeriodHertz(50);
  // move servos to start position
  accessoryServo1.attach(AccessoryServo1, minUs, maxUs);
  accessoryServo2.attach(AccessoryServo2, minUs, maxUs);

  // Starting Relays
  pinMode(IO1Pin, OUTPUT);
  digitalWrite(IO1Pin, RelayOff);
  pinMode(IO2Pin, OUTPUT);
  digitalWrite(IO2Pin, RelayOff);
  pinMode(IO3Pin, OUTPUT);
  digitalWrite(IO3Pin, RelayOff);
  pinMode(IO4Pin, OUTPUT);
  digitalWrite(IO4Pin, RelayOff);

  // Starting MQTT library
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);

  // Starting Roboclaw
  roboclaw.begin(38400);
}


void loop() {


  if (!client.connected()) {
    while (!client.connected()) {
      Hold();
      setup_wifi();
      reconnect();
    }
    hold = false;
  }

  estop = digitalRead(EStopPin);
  if (estop) {
    Hold();
    Serial.println("EStop");
    delay(1000);
  } else {
    client.loop();
    if (millis() - stopTimer > StopDelay) {
      roboclaw.ForwardM1(address, 0);
      roboclaw.ForwardM2(address, 0);
    }
  }
}


// Here's where the important stuff happens
void callback(String topic, byte* payload, unsigned int length) {
  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("]");
  char value[length];

  for (int i = 0; i < length; i++) {
    value[i] = (int)payload[i];
    Serial.print(value[i]);
  }
  Serial.println("");

  if (topic == AdminTag) {
    Admin(value[0]);
  } else if (topic == ActionTag) {
    if (!hold) {
      Move(value[0]);
    }
  }
}
void setup_wifi() {
  delay(10);
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);

  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}



// This is for connecting to the broker
void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Attempt to connect
    if (client.connect("TikTokRover", mqtt_user, mqtt_password)) {
      Serial.println("connected");
      // Once connected, publish an announcement...      // ... and resubscribe
      client.subscribe(ActionTag);
      client.subscribe(AdminTag);

    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in .5 seconds");
      // Wait 5 seconds before retrying
      delay(500);
    }
  }
}


//*********** Actions *************

void Hold() {
  //Serial.println("Hold");
  hold = true;
  digitalWrite(IO1Pin, RelayOff);
  digitalWrite(IO2Pin, RelayOff);
  digitalWrite(IO3Pin, RelayOff);
  digitalWrite(IO4Pin, RelayOff);
  roboclaw.ForwardM1(address, 0);
  roboclaw.ForwardM2(address, 0);
  accessoryServo1.write(servo1NeutralPos);
  accessoryServo2.write(servo2NeutralPos);
}

void Admin(char admin) {
  switch (admin) {
    case 'h':  // Hold
      Hold();
      break;

    case 'g':  // Go (unhold)
      hold = false;
      break;

    default:
      Move(admin);
      break;
  }
}

void Move(char movement) {
  Serial.print("Move: ");
  Serial.println(movement);
  switch (movement) {

    // Short movements
    case 'f':  // Forward
      roboclaw.ForwardM1(address, StraightSpeed);
      roboclaw.ForwardM2(address, StraightSpeed);
      delay(ShortActionDelay);
      break;

    case 'b':  // Backwards
      roboclaw.BackwardM1(address, StraightSpeed);
      roboclaw.BackwardM2(address, StraightSpeed);
      delay(ShortActionDelay);
      break;

    case 'l':  // Left
      roboclaw.BackwardM1(address, TurnSpeed);
      roboclaw.ForwardM2(address, TurnSpeed);
      delay(ActionDelay);
      break;

    case 'r':  // Right
      roboclaw.ForwardM1(address, TurnSpeed);
      roboclaw.BackwardM2(address, TurnSpeed);
      delay(ActionDelay);
      break;

      // Super movements

    case 'w':  // Forward
      roboclaw.ForwardM1(address, StraightSuperSpeed);
      roboclaw.ForwardM2(address, StraightSuperSpeed);
      delay(LongActionDelay);
      break;

    case 's':  // Backwards
      roboclaw.BackwardM1(address, StraightSuperSpeed);
      roboclaw.BackwardM2(address, StraightSuperSpeed);
      delay(LongActionDelay);
      break;

    case 'a':  // Left
      roboclaw.BackwardM1(address, TurnSpeed);
      roboclaw.ForwardM2(address, TurnSpeed);
      delay(LongActionDelay);
      break;

    case 'd':  // Right
      roboclaw.ForwardM1(address, TurnSpeed);
      roboclaw.BackwardM2(address, TurnSpeed);
      delay(LongActionDelay);
      break;

    case 't':  // Turnaround
      roboclaw.BackwardM1(address, StraightSpeed);
      roboclaw.BackwardM2(address, StraightSpeed);
      delay(2 * ActionDelay);
      roboclaw.BackwardM1(address, TurnSpeed);
      roboclaw.ForwardM2(address, TurnSpeed);
      delay(13 * ActionDelay);
      break;

    case 'x':  // Accessory Servo 1
      accessoryServo1.write(servo1ActionPos);
      delay(ActionDelay);
      accessoryServo1.write(servo1NeutralPos);
      delay(InertiaDelay);
      break;

    case 'z':  // Accessory Servo 1
      if (!hold) {
        accessoryServo2.write(servo2ActionPos);
        delay(ActionDelay);
        accessoryServo2.write(servo2NeutralPos);
        delay(InertiaDelay);
      }

      break;

    default:
      break;
  }
  stopTimer = millis();
}
