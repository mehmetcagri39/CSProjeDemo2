using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Globalization;

namespace CSProjeDemo2
{
    public abstract class Personnel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public double TotalHoursWorked { get; set; }
        public double HourlyRate { get; set; }

        public abstract double CalculateSalary();

        public virtual Dictionary<string, object> SalaryInfo(string month, int year)
        {
            var salary = CalculateSalary();

            return new Dictionary<string, object>
            {
                { "Personel Ismi", Name },
                { "Calisma Saati", TotalHoursWorked },
                { "Ana Odeme", salary },
                { "Mesai", 0 },
                { "Toplam Odeme", salary }
            };
        }
    }

    public class Yonetici : Personnel
    {
        public double Bonus { get; set; } = 22500; //dinamik yapılarak sonradan girilmesi de istenebilirdi

        public Yonetici()
        {
            HourlyRate = 750; //base saatlik ücret constructorda set edildi
        }

        public override double CalculateSalary()
        {
            return (TotalHoursWorked * HourlyRate) + Bonus;
        }

        public override Dictionary<string, object> SalaryInfo(string month, int year)
        {
            var baseSalary = TotalHoursWorked * HourlyRate;
            var totalSalary = baseSalary + Bonus;

            return new Dictionary<string, object>
            {
                { "Personel Ismi", Name },
                { "Calisma Saati", TotalHoursWorked },
                { "Ana Odeme", baseSalary },
                { "Bonus", Bonus },
                { "Toplam Odeme", totalSalary }
            };
        }
    }

    public class Memur : Personnel
    {
        public enum Kidem
        {
            BirinciKademe,
            IkinciKademe,
            UcuncuKademe,
            DorduncuKademe
        }

        public Kidem MemurKidem { get; set; } = Kidem.DorduncuKademe; 

        public Memur()
        {
            SetHourlyRate();
        }

        internal void SetHourlyRate()
        {
            switch (MemurKidem)
            {
                case Kidem.BirinciKademe:
                    HourlyRate = 500 * 1.3;
                    break;
                case Kidem.IkinciKademe:
                    HourlyRate = 500 * 1.2;
                    break;
                case Kidem.UcuncuKademe:
                    HourlyRate = 500 * 1.1;
                    break;
                case Kidem.DorduncuKademe:
                default:
                    HourlyRate = 500;
                    break;
            }
        }

        public override double CalculateSalary()
        {
            double normalSalary = Math.Min(TotalHoursWorked, 180) * HourlyRate; //normal katsayılı maaş hesabı için toplam mesai saati, 180 veya 180'den küçük

            double extraHours = Math.Max(0, TotalHoursWorked - 180); //180 saatten ne kadar fazla çalıştığına göre hesaplanan mesai saati 
            double extraHourPayment = extraHours * (HourlyRate * 1.5); //ve ücreti

            return normalSalary + extraHourPayment;
        }

        public double CalculateExtraHours() //raporda görüntülenmesi için ekstra oluşturulmuş mesai saati hesaplama methodu
        {
            double extraHours = Math.Max(0, TotalHoursWorked - 180);
            return extraHours * (HourlyRate * 1.5);
        }

        public override Dictionary<string, object> SalaryInfo(string month, int year)
        {
            double normalSalary = Math.Min(TotalHoursWorked, 180) * HourlyRate;
            double extraHourPayment = CalculateExtraHours();
            double totalSalary = normalSalary + extraHourPayment;

            return new Dictionary<string, object>
            {
                { "Personel İsmi", Name },
                { "Calisma Saati", TotalHoursWorked },
                { "Ana Odeme", normalSalary },
                { "Mesai", extraHourPayment },
                { "Toplam Odeme", totalSalary }
            };
        }
    }

    public class PersonnelJson
    {
        public string name { get; set; }
        public string title { get; set; }
    }

    public class FileReader
    {
        public List<Personnel> PersonnelFileReader(string filePath)
        {
            List<Personnel> personnels = new List<Personnel>();

            // Dosya var mı kontrol et
            bool filePathCheck = false;
            while (!filePathCheck)
            {
                // Dosya var mı kontrol et
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"HATA: {filePath} dosyası belirtilen konumda bulunamadı.");
                    Console.WriteLine("Lütfen dosyanın konumunu doğru şekilde girin.");
                    Console.WriteLine("Örnek dosya yolu: \"C:\\Users\\<username>\\source\\repos\\CSProjeDemo2\\Personeller.json\"");

                    Console.Write("Yeni dosya yolu girin: ");
                    filePath = Console.ReadLine();
                }
                else
                {
                    filePathCheck = true;
                }
            }

            string jsonContext = File.ReadAllText(filePath);
            List<PersonnelJson> personelListesi = JsonSerializer.Deserialize<List<PersonnelJson>>(jsonContext);

            //dosyadan okunan personellerin isimlerini ve mevkilerini sırayla ata
            foreach (var item in personelListesi)
            {
                if (item.title.Equals("Yonetici", StringComparison.OrdinalIgnoreCase))
                {
                    Yonetici yonetici = new Yonetici
                    {
                        Name = item.name,
                        Title = item.title
                    };

                    Console.WriteLine($"{yonetici.Name} bu ay kaç saat çalıştı:");
                    yonetici.TotalHoursWorked = Convert.ToDouble(Console.ReadLine());

                    // Yönetici için saatlik ücretin minimum 500 tl olduğunun kontrolü
                    double hourlyWage = 0;
                    bool hourlyWageCheck = false;

                    while (!hourlyWageCheck)
                    {
                        Console.WriteLine($"{yonetici.Name} için saatlik ücreti giriniz (minimum 500 TL):");
                        try
                        {
                            hourlyWage = Convert.ToDouble(Console.ReadLine());

                            if (hourlyWage < 500)
                            {
                                Console.WriteLine("Yönetici için saatlik ücret minimum 500 TL olmalıdır!");
                            }
                            else
                            {
                                hourlyWageCheck = true;
                            }
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("HATA: Lütfen geçerli bir sayı girin!");
                        }
                    }
                    yonetici.HourlyRate = Math.Max(500, hourlyWage); 

                    Console.WriteLine($"{yonetici.Name} için bonus miktarını giriniz:");
                    yonetici.Bonus = Convert.ToDouble(Console.ReadLine());

                    personnels.Add(yonetici);
                }
                else if (item.title.Equals("Memur", StringComparison.OrdinalIgnoreCase)) //memur için else if kullanıldı, daha sonra eklenmesi düşünülen personel için aynı yapının kullanılmasına devam edilebilir yahut switch case yapısına geçilebilir.
                {
                    Memur memur = new Memur
                    {
                        Name = item.name,
                        Title = item.title
                    };

                    Console.WriteLine($"{memur.Name} bu ay kaç saat çalıştı:");
                    memur.TotalHoursWorked = Convert.ToDouble(Console.ReadLine());

                    Console.WriteLine($"{memur.Name} adlı memurun kıdemini rakam olarak seçiniz (0:Birinci, 1:İkinci, 2:Üçüncü, 3:Dördüncü):");
                    int kidem = Convert.ToInt32(Console.ReadLine());
                    memur.MemurKidem = (Memur.Kidem)kidem;
                    memur.SetHourlyRate();

                    personnels.Add(memur);
                }
            }

            return personnels;
        }
    }

    //Maaş Bordrosu sınıfı
    public class Payroll
    {
        private readonly List<Personnel> _personnels;
        private readonly string _month;
        private readonly int _year;

        public Payroll(List<Personnel> personnels, string month, int year)
        {
            _personnels = personnels;
            _month = month;
            _year = year;
        }

        public void CreatePayroll()
        {
            //bütün çalışanlar için tek tek dosya oluşturan döngü
            foreach (var personnel in _personnels)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), personnel.Name.Replace(" ", "_")); //mevcut dosya yolunu personel ismi ile birleştiriyor ve dosya yolu o oluyor.
                if (!Directory.Exists(filePath))//böyle bir yol halihazırda mevcut değilse oluştur
                {
                    Directory.CreateDirectory(filePath);
                }

                var salaryInfo = personnel.SalaryInfo(_month, _year);

                string fileName = $"Maaş_Bordro_{_month}_{_year}.json";
                string filePathCombined = Path.Combine(filePath, fileName);


                string jsonContext = JsonSerializer.Serialize(salaryInfo, new JsonSerializerOptions { WriteIndented = true }); //WriteIntended dosyanın okunabilir olması için içeriği düzeltti.
                File.WriteAllText(filePathCombined,  jsonContext);

                Console.WriteLine($"{personnel.Name} için maaş bordrosu başarıyla oluşturuldu. Dosyanın konumu: {filePathCombined}");
            }
        }


        //Konsolda mini bir özet
        public void RaporGoruntule()
        {
            Console.WriteLine($"\n--- MAAŞ RAPORU ({_month} {_year}) ---");
            Console.WriteLine("İsim\t\tÜnvan\t\tÇalışma Saati\tToplam Ödeme");
            Console.WriteLine("----------------------------------------------------------");

            CultureInfo turkce = new CultureInfo("tr-TR"); //konsolda para birimi ₺ için eklendi


            foreach (var personnel in _personnels)
            {
                var salaryInfo = personnel.SalaryInfo(_month, _year);
                string formattedAmount = ((double)salaryInfo["Toplam Odeme"]).ToString("N2", turkce) + " ₺";
                Console.WriteLine($"{personnel.Name}\t{personnel.Title}\t{personnel.TotalHoursWorked}\t\t{formattedAmount}");

            }

            // 150 saatten az çalışanlar
            var azCalisanlar = _personnels.Where(p => p.TotalHoursWorked < 150).ToList();

            if (azCalisanlar.Any())
            {
                Console.WriteLine("\n--- 150 SAATTEN AZ ÇALIŞAN PERSONEL ---");
                Console.WriteLine("İsim\t\tÜnvan\t\tÇalışma Saati");
                Console.WriteLine("------------------------------------------");

                foreach (var personnel in azCalisanlar)
                {
                    Console.WriteLine($"{personnel.Name}\t{personnel.Title}\t{personnel.TotalHoursWorked}");
                }
            }
        }
    }
}