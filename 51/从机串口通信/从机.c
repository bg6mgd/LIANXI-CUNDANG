#include<reg52.h> //包含头文件，一般情况不需要改动，头文件包含特殊功能寄存器的定义   
#define uchar unsigned char//宏定义无符号字符型
#define uint unsigned int  //宏定义无符号整型
/********************************************************************
                            初始定义
*********************************************************************/

unsigned char dat; //用于存储单片机接收发送缓冲寄存器SBUF里面的内容
int i;


/*定义八位出为单片机P2口*/
sbit K1=P2^0;
sbit K2=P2^1;
sbit K3=P2^2;
sbit K4=P2^3;
sbit K5=P2^4;
sbit K6=P2^5;
sbit K7=P2^6;
sbit K8=P2^7;

/*定义八位入为单片机P1口*/
sbit SB1=P1^0;
sbit SB2=P1^1;
sbit SB3=P1^2;
sbit SB4=P1^3;
sbit SB5=P1^4;
sbit SB6=P1^5;
sbit SB7=P1^6;
sbit SB8=P1^7;

void delay(int t)
{
   while(t--);
}

                     

/*------------------------------------------------
                   函数声明
------------------------------------------------*/
void SendStr(unsigned char *s);
void SendByte(unsigned char da);

/*------------------------------------------------
                    串口初始化
------------------------------------------------*/
void InitUART  (void)
{
	PCON = 0x00;
    SCON  = 0x50;		        // SCON: 模式 1, 8-bit UART, 使能接收  
    TMOD |= 0x20;               // TMOD: timer 1, mode 2, 8-bit 重装
    TH1   = 0xFD;               // TH1:  重装值 9600 波特率 晶振 11.0592MHz 
	TL1 = 0xFd; 
    TR1   = 1;                  // TR1:  timer 1 打开                         
    EA    = 1;                  //打开总中断
    ES    = 1;                  //打开串口中断
}   


                      
/*------------------------------------------------
                    主函数
------------------------------------------------*/
//int m;
//int flag=0;
//unsigned char ss;
int judge[8];
unsigned char ZTS[8]={0xff,0xfe,0x00,0x00,0x00,0x00,0xfd,0xfd};
int cnt=0;
int ZT;
void main (void)
{
  	ZT=P1;
   //res=0;
   InitUART();

   ES= 1;//打开串口中断
   
  while (1)                       
    {	
	if(ZT!=P1)
	    {
		ZTS[2]=P1;
		SendStr(ZTS);
		ZT=P1;
		}
      
       if(judge[0]==0xFF&&judge[1]==0xFE&&judge[6]==0xFD&&judge[7]==0xFC)
	   { 

	    dat=judge[2];
		switch(dat)
		{


uchar k;
k=10;
case 0xff: P2=0X00;delay(k);dat=0xee;break; //  全开
case 0x00: P2=0XFF;delay(k);dat=0xee;break; //  全关

case 0x01: K1=0;delay(k);dat=0xee;break;	   //  第一路开
case 0x02: K2=0;delay(k);dat=0xee;break;	   //  第二路开
case 0x03: K3=0;delay(k);dat=0xee;break;	   //  第三路开
case 0x04: K4=0;delay(k);dat=0xee;break;	   //  第四路开
case 0x05: K5=0;delay(k);dat=0xee;break;	   //  第五路开
case 0x06: K6=0;delay(k);dat=0xee;break;	   //  第六路开
case 0x07: K7=0;delay(k);dat=0xee;break;	   //  第七路开
case 0x08: K8=0;delay(k);dat=0xee;break;	   //  第八路开

case 0xFE: K1=1;delay(k);dat=0xee;break;	   //  第一路关
case 0xFD: K2=1;delay(k);dat=0xee;break;	   //  第二路关
case 0xFC: K3=1;delay(k);dat=0xee;break;	   //  第三路关
case 0xFB: K4=1;delay(k);dat=0xee;break;	   //  第四路关
case 0xFA: K5=1;delay(k);dat=0xee;break;	   //  第五路关
case 0xF9: K6=1;delay(k);dat=0xee;break;	   //  第六路关
case 0xF8: K7=1;delay(k);dat=0xee;break;	   //  第七路关
case 0xF7: K8=1;delay(k);dat=0xee;break;	   //  第八路关

case 0x55: ZTS[2]=P1;delay(k);SendStr(ZTS);dat=0xee;break;	   //  读输入口状态
case 0xAA: ZTS[2]=P2;delay(k);SendStr(ZTS);dat=0xee;break;	   //  读输出口状态

case 0x11: P2=~P2;delay(k);dat=0xee;break;  //  输出口取反

default:break;					   //  跳出
		}
	  }

    }
}



/*------------------------------------------------
                    发送一个字节
------------------------------------------------*/
void SendByte(unsigned char da)
	{
 	SBUF = da;
	while(!TI);
      	TI = 0;
	}
/*------------------------------------------------
                    发送一个字符串
------------------------------------------------*/
  void SendStr(unsigned char *s)
{
 while(*s!='\0')// \0 表示字符串结束标志，通过检测是否字符串末尾
  {
  SendByte(*s);
  delay(50);
  s++;
  }
}




/*------------------------------------------------
                     串口中断程序
------------------------------------------------*/
void UART_SER (void) interrupt 4 //串行中断服务程序
{
    unsigned char Temp;          //定义临时变量 
   
   if(RI)                        //判断是接收中断产生
     {
	  RI=0;                      //标志位清零
	  Temp=SBUF;                 //读入缓冲区的值
	                     
	  judge[cnt++]=Temp; 

      //报头不满足或者一个协议帧发送完毕
      if(cnt==8||judge[0]!=0xFF)	  
      cnt=0;
      SBUF=Temp;     
	  //TI=1;
	 }
   if(TI) 
	TI=0;
} 