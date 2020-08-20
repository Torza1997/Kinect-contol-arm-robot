#include <AccelStepper.h>
#include <MultiStepper.h>

#define XSTEP 3 //Stepper Motor Step pin
#define XDIR 2 // Stepper motor Direction control pin

#define XMOTORACC 250 // Acceleration and Max Speed values
#define XMOTORMAXSPEED 1000

AccelStepper XMOTOR(1, XSTEP, XDIR);

int angLX;
String lx;
int Lx;

void setup() {
  Serial.begin(9600);
  autohome();
  pinsetup();
}

void loop() {
  int initial_xhome1 = 1;
  if (Serial.available() > 0) {
    char hand = Serial.read();
    switch (hand) {
      case 'S':
        lx = Serial.readStringUntil(',');
        Serial.read();
        Lx = lx.toInt();
       
        if(Lx > -65  and Lx < -10){
            angLX = map(Lx, 10,65 , 0, 300);
            XMOTOR.runToNewPosition(angLX);
          }
        break;
    }
  }
}
void autohome() { //We're using this to call our homing routine
  xyhome();
}

void xyhome() {
  XMOTOR.setCurrentPosition(0);  // Set the current position as zero for now
  XMOTOR.setMaxSpeed(20);      // Set Max Speed of Stepper (Slower to get better accuracy)
  XMOTOR.setAcceleration(10.0);  // Set Acceleration of Stepper
  int initial_xhome = 1;
  while (map(analogRead(A0), 0, 1023, 0, 200) == 2) {
    XMOTOR.moveTo(initial_xhome);  // Set the position to move to

    initial_xhome--;  // Decrease by 1 for next move if needed
    XMOTOR.run();  // Start moving the stepper
    delay(5);
  }
  while (map(analogRead(A0), 0, 1023, 0, 200) != 2) { // Make the Stepper move CW until the switch is deactivated

    XMOTOR.moveTo(initial_xhome);
    XMOTOR.run();
    initial_xhome++;
    delay(5);
  }
  XMOTOR.setCurrentPosition(0);
  XMOTOR.setMaxSpeed(XMOTORMAXSPEED);      // Set Max Speed of Stepper (Faster for regular movements)
  XMOTOR.setAcceleration(XMOTORACC);  // Set Acceleration of Stepper
}
void pinsetup() {
  ;
  XMOTOR.setCurrentPosition(0);
  XMOTOR.setMaxSpeed(XMOTORMAXSPEED);      // Set Max Speed of Stepper (Faster for regular movements)
  XMOTOR.setAcceleration(XMOTORACC);  // Set Acceleration of Stepper
}
