using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using net.zemberek.erisim;
using net.zemberek.tr.yapi;
namespace metinProje
{
	class Program
	{
        static Dictionary<string, Dictionary<string, Dictionary<string, int>>> siniflar = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        static Dictionary<string, int> kelimeler = new Dictionary<string, int>();
        static string[] sinifIsmi = { "Pozitif", "Negatif", "Etkisiz", "Kontrol" };
        static int pozitifDokumanSayisi = 0, negatifDokumanSayisi = 0, etkisizDokumanSayisi = 0;
        static int pozitifKelimeSayisi = 0, negatifKelimeSayisi = 0, etkisizKelimeSayisi = 0, toplamKelimeSayisi;
        static double oranPozitif = 1.0, oranNegatif = 1.0, oranEtkisiz = 1.0;

        static void Main(string[] args)
		{
            DosyaOkuma();
            KelimeSayisi();
            MNBHesap();
            Console.ReadKey();
		}

        static void DosyaOkuma()
        {
            int i;
            string[] path = { @"c:\metinler\Pozitif\", @"c:\metinler\Negatif\", @"c:\metinler\Etkisiz\", @"c:\metinler\Kontrol\" };
            for (i = 0; i < path.Length; i++)
            {
                Dictionary<string, Dictionary<string, int>> tempDictionary = new Dictionary<string, Dictionary<string, int>>();
                DirectoryInfo di = new DirectoryInfo(path[i]);
                FileInfo[] files = di.GetFiles();

                foreach (FileInfo fi in files)
                {
                    
                    Deasciifier deasciifier = new Deasciifier();
                    var zemberek = new Zemberek(new TurkiyeTurkcesi());
                    Dictionary<string, int> geciciKelimeler = new Dictionary<string, int>();
                    FileStream fs = new FileStream(path[i] + fi, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("iso-8859-9"), false);
                    string yazi = sr.ReadLine();
                    yazi = yazi.ToLower();
                    while (yazi != null)
                    {
                        char[] karakterler = { ' ', ',', '.', '?', '!', ';', ':', '\n', '\t', '\"', '\'', '(', ')', '#', '^', '@', '+', '-', '*', '/', '’', '_', '-' };
                        string[] gecici = yazi.Split(karakterler);

                        foreach (string s in gecici)
                        {
                            #region Turkcelestirmeden
                            /*
                            if (!geciciKelimeler.ContainsKey(s))
                                geciciKelimeler.Add(s, 1);
                            else
                                geciciKelimeler[s]++;
                            */
                            #endregion
                            #region Turkcelestirerek
                            if (zemberek.kelimeDenetle(s))
                            {
                                if (!geciciKelimeler.ContainsKey(s))
                                    geciciKelimeler.Add(s, 1);
                                else
                                    geciciKelimeler[s]++;
                            }
                            else
                            {
                                string duzelt = deasciifier.DeAsciify(s);
                                if (zemberek.kelimeDenetle(duzelt))
                                {
                                    if (!geciciKelimeler.ContainsKey(duzelt))
                                        geciciKelimeler.Add(duzelt, 1);
                                    else
                                        geciciKelimeler[duzelt]++;
                                }
                                else
                                {
                                    var oneriler = zemberek.oner(s);
                                    if (oneriler.Any())
                                    {
                                        duzelt = zemberek.oner(s)[0];

                                        if (!geciciKelimeler.ContainsKey(duzelt))
                                            geciciKelimeler.Add(duzelt, 1);
                                        else
                                            geciciKelimeler[duzelt]++;
                                    }
                                }
                            }
                            #endregion
                        }
                        yazi = sr.ReadLine();
                    }
                    sr.Close();
                    fs.Close();
                    tempDictionary.Add(fi.ToString(), geciciKelimeler);
                }
                siniflar.Add(path[i].Split('\\')[2], tempDictionary);
            }
        }

        static void Goster()
        {
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> dokumanlar in siniflar)
            {
                Console.WriteLine(dokumanlar.Key);
                foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in dokumanlar.Value)
                {
                    Console.WriteLine("\t" + dokuman.Key);
                    foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                    {
                        Console.WriteLine("\t\t" + kelime.Key + " " + kelime.Value);
                    }
                }
            }
        }

