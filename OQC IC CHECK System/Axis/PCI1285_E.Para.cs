using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Advantech.Motion;

namespace OQC_IC_CHECK_System
{
    partial class PCI1285_E
    {

        #region 轴参数设置
        //相对运动点距离设定
        public void SetPoxEnd_X(double value)
        {
            try
            {
                if (m_bInit != true) { return; }
                EndArray[GlobalVar.AxisX.LinkIndex] = value * GlobalVar.ServCMDRate;
            }
            catch { }
        }
        //相对运动点距离设定
        public void SetPoxEnd_Y(double value)
        {
            try
            {
                if (m_bInit != true) { return; }
                EndArray[GlobalVar.AxisY.LinkIndex] = value * GlobalVar.ServCMDRate;
            }
            catch { }
        }
        //相对运动点距离设定
        public void SetPoxEnd_A(double value)
        {
            try
            {
                if (m_bInit != true) { return; }
                EndArray[GlobalVar.AxisA.LinkIndex] = value * GlobalVar.ServCMDRate;
            }
            catch { }
        }//相对运动点距离设定
        public void SetPoxEnd_B(double value)
        {
            try
            {
                if (m_bInit != true) { return; }
                EndArray[GlobalVar.AxisB.LinkIndex] = value * GlobalVar.ServCMDRate;
            }
            catch { }
        }
        #endregion

        #region 轴速度设置
        //单轴最高速度设置-- type{true:群组，false:单轴}
        public uint SetProp_VelHigh(uint AxisNum, double VelHigh, bool isGpSetting)
        {
            uint result = Motion.mAcm_SetProperty(isGpSetting ? m_GpHand : m_Axishand[AxisNum],
                isGpSetting ? (uint)PropertyID.PAR_GpVelHigh : (uint)PropertyID.PAR_AxVelHigh,
                ref VelHigh, (uint)Marshal.SizeOf(typeof(double)));
            return result;
        }

        //单轴最低速度设置-- type{true:群组，false:单轴, bool type}
        public uint SetProp_VelLow(uint AxisNum, double VelLow, bool isGpSetting)
        {
            uint result = Motion.mAcm_SetProperty(isGpSetting ? m_GpHand : m_Axishand[AxisNum],
                isGpSetting ? (uint)PropertyID.PAR_GpVelLow : (uint)PropertyID.PAR_AxVelLow,
                ref VelLow, (uint)Marshal.SizeOf(typeof(double)));
            return result;
        }

        //单轴减速度设置-- type{true:群组，false:单轴, bool type}
        public uint SetProp_Dec(uint AxisNum, double Dec, bool isGpSetting)
        {
            uint result = Motion.mAcm_SetProperty(isGpSetting ? m_GpHand : m_Axishand[AxisNum],
                isGpSetting ? (uint)PropertyID.PAR_GpDec : (uint)PropertyID.PAR_AxDec,
                ref Dec, (uint)Marshal.SizeOf(typeof(double)));
            return result;
        }

        //单轴加速度设置-- type{true:群组，false:单轴, bool type}
        public uint SetProp_Acc(uint AxisNum, double Acc, bool isGpSetting)
        {
            uint result = Motion.mAcm_SetProperty(isGpSetting ? m_GpHand : m_Axishand[AxisNum],
                isGpSetting ? (uint)PropertyID.PAR_GpAcc : (uint)PropertyID.PAR_AxAcc,
                ref Acc, (uint)Marshal.SizeOf(typeof(double)));
            return result;
        }

        //速度曲线类型设置-- type{true:群组，false:单轴, bool type}, Jerk(0: T 形曲线（默认）)
        public uint SetProp_Jerk(uint AxisNum, double Jerk, bool isGpSetting)
        {
            uint result = Motion.mAcm_SetProperty(isGpSetting ? m_GpHand : m_Axishand[AxisNum],
                isGpSetting ? (uint)PropertyID.PAR_GpJerk : (uint)PropertyID.PAR_AxJerk,
                ref Jerk, (uint)Marshal.SizeOf(typeof(double)));
            return result;
        }
        #endregion
    }
}
