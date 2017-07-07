  #include <reg52.h>
 
 
  char s;
   int i;

   char xd1;
			 void Init_Com(void)
{
TMOD = 0x20;
PCON = 0x00;
SCON = 0x50;
TH1 = 0xFd;
TL1 = 0xFd;
TR1 = 1;
}
void delay(int t)
{
for(i=0;i<t;i++)
{;;}
}
void main()
{
Init_Com();


while(1)
{
  
  s=P1;
  if(s!=P1)
  {
  SBUF=P1;
  s=P1;
  }

  
  delay(50);

}

}