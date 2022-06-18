using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using TagLib;

namespace Mp3RenamerV2
{
    public partial class MainFrame : Form
    {
        const int COLOR_RED = 0;
        const int COLOR_BLUE = 1;
        const int COLOR_GREEN = 2;
        string path;                                 // открытый файл или папка
        List<String> pathFiles = new List<String>(); // список музыкальных файлов папки
        bool isSelectedFile = true;                  // файл или папка
        /// <summary>
        /// Список закрашенных слов
        /// </summary>
        List<PainterWord> words = new List<PainterWord>();
        /// <summary>
        /// Флаг открытого файла или папки
        /// </summary>
        String text = ""; // буфер

        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog openFolderDialog;
        private String startPath = "";
        private BackgroundWorker openBW;
        private BackgroundWorker checkTagsBW;
        private BackgroundWorker checkFilenameBW;

        public MainFrame()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mp3-файлы (*.mp3)|*.mp3|flac-файлы (*.flac)|*.flac|Все файлы (*.*)|*.*";
            openFolderDialog = new FolderBrowserDialog();
            updateStartPath(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            checkTagsMS.Enabled = false;

            openBW = new BackgroundWorker();
            checkTagsBW = new BackgroundWorker();
            checkFilenameBW = new BackgroundWorker();
            openBW.WorkerReportsProgress = true;
            checkTagsBW.WorkerReportsProgress = true;
            checkFilenameBW.WorkerReportsProgress = true;
            openBW.ProgressChanged += progressChangedBW;
            checkTagsBW.ProgressChanged += progressChangedBW;
            checkFilenameBW.ProgressChanged += progressChangedBW;
            openBW.RunWorkerCompleted += runCompletedBW;
            checkTagsBW.RunWorkerCompleted += runCompletedBW;
            checkFilenameBW.RunWorkerCompleted += runCompletedBW;
            openBW.DoWork += openFolderTask;
            checkTagsBW.DoWork += checkTagsTask;
            checkFilenameBW.DoWork += checkFileNameTask;
        }
        /// <summary>
        /// Событие завершения фоновой задачи
        /// </summary>
        private void runCompletedBW(object sender, RunWorkerCompletedEventArgs e)
        {
            infoField.Text += text;
            paintWords();
            checkStatusLabel.Text = "Готово";
        }
        /// <summary>
        /// Показывает теги файла
        /// </summary>
        private String showTags(String file)
        {
            String rslt = "";
            TagLib.File tagSong = TagLib.File.Create(file);
            rslt += "{";
            rslt += tagSong.Tag.FirstPerformer == null ? "Пусто" : tagSong.Tag.FirstPerformer;
            rslt += "} - {";
            rslt += tagSong.Tag.Title == null ? "Пусто" : tagSong.Tag.Title;
            rslt += "}";
            return rslt;
        }
        // Событие Открыть файл
        private void openFileMenuItem_Click(object sender, EventArgs e)
        {
            text = "";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            checkStatusLabel.Text = "";
            checkTagsMenuItem.Enabled = true;
            checkNameMenuItem.Enabled = true;

            isSelectedFile = true;
            path = openFileDialog.FileName;
            print(path + "\n");        // печать названия файла
            String tagsName = showTags(path);
            printAndAddWordForPainting(tagsName, tagsName.Contains("Пусто") ? COLOR_RED : COLOR_BLUE, false);
            paintWords();                 // покраска тегов
            updateStartPath(path.Substring(0, path.Length - openFileDialog.SafeFileName.Length));   
        }
        // Событие Открыть папку
        private void openFolderMenuItem_Click(object sender, EventArgs e)
        {
            if (openFolderDialog.ShowDialog() != DialogResult.OK) return;
            checkStatusLabel.Text = "Выполнение";
            checkTagsMenuItem.Enabled = true;
            checkNameMenuItem.Enabled = true;

            isSelectedFile = false;
            path = openFolderDialog.SelectedPath;
            print("    Открыта папка: " + path + "\n");
            pathFiles.Clear();
            openBW.RunWorkerAsync();
        }
        // Открыть папку. Считывает все названия папок и файлов в text
        private void openFolderTask(object sender, DoWorkEventArgs e)
        {
            text = "";
            String[] folderFileElements = Directory.GetFileSystemEntries(path);// элементы папки
            int progressLength = folderFileElements.Length;
            int progress = 0;
            // Считывает папки и музыкальные файлы
            string ext, str;
            foreach (String elem in folderFileElements)
            {
                ext = Path.GetExtension(elem);
                if (ext=="")
                    text += elem + "\n";
                else if (ext==".mp3" || ext==".flac")
                {
                    pathFiles.Add(elem);
                    text += elem + "\n";
                    str = showTags(elem);
                    printAndAddWordForPainting(str, str.Contains("Пусто") ? COLOR_RED : COLOR_BLUE, true);
                    text += "\n";
                }
                openBW.ReportProgress(++progress * 100 / progressLength);
            }
            // Обновляет место открытия проводника
            String[] foldersInPath = path.Split("\\"); // число папок в пути
            int rootFolderPathLength = 0;
            for (int i = 0; i < foldersInPath.Length - 1; i++)
                rootFolderPathLength += foldersInPath[i].Length + 1; // с учетом \
            updateStartPath(path.Substring(0, rootFolderPathLength));
        }
       // Событие проверки тегов
        private void checkTagsMenuItem_Click(object sender, EventArgs e)
        {
            text = "";
            // файла
            if (isSelectedFile)
            {
                checkStatusLabel.Text = "";
                path = deleteKissVK(path);
                String newpath = correctHyphen(path);
                if (!newpath.Contains("ALREADYEXISTS"))
                {
                    path = newpath;
                    checkTags(path);
                    print(showTags(path));
                    printAndAddWordForPainting("   правильные теги", COLOR_GREEN, false);
                }
                else
                {
                    print(newpath.Substring(13));
                    printAndAddWordForPainting("   уже существует", COLOR_RED, false);
                }     
            }
            // папка
            else
            {
                infoField.Text += "    Проверка тегов файлов\n";
                checkStatusLabel.Text = "Выполнение";
                checkTagsBW.RunWorkerAsync();
            }
            paintWords();
        }
        // Фоновая задача проверки тегов
        private void checkTagsTask(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < pathFiles.Count; i++)
            {
                pathFiles[i] = deleteKissVK(pathFiles[i]);
                pathFiles[i] = correctHyphen(pathFiles[i]);
                checkTags(pathFiles[i]);
                text += showTags(pathFiles[i]);
                printAndAddWordForPainting("   правильные теги", COLOR_GREEN, true);
                text += "\n";
                checkTagsBW.ReportProgress((i+1)*100/ pathFiles.Count);
            }
        }
        /// <summary>
        /// Проверяет наличие тегов и меняет на корректные
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        private void checkTags(String path)
        {
            TagLib.File tags = TagLib.File.Create(path);
            String filename = Path.GetFileName(path);
            // Проверяет название песни, вырезает из названия файла
            if (tags.Tag.Title == null)
            {
                int start = filename.LastIndexOf("-") + 2; // начало выреза
                int length = filename.Length - start - 4; // число символов для выреза, 4 - длина расширения
                tags.Tag.Title = filename.Substring(start, length);
                tags.Save();
            }
            // Проверяет исполнителя, вырезает из короткого имени файла
            if (tags.Tag.FirstPerformer == null)
            {
                tags.Tag.Performers = new String[1] { filename.Substring(0, filename.LastIndexOf("-") - 1) };
                tags.Save();
            }
        }
        // Событие 'Проверка имени файла'
        private void checkFileNameMenuItem_Click(object sender, EventArgs e)
        {
            path = checkFileName_Click(path, false);
        }
        // Событие 'Проверка имени файла альбома'
        private void checkAlbFileNameMenuItem_Click(object sender, EventArgs e)
        {
            path = checkFileName_Click(path, true);
        }
        // Проверяет и изменяет имя файла на соответствие тегам
        private string checkFileName_Click(String path, bool isAlbum)
        {
            text = "";
            if (isSelectedFile)
            {
                string newname = checkFileName(path, isAlbum);
                if (newname.Contains("NULLTAGS"))
                {
                    print(newname.Substring(8));
                    printAndAddWordForPainting("   пустые теги", COLOR_RED, false);
                }
                else if (newname == null)
                {
                    print(path);
                    printAndAddWordForPainting(": есть пустые теги", COLOR_RED, false);
                    print(": есть пустые теги\n");
                }
                else if (!path.Equals(newname))
                {
                    path = newname;
                    print(path + "\n");
                }
                else
                {
                    print(path);
                    printAndAddWordForPainting("   название файла соотвествует тегам", COLOR_GREEN, false);
                }
                paintWords();
            }
            else
            {
                infoField.Text += "    Проверка соответствия имен файлов тегам\n";
                checkStatusLabel.Text = "Выполнение";
                checkFilenameBW.RunWorkerAsync(isAlbum);
            }
            return path;
        }
        // Фоновая задача проверки имени файла
        private void checkFileNameTask(object sender, DoWorkEventArgs e)
        {
            text = "";
            string newname;
            for (int i = 0; i < pathFiles.Count; i++)
            {
                newname = checkFileName(pathFiles[i], (bool)e.Argument);
                if (newname.Contains("NULLTAGS"))
                {
                    text += pathFiles[i];
                    printAndAddWordForPainting("   пустые теги\n", COLOR_RED, true);
                }
                else if (newname.Equals("IOException"))
                {
                    text += pathFiles[i];
                    printAndAddWordForPainting("   файл занят другим процессом\n", COLOR_RED, true);
                }
                else if (newname.Equals("DirectoryNotFoundException"))
                {
                    text += pathFiles[i];
                    printAndAddWordForPainting("   DirectoryNotFoundException\n", COLOR_RED, true);
                }
                else if (!pathFiles[i].Equals(newname))
                {
                    pathFiles[i] = newname;
                    text += pathFiles[i] + "\n";
                }
                else
                {
                    text += pathFiles[i];
                    printAndAddWordForPainting("   название соответствует тегам\n", COLOR_GREEN, true);
                }
                checkFilenameBW.ReportProgress((i+1) * 100 / pathFiles.Count);
            }
        }
        /// <summary>
        /// Проверяет и изменяет имя файла на соответствие тегам
        /// </summary>
        private string checkFileName(String filename, bool isAlbum)
        {

            string newname = filename.Substring(0, filename.Length - Path.GetFileName(filename).Length);
            TagLib.File tags = TagLib.File.Create(filename);
            // Проверяется наличие тегов
            if(tags.Tag.Performers == null || tags.Tag.Title == null)
            {
                return "NULLTAGS"+filename;
            }
            // Записывается номер трека для файла из альбома
            if (isAlbum)
            {
                if (tags.Tag.Track < 10) newname += "0" + tags.Tag.Track + ". ";
                else newname += tags.Tag.Track + ". ";
            }
            // Создается имя файла из тегов
            newname += tags.Tag.Performers[0] + " - " + tags.Tag.Title;
            newname += Path.GetExtension(filename) == ".mp3" ? ".mp3" : ".flac";
            {
                 try
                 {
                    System.IO.File.Move(filename, newname);
                 }
                 catch (System.IO.DirectoryNotFoundException)
                 {
                    return "DirectoryNotFoundException";
                 }
                catch(System.IO.IOException)
                {
                    return "IOException";
                }
                  filename = newname;
            }
            newname = deleteKissVK(filename);
            newname = correctHyphen(filename);
            if (!filename.Equals(newname))
            {
                System.IO.File.Move(filename, newname);
                filename = newname;
            }
            return filename;
        }
        /// <summary>
        /// Отображает прогресс фоновой задачи
        /// </summary>
        private void progressChangedBW(object sender, ProgressChangedEventArgs e)
        {
            progressLabel.Text = (e.ProgressPercentage.ToString() + "%");
            
        }
        /// <summary>
        /// Обновляет точку открытия openFileDialog
        /// </summary>
        private void updateStartPath(String value)
        {
            startPath = value;
            openFileDialog.InitialDirectory = startPath;
            openFolderDialog.InitialDirectory = startPath;
        }
        /// <summary>
        /// Выводит информацию в infoField
        /// </summary>
        private void print(String text)
        {
            infoField.Invoke(new Action(() => { infoField.Text += text; }));
        }
        // Удаляет из имени файла -kissvk.com
        private string deleteKissVK(string filename)
        {
            String kissVK = "-kissvk.com";
            String newName;
            if (filename.Contains(kissVK))
            {
                newName = filename.Remove(filename.IndexOf(kissVK), kissVK.Length);
                System.IO.File.Move(filename, newName);
                return newName;
            }
            return filename;
        }
        // редактирует " - "
        private string correctHyphen(string filename)
        {
            String newName;
            if (filename.Contains("-") && !filename.Contains(" - "))
            {
                {
                    newName = filename.Replace("-", " - ");
                    if (!System.IO.File.Exists(newName))
                    {
                        System.IO.File.Move(filename, newName);
                        filename = newName;
                    }
                    else
                    {
                        return "ALREADYEXISTS" + newName;
                    }
                }
            }
            return filename;
        }

