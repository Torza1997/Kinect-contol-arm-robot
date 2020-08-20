#include <Servo.h>
Servo  servoLY, servoLY_2;
Servo servoLZ;
Servo servoRX, servoRY, servoRZ;

int angLY, angLY2, angLZ;
String  ly, lz;
int  Ly, Lz, Ly2;

int angRX, angRY, angRZ;
String rx, ry, rz;
int Rx, Ry, Rz;

void setup() {
  Serial.begin(115200);
  servoRX.attach(13);
  servoRY.attach(11);
  servoRZ.attach(10);


  servoLY.attach(5);
  servoLY_2.attach(9);
  servoLZ.attach(12);



  servoRY.write(0);
  servoRZ.write(0);
  servoRX.write(0);

  servoLY.write(0);
  servoLY_2.write(179);
  servoLZ.write(40);



}
void loop()
{
  if (Serial.available() > 0) {
    char hand = Serial.read();
    switch (hand) {
      case 'L':
        //digitalWrite(Pin[0], HIGH);
        ly = Serial.readStringUntil(',');
        Serial.read();
        lz = Serial.readStringUntil(',');
        Serial.read();
        
        
        Ly = ly.toInt();
        Lz = lz.toInt();

        angLY = map(Ly, 0, 40, 0, 90);
        angLY2 = map(Ly, 0, 40, 180, 90);
        angLZ = map(Lz, 140, 170, 40, 100);
        
        //angRZ = map(Rz, 140, 170, 40, 100);

        

        servoLY.write(angLY);
        servoLY_2.write(angLY2);
        servoLZ.write(angLZ);
        break;
      case 'R':
        //digitalWrite(Pin[1], HIGH);
        rx = Serial.readStringUntil(',');
        Serial.read();
        ry = Serial.readStringUntil(',');
        Serial.read();
        rz = Serial.readStringUntil(',');
        Serial.read();

        //Rx = rx.toInt();
        Ry = ry.toInt();
        Rz = rz.toInt();

        //angLZ = map(Lz, 150, 180, 0, 100);
        angRZ = map(Rz, 150, 180, 0, 100);
        angRY = map(Ry, 30, 0, 0, 90);
        //angRX = map(Rx, 5, 70, 0, 100);

        //servoRX.write(angRZ);
        servoRY.write(angRY);
        servoRZ.write(angRZ);
        break;
    }
  }
}
