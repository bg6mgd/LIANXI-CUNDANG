	 #include <string.h>
	 #include <reg52.h>

#define FOSC 11059200L

#define BAUD 9600

 

/***********************************/

// ���ڳ�ʼ������

/***********************************/

void uartInit( )

{

    SCON = 0x50;

    TMOD |= 0x20;

    TH1=TL1 = -(FOSC/12/32/BAUD);

    TR1 =1;

    ES = 1;

    EA =1;

}

 

/***********************************/

// ����1����һ���ֽڵ���λ��

/***********************************/

void uartSendData(unsigned char dat)	

{   

     SBUF=dat;

    while(TI==0);	

     TI=0;   

}

/***********************************/

// ���ڷ���һ�����鵽��λ��

/***********************************/

void uartSendArray(unsigned char *dat, unsigned  char len )

{

    unsigned char i;

   for(i=0; i<len; i++)

   {

         uartSendData(*dat);

 	   dat++;

   }

}

 

//-----------------------------------

//          ����1�жϳ���

//------------------------------------

 

void uart_Isr( )interrupt 4

{

  unsigned char dat =SBUF;

  if(RI)

  {

        RI=0; 

	uartRecive(dat);   //�������ݺ���      	

  }

    

 

if(TI)

  {

      / / TI=0;	

  } 

}

 

void main(  )

{

     unsigned char m,  n;

    char buf[ ]="hello world!\r\n";

      uartInit(   );

     while(1)

    {

        for(m=0;m<200;m++)

            for(n=0;n<200;n++);

           uartSendArray(buf,  strlen(buf));

    }

}
