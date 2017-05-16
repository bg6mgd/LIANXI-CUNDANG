using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SQWRZS.Devices
{
    public class EQ2008_DLL
    {
        //==========================1、节目操作函数======================//
        //添加节目
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddProgram(int CardNum, Boolean bWaitToEnd, int iPlayTime);
        //删除所有节目
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_DelAllProgram(int CardNum);

        //添加单行文本区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddSingleText(int CardNum, ref User_SingleText pSingleText, int iProgramIndex);
        //添加文本区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddText(int CardNum, ref User_Text pText, int iProgramIndex);
        //添加时间区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddTime(int CardNum, ref User_DateTime pdateTime, int iProgramIndex);
        //添加图文区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddBmpZone(int CardNum, ref User_Bmp pBmp, int iProgramIndex);
        //指定图像句柄添加图片
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern bool User_AddBmp(int CardNum, int iBmpPartNum, IntPtr hBitmap, ref User_MoveSet pMoveSet, int iProgramIndex);
        //指定图像路径添加图片
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern bool User_AddBmpFile(int CardNum, int iBmpPartNum, string strFileName, ref User_MoveSet pMoveSet, int iProgramIndex);

        //添加RTF区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddRTF(int CardNum, ref User_RTF pRTF, int iProgramIndex);
        //添加计时区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddTimeCount(int CardNum, ref User_Timer pTimeCount, int iProgramIndex);
        //添加温度区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddTemperature(int CardNum, ref User_Temperature pTemperature, int iProgramIndex);

        //发送数据
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_SendToScreen(int CardNum);
        //====================================================================//       

        //=======================2、实时发送数据（高频率发送）=================//
        //实时建立连接
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeConnect(int CardNum);
        //实时发送图片数据
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeSendData(int CardNum, int x, int y, int iWidth, int iHeight, IntPtr hBitmap);
        //实时发送图片文件
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeSendBmpData(int CardNum, int x, int y, int iWidth, int iHeight, string strFileName);
        //实时发送文本
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeSendText(int CardNum, int x, int y, int iWidth, int iHeight, string strText, ref User_FontSet pFontInfo);
        //实时关闭连接
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeDisConnect(int CardNum);
        //实时发送清屏
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeScreenClear(int CardNum);
        //====================================================================//

        //==========================3、显示屏控制函数组=======================//
        //校正时间
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_AdjustTime(int CardNum);
        //开屏
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_OpenScreen(int CardNum);
        //关屏
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_CloseScreen(int CardNum);
        //亮度调节
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_SetScreenLight(int CardNum, int iLightDegreen);
        //Reload参数文件
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern void User_ReloadIniFile(string strEQ2008_Dll_Set_Path);
        //====================================================================//

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);




        public static int g_iCardNum_small = 1;      //控制卡地址
        public static int g_iCardNum_big = 2;

        public static int g_iGreen = 0xFF00; //绿色
        public static int g_iYellow = 0xFFFF; //黄色
        public static int g_iRed = 0x00FF; //红色

        //颜色常量
        public static int RED = 0x0000FF;
        public static int GREEN = 0x00FF00;
        public static int YELLOW = 0x00FFFF;

        //返回值常量
        public static int EQ_FALSE = 0;
        public static int EQ_TRUE = 1;

        //节目索引
        public static int g_iProgramIndex = 0;
        public static int g_iProgramCount = 0;


    }
}
