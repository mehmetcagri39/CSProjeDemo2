using System;
using System.Collections.Generic;
using CSProjeDemo2;

namespace PayrollConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Maaş Bordrosu Oluşturma Programına Hoşgeldiniz!");

                Console.WriteLine("Personel listesinin bulunduğu JSON dosyasının yolunu giriniz: ");
                string filePathPersonnel = Console.ReadLine();

                Console.WriteLine("Maaş bordrosu hangi ay için oluşturulacak: ");
                string month = Console.ReadLine().ToUpper();

                Console.WriteLine("Maaş bordrosu hangi yıl için oluşturulacak: ");
                int year = Convert.ToInt32(Console.ReadLine());


                FileReader dosyaOku = new FileReader();
                List<Personnel> personeller = dosyaOku.PersonnelFileReader(filePathPersonnel);

                Payroll maasBordro = new Payroll(personeller, month, year);
                maasBordro.CreatePayroll();
                maasBordro.RaporGoruntule();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }

            Console.ReadLine();
        }
    }
}