﻿using DispatchSystem.Developer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DispatchSystem.User
{
    public partial class TaskForm : Form
    {
        ContextMenuStrip contextWaiting;
        ContextMenuStrip contextRunning;

        public TaskForm()
        {
            InitializeComponent();
        }

        Thread taskThread;
        private void TaskForm_Load(object sender, EventArgs e)
        {
            #region 创建等待任务列表右键菜单
            //等待任务列表右键菜单
            contextWaiting = new ContextMenuStrip();
            contextWaiting.Items.Add("创建任务");
            contextWaiting.Items.Add("取消任务");

            //添加点击事件
            contextWaiting.Items[0].Click += contextWaiting_AddTask_Click;
            contextWaiting.Items[1].Click += contextWaiting_DeleteTask_Click;

            //添加单元格点击事件
            dataGridViewWaiting.CellMouseClick += DataGridViewWaiting_CellMouseClick;
            //添加任意位置点击事件
            dataGridViewWaiting.MouseClick += DataGridViewWaiting_MouseClick;
            #endregion

            #region 创建正在进行任务列表右键菜单
            //等待任务列表右键菜单
            contextRunning = new ContextMenuStrip();
            contextRunning.Items.Add("任务重发");
            contextRunning.Items.Add("取消任务");

            //添加点击事件
            contextRunning.Items[0].Click += contextRunning_Repeat_Click;
            contextRunning.Items[1].Click += contextRunning_Delete_Click;

            //添加单机点击事件
            dataGridViewRunning.CellMouseClick += DataGridViewRunning_CellMouseClick;
            #endregion

            #region 创建已完成任务列表右键菜单
            //等待任务列表右键菜单
            ContextMenuStrip contextFinished = new ContextMenuStrip();
            #endregion

            #region 启动任务调度
            taskThread = new Thread(new ThreadStart(taskFunc));
            taskThread.IsBackground = true;
            taskThread.Start();
            #endregion
        }

        #region 等待任务事件
        /// <summary>
        /// 等待列表单元格右击事件 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewWaiting_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.ColumnIndex > -1 && e.RowIndex > -1)  //点击的是鼠标右键，并且不是表头
            {
                contextWaiting.Items[1].Enabled = true;
                dataGridViewWaiting.ClearSelection();
                dataGridViewWaiting.Rows[e.RowIndex].Selected = true;
                contextWaiting.Show(MousePosition.X, MousePosition.Y);
            }
        }

        /// <summary>
        /// 等待列表右击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewWaiting_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)  //点击的是鼠标右键，并且不是表头
            {
                contextWaiting.Items[1].Enabled = false;
                contextWaiting.Show(MousePosition.X, MousePosition.Y);
            }
        }

        /// <summary>
        /// 新增任务事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextWaiting_AddTask_Click(object sender, EventArgs e)
        {

            AddTask addtask = new AddTask();
            addtask.ShowDialog();
            if (addtask.DialogResult == DialogResult.OK)
            {
                //添加任务到列表
                //创建任务
                TaskInfo task = new TaskInfo();
                //[订单号]毫秒时间戳
                task.OrderNum = GetTimeStamp();
                //[任务编号]
                task.TaskNum = addtask.TaskNum;
                //[产线名称]
                task.LineName = addtask.LinekName;

                //添加任务到待执行列表
                TaskData.Waiting.Add(task);

                //更新到界面
                this.Invoke(new MethodInvoker(delegate
                {
                    var index = dataGridViewWaiting.Rows.Add();
                    //序号
                    dataGridViewWaiting.Rows[index].Cells[0].Value = index;
                    //订单号
                    dataGridViewWaiting.Rows[index].Cells[1].Value = task.OrderNum;
                    //任务编号
                    dataGridViewWaiting.Rows[index].Cells[2].Value = task.TaskNum;
                    //产线名称
                    dataGridViewWaiting.Rows[index].Cells[3].Value = task.LineName;
                    //下单时间
                    dataGridViewWaiting.Rows[index].Cells[4].Value = task.CreatTime.ToString("yyyy-MM-dd HH:mm:ss fff");

                    //滚动到最后一行
                    dataGridViewWaiting.FirstDisplayedScrollingRowIndex = dataGridViewWaiting.RowCount - 1;
                }));
            }
        }

        /// <summary>
        /// 删除任务事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextWaiting_DeleteTask_Click(object sender, EventArgs e)
        {
            //获取待删除任务索引
            int index = (int)dataGridViewWaiting.SelectedRows[0].Index;
            if (dataGridViewWaiting.Rows[index].Cells[1].Value != null)
            {
                TaskData.Waiting.RemoveAt(index);
                //从界面删除
                dataGridViewWaiting.Rows.Remove(dataGridViewWaiting.SelectedRows[0]);
            }
            else
            {
                MessageBox.Show("没有可删除的任务！");
            }
        }

        #endregion

        #region 进行中任务事件
        /// <summary>
        /// 任务重发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextRunning_Repeat_Click(object sender, EventArgs e)
        {
            //获取需要重发任务的AGV编号
            int num = (int)(dataGridViewRunning.SelectedRows[0].Cells[4].Value);
            if (UdpSever.Register[num, 8, 0] != 2)//判断AGV是否就绪
            {
                MessageBox.Show("AGV处于异常状态，请在AGV就绪后重试!");
            }
            else
            {
                //重新发送任务到AGV
                AssignTaskToAGV(num);
            }
        }
       
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextRunning_Delete_Click(object sender, EventArgs e)
        {
            //获取待删除任务索引
            int index = (int)dataGridViewRunning.SelectedRows[0].Index;
            if (dataGridViewRunning.Rows[index].Cells[1].Value != null)
            {
                //从任务列表删除
                TaskData.Waiting.RemoveAt(index);
                //从界面删除
                dataGridViewRunning.Rows.Remove(dataGridViewRunning.SelectedRows[0]);
            }
            else
            {
                MessageBox.Show("没有可删除的任务！");
            }
        }

        /// <summary>
        /// 正在执行列表右击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewRunning_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.ColumnIndex > -1 && e.RowIndex > -1)  //点击的是鼠标右键，并且不是表头
            {
                dataGridViewRunning.ClearSelection();
                dataGridViewRunning.Rows[e.RowIndex].Selected = true;
                contextRunning.Show(MousePosition.X, MousePosition.Y);
            }
        }
        #endregion


        /// <summary>
        /// 分配任务到AGV
        /// </summary>
        /// <param name="AgvNum"></param>
        private async void AssignTaskToAGV(int AgvNum)
        {
            await Task.Run(() =>
            {
                UdpSever.Register[AgvNum, 1, 0] = (int)dataGridViewRunning.Rows[dataGridViewRunning.SelectedRows.Count].Cells[2].Value;

                //等待收到任务
                while (UdpSever.Register[AgvNum, 2, 0] == 0)
                {
                    Thread.Sleep(10);
                }
                //清除AGV任务标志
                UdpSever.Register[AgvNum, 1, 0] = 0;
                ConsoleLog.WriteLog(string.Format("任务已经下发至AGV{0}", AgvNum));
            });
        }

        /// <summary>
        /// 任务信息
        /// </summary>
        public class TaskInfo
        {
            public long OrderNum = 0;//订单号
            public int TaskNum; //任务编号
            public string LineName;//产线名称
            public DateTime CreatTime = DateTime.Now;//任务下发时间
            public DateTime StartTime;//任务开始时间
            public DateTime StopTime;//任务结束时间

            public int AgvNum = 0; //AGV编号
        }

        public static class TaskData
        {
            public static class Parameter
            {
                //modbus 检测时间
                public static int SyncModbusTime = 500;
                //任务调度 检测时间
                public static int taskFuncTime = 1000;
            }

            /// <summary>
            /// 等待任务队列
            /// </summary>
            public static List<TaskInfo> Waiting = new List<TaskInfo>();

            /// <summary>
            /// 进行中任务队列
            /// </summary>
            public static List<TaskInfo> Runing = new List<TaskInfo>();

            /// <summary>
            ///已完成任务队列
            /// </summary>
            public static List<TaskInfo> Finished = new List<TaskInfo>();

            //定义两台AGV
            //public static Agv[] AGV = new Agv[3];

            public static bool AGVRuning_1 = false;
            public static bool AGVRuning_2 = false;
        }

        /// <summary>
        /// 任务方法
        /// </summary>
        private void taskFunc()
        {
            while (this.IsHandleCreated && this.IsDisposed == false)
            {
                if (DataTransmission.ListenState.ModbusTcp == false)
                {
                    // DataTransmission.StartListen();
                }
                else
                {
                    #region 判断MES是否有新任务
                    //扩散线
                    if (DataTransmission.Profinet.Register[0] > 0 && (DataTransmission.Profinet.Clear0 == false))
                    {
                        //创建任务
                        TaskInfo task = new TaskInfo();
                        //[订单号]毫秒时间戳
                        task.OrderNum = GetTimeStamp();
                        //[任务编号]
                        task.TaskNum = DataTransmission.Profinet.Register[0];
                        //[产线名称]
                        task.LineName = "扩散线";

                        //添加任务到待执行列表
                        TaskData.Waiting.Add(task);

                        //更新到界面
                        this.Invoke(new MethodInvoker(delegate
                        {
                            var index = dataGridViewWaiting.Rows.Add();
                            //序号
                            dataGridViewWaiting.Rows[index].Cells[0].Value = index;
                            //订单号
                            dataGridViewWaiting.Rows[index].Cells[1].Value = task.OrderNum;
                            //任务编号
                            dataGridViewWaiting.Rows[index].Cells[2].Value = task.TaskNum;
                            //产线名称
                            dataGridViewWaiting.Rows[index].Cells[3].Value = task.LineName;
                            //下单时间
                            dataGridViewWaiting.Rows[index].Cells[4].Value = task.CreatTime.ToString("yyyy-MM-dd HH:mm:ss fff");

                            //滚动到最后一行
                            dataGridViewWaiting.FirstDisplayedScrollingRowIndex = dataGridViewWaiting.RowCount - 1;
                        }));
                        //清除MES任务标志寄存器
                        DataTransmission.Profinet.Clear0 = true;
                    }
                    //PE线
                    if (DataTransmission.Profinet.Register[20] > 0 && (DataTransmission.Profinet.Clear20 == false))
                    {
                        //创建任务
                        TaskInfo task = new TaskInfo();
                        //[订单号]毫秒时间戳
                        task.OrderNum = GetTimeStamp();
                        //[任务编号]
                        task.TaskNum = DataTransmission.Profinet.Register[20];
                        //[产线名称]
                        task.LineName = "PE线";

                        //添加任务到待执行列表
                        TaskData.Waiting.Add(task);

                        //按优先级排序(降序)
                        // TaskData.taskWaiting = TaskData.taskWaiting.OrderByDescending(s => s.Level).ToList();
                        //按下单时间排序(升序)
                        //TaskData.taskWaiting = TaskData.taskWaiting.OrderBy(s => s.Level).ToList();  


                        //更新到界面
                        this.Invoke(new MethodInvoker(delegate
                        {
                            var index = dataGridViewWaiting.Rows.Add();
                            //序号
                            dataGridViewWaiting.Rows[index].Cells[0].Value = index;
                            //订单号
                            dataGridViewWaiting.Rows[index].Cells[1].Value = task.OrderNum;
                            //任务编号
                            dataGridViewWaiting.Rows[index].Cells[2].Value = task.TaskNum;
                            //产线名称
                            dataGridViewWaiting.Rows[index].Cells[3].Value = task.LineName;
                            //下单时间
                            dataGridViewWaiting.Rows[index].Cells[4].Value = task.CreatTime.ToString("yyyy-MM-dd HH:mm:ss fff");

                            //滚动到最后一行
                            dataGridViewWaiting.FirstDisplayedScrollingRowIndex = dataGridViewWaiting.RowCount - 1;
                        }));

                        //清除MES任务标志寄存器
                        DataTransmission.Profinet.Clear20 = true;
                    }
                    #endregion

                    #region 派发任务到AGV
                    //有需要完成的任务
                    if (TaskData.Waiting.Count > 0)
                    {
                        //1号产线有就绪的AGV
                        if (UdpSever.Register[1, 8, 0] == 2 && TaskData.AGVRuning_1 == false)
                        {
                            for (int i = 0; i < TaskData.Waiting.Count; i++)
                            {
                                if (TaskData.Waiting[i].LineName == "扩散线")
                                {
                                    //发送任务到AGV
                                    AssignTaskToAGV(1);

                                    TaskData.Waiting[i].AgvNum = 1;

                                    TaskData.AGVRuning_1 = true;

                                    //更新任务状态
                                    TaskData.Waiting[i].StartTime = DateTime.Now;//任务启动时间

                                    //将该任务转至正在进行任务列表
                                    TaskData.Runing.Add(TaskData.Waiting[i]);

                                    #region 更新-正在执行-到界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        var index = dataGridViewRunning.Rows.Add();
                                        //序号
                                        dataGridViewRunning.Rows[index].Cells[0].Value = index;
                                        //订单编号
                                        dataGridViewRunning.Rows[index].Cells[1].Value = TaskData.Waiting[i].OrderNum;
                                        //任务编号
                                        dataGridViewRunning.Rows[index].Cells[2].Value = TaskData.Waiting[i].TaskNum;
                                        //产线编号
                                        dataGridViewRunning.Rows[index].Cells[3].Value = TaskData.Waiting[i].LineName;
                                        //AGV编号
                                        dataGridViewRunning.Rows[index].Cells[4].Value = TaskData.Waiting[i].AgvNum;
                                        //下单时间
                                        dataGridViewRunning.Rows[index].Cells[5].Value = TaskData.Waiting[i].CreatTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //启动时间
                                        dataGridViewRunning.Rows[index].Cells[6].Value = TaskData.Waiting[i].StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //滚动到最后一行
                                        dataGridViewRunning.FirstDisplayedScrollingRowIndex = dataGridViewRunning.RowCount - 1;
                                    }));
                                    #endregion

                                    //从等待执行任务列表删除
                                    TaskData.Waiting.RemoveAt(i);
                                    #region 更新-等待执行-界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        dataGridViewWaiting.Rows.RemoveAt(i);
                                    }));
                                    #endregion
                                    break;
                                }
                            }
                        }

                        //2号产线有空闲的AGV
                        if (UdpSever.Register[2, 8, 0] == 2 && TaskData.AGVRuning_2 == false)
                        {
                            for (int i = 0; i < TaskData.Waiting.Count; i++)
                            {
                                if (TaskData.Waiting[i].LineName == "PE线")
                                {
                                    //发送任务到AGV
                                    AssignTaskToAGV(2);

                                    TaskData.Waiting[i].AgvNum = 2;

                                    TaskData.AGVRuning_2 = true;

                                    //更新任务状态
                                    TaskData.Waiting[i].StartTime = DateTime.Now;//任务启动时间

                                    //将该任务转至正在进行任务列表
                                    TaskData.Runing.Add(TaskData.Waiting[i]);

                                    #region 更新-正在执行-到界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        var index = dataGridViewRunning.Rows.Add();
                                        //序号
                                        dataGridViewRunning.Rows[index].Cells[0].Value = index;
                                        //订单编号
                                        dataGridViewRunning.Rows[index].Cells[1].Value = TaskData.Waiting[i].OrderNum;
                                        //任务编号
                                        dataGridViewRunning.Rows[index].Cells[2].Value = TaskData.Waiting[i].TaskNum;
                                        //产线名称
                                        dataGridViewRunning.Rows[index].Cells[3].Value = TaskData.Waiting[i].LineName;
                                        //AGV编号
                                        dataGridViewRunning.Rows[index].Cells[4].Value = TaskData.Waiting[i].AgvNum;
                                        //下单时间
                                        dataGridViewRunning.Rows[index].Cells[5].Value = TaskData.Waiting[i].CreatTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //启动时间
                                        dataGridViewRunning.Rows[index].Cells[6].Value = TaskData.Waiting[i].StartTime.ToString("yyyy-MM-dd HH:mm:ss");

                                        //滚动到最后一行
                                        dataGridViewRunning.FirstDisplayedScrollingRowIndex = dataGridViewRunning.RowCount - 1;
                                    }));
                                    #endregion

                                    //从等待执行任务列表删除
                                    TaskData.Waiting.RemoveAt(i);
                                    #region 更新-等待执行-界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        dataGridViewWaiting.Rows.RemoveAt(i);
                                    }));
                                    #endregion
                                    break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region 更新任务执行状态
                    //是否有正在执行的任务
                    if (TaskData.Runing.Count > 0)
                    {
                        for (int i = 0; i < TaskData.Runing.Count; i++)
                        {
                            //判断任务是否执行完成
                            if (TaskData.Runing[i].LineName == "扩散线")
                            {
                                if (UdpSever.Register[1, 4, 0] == 1)
                                {
                                    TaskData.Runing[i].StopTime = DateTime.Now;

                                    TaskData.AGVRuning_1 = false;

                                    //将该任务转至执行完成任务列表
                                    TaskData.Finished.Add(TaskData.Runing[i]);
                                    #region 更新-完成任务-到界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        var index = dataGridViewFinished.Rows.Add();
                                        //序号
                                        dataGridViewFinished.Rows[index].Cells[0].Value = index;
                                        //订单编号
                                        dataGridViewFinished.Rows[index].Cells[1].Value = TaskData.Runing[i].OrderNum;
                                        //任务编号
                                        dataGridViewFinished.Rows[index].Cells[2].Value = TaskData.Runing[i].TaskNum;
                                        //产线名称
                                        dataGridViewFinished.Rows[index].Cells[3].Value = TaskData.Runing[i].LineName;
                                        //AGV编号
                                        dataGridViewFinished.Rows[index].Cells[4].Value = TaskData.Runing[i].AgvNum;
                                        //下单时间
                                        dataGridViewFinished.Rows[index].Cells[5].Value = TaskData.Runing[i].CreatTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //启动时间
                                        dataGridViewFinished.Rows[index].Cells[6].Value = TaskData.Runing[i].StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //完成时间
                                        dataGridViewFinished.Rows[index].Cells[7].Value = TaskData.Runing[i].StopTime.ToString("yyyy-MM-dd HH:mm:ss");

                                        //滚动到最后一行
                                        dataGridViewFinished.FirstDisplayedScrollingRowIndex = dataGridViewFinished.RowCount - 1;
                                    }));
                                    #endregion

                                    #region 更新-正在执行-界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        dataGridViewRunning.Rows.RemoveAt(i);
                                    }));

                                    //将该任务从正在执行任务列表删除
                                    TaskData.Runing.RemoveAt(i);
                                    #endregion
                                }
                                else if (UdpSever.Register[1, 4, 0] == 0)
                                {
                                    //正在执行
                                    #region 更新正在执行界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        //当前站点
                                        dataGridViewRunning.Rows[i].Cells[7].Value = (ushort)UdpSever.Register[1, 5, 0];
                                        //下一个站点
                                        dataGridViewRunning.Rows[i].Cells[8].Value = (ushort)UdpSever.Register[1, 7, 0];
                                        //报警信息
                                        switch (UdpSever.Register[1, 9, 0])
                                        {
                                            case 0:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "正常";
                                                break;
                                            case 1:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "机械碰撞";
                                                break;
                                            case 2:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "出轨";
                                                break;
                                            case 3:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "红外避障";
                                                break;
                                            case 4:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "电量低";
                                                break;
                                            case 5:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "驱动断电";
                                                break;
                                            default:
                                                break;
                                        }
                                    }));
                                    #endregion
                                }
                            }
                            else
                            //判断任务是否执行完成
                            if (TaskData.Runing[i].LineName == "PE线")
                            {
                                if (UdpSever.Register[2, 4, 0] == 1)
                                {
                                    TaskData.Runing[i].StopTime = DateTime.Now;

                                    TaskData.AGVRuning_2 = false;

                                    //将该任务转至执行完成任务列表
                                    TaskData.Finished.Add(TaskData.Runing[i]);
                                    #region 更新-完成任务-到界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        var index = dataGridViewFinished.Rows.Add();
                                        //序号
                                        dataGridViewFinished.Rows[index].Cells[0].Value = index;
                                        //订单编号
                                        dataGridViewFinished.Rows[index].Cells[1].Value = TaskData.Runing[i].OrderNum;
                                        //任务编号
                                        dataGridViewFinished.Rows[index].Cells[2].Value = TaskData.Runing[i].TaskNum;
                                        //产线名称
                                        dataGridViewFinished.Rows[index].Cells[3].Value = TaskData.Runing[i].LineName;
                                        //AGV编号
                                        dataGridViewFinished.Rows[index].Cells[4].Value = TaskData.Runing[i].AgvNum;
                                        //下单时间
                                        dataGridViewFinished.Rows[index].Cells[5].Value = TaskData.Runing[i].CreatTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //启动时间
                                        dataGridViewFinished.Rows[index].Cells[6].Value = TaskData.Runing[i].StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //完成时间
                                        dataGridViewFinished.Rows[index].Cells[7].Value = TaskData.Runing[i].StopTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        //执行时间
                                        dataGridViewFinished.Rows[index].Cells[8].Value = TaskData.Runing[i].StopTime - TaskData.Runing[i].StartTime;

                                        //滚动到最后一行
                                        dataGridViewFinished.FirstDisplayedScrollingRowIndex = dataGridViewFinished.RowCount - 1;
                                    }));
                                    #endregion

                                    #region 更新-正在执行-界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        dataGridViewRunning.Rows.RemoveAt(i);
                                    }));

                                    //将该任务从正在执行任务列表删除
                                    TaskData.Runing.RemoveAt(i);
                                    #endregion
                                }
                                else if (UdpSever.Register[2, 4, 0] == 0)
                                {
                                    #region 更新正在执行界面
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        //当前站点
                                        dataGridViewRunning.Rows[i].Cells[7].Value = (ushort)UdpSever.Register[2, 5, 0]; ;
                                        //下一个站点
                                        dataGridViewRunning.Rows[i].Cells[8].Value = (ushort)UdpSever.Register[2, 7, 0]; ;
                                        //报警信息
                                        switch (UdpSever.Register[2, 9, 0])
                                        {
                                            case 0:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "正常";
                                                break;
                                            case 1:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "机械碰撞";
                                                break;
                                            case 2:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "出轨";
                                                break;
                                            case 3:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "红外避障";
                                                break;
                                            case 4:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "电量低";
                                                break;
                                            case 5:
                                                dataGridViewRunning.Rows[i].Cells[9].Value = "驱动断电";
                                                break;
                                            default:
                                                break;
                                        }
                                    }));
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion

                    #region 更新MES状态
                    //正在执行
                    if (TaskData.Runing.Count > 0)
                    {
                        foreach (var item in TaskData.Runing)
                        {
                            if (item.LineName == "扩散线")
                            {
                                //正在执行任务
                                DataTransmission.Profinet.Register[1] = (ushort)item.TaskNum;
                            }
                            else
                            if (item.LineName == "PE线")
                            {
                                //正在执行任务
                                DataTransmission.Profinet.Register[21] = (ushort)item.TaskNum;
                            }
                        }
                    }
                    //待执行
                    int[] waiting = new int[2];
                    if (TaskData.Waiting.Count > 0)
                    {
                        foreach (var item in TaskData.Waiting)
                        {
                            if (item.LineName == "扩散线" && waiting[0] < 5)
                            {
                                //待执行任务
                                DataTransmission.Profinet.Register[2 + waiting[0]++] = (ushort)item.TaskNum;
                            }
                            else
                            if (item.LineName == "PE线" && waiting[1] < 5)
                            {
                                //待执行任务
                                DataTransmission.Profinet.Register[22 + waiting[1]++] = (ushort)item.TaskNum;
                            }
                        }
                    }
                    //已完成
                    int[] finished = new int[2];
                    if (TaskData.Finished.Count > 0)
                    {
                        foreach (var item in TaskData.Finished)
                        {
                            if (item.LineName == "扩散线" && finished[0] < 5)
                            {
                                //已完成任务
                                DataTransmission.Profinet.Register[7 + finished[0]++] = (ushort)item.TaskNum;
                            }
                            else
                            if (item.LineName == "PE线" && finished[1] < 5)
                            {
                                //已完成任务
                                DataTransmission.Profinet.Register[27 + finished[1]++] = (ushort)item.TaskNum;
                            }
                        }
                    }

                    //上一个位置
                    DataTransmission.Profinet.Register[12] = (ushort)UdpSever.Register[1, 5, 0];
                    //当前位置
                    DataTransmission.Profinet.Register[13] = (ushort)UdpSever.Register[1, 6, 0];
                    //运行状态
                    DataTransmission.Profinet.Register[14] = (ushort)UdpSever.Register[1, 8, 0];

                    //上一个位置
                    DataTransmission.Profinet.Register[32] = (ushort)UdpSever.Register[1, 5, 0];
                    //当前位置
                    DataTransmission.Profinet.Register[33] = (ushort)UdpSever.Register[1, 6, 0];
                    //运行状态
                    DataTransmission.Profinet.Register[34] = (ushort)UdpSever.Register[1, 8, 0];
                    #endregion
                }
                Thread.Sleep(TaskData.Parameter.taskFuncTime);
            }
        }

        private long GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds; // 相差毫秒数
                                                                                 //System.Console.WriteLine(timeStamp);
            return timeStamp;
        }
    }
}
