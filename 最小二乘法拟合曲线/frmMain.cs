using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace 最小二乘法拟合曲线
{
    public partial class frmMain : Form
    {
        DataTable dtXY = new DataTable();
        DataTable dtResult = new DataTable();

        public frmMain()
        {
            InitializeComponent();
            zedGraph.GraphPane.Title = "最小二乘法拟合结果";
            zedGraph.GraphPane.XAxis.Title = "";
            zedGraph.GraphPane.YAxis.Title = "";
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            dtXY.Columns.Add("X", typeof(double));
            dtXY.Columns.Add("Y", typeof(double));
            dtResult.Columns.Add("系数", typeof(string));
            dtResult.Columns.Add("值",typeof(double));

            dgvXY.DataSource = dtXY;
            dgvResult.DataSource = dtResult;
            dgvResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvXY.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            this.Text = "最小二乘法拟合曲线 V" + AssemblyVersion;
            tBarPower.Value = 2;
            tSLTime.Text = DateTime.Now.ToString();
            tSLAuthor.Text = AssemblyCopyright;

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = "文本文件(*.txt)|*.txt";
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = this.openFileDialog1.FileName;

                dtXY.Rows.Clear();
                dtResult.Rows.Clear();
                zedGraph.GraphPane.CurveList.Clear();
                try
                {
                    readData(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            CalculateDraw(tBarPower.Value);
        }



        private DataTable readData(string file)
        {

            StreamReader sr = new StreamReader(file, Encoding.Default);
            string str = sr.ReadLine();
            while (str != null)
            {
                DataRow dr = dtXY.NewRow();
                string[] strs = str.Split('\t');
                try
                {
                    if (strs.Length == 2)
                    {
                        dr["X"] = Convert.ToDouble(strs[0]);
                        dr["Y"] = Convert.ToDouble(strs[1]);
                    }
                    else
                    {
                        throw new Exception("读取文件过程中发生错误！文件格式不正确！");
                    }
                }
                catch
                {
                    throw new Exception("读取文件过程中发生错误！文件中含有非法数据！");
                }
                dtXY.Rows.Add(dr);
                str = sr.ReadLine();
                
            }
 
            return dtXY;
        }

        //dataGridView显示行号
        private void dgvXY_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
            e.RowBounds.Location.Y, dgvXY.RowHeadersWidth - 4, e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dgvXY.RowHeadersDefaultCellStyle.Font, rectangle,
                dgvXY.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }
        private void dgvResult_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
            e.RowBounds.Location.Y, dgvResult.RowHeadersWidth - 4, e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dgvResult.RowHeadersDefaultCellStyle.Font, rectangle,
                dgvResult.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void btnLS_Click(object sender, EventArgs e)
        {
            CalculateDraw(tBarPower.Value);
        }

        private void CalculateDraw(int power)
        {
            if (dtXY.Rows.Count == 0) return;
            double[] X = new double[dtXY.Rows.Count];
            double[] Y = new double[dtXY.Rows.Count];
            for (int i = 0; i < dtXY.Rows.Count; i++)
            {
                X[i] = Convert.ToDouble(dtXY.Rows[i]["X"]);
                Y[i] = Convert.ToDouble(dtXY.Rows[i]["Y"]);

            }
            double[] args = LeastSquares.MultiLine(X, Y, dtXY.Rows.Count, power);

            dtResult.Rows.Clear();
            for (int i = 0; i < args.Length; i++)
            {
                DataRow dr = dtResult.NewRow();
                dr["系数"] = ((char)('a' + i)).ToString();
                dr["值"] = args[i];
                dtResult.Rows.Add(dr);
            }

            #region 画图
            zedGraph.GraphPane.CurveList.Clear();
            zedGraph.IsShowPointValues = true;
            zedGraph.GraphPane.Title = "最小二乘法拟合结果";
            zedGraph.GraphPane.XAxis.Title = "";
            zedGraph.GraphPane.YAxis.Title = "";

            LineItem curve = zedGraph.GraphPane.AddCurve("数据点", X, Y, Color.Red, SymbolType.Circle);
            curve.Line.IsVisible = false;

            double[] Xs = new double[1000];
            double[] Ys = new double[1000];
            Xs[0] = X[0];
            Ys[0] = getYvalue(Xs[0], args);
            double step = (max(X) - min(X)) / 1000;
            for (int i = 1; i < 1000; i++)
            {
                Xs[i] = Xs[i - 1] + step;
                Ys[i] = getYvalue(Xs[i], args);
            }

            zedGraph.GraphPane.AddCurve("拟合曲线", Xs, Ys, Color.Blue, SymbolType.None);
            zedGraph.AxisChange();
            zedGraph.Invalidate();
            #endregion
        }



        private void btnClearData_Click(object sender, EventArgs e)
        {
            dtXY.Rows.Clear();
            dtResult.Rows.Clear();
            zedGraph.GraphPane.CurveList.Clear();
            zedGraph.Invalidate();
        }

        private void btnClearVisual_Click(object sender, EventArgs e)
        {
            zedGraph.GraphPane.CurveList.Clear();
            zedGraph.Invalidate();
        }

        private double max(double[] arr)
        {
            double maxValue = arr[0];
            for (int i = 0; i < arr.Length; i++)
            {
                maxValue = maxValue > arr[i] ? maxValue : arr[i];
            }
            return maxValue;
        }

        private double min(double[] arr)
        {
            double minValue = arr[0];
            for (int i = 0; i < arr.Length; i++)
            {
                minValue = minValue < arr[i] ? minValue : arr[i];
            }
            return minValue;
        }

        private double getYvalue(double x, double[] args)
        {
            double y = args[0];
            double xPower = 1;
            for (int i = 1; i < args.Length; i++)
            {
                xPower *= x;
                y += args[i] * xPower;
            }
            return y;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            labPower.Text = "拟合多项式最高次数:" + tBarPower.Value;
            CalculateDraw(tBarPower.Value);
        }

        #region 状态栏部分
        private void timer1_Tick(object sender, EventArgs e)
        {
            tSLTime.Text = DateTime.Now.ToString();
        }

        private void btnLoad_MouseHover(object sender, EventArgs e)
        {
            tSLAction.Text = "导入数据：导入txt型数据";
        }

        private void btnClearData_MouseHover(object sender, EventArgs e)
        {
            tSLAction.Text = "清除数据：清除当前数据";
        }

        private void btnClearVisual_MouseHover(object sender, EventArgs e)
        {
            tSLAction.Text = "清除显示：清除当前显示";
        }

        private void tBarPower_MouseHover(object sender, EventArgs e)
        {
            tSLAction.Text = "拟合次数：滑动可调整拟合曲线最高次数";
        }

        private void btnLoad_MouseLeave(object sender, EventArgs e)
        {
            tSLAction.Text = "";
        }

        private void btnClearData_MouseLeave(object sender, EventArgs e)
        {
            tSLAction.Text = "";
        }

        private void btnClearVisual_MouseLeave(object sender, EventArgs e)
        {
            tSLAction.Text = "";
        }

        private void tBarPower_MouseLeave(object sender, EventArgs e)
        {
            tSLAction.Text = "";
        }

        private void dgvResult_MouseHover(object sender, EventArgs e)
        {
            if (dgvResult.RowCount > 0)
            {
                tSLAction.Text = "拟合结果：拟合结果的各项系数，其中a表示常数项";
            }
        }

        private void dgvResult_MouseLeave(object sender, EventArgs e)
        {
            tSLAction.Text = "";
        }

        private void zedGraph_MouseHover(object sender, EventArgs e)
        {
            if (dgvResult.RowCount > 0)
            {
                tSLAction.Text = "拟合结果：显示原始点数据和拟合后的曲线";
            }
        }

        private void zedGraph_MouseLeave(object sender, EventArgs e)
        {
            tSLAction.Text = "";
        }


        #endregion

        #region 程序集属性访问器

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion
        







    }
}
