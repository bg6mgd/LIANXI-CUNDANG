  #include <reg52.h>
 //#include "MY51.h"
  //#define uchar unsigned char;
void 	initSer(); 
         //���ڳ�ʼ��
		 char  table1[2];
		 unsigned char  j = 0;
unsigned char  sendFlag = 0;     //δ��������ʱ
unsigned char  receFlag =0;		//δ���ܵ�����ʱ

//code  unsigned char  TEXT0[] = "****************************\r\n";  //\r\n�ǻس�����
//code  unsigned char  TEXT1[] = "��Ƭ������ͨѶ����\r\n";
//code  unsigned char  TEXT2[] = "http://xouou.iteye.com\r\n";
//code  unsigned char  TEXT3[] = "****************************\r\n\r\n";//�س����в���1��

void sendChar(unsigned char sendValue);  //����һ�ֽ�����
void sendAll(unsigned char *pValue);       //����һ������

void main(void)
{
	initSer();
	while(1)
	{
	
	  if(receFlag)				//��Ƭ�������յ��������ݺ�,��ʼ��PC��������
	  {
		  // sendAll(TEXT0);         //��������
		  // sendAll(TEXT1);
		  // sendAll(TEXT2);
		  // sendAll(TEXT3);
		   sendAll(table1);
		 

		   j=0;
			receFlag=0;				//���������־	
	  }
	}
}

void serInt() interrupt 4	//�жϺ���
{
	while(RI)                  //����յ���������
	{
		RI = 0;
		table1[j++]=SBUF;                                //�����ݵ����ջ���
                if(j==2)      
				//���ݵı仯��led��ֱ�۷�Ӧ����
		receFlag=1;   //�޸Ľ��ܱ�־,��������������while�з�����
	}
	
	if(TI)
	{
		TI = 0;         	   //������һ������
		sendFlag = 0;        //���־λ
	}
}


void sendChar(unsigned char Value)  //����һ���ֽ�����
{
	 SBUF = Value;     
	 sendFlag = 1;       //���÷��ͱ�־λ,��һ�ֽھ���λ
	 while(sendFlag);	 //ֱ����������,��sendFlag�����,���˳�sendChar����
}

void sendAll(unsigned char *pValue)	//����һ������
{
	//while((*pValue) != '\0')   //���û�з�����Ͼͼ�����
	while(*(pValue+3))   //���û�з�����Ͼͼ�����
	{
		sendChar(*pValue);      //����1�ֽ�����
		pValue++;         		  //ָ����1���ֽ�
	}
}

void initSer()		//��ʼ��
{
    //ע��:���ϵ�ʱ,SCON��Ϊ0��
	TMOD=TMOD|0x20; //T1��ʱ��ģʽ,������ʽ2
	TH1=0Xfd;	  	 // 256-(11059200/(32*12*9600))
	TL1=0xfd;
	SM0=0;			 //����SCON�Ĵ���
	SM1=1;			 //���ڹ�����ʽ1,10λ�첽,�����ʿɸ�
	REN=1;			 //�����ڽ���
	ES=1;			 //�������ж�
    EA=1;			 //�����ж�
	TR1=1;		 //������ʱ��
}
