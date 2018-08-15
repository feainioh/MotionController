using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class ICForm : Frame
    {
        MyFunction myfunction = new MyFunction();
        OBJ_DWGDirect obj_dwg = new OBJ_DWGDirect();
        public ICForm()
        {
            InitializeComponent();
            this.splitContainer_CAD.Panel2.Controls.Add(obj_dwg);
            obj_dwg.Dock = DockStyle.Fill;
        }

        private void ICForm_Load(object sender, EventArgs e)
        {
            string filename   = myfunction.GetProductIniPath(GlobalVar.Product + ".dwg");

            obj_dwg.LoadCADFile(filename,1);
            InitialBlock();
        }

        private void InitialBlock()
        {
            this.splitContainer_CAD.Panel1.Controls.Clear();
            if (GlobalVar.CADPointList.m_List_PointInfo.Count > 0)
            {
                foreach(OnePointGroup group in GlobalVar.CADPointList.m_List_PointInfo)
                {
                    foreach(SPoint sp in group.m_ListGroup)
                    {
                        TipPoint tp = new TipPoint();
                        tp.TipPointName = sp.Point_name;
                        tp.Point_X = sp.Pos_X.ToString("0.000") ;
                        tp.Point_Y = sp.Pos_Y.ToString("0.000") ;
                        this.splitContainer_CAD.Panel1.Controls.Add(tp);
                        tp.Dock = DockStyle.Top;
                    }
                }
            }
        }

        private void ICForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //OBJ_DWGDirect.m_dwgdirectServices.Dispose();
            obj_dwg.quit();
        }
    }
}
