using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using TestCam.ArduinoConnection;

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;
        private Image<Bgr, Byte> IMG_Post;
        private Image<Gray, Byte> IMG_Post_Gray;
        private Image<Gray, Byte> GrayImg;
        InoConn inoConn;

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
        
        public Form1()
        {
            InitializeComponent();
            inoConn = new InoConn();
        }
        
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)//very important to handel excption
            {
                try
                {
                    capture = new Capture();
                    //Capture(0) is default camera, Camera(1) is external camera
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();
            IMG_Post = IMG.CopyBlank();    
            GrayImg = IMG.Convert<Gray, Byte>();

            int i, j;
            int x1, y1, x2, y2;
            int Xp, Yp;
            int Max_Y, Min_Y;
            double Xc, Yc, Zc;

            // 1- Find Xp and Yp
            for (i = 0; i < GrayImg.Width; i++)
                for (j = 0; j < GrayImg.Height; j++)
                    if (GrayImg[j, i].Intensity > 40)
                        IMG_Post[j, i] = new Bgr(0, 0, 0);
                    else
                        IMG_Post[j, i] = new Bgr(255,255,255);
            IMG_Post_Gray = IMG_Post.Convert<Gray, Byte>();

            x1 = -1;
            for (i=10; i<IMG_Post_Gray.Width-10; i++)
            {
                for (j = 10; j < IMG_Post_Gray.Height-10; j++)
                    if(IMG_Post_Gray[j,i].Intensity>128)
                    {
                            x1 = i;
                            break;
                    }
                    if (x1 >= 0) break;
            }

            x2 = -1;
            for (i = IMG_Post_Gray.Width-10; i>=10; i--)
            {
                for (j = 10; j < IMG_Post_Gray.Height-10; j++)
                    if (IMG_Post_Gray[j, i].Intensity > 128)
                    {
                        x2 = i;
                        break;
                    }
                    if (x2 >= 0) break;
            }

            y1 = -1;
            for (j = 10; j < IMG_Post_Gray.Height-10; j++)
            {
                for (i = 10; i < IMG_Post_Gray.Width-10; i++)
                    if (IMG_Post_Gray[j, i].Intensity > 128)
                    {
                        y1 = j;
                        break;
                    }
                    if (y1 >= 0) break;
            }

            y2 = -1;
            for (j = IMG_Post_Gray.Height-10; j>=10; j--)
            {
                for (i = 10; i < IMG_Post_Gray.Width-10; i++)
                    if (IMG_Post_Gray[j, i].Intensity > 128)
                    {
                        y2 = j;
                        break;
                    }
                    if (y2 >= 0) break;
            }

            Xp = (x1 + x2) / 2;
            Yp = (y1 + y2) / 2;

            
            // 2- Find Xc and Yc
            Xc = (float)Xp / IMG_Post_Gray.Width * 40; //40cm
            Yc = (float)Yp / IMG_Post_Gray.Height * 24; //24cm

            Min_Y = 0;
            Max_Y = 24;

            // 3- Find Zc
            Zc = Yc * (24.0 - 40.0) / (Max_Y - Min_Y) + Min_Y;

            textBox1.Text = Xp.ToString();
            textBox2.Text = Yp.ToString();
            textBox3.Text = Xc.ToString();
            textBox4.Text = Yc.ToString();
            textBox5.Text = Zc.ToString();

            // 4- Calculate the Inverse
            double th1, th2, th1_radian, th2_radian, Px, Py, Pz;
            Px = Xc; Py = Yc; Pz = Zc;

            th1_radian = Math.Atan(Py / Px);
            th2_radian = Math.Atan(Math.Sin(th1_radian * (Pz - 28) / Py));

            th1 = (th1_radian * 180) / Math.PI;
            th2 = (th2_radian * 180) / Math.PI;

            // 5- Send to arduino using serial port (USB)

            if (x1 != -1 && x2 != -1 && y1 != -1 && y2 != -1)
                inoConn.turnTheLaserOn(th1, th2);
            else
                inoConn.turnTheLaserOff();

            try
            {           
                imageBox1.Image = IMG;
                imageBox2.Image = GrayImg;
                imageBox3.Image = IMG_Post;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Idle += processFrame;
            button1.Enabled = false;
            button2.Enabled = true;
        }
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button2_Click(object sender, EventArgs e)
        {
            inoConn.turnTheLaserOff();
            System.Threading.Thread.Sleep(2000);
            InoConn._serialPort.Close();
            Application.Idle -= processFrame;
            button1.Enabled = true;
            button2.Enabled = false;
        }    
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("D:\\Image" +  ".jpg");
        }       
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
    }
}
