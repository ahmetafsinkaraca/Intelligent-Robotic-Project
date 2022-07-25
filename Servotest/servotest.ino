#include <Servo.h>
Servo M1, M2;
String Th1, Th2, tmp;
String readingString;

#define M1_Zero 86 // Hardware Correction
// Range -86 to 86
#define M1_Min -85
#define M1_Max 85

#define M2_Zero 0
// Range 0 to 60
#define M2_Min 0
#define M2_Max 60

void Move_Motors(int th1, int th2)
{
  if(th1<M1_Min) th1=M1_Min;
  if(th1>M1_Max) th1=M1_Max;
  
  if(th2<M2_Min) th2=M2_Min;
  if(th2>M2_Max) th2=M2_Max;
  
  M1.write(M1_Zero+th1);
  M2.write(M2_Zero+th2);
}

String getValue(String data, char separator, int index)
{
  int found = 0;
  int strIndex[] = {0, -1};
  int maxIndex = data.length()-1;

  for(int i=0; i<=maxIndex && found<=index; i++)
  {
    if(data.charAt(i)==separator || i==maxIndex)
    {
      found++;
      strIndex[0] = strIndex[1]+1;
      strIndex[1] = (i == maxIndex) ? i+1 : i;
    }
  }
  return found>index ? data.substring(strIndex[0], strIndex[1]) : "";
}
void setup()
{
  M1.attach(2);
  M2.attach(3);
  Serial.begin(9600);
  pinMode(4, OUTPUT);
  
  M1.write(M1_Zero);
  M2.write(M2_Zero);
  digitalWrite(4,0);
  Th1 = "0";
  Th2 = "0";
}

void loop()
{
    delay(200);
    
    if(Serial.available()>=2)
    {
      readingString = Serial.readStringUntil('\n');
      Th1 = getValue(readingString,',',2);
      Th2 = getValue(readingString,',',3);
    
      while(Serial.available()) tmp = Serial.read();    
    
      if(readingString.startsWith("1,",0))
      {
        digitalWrite(4,1);
        Move_Motors(Th1.toInt(), Th2.toInt());
      }
      else if(readingString.startsWith("0,",0))
      {
        digitalWrite(4,0);
        Move_Motors(M1_Zero, M2_Zero);
      }
   }
}
