#include<reg52.h>
 #define uchar unsigned char
 #define uint unsigned int
 #define N 2                                                        //��һ�ν���������
 void rs232_init();
 uchar flag,i;                                                       //ɾ�����ñ���                           
 uchar code table[]="I get ";
 uchar table1[N];                                              //���ջ�������
 uchar j=0;                                                             //���ռ�����
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
                         for(j=0;j<N;j++)                        //���ͽ�������
                                                 {
                                                         SBUF=table1[j];
                                 while(!TI);
                                 TI=0;
                                                }
                         j=0;                                           //������ռ�����
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
                 REN=1;                                                        //���趨�Ź�����ʽ���ڴ��������
         EA=1;
         ES=1;        
 }
 void ser()interrupt 4
 {                 
                RI=0;
                table1[j++]=SBUF;                                //�����ݵ����ջ���
                if(j==N)                                                //������ʱ��������
                flag=1;
 }