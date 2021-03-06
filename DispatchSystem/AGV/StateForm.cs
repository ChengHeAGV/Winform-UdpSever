﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace DispatchSystem.AGV
{
    public partial class StateForm : Form
    {
        int deviceNum = 0;
        Thread threaMain;
        Int64[] comparebuf = new Int64[UdpSever.RegisterNum];
        public StateForm(int num)
        {
            deviceNum = num;
            InitializeComponent();
        }

        private void StateForm_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("AGV{0}-运行状态", deviceNum);
            this.dataGridView1.DefaultCellStyle.Font = new Font("新宋体", 20, FontStyle.Regular); //字体颜色

            for (int i = 0; i < 17; i++)
            {
                dataGridView1.Rows.Add();
            }

            for (int i = 0; i < UdpSever.RegisterNum; i++)
            {
                dataGridView1.Rows.Add();
            }

            update(false);

            threaMain = new Thread(new ThreadStart(func));
            threaMain.IsBackground = false;
            threaMain.Start();
        }

        private void func()
        {
            while (this.IsHandleCreated && this.IsDisposed == false)
            {
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    update(true);
                }));
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 更新界面
        /// </summary>
        /// <param name="change">有变化才更新</param>
        private void update(bool change)
        {
            var num = 0;
            var reg = 51;

            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "电量";
                dataGridView1.Rows[num].Cells[3].Value = string.Format("{0}%", UdpSever.Register[deviceNum, reg, 0]);
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 1;
            reg = 52;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "电压";
                dataGridView1.Rows[num].Cells[3].Value = string.Format("{0}mV", UdpSever.Register[deviceNum, reg, 0]);
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 2;
            reg = 53;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "电池循环次数";
                dataGridView1.Rows[num].Cells[3].Value = string.Format("{0}次", UdpSever.Register[deviceNum, reg, 0]);
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }
            num = 3;
            reg = 54;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "低电压报警阈值";
                dataGridView1.Rows[num].Cells[3].Value = string.Format("{0}%", UdpSever.Register[deviceNum, reg, 0]);
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 4;
            reg = 55;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "当前速度档位";
                switch (UdpSever.Register[deviceNum, reg, 0])
                {
                    case 0:
                        dataGridView1.Rows[num].Cells[3].Value = "高速";
                        break;
                    case 1:
                        dataGridView1.Rows[num].Cells[3].Value = "中速";
                        break;
                    case 2:
                        dataGridView1.Rows[num].Cells[3].Value = "低速";
                        break;
                    case 3:
                        dataGridView1.Rows[num].Cells[3].Value = "对接速度";
                        break;
                }
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 5;
            reg = 56;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "当前模式";
                dataGridView1.Rows[num].Cells[3].Value = UdpSever.Register[deviceNum, reg, 0] == 0 ? "自动" : "手动";
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 6;
            reg = 57;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "当前方向";
                dataGridView1.Rows[num].Cells[3].Value = UdpSever.Register[deviceNum, reg, 0] == 0 ? "前进" : "后退";
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 7;
            reg = 58;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "当前分叉";

                switch (UdpSever.Register[deviceNum, reg, 0])
                {
                    case 1:
                        dataGridView1.Rows[num].Cells[3].Value = "左分叉";
                        break;
                    case 2:
                        dataGridView1.Rows[num].Cells[3].Value = "右分叉";
                        break;
                    case 11:
                        dataGridView1.Rows[num].Cells[3].Value = "前左分叉";
                        break;
                    case 12:
                        dataGridView1.Rows[num].Cells[3].Value = "前左分叉";
                        break;
                    case 21:
                        dataGridView1.Rows[num].Cells[3].Value = "后左分叉";
                        break;
                    case 22:
                        dataGridView1.Rows[num].Cells[3].Value = "后又分叉";
                        break;
                }
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 8;
            reg = 59;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "磁导航1";
                dataGridView1.Rows[num].Cells[3].Value = Convert.ToString(UdpSever.Register[deviceNum, reg, 0], 2).PadLeft(16, '0');
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 9;
            reg = 60;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "磁导航2";
                dataGridView1.Rows[num].Cells[3].Value = Convert.ToString(UdpSever.Register[deviceNum, reg, 0], 2).PadLeft(16, '0');
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 10;
            reg = 61;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "磁导航3";
                dataGridView1.Rows[num].Cells[3].Value = Convert.ToString(UdpSever.Register[deviceNum, reg, 0], 2).PadLeft(16, '0');
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 11;
            reg = 62;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "磁导航4";
                dataGridView1.Rows[num].Cells[3].Value = Convert.ToString(UdpSever.Register[deviceNum, reg, 0], 2).PadLeft(16, '0');
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 12;
            reg = 63;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "急停状态";
                dataGridView1.Rows[num].Cells[3].Value = UdpSever.Register[deviceNum, reg, 0] == 0 ? "正常" : "急停";
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 13;
            reg = 64;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "滚筒左侧红外";
                dataGridView1.Rows[num].Cells[3].Value = UdpSever.Register[deviceNum, reg, 0] == 0 ? "正常" : "触发";
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 14;
            reg = 65;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "滚筒右侧红外";
                dataGridView1.Rows[num].Cells[3].Value = UdpSever.Register[deviceNum, reg, 0] == 0 ? "正常" : "触发";
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 15;
            reg = 8;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "运行状态";
                switch (UdpSever.Register[deviceNum, reg, 0])
                {
                    case 0:
                        dataGridView1.Rows[num].Cells[3].Value = "未就绪";
                        break;
                    case 1:
                        dataGridView1.Rows[num].Cells[3].Value = "运行";
                        break;
                    case 2:
                        dataGridView1.Rows[num].Cells[3].Value = "就绪";
                        break;
                    case 3:
                        dataGridView1.Rows[num].Cells[3].Value = "充电";
                        break;
                    default:
                        break;
                }
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }

            num = 16;
            reg = 9;
            if (comparebuf[reg] != UdpSever.Register[deviceNum, reg, 0] || change)
            {
                dataGridView1.Rows[num].Cells[0].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "yyyy - MM - dd");
                dataGridView1.Rows[num].Cells[1].Value = UdpSever.StampToString(UdpSever.Register[deviceNum, reg, 1], "HH: mm:ss");
                dataGridView1.Rows[num].Cells[2].Value = "报警信息";

                string temp = string.Empty;

                if (UdpSever.Register[deviceNum, reg, 0] == 0)
                    temp = "正常";
                else
                {
                    if ((UdpSever.Register[deviceNum, reg, 0] & (0x0001 << 1)) > 0)
                        temp += "[机械碰撞]";
                    if ((UdpSever.Register[deviceNum, reg, 0] & (0x0001 << 2)) > 0)
                        temp += "[出轨]";
                    if ((UdpSever.Register[deviceNum, reg, 0] & (0x0001 << 3)) > 0)
                        temp += "[红外避障]";
                    if ((UdpSever.Register[deviceNum, reg, 0] & (0x0001 << 4)) > 0)
                        temp += "[电量低]";
                    if ((UdpSever.Register[deviceNum, reg, 0] & (0x0001 << 5)) > 0)
                        temp += "[驱动断电]";
                    if ((UdpSever.Register[deviceNum, reg, 0] & (0x0001 << 6)) > 0)
                        temp += "[地标出错]";
                }
                dataGridView1.Rows[num].Cells[3].Value = temp;
                comparebuf[reg] = UdpSever.Register[deviceNum, reg, 0];
            }
        }
    }
}
