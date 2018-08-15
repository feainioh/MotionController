using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskScheduler;

namespace OQC_IC_CHECK_System
{   
    class WindowsSchedule
    {
        /// <summary>
        /// delete task
        /// </summary>
        /// <param name="taskName"></param>
        private static void DeleteTask(string taskName)
        {
            TaskSchedulerClass ts = new TaskSchedulerClass();
            ts.Connect(null, null, null, null);
            ITaskFolder folder = ts.GetFolder("\\");
            folder.DeleteTask(taskName, 0);
        }

        /// <summary>
        /// get all tasks
        /// </summary>
        public static IRegisteredTaskCollection GetAllTasks()
        {
            TaskSchedulerClass ts = new TaskSchedulerClass();
            ts.Connect(null, null, null, null);
            ITaskFolder folder = ts.GetFolder("\\");
            IRegisteredTaskCollection tasks_exists = folder.GetTasks(1);
            return tasks_exists;
        }
        /// <summary>
        /// check task isexists
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <returns></returns>
        public static bool IsExists(string taskName)
        {
            var isExists = false;
            IRegisteredTaskCollection tasks_exists = GetAllTasks();
            for (int i = 1; i <= tasks_exists.Count; i++)
            {
                IRegisteredTask t = tasks_exists[i];
                if (t.Name.Equals(taskName))
                {
                    isExists = true;
                    break;
                }
            }
            return isExists;
        }

        /// <summary>
        /// create task
        /// </summary>
        /// <param name="creator">创建计划的用户</param>
        /// <param name="taskName">任务名称</param>
        /// <param name="description">描述</param>
        /// <param name="path">启动程序</param>
        /// <param name="interval">时间间隔</param>
        /// <returns>state</returns>
        public static _TASK_STATE CreateTaskScheduler(string creator, string taskName, string description, string path, string interval)
        {
            try
            {
                if (IsExists(taskName))
                {
                    DeleteTask(taskName);
                }

                //new scheduler
                TaskSchedulerClass scheduler = new TaskSchedulerClass();
                //pc-name/ip,username,domain,password
                scheduler.Connect(null, null, null, null);
                //get scheduler folder
                ITaskFolder folder = scheduler.GetFolder("\\");


                //set base attr 
                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = string.IsNullOrEmpty(creator) ? Environment.UserName : creator;//creator
                task.RegistrationInfo.Description = description;//description

                //set trigger  (IDailyTrigger ITimeTrigger)
                //ITimeTrigger tt = (ITimeTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);//触发器一次
                IDailyTrigger tt = (IDailyTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);//触发器每天
                //tt.Repetition.Interval = interval;// format PT1H1M==1小时1分钟 设置的值最终都会转成分钟加入到触发器
                tt.StartBoundary = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd").Insert(10, "T04:00:00");// "2015-04-09T14:27:25";//start time

                //set action
                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = path;

                task.Settings.ExecutionTimeLimit = "PT0S"; //运行任务时间超时停止任务吗? PTOS 不开启超时
                task.Settings.DisallowStartIfOnBatteries = false;//只有在交流电源下才执行
                task.Settings.RunOnlyIfIdle = false;//仅当计算机空闲下才执行

                IRegisteredTask regTask = folder.RegisterTaskDefinition(taskName, task,
                                                                    (int)_TASK_CREATION.TASK_CREATE, null, //user
                                                                    null, // password
                                                                    _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                                                                    "");
                //IRunningTask runTask = regTask.Run(null);//启动计划任务运行的程序
                //return runTask.State;
                return regTask.State;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// create task
        /// </summary>
        /// <param name="creator">创建计划的用户</param>
        /// <param name="taskName">任务名称</param>
        /// <param name="description">描述</param>
        /// <param name="path">启动程序</param>
        /// <param name="arguments">添加参数</param>
        /// <param name="interval">时间间隔</param>
        /// <returns>state</returns>
        public static _TASK_STATE CreateTaskScheduler(string creator, string taskName, string description, string path, string arguments, string interval)
        {
            try
            {
                if (IsExists(taskName))
                {
                    DeleteTask(taskName);
                }

                //new scheduler
                TaskSchedulerClass scheduler = new TaskSchedulerClass();
                //pc-name/ip,username,domain,password
                scheduler.Connect(null, null, null, null);
                //get scheduler folder
                ITaskFolder folder = scheduler.GetFolder("\\");


                //set base attr 
                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = string.IsNullOrEmpty(creator)?Environment.UserName:creator;//creator
                task.RegistrationInfo.Description = description;//description

                //set trigger  (IDailyTrigger ITimeTrigger)
                //ITimeTrigger tt = (ITimeTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);//触发器一次
                IDailyTrigger tt = (IDailyTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);//触发器每天
                //tt.Repetition.Interval = interval;// format PT1H1M==1小时1分钟 设置的值最终都会转成分钟加入到触发器
                tt.StartBoundary = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd").Insert(10, "T04:00:00");// "2015-04-09T14:27:25";//start time

                //set action
                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = path;
                action.Arguments = arguments;

                task.Settings.ExecutionTimeLimit = "PT0S"; //运行任务时间超时停止任务吗? PTOS 不开启超时
                task.Settings.DisallowStartIfOnBatteries = false;//只有在交流电源下才执行
                task.Settings.RunOnlyIfIdle = false;//仅当计算机空闲下才执行

                IRegisteredTask regTask = folder.RegisterTaskDefinition(taskName, task,
                                                                    (int)_TASK_CREATION.TASK_CREATE, null, //user
                                                                    null, // password
                                                                    _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                                                                    "");
                //IRunningTask runTask = regTask.Run(null);//启动计划任务运行的程序
                //return runTask.State;
                return regTask.State;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