        /// <summary>
        /// Очищает infoField
        /// </summary>
        private void clearInfoField_Click(object sender, EventArgs e)
        {
            infoField.Text = "";
            words = new List<PainterWord>();

        }
        ///
        /// Класс Закрашенные слова
        /// 
        private struct PainterWord {
            public int start;
            public int length;
            public int type; // 0-Красный цвет 1-Синий цвет 2-Голубой цвет
            public PainterWord(int start, int length, int type)
            {
                this.start = start;
                this.length = length;
                this.type = type;
            }
        }

        private void paintWords()
        {
            for (int i = 0; i < words.Count; i++)
            {
                infoField.SelectionStart = words[i].start;
                infoField.SelectionLength = words[i].length;
                switch (words[i].type)
                {
                    case 0:
                        infoField.SelectionColor = Color.Red;
                        break;
                    case 1:
                        infoField.SelectionColor = Color.Blue;
                        break;
                    default:
                        infoField.SelectionColor= Color.Green;
                        break;
                }
            }
            // Заканчивает окрашивание слов
            infoField.SelectionStart = infoField.TextLength;
            infoField.SelectionLength = 0;
        }
        /// <summary>
        /// Добавляет слово для рисования
        /// </summary>
        /// <param name="word"> слово </param>
        /// <param name="isBuffer"> true - если слово для буфера </param>
        private void printAndAddWordForPainting(string word, int color, bool isBuffer)
        {
            int start=0;
            infoField.Invoke(new Action(() => { start = infoField.TextLength + text.Length; })); // Старт строки
            words.Add(new PainterWord(start, word.Length, color));
            if (!isBuffer)
            {
                
                print(word+"\n");
            }
            else
            {
                text += word;
            }
        }
    }
}