        static void KelimeSayisi()
        {
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> dokumanlar in siniflar)
            {
                if (dokumanlar.Key == sinifIsmi[0])
                {
                    foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in dokumanlar.Value)
                    {
                        pozitifDokumanSayisi++;
                        foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                        {
                            pozitifKelimeSayisi += kelime.Value;
                            if (!kelimeler.ContainsKey(kelime.Key))
                                kelimeler.Add(kelime.Key, 1);
                        }
                    }
                }
                else if (dokumanlar.Key == sinifIsmi[1])
                {
                    foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in dokumanlar.Value)
                    {
                        negatifDokumanSayisi++;
                        foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                        {
                            negatifKelimeSayisi += kelime.Value;
                            if (!kelimeler.ContainsKey(kelime.Key))
                                kelimeler.Add(kelime.Key, 1);
                        }
                    }
                }
                else if (dokumanlar.Key == sinifIsmi[2])
                {
                    foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in dokumanlar.Value)
                    {
                        etkisizDokumanSayisi++;
                        foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                        {
                            etkisizKelimeSayisi += kelime.Value;
                            if (!kelimeler.ContainsKey(kelime.Key))
                                kelimeler.Add(kelime.Key, 1);
                        }
                    }
                }
            }
            toplamKelimeSayisi = kelimeler.Count;
        }

        static int SiniftaKacKereGecmis(string gelenSinif,string gelenKelime)
        {
            int sonuc = 0;
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> dokumanlar in siniflar)
            {
                if(dokumanlar.Key==gelenSinif)
                {
                    foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in dokumanlar.Value)
                    {
                        foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                        {
                            if (kelime.Key == gelenKelime)
                            {
                                sonuc+=kelime.Value;
                            }
                        }
                    }
                }
            }
            return sonuc;
        }

        static int KacSiniftaGecmis(string gelenSinif, string gelenKelime)
        {
            int sonuc = 0;
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> dokumanlar in siniflar)
            {
                if (dokumanlar.Key == gelenSinif)
                {
                    foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in dokumanlar.Value)
                    {
                        foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                        {
                            if (kelime.Key == gelenKelime)
                            {
                                sonuc++;
                                break;
                            }
                        }
                    }
                }
            }
            return sonuc;
        }

        static void MNBHesap()
        {
            double temp;
            
            foreach (KeyValuePair<string, Dictionary<string, int>> dokuman in siniflar[sinifIsmi[3]])
            {
                oranPozitif = 1;
                oranNegatif = 1;
                oranEtkisiz = 1;
                //pozitif
                foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                {
                    temp = (SiniftaKacKereGecmis(sinifIsmi[0], kelime.Key) + 1.0) / (pozitifKelimeSayisi + toplamKelimeSayisi);
                    oranPozitif *= Math.Pow(temp, KacSiniftaGecmis(sinifIsmi[0], kelime.Key));
                }

                //negatif
                foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                {
                    temp = (SiniftaKacKereGecmis(sinifIsmi[1], kelime.Key) + 1.0) / (negatifKelimeSayisi + toplamKelimeSayisi);
                    oranNegatif *= Math.Pow(temp, KacSiniftaGecmis(sinifIsmi[1], kelime.Key));
                }

                //etkisiz
                foreach (KeyValuePair<string, int> kelime in dokuman.Value)
                {
                    temp = (SiniftaKacKereGecmis(sinifIsmi[2], kelime.Key) + 1.0) / (etkisizKelimeSayisi + toplamKelimeSayisi);
                    oranEtkisiz *= Math.Pow(temp, KacSiniftaGecmis(sinifIsmi[2], kelime.Key));
                }

                if (oranPozitif > oranNegatif && oranPozitif > oranEtkisiz)
                    Console.WriteLine(dokuman.Key + " Pozitif");
                else if (oranNegatif > oranPozitif && oranNegatif > oranEtkisiz)
                    Console.WriteLine(dokuman.Key + " Negatif");
                else if (oranEtkisiz > oranPozitif && oranEtkisiz > oranNegatif)
                    Console.WriteLine(dokuman.Key + " Etkisiz");
            }
        }
    }
}


/* KAYNAKÇA
 * https://github.com/otuncelli/turkish-deasciifier-csharp
 * http://cagrisisman.com/posts/csharp-zemberek-kutuphanesi-kullanimi/6
 * 
 * Alt yorum
 * dosyalama işlemlerine tam hakim olamadığım için alt klasöre entegre edemedim
 * c:\metinler klasörünü açmamız gerekiyor
 */
