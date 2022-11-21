using Microsoft.VisualBasic.Logging;
using System;
using System.Reflection.PortableExecutable;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace GISTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //■■■■■■■■■■■■■■■■■■■■定义变量和常量■■■■■■■■■■■■■■■■■■■■
        //常量
        static double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        static double pi = 3.1415926535897932384626; //π
        static double a = 6378245.0; //长半轴
        static double ee = 0.00669342162296594323; //扁率
        //变量
        static double J, W, CJ;//经度、纬度、中央经线


        //■■■■■■■■■■■■■■■■■■■■主函数■■■■■■■■■■■■■■■■■■■■
        private void button1_Click(object sender, EventArgs e)
        {
            //获取数据
            J = Convert.ToDouble(textBox1.Text);
            W = Convert.ToDouble(textBox2.Text);
            CJ = Convert.ToDouble(textBox3.Text);
            double H = 0.00;
            
            //判断选择哪一种投影坐标系
            String Ttype = PTtype();
            //MessageBox.Show(Ttype);
            //判断是否纠偏
            if (checkBox1.Checked == true)
            {
                //MessageBox.Show("Checked");
                double MJ = gcj02towgs84(J, W)[0]; double MW = gcj02towgs84(J, W)[1];
                //String Write = MJ.ToString()+"\t"+MW.ToString();
                //定义一个数组来接收gcj02towgs84
                double[] array1;
                array1 = GaussProjection(MJ, MW, H, CJ, Ttype);
                string x = array1[0].ToString("#0.0000");
                string y = array1[1].ToString("#0.0000");
                //string x = GaussProjection(MJ, MW, H, CJ, Ttype)[0].ToString("#0.0000");
                //string y = GaussProjection(MJ, MW, H, CJ, Ttype)[1].ToString("#0.0000");
                //string h = GaussProjection(MJ, MW, H, CJ, Ttype)[2].ToString();
                String Write = x + ',' + y;// + '\t';// + h;
                textBox4.Text=Write;
                
            }
            else
            {
                double[] array2;
                array2 = GaussProjection(J, W, H, CJ, Ttype);
                string x = array2[0].ToString("#0.0000");
                string y = array2[1].ToString("#0.0000");
                //string x = GaussProjection(J, W, H, CJ, Ttype)[0].ToString("#0.0000");
                //string y = GaussProjection(J, W, H, CJ, Ttype)[1].ToString("#0.0000");
                //string h = GaussProjection(MJ, MW, H, CJ, Ttype)[2].ToString();
                String Write = x + ',' + y;// + '\t';// + h;
                textBox4.Text = Write;
            }
        }



        //■■■■■■■■■■■■■■■■■■■■GCJ02坐标纠偏■■■■■■■■■■■■■■■■■■■■
        //GCJ02坐标纠偏为WGS84
        public static double[] gcj02towgs84(double J,double W)
        {
            double[] array = {0,0};
            if (out_of_china(J, W))
            {
                array[0]=J;
                array[1]=W;
                return array;
            }
            else
            {
                //魔法纠偏
                double dW = transformJ(J - 105.0, W - 35.0);
                double dJ = transformW(J - 105.0, W - 35.0);
                double radW = W / 180.0 * pi;
                double magic = Math.Sin(radW);
                magic = 1 - ee * magic * magic;
                double sqrtmagic = Math.Sqrt(magic);
                dW = (dW * 180.0) / ((a * (1 - ee)) / (magic * sqrtmagic) * pi);
                dJ = (dJ * 180.0) / (a / sqrtmagic * Math.Cos(radW) * pi);
                double mgW = W + dW;
                double mgJ = J + dJ;
                array[1] = W * 2 - mgW;
                array[0] = J * 2 - mgJ;
                return array;
            }
        }
        //对经度纠偏
        public static double transformJ(double J, double W)
        {
            double TJ;
            TJ = -100.0 + 2.0 * J + 3.0 * W + 0.2 * W * W + 0.1 * J * W + 0.2 * Math.Sqrt(Math.Abs(J));
            TJ += (20.0 * Math.Sin(6.0 * J * pi) + 20.0 * Math.Sin(2.0 * J * pi)) * 2.0 / 3.0;
            TJ += (20.0 * Math.Sin(W * pi) + 40.0 * Math.Sin(W / 3.0 * pi)) * 2.0 / 3.0;
            TJ += (160.0 * Math.Sin(W / 12.0 * pi) + 320 * Math.Sin(W * pi / 30.0)) * 2.0 / 3.0;
            return TJ;
        }
        //对纬度纠偏
        public static double transformW(double J, double W)
        {
            double TW;
            TW = 300.0 + J + 2.0 * W + 0.1 * J * J + 0.1 * J * W + 0.1 * Math.Sqrt(Math.Abs(J));
            TW += (20.0 * Math.Sin(6.0 * J * pi) + 20.0 * Math.Sin(2.0 * J * pi)) * 2.0 / 3.0;
            TW += (20.0 * Math.Sin(J * pi) + 40.0 * Math.Sin(J / 3.0 * pi)) * 2.0 / 3.0;
            TW += (150.0 * Math.Sin(J / 12.0 * pi) + 300.0 * Math.Sin(J / 30.0 * pi)) * 2.0 / 3.0;
            return TW;
        }
        //判断是否在国内，不在国内不纠偏
        public static bool out_of_china(double J, double W)
        {
            bool BT = true;bool BF = false;
            if (((J < 72.004) & (J > 137.8347))&((W < 0.8293) & (W > 55.8271))){
                return BT;
            }
            else
            {
                return BF;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■经纬度转投影坐标■■■■■■■■■■■■■■■■■■■■
        public double[] GaussProjection(double J,double W,double H,double CJ,string Ttype)
        {
            double a, f, b, e, e2, d2r,xs=0.0,ys=0.0,hs=0.0,B=W,L=J;
            double[] array = { xs, ys, hs };
            double[] ifTtypeR ;
            //判断投影坐标类型及基本参数
            ifTtypeR = ifTtype(Ttype);
            //a = ifTtype(Ttype)[0];f = ifTtype(Ttype)[1];
            a = ifTtypeR[0]; f = ifTtypeR[1];
            //MessageBox.Show(a.ToString());
            b = a * (1 - f);
            e = Math.Sqrt(a * a - b * b) / a;  //第一偏心率
            e2 = Math.Sqrt(a * a - b * b) / b;  //第二偏心率
            d2r = Math.PI / 180;

            double L0 = CJ;
            double rho = 180 / Math.PI;  // 单位是度.度
            double l = (L - L0) / rho;  // 公式中的参数 l

            double cB = Math.Cos(B * d2r);
            double sB = Math.Sin(B * d2r);
            double N = a / Math.Sqrt(1 - e * e * sB * sB);  // 卯酉圈曲率半径
            double t = Math.Tan(B * d2r);  //tanB 对应公式中的参数t
            double eta = e2 * cB;  // 对应公式中的参数 eta

            double m0 = a * (1 - e * e);
            double m2 = 3.0 / 2.0 * e * e * m0;
            double m4 = 5.0 / 4.0 * e * e * m2;
            double m6 = 7.0 / 6.0 * e * e * m4;
            double m8 = 9.0 / 8.0 * e * e * m6;

            double a0 = m0 + 1.0 / 2.0 * m2 + 3.0 / 8.0 * m4 + 5.0 / 16.0 * m6 + 35.0 / 128.0 * m8;
            double a2 = 1.0 / 2.0 * m2 + 1.0 / 2.0 * m4 + 15.0 / 32.0 * m6 + 7.0 / 16.0 * m8;
            double a4 = 1.0 / 8.0 * m4 + 3.0 / 16.0 * m6 + 7.0 / 32.0 * m8;
            double a6 = 1.0 / 32.0 * m6 + 1.0 / 16.0 * m8;
            double a8 = 1.0 / 128.0 * m8;

            double s2b = Math.Sin(B * d2r * 2);
            double s4b = Math.Sin(B * d2r * 4);
            double s6b = Math.Sin(B * d2r * 6);
            double s8b = Math.Sin(B * d2r * 8);
            //MessageBox.Show(a.ToString()+ '\t'+f.ToString()+ '\t'+b.ToString() + '\t'+e.ToString() + '\t'+e2.ToString() + '\t'+d2r.ToString() + '\t'+l.ToString() + '\t'+cB.ToString() + '\t'+sB.ToString() + '\t'+N.ToString() + '\t'+t.ToString() + '\t'+eta.ToString() + '\t'+m0.ToString() + '\t'+m2.ToString() + '\t'+m4.ToString() + '\t'+m6.ToString() + '\t'+m8.ToString() + '\t'+a0.ToString() + '\t'+a2.ToString() + '\t'+a4.ToString() + '\t'+a6.ToString() + '\t'+a8.ToString() + '\t'+s2b.ToString() + '\t'+s4b.ToString() + '\t'+s6b.ToString() + '\t'+s8b.ToString() + '\t');

            // X为自赤道量起的子午线弧长
            double X = a0 * (B * d2r) - 1.0 / 2.0 * a2 * s2b + 1.0 / 4.0 * a4 * s4b - 1.0 / 6.0 * a6 * s6b + 1.0 / 8.0 * a8 * s8b;
            // 坐标X
            xs = X + N / 2 * t * cB * cB * l * l + N / 24 * t * (5 - t * t + 9 * Math.Pow(eta, 2) + 4 * Math.Pow(eta, 4)) * Math.Pow(cB,4) * Math.Pow(l, 4)+ N / 720 * t * (61 - 58 * t * t + Math.Pow(t, 4)) * Math.Pow(cB,6) * Math.Pow(l, 6);
            // 坐标Y
            ys = N * cB * l + N / 6 * (1 - t * t + eta * eta) * Math.Pow(cB,3) * Math.Pow(l,3) +N / 120 * (5 - 18 * t * t + Math.Pow(t, 4) + 14 * Math.Pow(eta,2) - 58 * eta * eta * t * t) * Math.Pow(cB, 5) * Math.Pow(l, 5) + 500000;
            // 海拔高度H
            hs = H;


            array[0] = xs;array[1] = ys;array[2] = hs;
            return array;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        //■■■■■■■■■■■■■■■■■■■■判断参考椭球体参数■■■■■■■■■■■■■■■■■■■■
        public double[] ifTtype(string Ttype)
        {
            double a, f;
            double[] array = { 0, 0 };
            // 椭球参数
            // CGCS2000
            if (Ttype == "1")
            {
                a = 6378137.0;
                f = 1 / 298.257222101;
                array[0] = a;
                array[1] = f;
            }
            // 西安80
            else if (Ttype == "2")
            {
                a = 6378140.0;
                f = 1 / 298.257;
                array[0] = a;
                array[1] = f;
            }

            // 北京54
            else if (Ttype == "3")
            {
                a = 6378245.0;
                f = 1 / 298.3;
                array[0] = a;
                array[1] = f;
            }
            // WGS-84
            else if (Ttype == "4")
            {
                a = 6378137.0;
                f = 1 / 298.257223563;
                array[0] = a;
                array[1] = f;
            }
            return array;
        }
        //■■■■■■■■■■■■■■■■■■■■判断投影坐标类型■■■■■■■■■■■■■■■■■■■■
        public String PTtype()
        {
            String Ttype="";
            if (radioButton1.Checked)
            {
                Ttype = "1";
            }
            else if (radioButton2.Checked)
            {
                Ttype = "2";
            }
            else if (radioButton3.Checked)
            {
                Ttype = "3";
            }
            else if (radioButton4.Checked)
            {
                Ttype = "4";
            }
            return Ttype;
        }

    }
}