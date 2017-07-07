#include<reg52.h> //����ͷ�ļ���һ���������Ҫ�Ķ���ͷ�ļ��������⹦�ܼĴ����Ķ���   
#define uchar unsigned char//�궨���޷����ַ���
#define uint unsigned int  //�궨���޷�������
/********************************************************************
                            ��ʼ����
*********************************************************************/

unsigned char dat; //���ڴ洢��Ƭ�����շ��ͻ���Ĵ���SBUF���������
int i;


/*�����λ��Ϊ��Ƭ��P2��*/
sbit K1=P2^0;
sbit K2=P2^1;
sbit K3=P2^2;
sbit K4=P2^3;
sbit K5=P2^4;
sbit K6=P2^5;
sbit K7=P2^6;
sbit K8=P2^7;

/*�����λ��Ϊ��Ƭ��P1��*/
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
                   ��������
------------------------------------------------*/
void SendStr(unsigned char *s);
void SendByte(unsigned char da);

/*------------------------------------------------
                    ���ڳ�ʼ��
------------------------------------------------*/
void InitUART  (void)
{
	PCON = 0x00;
    SCON  = 0x50;		        // SCON: ģʽ 1, 8-bit UART, ʹ�ܽ���  
    TMOD |= 0x20;               // TMOD: timer 1, mode 2, 8-bit ��װ
    TH1   = 0xFD;               // TH1:  ��װֵ 9600 ������ ���� 11.0592MHz 
	TL1 = 0xFd; 
    TR1   = 1;                  // TR1:  timer 1 ��                         
    EA    = 1;                  //�����ж�
    ES    = 1;                  //�򿪴����ж�
}   


                      
/*------------------------------------------------
                    ������
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

   ES= 1;//�򿪴����ж�
   
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
case 0xff: P2=0X00;delay(k);dat=0xee;break; //  ȫ��
case 0x00: P2=0XFF;delay(k);dat=0xee;break; //  ȫ��

case 0x01: K1=0;delay(k);dat=0xee;break;	   //  ��һ·��
case 0x02: K2=0;delay(k);dat=0xee;break;	   //  �ڶ�·��
case 0x03: K3=0;delay(k);dat=0xee;break;	   //  ����·��
case 0x04: K4=0;delay(k);dat=0xee;break;	   //  ����·��
case 0x05: K5=0;delay(k);dat=0xee;break;	   //  ����·��
case 0x06: K6=0;delay(k);dat=0xee;break;	   //  ����·��
case 0x07: K7=0;delay(k);dat=0xee;break;	   //  ����·��
case 0x08: K8=0;delay(k);dat=0xee;break;	   //  �ڰ�·��

case 0xFE: K1=1;delay(k);dat=0xee;break;	   //  ��һ·��
case 0xFD: K2=1;delay(k);dat=0xee;break;	   //  �ڶ�·��
case 0xFC: K3=1;delay(k);dat=0xee;break;	   //  ����·��
case 0xFB: K4=1;delay(k);dat=0xee;break;	   //  ����·��
case 0xFA: K5=1;delay(k);dat=0xee;break;	   //  ����·��
case 0xF9: K6=1;delay(k);dat=0xee;break;	   //  ����·��
case 0xF8: K7=1;delay(k);dat=0xee;break;	   //  ����·��
case 0xF7: K8=1;delay(k);dat=0xee;break;	   //  �ڰ�·��

case 0x55: ZTS[2]=P1;delay(k);SendStr(ZTS);dat=0xee;break;	   //  �������״̬
case 0xAA: ZTS[2]=P2;delay(k);SendStr(ZTS);dat=0xee;break;	   //  �������״̬

case 0x11: P2=~P2;delay(k);dat=0xee;break;  //  �����ȡ��

default:break;					   //  ����
		}
	  }

    }
}



/*------------------------------------------------
                    ����һ���ֽ�
------------------------------------------------*/
void SendByte(unsigned char da)
	{
 	SBUF = da;
	while(!TI);
      	TI = 0;
	}
/*------------------------------------------------
                    ����һ���ַ���
------------------------------------------------*/
  void SendStr(unsigned char *s)
{
 while(*s!='\0')// \0 ��ʾ�ַ���������־��ͨ������Ƿ��ַ���ĩβ
  {
  SendByte(*s);
  delay(50);
  s++;
  }
}




/*------------------------------------------------
                     �����жϳ���
------------------------------------------------*/
void UART_SER (void) interrupt 4 //�����жϷ������
{
    unsigned char Temp;          //������ʱ���� 
   
   if(RI)                        //�ж��ǽ����жϲ���
     {
	  RI=0;                      //��־λ����
	  Temp=SBUF;                 //���뻺������ֵ
	                     
	  judge[cnt++]=Temp; 

      //��ͷ���������һ��Э��֡�������
      if(cnt==8||judge[0]!=0xFF)	  
      cnt=0;
      SBUF=Temp;     
	  //TI=1;
	 }
   if(TI) 
	TI=0;
} 