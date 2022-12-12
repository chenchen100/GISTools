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
        //������������������������������������������������ͳ�������������������������������������������
        //����
        static double pi = 3.1415926535897932384626; //��
        static double a = 6378245.0; //������
        static double ee = 0.00669342162296594323; //����
        //����
        static double J, W, CJ;//���ȡ�γ�ȡ����뾭��


        //����������������������������������������ת����ť�¼�����������������������������������������
        private void button1_Click(object sender, EventArgs e)
        {
            ////����һ������������transDegree
            double[] arrayBC = { 0, 0 };

            //�жϲ�ת����γ��Ϊ����ʽ
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
                    MessageBox.Show("         δѡ�����뾭�ߣ�������ѡ�������         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                double H = 0.00;

                //�ж�ѡ����һ��ͶӰ����ϵ
                String Ttype = PTtype();
                //MessageBox.Show(Ttype);

                //�ж��Ƿ��ƫ
                if (checkBox1.Checked == true)
                {
                    //MessageBox.Show("Checked1");
                    //GCJ02�����ƫ
                    double MJ = gcj02towgs84(J, W)[0]; double MW = gcj02towgs84(J, W)[1];
                    //String Write1 = MJ.ToString()+"\t"+MW.ToString();
                    //MessageBox.Show(Write1);
                    //����һ������������gcj02towgs84
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
        //������������������������������������������γ�Ȱ�����Ϣ����������������������������������������
        string StartupPath = System.Windows.Forms.Application.StartupPath;
        private void button2_Click(object sender, EventArgs e)
        {
            string HelpPATH = StartupPath + "src/help/" + "LongitudeAndLatitudeHELP.html";

            MessageBox.Show("֧�ֵĸ�ʽ��:\t1. 114.10\t\t\n2. 114.10��\t\n3. 114��06��00��\t\n4. 114��06��00\t", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign, HelpPATH);
        }
        //�������������������������������������������뾭�߰�����Ϣ����������������������������������������
        private void button3_Click(object sender, EventArgs e)
        {
            string HelpPATH = StartupPath + "src/help/" + "ProjectionZoningHELP.html";
            MessageBox.Show("��˹��������ͶӰ�ִ��涨��GKͶӰ�ǹ��һ��������ߵ���ͼ\n����ѧ������Ϊ���Ʊ��Σ����÷ִ�ͶӰ�ķ�����\t\t\n1. �ڱ�����1��2.5��1��50��ͼ�ϲ���6��ִ�\t\n2. �ڱ�����Ϊ1��1�򼰴���1��1���ͼ����3��ִ�\t\n3. ����ʵ����Ҫʹ���Զ���ִ�\t\t", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
               MessageBoxOptions.RightAlign, HelpPATH);
        }
        //����������������������������������������GCJ02���������ƫ������Ϣ��������������������������������������
        private void button4_Click(object sender, EventArgs e)
        {
            string HelpPATH = StartupPath + "src/help/" + "GCJ02CoordinateCorrectionHELP.html";
            MessageBox.Show("GCJ-02�����й����Ҳ��֣�G��ʾ���ң�C��ʾ��棬\t\nJ��ʾ�֣��ƶ��ĵ�����Ϣϵͳ������ϵͳ��\t\t\n������:���Ҳ�����02�ű�׼\t\t\t\t\n������:GCJ-02\t\t\t\t\t\n����һ�ֶԾ�γ�����ݵļ����㷨�������������ƫ�\t\n���ڳ���ĸ��ֵ�ͼϵͳ������������ʽ����\t\t\n�������ٲ���GCJ-02�Ե���λ�ý����״μ��ܡ�\t\t",
                "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
               MessageBoxOptions.RightAlign, HelpPATH);
        }

        //�����������������������������������������жϲ�ת����γ��Ϊ����ʽ����������������������������������������
        private double[] transDegree()
        {
            // �ȷ��뻯Ϊ ��.��
            //    114��30��30��  29��30��30��
            double[] arrayD = { 0, 0 };
            string SJ = textBox1.Text; //����
            string SW = textBox2.Text; //γ��
            try
            {
                if ((SJ.Length == 0) | (SW.Length == 0))
                {
                    MessageBox.Show("         ��γ�ȸ�ʽ�����δ���룬��������ȷ�ľ�γ��         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show("         �����й���Χ�ڣ�����������         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Clear();
                        textBox2.Clear();
                    }
                    /*
                    try
                    {
                        if (!SJ.Contains("."))
                        {
                            double degree_J = Convert.ToDouble(SJ.Split("��")[0]);
                            double minute_J = Convert.ToDouble(SJ.Split("��")[1].Split("��")[0]);
                            double second_J = Convert.ToDouble(SJ.Split("��")[1].Split("��")[1].Split("��")[0]);
                            double degree_W = Convert.ToDouble(SW.Split("��")[0]);
                            double minute_W = Convert.ToDouble(SW.Split("��")[1].Split("��")[0]);
                            double second_W = Convert.ToDouble(SW.Split("��")[1].Split("��")[1].Split("��")[0]);
                            double DJ = (degree_J + minute_J / 60.0 + second_J / 3600.0);
                            double DW = (degree_W + minute_W / 60.0 + second_W / 3600.0);
                            if (out_of_china(DJ, DW))
                            {
                                arrayD[0] = DJ; arrayD[1] = DW;
                            }
                            else
                            {
                                MessageBox.Show("         �����й���Χ�ڣ�����������         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox1.Clear();
                                textBox2.Clear();
                            }
                        }
                        else if (SJ.Contains("."))
                        {
                            double DFMJ = Convert.ToDouble(SJ.Split("��")[0]); double DFMW = Convert.ToDouble(SW.Split("��")[0]);
                            if (out_of_china(DFMJ, DFMW))
                            {
                                arrayD[0] = DFMJ; arrayD[1] = DFMW;
                            }
                            else
                            {
                                MessageBox.Show("         �����й���Χ�ڣ�����������         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox1.Clear();
                                textBox2.Clear();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("         ��γ�ȸ�ʽ��������������         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                    }
                    */
                }
            }
            catch
            {
                MessageBox.Show("         ��γ�ȸ�ʽ�����δ���룬��������ȷ�ľ�γ��         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Clear();
                textBox2.Clear();
                return arrayD;
            }
            return arrayD;
        }




        //�����������������������������������������жϲ�����ֶȴ�����������������������������������������
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string combobox1_value = this.comboBox1.Text;
            double[] arrayCom = { 0, 0 };
            //MessageBox.Show(combobox1_value);
            try
            {
                //�жϲ�ת����γ��Ϊ����ʽ
                arrayCom = transDegree();
                J = arrayCom[0];
                W = arrayCom[1];
                //MessageBox.Show(J.ToString());
                //�ж��Ƿ��ڹ��ڣ����ڹ��ڲ���ƫ���ڹ����򷵻�ture
                if (out_of_china(J, W))
                {
                    if (combobox1_value == "���ȷִ�")
                    {
                        //MessageBox.Show(combobox1_value);
                        double CJ6 = ((int)(((int)(J) / 6)) + 1) * 6 - 3;
                        String Write6 = CJ6.ToString();
                        textBox3.Text = Write6;
                    }
                    else if (combobox1_value == "���ȷִ�")
                    {
                        //MessageBox.Show(combobox1_value);
                        double CJ3 = 3 * (int)((J + 1.5) / 3);
                        String Write3 = CJ3.ToString();
                        textBox3.Text = Write3;
                    }
                    else if (combobox1_value == "�Զ���ִ�")
                    {
                        //MessageBox.Show(combobox1_value);
                        textBox3.Clear();
                    }
                }
                else
                {
                    //MessageBox.Show("         �����й���Χ�ڣ�����������         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("         ��γ�ȸ�ʽ�����δ���룬��������ȷ�ľ�γ��         ", "GISToolsError", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        //����������������������������������������GCJ02�����ƫ����������������������������������������
        //GCJ02�����ƫΪWGS84
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
                //ħ����ƫ���Է����Լ�ƫ�ıƽ�
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
        //�Ծ��Ⱦ�ƫ
        public static double transformJ(double J, double W)
        {
            double TJ;
            TJ = -100.0 + 2.0 * J + 3.0 * W + 0.2 * W * W + 0.1 * J * W + 0.2 * Math.Sqrt(Math.Abs(J));
            TJ += (20.0 * Math.Sin(6.0 * J * pi) + 20.0 * Math.Sin(2.0 * J * pi)) * 2.0 / 3.0;
            TJ += (20.0 * Math.Sin(W * pi) + 40.0 * Math.Sin(W / 3.0 * pi)) * 2.0 / 3.0;
            TJ += (160.0 * Math.Sin(W / 12.0 * pi) + 320 * Math.Sin(W * pi / 30.0)) * 2.0 / 3.0;
            return TJ;
        }
        //��γ�Ⱦ�ƫ
        public static double transformW(double J, double W)
        {
            double TW;
            TW = 300.0 + J + 2.0 * W + 0.1 * J * J + 0.1 * J * W + 0.1 * Math.Sqrt(Math.Abs(J));
            TW += (20.0 * Math.Sin(6.0 * J * pi) + 20.0 * Math.Sin(2.0 * J * pi)) * 2.0 / 3.0;
            TW += (20.0 * Math.Sin(J * pi) + 40.0 * Math.Sin(J / 3.0 * pi)) * 2.0 / 3.0;
            TW += (150.0 * Math.Sin(J / 12.0 * pi) + 300.0 * Math.Sin(J / 30.0 * pi)) * 2.0 / 3.0;
            return TW;
        }
        //�ж��Ƿ��ڹ��ڣ����ڹ��ڲ���ƫ���ڹ����򷵻�ture
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
        //����������������������������������������WGS84�����ƫΪgcj02����������������������������������������
        //WGS84�����ƫΪgcj02
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

        //����������������������������������������gcj02תbd09����������������������������������������
        //gcj02תbd09
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

        //����������������������������������������bd09תgcj02����������������������������������������
        //bd09תgcj02
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

        //����������������������������������������wgs84תbd09����������������������������������������
        //wgs84תbd09
        public double[] wgs84tobd09(double J, double W)
        {
            double[] array1 = new double[2];
            double[] array2 = new double[2];
            array1 =wgs84togcj02(J, W);
            array2 = gcj02tobd09(array1[0], array1[1]);
            return array2;
        }

        //����������������������������������������bd09תwgs84����������������������������������������
        //bd09תwgs84
        public double[] bd09towgs84(double J, double W)
        {
            double[] array1 = new double[2];
            double[] array2 = new double[2];
            array1 = bd09togcj02(J, W);
            array2 = gcj02towgs84(array1[0], array1[1]);
            return array2;
        }

        //������������������������������������������γ��תͶӰ�������������������������������������������
        public double[] GaussProjection(double J, double W, double H, double CJ, string Ttype)
        {
            double a, f, b, e, e2, d2r, xs = 0.0, ys = 0.0, hs = 0.0, B = W, L = J;
            double[] array = { xs, ys, hs };
            double[] ifTtypeR;

            //�ж�ͶӰ�������ͼ���������
            ifTtypeR = ifTtype(Ttype);
            //a = ifTtype(Ttype)[0];f = ifTtype(Ttype)[1];
            a = ifTtypeR[0]; //���򳤰���
            f = ifTtypeR[1]; //������ʦ�
            //MessageBox.Show(a.ToString());
            b = a * (1 - f); //����̰���
            e = Math.Sqrt(a * a - b * b) / a;  //��һƫ����
            e2 = Math.Sqrt(a * a - b * b) / b;  //�ڶ�ƫ����
            d2r = Math.PI / 180; //D2R   ��ת���� Degrees to Radians

            double L0 = CJ; //���뾭��
            double rho = 180 / Math.PI;  //   �� ��λ�Ƕ�.��
            double l = (L - L0) / rho;  // ���� l,L0Ϊ���뾭��
            double cosB = Math.Cos(B * d2r); //cos(B)
            double sinB = Math.Sin(B * d2r); //sin(B)
            double N = a / Math.Sqrt(1 - e * e * sinB * sinB);  // î��Ȧ���ʰ뾶
            double t = Math.Tan(B * d2r);  //tanB �Բ���t
            double eta = e2 * cosB;  // ���� ��

            //MΪ����Ȧ���ʰ뾶��M����ţ�ٶ���ʽ����չ��������ȡ��8������У�
            double m0 = a * (1 - e * e);
            double m2 = 3.0 / 2.0 * e * e * m0;
            double m4 = 5.0 / 4.0 * e * e * m2;
            double m6 = 7.0 / 6.0 * e * e * m4;
            double m8 = 9.0 / 8.0 * e * e * m6;

            //�����ҵ��ݺ���չ��Ϊ���ҵı���������
            double a0 = m0 + 1.0 / 2.0 * m2 + 3.0 / 8.0 * m4 + 5.0 / 16.0 * m6 + 35.0 / 128.0 * m8;
            double a2 = 1.0 / 2.0 * m2 + 1.0 / 2.0 * m4 + 15.0 / 32.0 * m6 + 7.0 / 16.0 * m8;
            double a4 = 1.0 / 8.0 * m4 + 3.0 / 16.0 * m6 + 7.0 / 32.0 * m8;
            double a6 = 1.0 / 32.0 * m6 + 1.0 / 16.0 * m8;
            double a8 = 1.0 / 128.0 * m8;

            // XΪ�Գ������������߻���
            double s2b = Math.Sin(B * d2r * 2);
            double s4b = Math.Sin(B * d2r * 4);
            double s6b = Math.Sin(B * d2r * 6);
            double s8b = Math.Sin(B * d2r * 8);
            double X = a0 * (B * d2r) - 1.0 / 2.0 * a2 * s2b + 1.0 / 4.0 * a4 * s4b - 1.0 / 6.0 * a6 * s6b + 1.0 / 8.0 * a8 * s8b;

            //MessageBox.Show(a.ToString()+ '\t'+f.ToString()+ '\t'+b.ToString() + '\t'+e.ToString() + '\t'+e2.ToString() + '\t'+d2r.ToString() + '\t'+l.ToString() + '\t'+cosB.ToString() + '\t'+sinB.ToString() + '\t'+N.ToString() + '\t'+t.ToString() + '\t'+eta.ToString() + '\t'+m0.ToString() + '\t'+m2.ToString() + '\t'+m4.ToString() + '\t'+m6.ToString() + '\t'+m8.ToString() + '\t'+a0.ToString() + '\t'+a2.ToString() + '\t'+a4.ToString() + '\t'+a6.ToString() + '\t'+a8.ToString() + '\t'+s2b.ToString() + '\t'+s4b.ToString() + '\t'+s6b.ToString() + '\t'+s8b.ToString() + '\t');

            // ����X
            xs = X + N / 2 * t * cosB * cosB * l * l + N / 24 * t * (5 - t * t + 9 * Math.Pow(eta, 2) + 4 * Math.Pow(eta, 4)) * Math.Pow(cosB, 4) * Math.Pow(l, 4) + N / 720 * t * (61 - 58 * t * t + Math.Pow(t, 4)) * Math.Pow(cosB, 6) * Math.Pow(l, 6);
            // ����Y
            ys = N * cosB * l + N / 6 * (1 - t * t + eta * eta) * Math.Pow(cosB, 3) * Math.Pow(l, 3) + N / 120 * (5 - 18 * t * t + Math.Pow(t, 4) + 14 * Math.Pow(eta, 2) - 58 * eta * eta * t * t) * Math.Pow(cosB, 5) * Math.Pow(l, 5) + 500000;
            // ���θ߶�H����δ�������
            hs = H;


            array[0] = xs; array[1] = ys; array[2] = hs;
            return array;
        }







        //�����������������������������������������жϲο��������������������������������������������������
        public double[] ifTtype(string Ttype)
        {
            double a, f;
            double[] array = { 0, 0 };
            // �������
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



        //�����������������������������������������ж�ͶӰ�������͡���������������������������������������
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



        //�������������������������������������������κξ�γ�ȸ�ʽ��תΪ��.����ʽ����������������������������������������
        public Double AnytoDD(string input){
            double DD=0.0;
            if (input.Contains("��")) {
                // 113.10�� or 113��10��10.0��
                if (input.Contains("."))
                {
                    // 113.10�� or 113��10��10.0��
                    if (input.IndexOf(".") < input.IndexOf("��"))
                    {
                        // 113.10��
                        DD = Convert.ToDouble(input.Split('��')[0]);
                    }
                    else
                    {
                        // 113��10��10.0�� or 113��10�� or 113��10
                        double D = Convert.ToDouble(input.Split('��')[0]);
                        if (input.Split('��')[1].Contains("��"))
                        {
                            // 113��10�� or 113��10��10.0��
                            double M = Convert.ToDouble(input.Split('��')[1].Split("��")[0]);
                            if (input.Split('��')[1].Split("��")[1].Contains("��"))
                            {
                                // 113��10��10.0��
                                double S = Convert.ToDouble(input.Split('��')[1].Split("��")[1].Split('��')[0]);
                                DD = D + M / 60.0 + S / 3600.0;
                            }
                            else
                            {
                                // 113��10��
                                DD = D + M / 60.0;
                            }
                        }
                        else
                        {
                            // 113��10
                            double M = Convert.ToDouble(input.Split('��')[1]);
                            DD = D + M / 60.0;
                        }
                    }
                }
                else
                {
                    // 113��10��10�� or 113��10��10 or 113��10�� or 113��10 or 113��
                    double D = Convert.ToDouble(input.Split('��')[0]);
                    if ((input.Split('��')[1]).Length == 0)
                    {
                        // 113��
                        DD = D;
                    }
                    else
                    {
                        // 113��10��10�� or 113��10��10 or 113��10�� or 113��10
                        if (input.Split('��')[1].Contains("��"))
                        {
                            // 113��10��10�� or 113��10��10 or 113��10��
                            double M = Convert.ToDouble(input.Split('��')[1].Split("��")[0]);
                            if ((input.Split('��')[1].Split("��")[1]).Length == 0)
                            {
                                // 113��10��
                                DD = D + M / 60.0;
                            }
                            else
                            {
                                // 113��10��10�� or 113��10��10
                                if (input.Split('��')[1].Split("��")[1].Contains("��"))
                                {
                                    // 113��10��10��
                                    double S = Convert.ToDouble(input.Split('��')[1].Split("��")[1].Split('��')[0]);
                                    DD = D + M / 60.0 + S / 3600.0;
                                }
                                else
                                {
                                    // 13��10��10
                                    double S = Convert.ToDouble(input.Split('��')[1].Split("��")[1]);
                                    DD = D + M / 60.0 + S / 3600.0;
                                }
                            }
                        }
                        else
                        {
                            // 113��10
                            double M = Convert.ToDouble(input.Split('��')[1]);
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



        //������������������������������������������ԴЭ�����������������������������������������
        private void button5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://opensource.org/licenses");
        }
        //����������������������������������������Github����������������������������������������
        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/chenchen100/GISTools");
        }
        //������������������������������������������������������������������������������������
        private void button7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://gitee.com/chenchen1001/GISTools");
        }
        //�����������������������������������������������ۡ���������������������������������������������
        private void button8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://chenchen1001.gitee.io/2022/103050527/");
        }
    }
} 