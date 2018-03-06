using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace AutoZhenJiu.fn
{
    class FnComm
    {

        public FnComm(){}

        /// <summary>
        /// 发送指令
        /// </summary>o
        public byte[] sendOrder(byte[] order)
        {
            if (Global.sp == null || !Global.sp.IsOpen)
            {
                //MessageBox.Show("串口未打开，请设置");
                Console.WriteLine("串口未打开，请设置");
                return null;
            }
            checkOrder(order);
           Console.WriteLine("ORDER 发送");
            foreach(byte b in order){
                Console.Write(Convert.ToString(b, 16).ToUpper() + " ");
            }

            Global.sp.Write(order, 0, order.Length);
            return order;
        }

        /// <summary>
        /// 为命令填充校验位
        /// </summary>
        public void checkOrder(byte[] order)
        {
            byte[] crc = CRC16MODBUS(order, order.Length - 2);
            order[order.Length - 2] = crc[0];
            order[order.Length - 1] = crc[1];
        }

        /// <summary>
        /// 获取校验位（其他模块使用）
        /// </summary>
        public bool compareCheck(byte[] send, int[] resp)
        {
            if (send[send.Length - 1] == resp[resp.Length - 1]) { return false; }
            if (send[send.Length - 2] == resp[resp.Length - 2]) { return false; }

            return true;
        }

        /// <summary>
        /// Good ! dataLen为int
        /// </summary>
        public byte[] CRC16MODBUS(byte[] dataBuff, int checkLengh)
        {
            byte CRC16High = 0;
            byte CRC16Low = 0;

            int CRCResult = 0xFFFF;
            for (int i = 0; i < checkLengh; i++)
            {
                CRCResult = CRCResult ^ dataBuff[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((CRCResult & 1) == 1)
                        CRCResult = (CRCResult >> 1) ^ 0xA001;
                    else
                        CRCResult >>= 1;
                }
            }
            CRC16High = Convert.ToByte(CRCResult & 0xff);
            CRC16Low = Convert.ToByte(CRCResult >> 8);

            byte[] result = new byte[2];
            result[0] = CRC16Low;
            result[1] = CRC16High;

            return result;

        }

         

    }
}
