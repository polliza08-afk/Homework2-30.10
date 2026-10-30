using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace WpfImageReader;

/// <summary>
/// Логіка взаємодії для MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //Масив допустимих розширень для фільтрації зображень
    private static readonly string[] Extensions =
        { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Обробник події натискання кнопки завантаження зображення за URL
    /// </summary>
    private void btnDownload_Click(object sender, RoutedEventArgs e)
    {
        //Отримуємо URL зображення з текстового поля
        string imageUrl = txtUrl.Text;
        //Назва папки для збереження завантажених файлів
        string dir = "images";
        //Створюємо папку, якщо вона ще не існує
        Directory.CreateDirectory(dir);

        try
        {
            //Створюємо екземпляр HttpClient для виконання HTTP-запитів
            HttpClient client = new HttpClient();
            //Синхронно завантажуємо байти зображення за посиланням
            var buffer = client.GetByteArrayAsync(imageUrl).Result;
            //Генеруємо унікальне ім'я для файлу за допомогою Guid
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            //Формуємо повний шлях до майбутнього файлу
            var imageSave = Path.Combine(dir, fileName);
            //Зберігаємо завантажені байти у файл на диск
            File.WriteAllBytes(imageSave, buffer);
            //Повідомляємо користувача про успішне збереження
            MessageBox.Show("Файл успішно збережено! " + imageSave);
        }
        catch (Exception ex)
        {
            //Виводимо повідомлення про помилку у разі невдалого завантаження
            MessageBox.Show(ex.Message);
        }
    }

    /// <summary>
    /// Метод для читання зображень із вказаної папки та заповнення ними галереї.
    /// </summary>
    private void ReadDataFolder(string path)
    {
        //Отримуємо всі файли з папки та фільтруємо їх за дозволеними розширеннями (у нижньому регістрі)
        var files = Directory.GetFiles(path)
            .Where(f => Extensions.Contains(Path.GetExtension(f).ToLower()));

        //Колекція для збереження об'єктів зображень
        var images = new List<BitmapImage>();
        foreach (var file in files)
        {
            //Отримуємо додаткову інформацію про файл (за бажанням можна використовувати)
            var dateCreation = File.GetCreationTime(file);
            var fileSize = file.Length;
            var fileName = Path.GetFileName(file);

            //Створюємо об'єкт BitmapImage для завантаження картинки у пам'ять WPF
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(file);
            bitmap.DecodePixelWidth = 300; //Зменшуємо ширину завантаження для економії пам'яті
            bitmap.CacheOption = BitmapCacheOption.OnLoad; //Завантажуємо в пам'ять одразу, щоб не блокувати файл на диску
            bitmap.EndInit();

            images.Add(bitmap);
        }

        //Прив'язуємо заповнену колекцію зображень до елемента інтерфейсу GalleryList
        GalleryList.ItemsSource = images;
    }

    /// <summary>
    /// Обробник події натискання кнопки для зчитування та відображення завантажених зображень
    /// </summary>
    private void btnReadImages_Click(object sender, RoutedEventArgs e)
    {
        //Отримуємо поточну робочу директорію застосунку
        string currendDirectory = Directory.GetCurrentDirectory();
        //Формуємо шлях до папки "images" всередині робочої директорії
        string folder = Path.Combine(currendDirectory, "images");
        //Викликаємо метод для читання картинок із цієї папки
        ReadDataFolder(folder);
    }
}