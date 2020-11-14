#include "M5CoreInk.h"
#include "sample_image.h"

Ink_Sprite InkPageSprite(&M5.M5Ink);

void setup() {
    uint8_t* images[3] = {(uint8_t*)image_1,(uint8_t*)image_2,(uint8_t*)image_3};
    
    M5.begin();
    if( !M5.M5Ink.isInit())
    {
        Serial.printf("Ink Init faild");
        while (1) delay(100);   
    }
    M5.M5Ink.clear();
    delay(1000);
    //creat ink refresh Sprite
    if( InkPageSprite.creatSprite(0,0,200,200,true) != 0 )
    {
        Serial.printf("Ink Sprite creat faild");
    }
    InkPageSprite.drawBuff(0,0,200,200,images[0]);
    InkPageSprite.pushSprite();
}

void loop() {

}
