using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Xml;

namespace YuzTanima
{
    class Tanima
    {
        //string tipinde dosya konumu, klasörün adı ve xml dosyasını tutacak değişkenler tanımlandı.
        string Dizin, KlasorAdi, XmlDosyasi;

        //Tanima sınıfının yapıcı metotları oluşturuldu. 
        //Birincisi hiçbir parametre almayan, ikincisi dizin ve klasör adı parametrelerini alan, 
        //üçüncüsü ise dizin, klasör adı ve xml dosyasını parametre olarak alan yapıcı fonksiyonlardır.
        #region yapıcı_metotlar
        public Tanima()
        {
            KlasorAdi = "YuzTanima";
            XmlDosyasi = "Yuzler.xml";
            Dizin = Application.StartupPath + "/" + KlasorAdi + "/";
        }

        public Tanima(string Dizin, string KlasorAdi)
        {
            this.Dizin = Dizin + "/" + KlasorAdi + "/";
        }

        public Tanima(string Dizin, string KlasorAdi, string XmlDosyasi)
        {
            this.Dizin = Dizin + "/" + KlasorAdi + "/";
            this.XmlDosyasi = XmlDosyasi;
        }
        #endregion

        XmlDocument docu = new XmlDocument(); //xml belgesiyle işlemler yapılabilmesi için XmlDocument sınıfından bir nesne tanımlandı.

        //Yüzümüzü kaydetmesi ve xml dosyasını oluşturması için VeriKaydet isimli sınıf oluşturulmuştur.
        //Bu fonksiyonda tanımlanan yüzler belirtilen kaynağa aktarılır ve ihtiyacımız olan xml formatındaki veri setine ekleme görevini yapar. 
        //Program yüzlere göre eğitilmiş olur. Fonksiyon, geriye true veya false değer döndürür. Eğer eğitim başarılıysa true, başarısızsa false döndürecektir.
        #region Veri Kaydet
        public bool VeriKaydet(Image face_data, string FaceName)
        {
            try
            {
                string NAME_PERSON = FaceName;
                Random rand = new Random();
                bool file_create = true;
                string facename = "face_" + NAME_PERSON + "_" + rand.Next().ToString() + ".jpg";
                while (file_create)
                {

                    if (!File.Exists(Dizin + facename)) //Eğer belirtilen dizindeki isim yoksa çalışacak ve döngüyü durduracaktır.
                    {
                        file_create = false;
                    }
                    else //Değilse facename değişkenine kaydetmesini istediğimiz kişinin dosya ismini atayacaktır.
                    {
                        facename = "face_" + NAME_PERSON + "_" + rand.Next().ToString() + ".jpg";
                    }
                }


                if (Directory.Exists(Dizin)) //Girilen dizin bulunduysa yüzü jpeg formatında dosyaya kaydedecektir.
                {
                    face_data.Save(Dizin + facename, ImageFormat.Jpeg);
                }
                else //Dizin bulunamadıysa dizini oluşturup kaydetme işlemini yapacaktır.
                {
                    Directory.CreateDirectory(Dizin);
                    face_data.Save(Dizin + facename, ImageFormat.Jpeg);
                }
                if (File.Exists(Dizin + XmlDosyasi)) //Girilen dizindeki dosya ve xmldosyası bulunduysa xml belgesini yükleme işlemi yapar.
                {
                    bool loading = true;
                    while (loading)
                    {
                        try
                        {
                            docu.Load(Dizin + XmlDosyasi);
                            loading = false;
                        }
                        catch
                        {
                            docu = null;
                            docu = new XmlDocument();
                            Thread.Sleep(10);
                        }
                    }

                    //Aşağıda xml dosyası için gerekli işlemler yapılmıştır.
                    XmlElement root = docu.DocumentElement;
                    XmlElement face_D = docu.CreateElement("FACE");
                    XmlElement name_D = docu.CreateElement("NAME");
                    XmlElement file_D = docu.CreateElement("FILE");

                    name_D.InnerText = NAME_PERSON;
                    file_D.InnerText = facename;
                    face_D.AppendChild(name_D);
                    face_D.AppendChild(file_D);
                    root.AppendChild(face_D);
                    docu.Save(Dizin + XmlDosyasi);
                }
                else //Dosya bulunamadıysa bu blok çalışacaktır.
                {
                    FileStream FS_Face = File.OpenWrite(Dizin + XmlDosyasi);//FileStream sınıfının OpenWrite fonksiyonu yardımıyla yazmak için yeni bir dosya oluşturulur.
                    using (XmlWriter writer = XmlWriter.Create(FS_Face)) //XmlWriter sınıfı yardımıyla xml dosyasının yazılma işlemi yapılır.
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Faces_For_Training");

                        writer.WriteStartElement("FACE");
                        writer.WriteElementString("NAME", NAME_PERSON);
                        writer.WriteElementString("FILE", facename);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    FS_Face.Close();
                }

                return true; //İşlemler başarılı olursa geriye true değer döndürür.
            }
            catch (Exception ex)
            {
                return false; //İşlemler başarısız ise geriye false değer döndürür.
            }

        }
        #endregion
    }
}
