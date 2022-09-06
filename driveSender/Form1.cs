using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace driveSender
{
    public partial class Form1 : Form
    {
        int sayac = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }
        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Tiff);
                return stream.ToArray();
            }
        }
        Rectangle section;
        public void GetPictures(string path)
        {

            var files = Directory.GetFiles(path);
            for (int i = 0; i <files.Length; i++)
            {

                System.Threading.Thread.Sleep(10);
                label1.Invoke((MethodInvoker)delegate {
                    label1.Text = i.ToString() + " Files scanned";
                });
                section = new Rectangle(new Point(500, 270), new Size(650, 700));
                Bitmap bmp = new Bitmap(files[i]);
                Bitmap croppedImage = CropImage(bmp, section);              
                byte[] imageData = ImageToByte2(croppedImage);
                if (SendRequest(imageData, "24150709", "512002") > 0)
                {
                    bmp.Save(@"D:\5_10_2022Hata\" + files[i] + ".tiff", ImageFormat.Tiff);
                }   
            }
       

        }
        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }
        private byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);

            return bytes;
        }

       
        public int SendRequest(byte[] imageBytes, string _cameraId, string _kumasId)
        {

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5000/predict");
            //httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    camera_id = _cameraId,
                    kumas_id = _kumasId,
                    timestamp = 1000
                });

                //var bjson = Encoding.UTF8.GetBytes(json);
                var bjson = Encoding.UTF8.GetBytes(json);
                string split = "split";
                var splitByte = Encoding.UTF8.GetBytes(split);
                byte[] result1 = new byte[bjson.Length + splitByte.Length];
                byte[] result2 = new byte[bjson.Length + splitByte.Length + imageBytes.Length];
                result1 = Combine(bjson, splitByte);
                result2 = Combine(result1, imageBytes);

                using (Stream stream1 = httpWebRequest.GetRequestStream())
                {
                    // stream1.Write(bytes, 0, bytes.Length);
                    stream1.Write(result2, 0, result2.Length);
                    stream1.Close();
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (Stream stream2 = httpResponse.GetResponseStream())
                    {
                        ResultObject resultObject = new ResultObject();
                        var sonuc = (new StreamReader(stream2)).ReadToEnd();
                        resultObject = JsonConvert.DeserializeObject<ResultObject>(sonuc);
                        int defectCount = resultObject.DefectCount;
                        return defectCount;
                    }
                }
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                GetPictures(@"D:\paint");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Bitti");
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label1.Text = sayac.ToString();
        }
    }
}
