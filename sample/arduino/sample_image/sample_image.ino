#include <M5CoreInk.h>
#include <esp_adc_cal.h>
#include "sample_image.h"

Ink_Sprite InkPageSprite(&M5.M5Ink);
unsigned long _lastUpdate = 0;
int _pos = 0;

// see sample_image.h
uint8_t* _images[3] = {(uint8_t*)image_1,(uint8_t*)image_2,(uint8_t*)image_3};

void setup() {
    
    M5.begin();
    if( !M5.M5Ink.isInit())
    {
        Serial.printf("Ink Init faild");
        while (1) delay(100);   
    }
    //creat ink refresh Sprite
    updateImage(millis(),true);
}

void loop() {
  unsigned long now = millis();
  if( M5.BtnUP.wasPressed()){
    _pos = _pos - 1;
    updateImage(now,false);
  }
  if( M5.BtnDOWN.wasPressed()){
    _pos = _pos + 1;
    updateImage(now,false);
  }
  if( M5.BtnMID.wasPressed()){
    showBattery();
    updateImage(now,true);
  }

  if(now - _lastUpdate > 15 * 1000){ // Wait 15 sec. then sleep.
    M5.shutdown();
  }
  delay(100);
  M5.update();
}

void updateImage(long now, bool forceUpdate){
    if(_pos < 0){
      _pos = 0;
      if(!forceUpdate){
        return;
      }
    }
    if(_pos >= sizeof(_images)/sizeof(int)){
      _pos = sizeof(_images)/sizeof(int)-1;
      if(!forceUpdate){
        return;
      }
    }

    M5.M5Ink.clear();
    if( InkPageSprite.creatSprite(0,0,200,200,true) != 0 )
    {
        Serial.printf("Ink Sprite creat faild");
    }
    InkPageSprite.drawBuff(0,0,200,200,_images[_pos]);
    InkPageSprite.pushSprite();
    _lastUpdate = now;
}

void showBattery(){
    char text[64];
    M5.M5Ink.clear();
    if( InkPageSprite.creatSprite(0,0,200,200,true) != 0 )
    {
        Serial.printf("Ink Sprite creat faild");
    }

    int battery = getBatCapacity();
    sprintf(text,"Battery: %d %%",battery);

    InkPageSprite.drawString(5,70,text,&AsciiFont8x16);

    int cx = 100;
    int cy = 100;
    int w = 190;
    int h = 20;
    int b = w * (100-battery) / 100;
    
    InkPageSprite.FillRect(cx - w/2 - 1,  cy - h /2 - 1,  w + 2, h + 2  , 0);
    if(battery < 100){
      InkPageSprite.FillRect(cx + w/2 - b ,cy - h / 2, b, h, 1);
    }
    InkPageSprite.pushSprite();
    delay(3000);
}
int getBatCapacity(){
    // Simple implementation
    // see https://www.maximintegrated.com/jp/design/technical-documents/app-notes/3/3958.html
    // 4.02 = 100%, 3.80 = 0%
    const float maxVoltage = 4.02;
    const float minVoltage = 3.80;
    int cap = (int)(100.0 * (getBatVoltage() - minVoltage) / (maxVoltage - minVoltage));
    if(cap > 100){
      cap = 100;
    }
    if(cap < 0){
      cap = 0;
    }
    return cap;
}
float getBatVoltage()
{
    analogSetPinAttenuation(35,ADC_11db);
    esp_adc_cal_characteristics_t *adc_chars = (esp_adc_cal_characteristics_t *)calloc(1, sizeof(esp_adc_cal_characteristics_t));
    esp_adc_cal_characterize(ADC_UNIT_1, ADC_ATTEN_DB_11, ADC_WIDTH_BIT_12, 3600, adc_chars);
    uint16_t ADCValue = analogRead(35);
    
    uint32_t BatVolmV  = esp_adc_cal_raw_to_voltage(ADCValue,adc_chars);
    float BatVol = float(BatVolmV) * 25.1 / 5.1 / 1000;
    return BatVol;
}
