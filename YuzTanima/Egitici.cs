using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace YuzTanima
{
    class Egitici : IDisposable
    {
        //Egitici sınıfının yapıcı metotları oluşturuldu.
        //Birincisi hiçbir parametre almayan, ikincisi dizin parametresi alan, üçüncüsü dizin ve klasör adı parametresi alan, 
        //dördüncüsü ise dizin, klasör adı ve xml dosyasını parametre olarak alan fonksiyonlardır.
        #region yapıcı_metotlar

        public Egitici()
        {
            KlasorAdi = "TrainedFaces";
            Dizin = Application.StartupPath + "\\" + KlasorAdi;
            XmlDosyasi = "TrainedLabels.xml";

            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            IsTrained = VeriYukle(Dizin);
        }
        public Egitici(string Dizin)
        {
            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            IsTrained = VeriYukle(Dizin);
        }
        public Egitici(string Dizin, string KlasorAdi)
        {
            this.Dizin = Dizin + KlasorAdi;
            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            IsTrained = VeriYukle(this.Dizin);
        }
        public Egitici(string Dizin, string KlasorAdi, string XmlDosyasi)
        {
            this.Dizin = Dizin + KlasorAdi;
            this.XmlDosyasi = XmlDosyasi;
            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            IsTrained = VeriYukle(this.Dizin);
        }
        #endregion

        #region değişkenler

        //string tipinde dosya konumu, klasörün adı ve xml dosyasını tutacak değişkenler tanımlandı.
        string Dizin, KlasorAdi, XmlDosyasi;
        //Eigen yöntemi için gerekli nesneler oluşturuldu.
        MCvTermCriteria termCrit; //Epsilon'un yanı sıra maksimum yineleme kısıtlamalarını kullanarak sonlandırma kriterleri oluşturur.
        EigenObjectRecognizer recognizer;

        //Eğitim değişkenleri tanımlandı.
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> Names_List = new List<string>();
        int ContTrain, NumLabels;
        float Eigen_Distance = 0;
        string Eigen_label;
        int Eigen_threshold = 0;

        string Error;
        public bool IsTrained = false;

        #endregion

        #region public_fonksiyonlar
        //Recognise fonksiyonunda gönderilen image nesnesi hafızadakilerle kıyaslanarak eşleşen kaydın bilgileri döndürülür.
        public string Recognise(Image<Gray, byte> Input_image, int Eigen_Thresh = -1)
        {
            if (IsTrained)//IsTrained değişkeni true ise yani eğitim yapıldıysa çalışacak.
            {
                EigenObjectRecognizer.RecognitionResult ER = recognizer.Recognize(Input_image);
                if (ER == null)//ER nesnesi boş ise geriye bilinmeyen döndürecek.
                {
                    Eigen_label = "Bilinmeyen";
                    Eigen_Distance = 0;
                    return Eigen_label;
                }
                else//ER nesnesi boş değil ise kaydın bilgilerini döndürecek.
                {
                    Eigen_label = ER.Label;
                    Eigen_Distance = ER.Distance;
                    if (Eigen_Thresh > -1) Eigen_threshold = Eigen_Thresh;
                    if (Eigen_Distance > Eigen_threshold) return Eigen_label;
                    else return "Bilinmeyen";
                }

            }
            else return "";//IsTrained değişkeni false ise yani eğitim yapılmadıysa boş metin döndürecek.
        }

        public void Dispose() //Nesnelerin bellekten atılmasını sağlayan fonksiyon.
        {
            recognizer = null;
            trainingImages = null;
            Names_List = null;
            Error = null;
            GC.Collect();
        }

        #endregion

        #region private_fonksiyonlar
        //VeriYukle isimli fonksiyon oluşturduğumuz xml dosyasının okunması ve yüz eğitiminin yapılması işlerinden sorumludur.
        private bool VeriYukle(string Folder_location)
        {
            if (File.Exists(Folder_location + "\\" + XmlDosyasi))//Verilen dosya konumunda dosya var ise çalışacaktır.
            {
                try
                {
                    Names_List.Clear();
                    trainingImages.Clear();
                    //FileStream sınıfıyla xml dosyasını okur, böylelikle dosyanın uzunluğu gibi bilgilere erişim sağlanır.
                    FileStream filestream = File.OpenRead(Folder_location + "\\" + XmlDosyasi);
                    long filelength = filestream.Length;
                    byte[] xmlBytes = new byte[filelength];
                    filestream.Read(xmlBytes, 0, (int)filelength);
                    filestream.Close();

                    MemoryStream xmlStream = new MemoryStream(xmlBytes);

                    using (XmlReader xmlreader = XmlTextReader.Create(xmlStream))
                    {
                        while (xmlreader.Read())
                        {
                            if (xmlreader.IsStartElement())
                            {
                                switch (xmlreader.Name)
                                {
                                    case "NAME": //Eğer okuduğu bir NAME ise, isimler listesine ekler.
                                        if (xmlreader.Read())
                                        {
                                            Names_List.Add(xmlreader.Value.Trim());
                                            NumLabels += 1;
                                        }
                                        break;
                                    case "FILE": //Eğer okuduğu bir FILE ise, trainingImages listesine ekler.
                                        if (xmlreader.Read())
                                        {
                                            trainingImages.Add(new Image<Gray, byte>(Dizin + "\\" + xmlreader.Value.Trim()));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    ContTrain = NumLabels; //Kayıtlı isim sayısısını ContTrain değişkenine atar.
                    if (trainingImages.ToArray().Length != 0)//Eğer trainingImages dizisinin uzunluğu sıfırdan farklı ise çalışacaktır.
                    {
                        //Yüz tanıma işleminin(Eigenface) yapıldığı yer.
                        recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), Names_List.ToArray(), 5000, ref termCrit);
                        return true;
                    }
                    else return false;//trainingImages dizisinin uzunluğu sıfır ise false değeri döndürecektir.
                }
                catch (Exception ex)//try bloğu çalışamadığı zaman geriye hata ve false değeri döndürecektir.
                {
                    Error = ex.ToString();
                    return false;
                }
            }
            else return false;//Dosya konumunda dosya yok ise false değeri döndürecektir.
        }

        #endregion
    }
}
