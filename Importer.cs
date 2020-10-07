using LanguageSchool.EF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageSchool
{
    /// <summary>
    /// Класс для импорта не структурированных данных
    /// </summary>
    public static class Importer
    {
       /// <summary>
       /// Импорт фотографий клиентов
       /// </summary>
        public static void ImportClientPhotos()
        {
            string mainPath = @"C:\Users\Student\Desktop\09_1.1_3\Сессия 1\";

            using(var context = new LanguageContext())
            {
                var clients = context.Client.ToList();

                var fileNames = File.ReadAllLines(@"C:\Users\Student\source\repos"+
                    @"\LanguageSchool\LanguageSchool\Resources\ClientPaths.txt");

                for(int i = 0; i<clients.Count; i++)
                {
                    clients[i].Photo = File.ReadAllBytes(mainPath + fileNames[i]);
                }

                context.SaveChanges();
            }
        }
    }
}
