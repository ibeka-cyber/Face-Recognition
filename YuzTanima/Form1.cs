using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

//Balıkesir Üniversitesi Bilgisayar Mühendisliği 4. Sınıf
//Bilgisayar Mühendisliği Tasarımı dersi Bitirme Projesi
//201813709033 Mert Çikendin
//201813709047 İrem Bahar Koç
//201813709052 Ataberk Özdemir
namespace YuzTanima
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }
        //Form'daki button1'e tıklandığında aşağıdaki event çalıştırılacak ve tanınan yüz kaydedilecektir.
        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)//10 tane fotoğraf kaydetmesi için yazılan döngü.
                //Kaydedilen fotoğraf ne kadar çok olursa doğru sonuç elde edilme ihtimali o kadar artar.
                {
                    if (!recognition.VeriKaydet(pictureBox2.Image, textBox1.Text))//Veri kaydedilemediyse ekranda hata yazdıracaktır.
                        MessageBox.Show("Hata", "Kişi Kaydedilemedi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Thread.Sleep(100);//Geçerli iş parçacağının 100ms askıya alınmasını sağlar.
                }
                recognition = null;
                train = null;
                recognition = new Tanima("D:\\", "Faces", "yuz.xml");
                train = new Egitici("D:\\", "Faces", "yuz.xml");
            });
        }
        //Yüz tanıma işlemi için Tanima ve Egitici sınıfından nesneler oluşturuldu.
        Tanima recognition = new Tanima("D:\\", "Faces", "yuz.xml");
        Egitici train = new Egitici("D:\\", "Faces", "yuz.xml");
        //Kamera görüntüsü alma, yüz tanımlama gibi işlemler Form1_Load eventinde yapılmaktadır.
        private void Form1_Load(object sender, EventArgs e)
        {
            //EmguCV kütüphanesinden bir sınıf olan Capture sınıfından bir nesne tanımlandı. Bu nesne sayesinde kameraya erişilecek.
            Capture capture = new Capture();
            capture.Start();//Kamera başlatıldı.
            capture.ImageGrabbed += (a, b) => //Görüntü yakalamak için kullanılır.
            {
                var image = capture.RetrieveBgrFrame();//Resim kaydedilir.
                var grayimage = image.Convert<Gray, byte>();//Image isimli resmi siyah-beyaz hale çevirir.
                HaarCascade haaryuz = new HaarCascade("haarcascade_frontalface_alt2.xml");//HaarCascade sınıfı, gri renkli fotoğrafta yüzü yakalamak için kullanılır.
                MCvAvgComp[][] Yuzler = grayimage.DetectHaarCascade(haaryuz, 1.2, 5, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(15, 15));//HaarCascade nesnesi kullanılarak yüz algılanır.
                MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 0.5, 0.5);//Tanınan yüzün yazılacağı yazı tipi ve boyutu belirlenir.
                foreach (MCvAvgComp yuz in Yuzler[0])//Kayıtlı tüm yüzlerde gezer.
                {
                    //Resimler aynı boyutta olması gerektiğinden yeniden boyutlandırılır.
                    var sadeyuz = grayimage.Copy(yuz.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    pictureBox2.Image = sadeyuz.ToBitmap(); //pictureBox2'ye algılanan yüzün yeniden boyutlandırılmış siyah-beyaz hali atanır.
                    if (train != null)//train nesnesi boş değilse çalışacaktır.
                        if (train.IsTrained)//Egitici sınıfında eğitim yapıldıysa, IsTrained true olur ve bu if bloğu çalışır.
                        {
                            string name = train.Recognise(sadeyuz);//"sadeyuz" nesnesi hafızadakilerle kıyaslanır ve geriye döndürülen isim name değişkenine atanır.
                            image.Draw(name + " ", ref font, new Point(yuz.rect.X - 2, yuz.rect.Y - 2), new Bgr(Color.LightGreen));//Tanınan yüzün sol üst tarafına, yeşil renkle yüzün ait olduğu isim yazdırılacaktır.
                        }
                    image.Draw(yuz.rect, new Bgr(Color.Red), 2);//Tanınan yüzü çerçeveye alır.
                }
                pictureBox1.Image = image.ToBitmap();//pictureBox1'de kameramızın görünmesini sağlar.
            };
        }
    }
}
