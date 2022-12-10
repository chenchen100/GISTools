using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
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
        static double pi = 3.1415926535897932384626; //π
        static double a = 6378245.0; //长半轴
        static double ee = 0.00669342162296594323; //扁率
        //变量
        static double J, W, CJ;//经度、纬度、中央经线


        //■■■■■■■■■■■■■■■■■■■■转换按钮事件■■■■■■■■■■■■■■■■■■■■
        private void button1_Click(object sender, EventArgs e)
        {
            ////定义一个数组来接收transDegree
            double[] arrayBC = { 0, 0 };

            //判断并转换经纬度为度形式
            arrayBC = transDegree();
            J = arrayBC[0];
            W = arrayBC[1];
            if (J == 0)
            {
                return;
            }
            else
            {
                if (textBox3.Text.Length != 0)
                {
                    CJ = Convert.ToDouble(textBox3.Text);
                }
                else
                {
                    MessageBox.Show("         未选择中央经线！请重新选择或输入         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                double H = 0.00;

                //判断选择哪一种投影坐标系
                String Ttype = PTtype();
                //MessageBox.Show(Ttype);

                //判断是否纠偏
                if (checkBox1.Checked == true)
                {
                    //MessageBox.Show("Checked1");
                    //GCJ02坐标纠偏
                    double MJ = gcj02towgs84(J, W)[0]; double MW = gcj02towgs84(J, W)[1];
                    //String Write1 = MJ.ToString()+"\t"+MW.ToString();
                    //MessageBox.Show(Write1);
                    //定义一个数组来接收gcj02towgs84
                    double[] array1;
                    array1 = GaussProjection(MJ, MW, H, CJ, Ttype);
                    string x = array1[0].ToString("#0.0000");
                    string y = array1[1].ToString("#0.0000");
                    //string h = array1[2].ToString("#0.0000");
                    String Write = x + ',' + y;// + '\t';// + h;
                    textBox4.Text = Write;

                }
                else
                {
                    //MessageBox.Show("Checked2");
                    double[] array2;
                    array2 = GaussProjection(J, W, H, CJ, Ttype);
                    string x = array2[0].ToString("#0.0000");
                    string y = array2[1].ToString("#0.0000");
                    //string h = array1[2].ToString("#0.0000");
                    String Write = x + ',' + y;// + '\t';// + h;
                    textBox4.Text = Write;
                }
            }

        }
        //■■■■■■■■■■■■■■■■■■■■经纬度帮助信息■■■■■■■■■■■■■■■■■■■■
        string StartupPath = System.Windows.Forms.Application.StartupPath;
        private void button2_Click(object sender, EventArgs e)
        {
            string HelpPATH = StartupPath + "src/help/" + "LongitudeAndLatitudeHELP.html";

            MessageBox.Show("支持的格式有:\t1. 114.10\t\t\n2. 114.10°\t\n3. 114°06′00″\t\n4. 114°06′00\t", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign, HelpPATH);
        }
        //■■■■■■■■■■■■■■■■■■■■中央经线帮助信息■■■■■■■■■■■■■■■■■■■■
        private void button3_Click(object sender, EventArgs e)
        {
            string HelpPATH = StartupPath + "src/help/" + "ProjectionZoningHELP.html";
            MessageBox.Show("高斯－克吕格投影分带规定：GK投影是国家基本比例尺地形图\n的数学基础，为控制变形，采用分带投影的方法：\t\t\n1. 在比例尺1：2.5万—1：50万图上采用6°分带\t\n2. 在比例尺为1：1万及大于1：1万的图采用3°分带\t\n3. 根据实际需要使用自定义分带\t\t", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
               MessageBoxOptions.RightAlign, HelpPATH);
        }
        //■■■■■■■■■■■■■■■■■■■■GCJ02地理坐标纠偏帮助信息■■■■■■■■■■■■■■■■■■■
        private void button4_Click(object sender, EventArgs e)
        {
            string HelpPATH = StartupPath + "src/help/" + "GCJ02CoordinateCorrectionHELP.html";
            MessageBox.Show("GCJ-02是由中国国家测绘局（G表示国家，C表示测绘，\t\nJ表示局）制订的地理信息系统的坐标系统。\t\t\n中文名:国家测量局02号标准\t\t\t\t\n外文名:GCJ-02\t\t\t\t\t\n它是一种对经纬度数据的加密算法，即加入随机的偏差。\t\n国内出版的各种地图系统（包括电子形式），\t\t\n必须至少采用GCJ-02对地理位置进行首次加密。\t\t",
                "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
               MessageBoxOptions.RightAlign, HelpPATH);
        }

        //■■■■■■■■■■■■■■■■■■■■判断并转换经纬度为度形式■■■■■■■■■■■■■■■■■■■■
        private double[] transDegree()
        {
            // 度分秒化为 度.度
            //    114°30′30″  29°30′30″
            double[] arrayD = { 0, 0 };
            string SJ = textBox1.Text; //经度
            string SW = textBox2.Text; //纬度
            try
            {
                if ((SJ.Length == 0) | (SW.Length == 0))
                {
                    MessageBox.Show("         经纬度格式错误或未输入，请输入正确的经纬度         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Clear();
                    textBox2.Clear();
                    return arrayD;
                }
                else
                {
                    double DJ = AnytoDD(SJ);
                    double DW = AnytoDD(SW);
                    if (out_of_china(DJ, DW))
                    {
                        arrayD[0] = DJ; arrayD[1] = DW;
                    }
                    else
                    {
                        MessageBox.Show("         不在中国范围内！请重新输入         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Clear();
                        textBox2.Clear();
                    }
                    /*
                    try
                    {
                        if (!SJ.Contains("."))
                        {
                            double degree_J = Convert.ToDouble(SJ.Split("°")[0]);
                            double minute_J = Convert.ToDouble(SJ.Split("°")[1].Split("′")[0]);
                            double second_J = Convert.ToDouble(SJ.Split("°")[1].Split("′")[1].Split("″")[0]);
                            double degree_W = Convert.ToDouble(SW.Split("°")[0]);
                            double minute_W = Convert.ToDouble(SW.Split("°")[1].Split("′")[0]);
                            double second_W = Convert.ToDouble(SW.Split("°")[1].Split("′")[1].Split("″")[0]);
                            double DJ = (degree_J + minute_J / 60.0 + second_J / 3600.0);
                            double DW = (degree_W + minute_W / 60.0 + second_W / 3600.0);
                            if (out_of_china(DJ, DW))
                            {
                                arrayD[0] = DJ; arrayD[1] = DW;
                            }
                            else
                            {
                                MessageBox.Show("         不在中国范围内！请重新输入         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox1.Clear();
                                textBox2.Clear();
                            }
                        }
                        else if (SJ.Contains("."))
                        {
                            double DFMJ = Convert.ToDouble(SJ.Split("°")[0]); double DFMW = Convert.ToDouble(SW.Split("°")[0]);
                            if (out_of_china(DFMJ, DFMW))
                            {
                                arrayD[0] = DFMJ; arrayD[1] = DFMW;
                            }
                            else
                            {
                                MessageBox.Show("         不在中国范围内！请重新输入         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox1.Clear();
                                textBox2.Clear();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("         经纬度格式错误！请重新输入         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                    }
                    */
                }
            }
            catch
            {
                MessageBox.Show("         经纬度格式错误或未输入，请输入正确的经纬度         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Clear();
                textBox2.Clear();
                return arrayD;
            }
            return arrayD;
        }




        //■■■■■■■■■■■■■■■■■■■■判断并计算分度带■■■■■■■■■■■■■■■■■■■■
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string combobox1_value = this.comboBox1.Text;
            double[] arrayCom = { 0, 0 };
            //MessageBox.Show(combobox1_value);
            try
            {
                //判断并转换经纬度为度形式
                arrayCom = transDegree();
                J = arrayCom[0];
                W = arrayCom[1];
                //MessageBox.Show(J.ToString());
                //判断是否在国内，不在国内不纠偏，在国内则返回ture
                if (out_of_china(J, W))
                {
                    if (combobox1_value == "六度分带")
                    {
                        //MessageBox.Show(combobox1_value);
                        double CJ6 = ((int)(((int)(J) / 6)) + 1) * 6 - 3;
                        String Write6 = CJ6.ToString();
                        textBox3.Text = Write6;
                    }
                    else if (combobox1_value == "三度分带")
                    {
                        //MessageBox.Show(combobox1_value);
                        double CJ3 = 3 * (int)((J + 1.5) / 3);
                        String Write3 = CJ3.ToString();
                        textBox3.Text = Write3;
                    }
                    else if (combobox1_value == "自定义分带")
                    {
                        //MessageBox.Show(combobox1_value);
                        textBox3.Clear();
                    }
                }
                else
                {
                    //MessageBox.Show("         不在中国范围内！请重新输入         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("         经纬度格式错误或未输入，请输入正确的经纬度         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        //■■■■■■■■■■■■■■■■■■■■GCJ02坐标纠偏■■■■■■■■■■■■■■■■■■■■
        //GCJ02坐标纠偏为WGS84
        public static double[] gcj02towgs84(double J, double W)
        {
            double[] array = { 0, 0 };
            if (!out_of_china(J, W))
            {
                array[0] = J;
                array[1] = W;
                return array;
            }
            else
            {
                //魔法纠偏，对非线性加偏的逼近
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
        //判断是否在国内，不在国内不纠偏，在国内则返回ture
        public static bool out_of_china(double J, double W)
        {
            bool BT = true; bool BF = false;
            if (((J > 72.004) & (J < 137.8347)) & ((W > 0.8293) & (W < 55.8271))) {
                return BT;
            }
            else
            {
                return BF;
            }
        }
        //■■■■■■■■■■■■■■■■■■■■WGS84坐标加偏为gcj02■■■■■■■■■■■■■■■■■■■■
        //WGS84坐标加偏为gcj02
        public double[] wgs84togcj02(double J, double W)
        {
            double[] array = new double[2];
            if (!out_of_china(J, W))
            {
                array[0] = J;
                array[1] = W;
                return array;
            }
            else
            {
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
                array[1] = mgW;
                array[0] = mgJ;
                return array;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■gcj02转bd09■■■■■■■■■■■■■■■■■■■■
        //gcj02转bd09
        public double[] gcj02tobd09(double J, double W)
        {
            double[] array = new double[2];
            if (!out_of_china(J, W))
            {
                array[0] = J;
                array[1] = W;
                return array;
            }
            else
            {
                double a = Math.Sqrt(J * J + W * W) + 0.00002 * Math.Sin(W * Math.PI);
                double b = Math.Atan2(W,J)+0.000003*Math.Cos(J*Math.PI);
                array[0] = a * Math.Cos(b) + 0.0065;
                array[1] = a * Math.Sin(b) + 0.006;
                return array;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■bd09转gcj02■■■■■■■■■■■■■■■■■■■■
        //bd09转gcj02
        public double[] bd09togcj02(double J, double W)
        {
            double[] array = new double[2];
            if (!out_of_china(J, W))
            {
                array[0] = J;
                array[1] = W;
                return array;
            }
            else
            {
                double a = J - 0.0065;
                double b = W - 0.006;
                double c = Math.Sqrt(a*a + b*b)-0.00002*Math.Sin(b*Math.PI);
                double d = Math.Atan2(b,a)-0.000003*Math.Cos(a*Math.PI);
                array[0] = c * Math.Cos(d);
                array[1] = c * Math.Sin(d);
                return array;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■wgs84转bd09■■■■■■■■■■■■■■■■■■■■
        //wgs84转bd09
        public double[] wgs84tobd09(double J, double W)
        {
            double[] array1 = new double[2];
            double[] array2 = new double[2];
            array1 =wgs84togcj02(J, W);
            array2 = gcj02tobd09(array1[0], array1[1]);
            return array2;
        }

        //■■■■■■■■■■■■■■■■■■■■bd09转wgs84■■■■■■■■■■■■■■■■■■■■
        //bd09转wgs84
        public double[] bd09towgs84(double J, double W)
        {
            double[] array1 = new double[2];
            double[] array2 = new double[2];
            array1 = bd09togcj02(J, W);
            array2 = gcj02towgs84(array1[0], array1[1]);
            return array2;
        }

        //■■■■■■■■■■■■■■■■■■■■经纬度转投影坐标■■■■■■■■■■■■■■■■■■■■
        public double[] GaussProjection(double J, double W, double H, double CJ, string Ttype)
        {
            double a, f, b, e, e2, d2r, xs = 0.0, ys = 0.0, hs = 0.0, B = W, L = J;
            double[] array = { xs, ys, hs };
            double[] ifTtypeR;

            //判断投影坐标类型及基本参数
            ifTtypeR = ifTtype(Ttype);
            //a = ifTtype(Ttype)[0];f = ifTtype(Ttype)[1];
            a = ifTtypeR[0]; //地球长半轴
            f = ifTtypeR[1]; //地球扁率α
            //MessageBox.Show(a.ToString());
            b = a * (1 - f); //地球短半轴
            e = Math.Sqrt(a * a - b * b) / a;  //第一偏心率
            e2 = Math.Sqrt(a * a - b * b) / b;  //第二偏心率
            d2r = Math.PI / 180; //D2R   度转弧度 Degrees to Radians

            double L0 = CJ; //中央经线
            double rho = 180 / Math.PI;  //   ρ 单位是度.度
            double l = (L - L0) / rho;  // 参数 l,L0为中央经线
            double cosB = Math.Cos(B * d2r); //cos(B)
            double sinB = Math.Sin(B * d2r); //sin(B)
            double N = a / Math.Sqrt(1 - e * e * sinB * sinB);  // 卯酉圈曲率半径
            double t = Math.Tan(B * d2r);  //tanB 对参数t
            double eta = e2 * cosB;  // 参数 η

            //M为子午圈曲率半径，M按照牛顿二项式定理展开级数，取至8次项，则有：
            double m0 = a * (1 - e * e);
            double m2 = 3.0 / 2.0 * e * e * m0;
            double m4 = 5.0 / 4.0 * e * e * m2;
            double m6 = 7.0 / 6.0 * e * e * m4;
            double m8 = 9.0 / 8.0 * e * e * m6;

            //将正弦的幂函数展开为余弦的倍数函数：
            double a0 = m0 + 1.0 / 2.0 * m2 + 3.0 / 8.0 * m4 + 5.0 / 16.0 * m6 + 35.0 / 128.0 * m8;
            double a2 = 1.0 / 2.0 * m2 + 1.0 / 2.0 * m4 + 15.0 / 32.0 * m6 + 7.0 / 16.0 * m8;
            double a4 = 1.0 / 8.0 * m4 + 3.0 / 16.0 * m6 + 7.0 / 32.0 * m8;
            double a6 = 1.0 / 32.0 * m6 + 1.0 / 16.0 * m8;
            double a8 = 1.0 / 128.0 * m8;

            // X为自赤道量起的子午线弧长
            double s2b = Math.Sin(B * d2r * 2);
            double s4b = Math.Sin(B * d2r * 4);
            double s6b = Math.Sin(B * d2r * 6);
            double s8b = Math.Sin(B * d2r * 8);
            double X = a0 * (B * d2r) - 1.0 / 2.0 * a2 * s2b + 1.0 / 4.0 * a4 * s4b - 1.0 / 6.0 * a6 * s6b + 1.0 / 8.0 * a8 * s8b;

            //MessageBox.Show(a.ToString()+ '\t'+f.ToString()+ '\t'+b.ToString() + '\t'+e.ToString() + '\t'+e2.ToString() + '\t'+d2r.ToString() + '\t'+l.ToString() + '\t'+cosB.ToString() + '\t'+sinB.ToString() + '\t'+N.ToString() + '\t'+t.ToString() + '\t'+eta.ToString() + '\t'+m0.ToString() + '\t'+m2.ToString() + '\t'+m4.ToString() + '\t'+m6.ToString() + '\t'+m8.ToString() + '\t'+a0.ToString() + '\t'+a2.ToString() + '\t'+a4.ToString() + '\t'+a6.ToString() + '\t'+a8.ToString() + '\t'+s2b.ToString() + '\t'+s4b.ToString() + '\t'+s6b.ToString() + '\t'+s8b.ToString() + '\t');

            // 坐标X
            xs = X + N / 2 * t * cosB * cosB * l * l + N / 24 * t * (5 - t * t + 9 * Math.Pow(eta, 2) + 4 * Math.Pow(eta, 4)) * Math.Pow(cosB, 4) * Math.Pow(l, 4) + N / 720 * t * (61 - 58 * t * t + Math.Pow(t, 4)) * Math.Pow(cosB, 6) * Math.Pow(l, 6);
            // 坐标Y
            ys = N * cosB * l + N / 6 * (1 - t * t + eta * eta) * Math.Pow(cosB, 3) * Math.Pow(l, 3) + N / 120 * (5 - 18 * t * t + Math.Pow(t, 4) + 14 * Math.Pow(eta, 2) - 58 * eta * eta * t * t) * Math.Pow(cosB, 5) * Math.Pow(l, 5) + 500000;
            // 海拔高度H，并未参与计算
            hs = H;


            array[0] = xs; array[1] = ys; array[2] = hs;
            return array;
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
            // Xian80
            else if (Ttype == "2")
            {
                a = 6378140.0;
                f = 1 / 298.257;
                array[0] = a;
                array[1] = f;
            }

            // Beijing54
            else if (Ttype == "3")
            {
                a = 6378245.0;
                f = 1 / 298.3;
                array[0] = a;
                array[1] = f;
            }
            // WGS84
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
            String Ttype = "";
            if (radioButton1.Checked)
            {
                // CGCS2000
                Ttype = "1";
            }
            else if (radioButton2.Checked)
            {
                // WGS84
                Ttype = "4";
            }
            else if (radioButton3.Checked)
            {
                // Xian80
                Ttype = "2";
            }
            else if (radioButton4.Checked)
            {
                // Beijing54
                Ttype = "3";
            }
            return Ttype;
        }



        //■■■■■■■■■■■■■■■■■■■■“任何经纬度格式”转为度.度形式■■■■■■■■■■■■■■■■■■■■
        public Double AnytoDD(string input){
            double DD=0.0;
            if (input.Contains("°")) {
                // 113.10° or 113°10′10.0″
                if (input.Contains("."))
                {
                    // 113.10° or 113°10′10.0″
                    if (input.IndexOf(".") < input.IndexOf("°"))
                    {
                        // 113.10°
                        DD = Convert.ToDouble(input.Split('°')[0]);
                    }
                    else
                    {
                        // 113°10′10.0″ or 113°10′ or 113°10
                        double D = Convert.ToDouble(input.Split('°')[0]);
                        if (input.Split('°')[1].Contains("′"))
                        {
                            // 113°10′ or 113°10′10.0″
                            double M = Convert.ToDouble(input.Split('°')[1].Split("′")[0]);
                            if (input.Split('°')[1].Split("′")[1].Contains("″"))
                            {
                                // 113°10′10.0″
                                double S = Convert.ToDouble(input.Split('°')[1].Split("′")[1].Split('″')[0]);
                                DD = D + M / 60.0 + S / 3600.0;
                            }
                            else
                            {
                                // 113°10′
                                DD = D + M / 60.0;
                            }
                        }
                        else
                        {
                            // 113°10
                            double M = Convert.ToDouble(input.Split('°')[1]);
                            DD = D + M / 60.0;
                        }
                    }
                }
                else
                {
                    // 113°10′10″ or 113°10′10 or 113°10′ or 113°10 or 113°
                    double D = Convert.ToDouble(input.Split('°')[0]);
                    if ((input.Split('°')[1]).Length == 0)
                    {
                        // 113°
                        DD = D;
                    }
                    else
                    {
                        // 113°10′10″ or 113°10′10 or 113°10′ or 113°10
                        if (input.Split('°')[1].Contains("′"))
                        {
                            // 113°10′10″ or 113°10′10 or 113°10′
                            double M = Convert.ToDouble(input.Split('°')[1].Split("′")[0]);
                            if ((input.Split('°')[1].Split("′")[1]).Length == 0)
                            {
                                // 113°10′
                                DD = D + M / 60.0;
                            }
                            else
                            {
                                // 113°10′10″ or 113°10′10
                                if (input.Split('°')[1].Split("′")[1].Contains("″"))
                                {
                                    // 113°10′10″
                                    double S = Convert.ToDouble(input.Split('°')[1].Split("′")[1].Split('″')[0]);
                                    DD = D + M / 60.0 + S / 3600.0;
                                }
                                else
                                {
                                    // 13°10′10
                                    double S = Convert.ToDouble(input.Split('°')[1].Split("′")[1]);
                                    DD = D + M / 60.0 + S / 3600.0;
                                }
                            }
                        }
                        else
                        {
                            // 113°10
                            double M = Convert.ToDouble(input.Split('°')[1]);
                            DD = D + M / 60.0;
                        }
                    }
                }
            }
            else
            {
                // 113.10 or 113
                DD = Convert.ToDouble(input);
            }
            return DD;
        }



        //■■■■■■■■■■■■■■■■■■■■开源协议■■■■■■■■■■■■■■■■■■■■
        private void button5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://opensource.org/licenses");
        }
        //■■■■■■■■■■■■■■■■■■■■Github■■■■■■■■■■■■■■■■■■■■
        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/chenchen100/GISTools");
        }
        //■■■■■■■■■■■■■■■■■■■■捐赠■■■■■■■■■■■■■■■■■■■■
        private void button7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://gitee.com/chenchen1001/GISTools");
        }
        //■■■■■■■■■■■■■■■■■■■■博客讨论、反馈■■■■■■■■■■■■■■■■■■■■
        private void button8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://chenchen1001.gitee.io/2022/103050527/");
        }
    }
} 