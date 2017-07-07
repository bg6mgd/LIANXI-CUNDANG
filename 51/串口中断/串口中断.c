#include<reg52.h>
 #define uchar unsigned char
 #define uint unsigned int
 #define N 2                                                        //可一次接收数据量
 void rs232_init();
 uchar flag,i;                                                       //删除无用变量                           
 uchar code table[]="I get ";
 uchar table1[N];                                              //接收缓存数组
 uchar j=0;                                                             //接收计数器
 //sbit led=P1^0;
 main()
 {
         rs232_init();
         while(1)
         {
                 if(flag==1)
                 {
                         ES=0;
                         for(i=0;i<6;i++)
                         {
                                 SBUF=table[i];
                                 while(!TI);
                                 TI=0;
                         }
                         for(j=0;j<N;j++)                        //发送接收数组
                                                 {
                                                         SBUF=table1[j];
                                 while(!TI);
                                 TI=0;
                                                }
                         j=0;                                           //清零接收计数器
                         ES=1;
                         flag=0;
                 }                
         }
 }
 void rs232_init()
 {
         TMOD=0x20;
         TH1=0xfd;
         TL1=0xfd;
         TR1=1;
         SM0=0;
         SM1=1;
                 REN=1;                                                        //先设定号工作方式，在打开允许接收
         EA=1;
         ES=1;        
 }
 void ser()interrupt 4
 {                 
                RI=0;
                table1[j++]=SBUF;                                //存数据到接收缓存
                if(j==N)                                                //数组满时，允许发送
                flag=1;
 }