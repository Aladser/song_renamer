using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using TagLib;

namespace Mp3RenamerV2
{
    public partial class MainFrame : Form
    {
        String text = ""; // буфер
        int start = 0, length = 0; // Начало и длина крашеного слова
        /// <summary>
        /// Список закрашенных слов
        /// </summary>
        List<PainterWord> words = new List<PainterWord>();
        /// <summary>
        /// Путь открытому файлу или папке
        /// </summary>
        private String selectedPath;
        /// <summary>
        /// Флаг открытого файла или папки
        /// </summary>
        private bool isSelectedFile = true; // Флаг выбора файла или папки
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog openFolderDialog;
        private String startPath = "";
        private BackgroundWorker bw; // фоновый поток

        public MainFrame()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mp3-файлы (*.mp3)|*.mp3|flac-файлы (*.flac)|*.flac|Все файлы (*.*)|*.*";
            openFolderDialog = new FolderBrowserDialog();
            updateStartPath(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            checkTagsMS.Enabled = false;
        }

        // Событие Открыть файл
        private void openFileMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                checkStatusLabel.Text = "";
                checkTagsMenuItem.Enabled = true;
                checkNameMenuItem.Enabled = true;

                isSelectedFile = true;
                selectedPath = openFileDialog.FileName;
                print(selectedPath + "\n");
                start = infoField.TextLength;
                String str = showTags(selectedPath);
                print(str);
                length = infoField.TextLength - start;
                if(str.Contains("Пусто"))
                    words.Add(new PainterWord(start, length, 0));
                else
                    words.Add(new PainterWord(start, length, 1));
                infoField.Text += "\n";
                paintWords();
                updateStartPath(selectedPath.Substring(0, selectedPath.Length - openFileDialog.SafeFileName.Length));   
            }
        }
        // Событие Открыть папку
        private void openFolderMenuItem_Click(object sender, EventArgs e)
        {
            if (openFolderDialog.ShowDialog() != DialogResult.OK) return;
            checkStatusLabel.Text = "Выполнение";
            checkTagsMenuItem.Enabled = true;
            checkNameMenuItem.Enabled = true;

            isSelectedFile = false;
            selectedPath = openFolderDialog.SelectedPath;
            print("    Открыта папка: " + selectedPath + "\n");
           
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += progressChangedBW;
            bw.RunWorkerCompleted += runCompletedBW;
            bw.DoWork += openFolderBW;
            bw.RunWorkerAsync();
            
        }
        // Открыть папку. Считывает все названия папок и файлов в text
        private void openFolderBW(object sender, DoWorkEventArgs e)
        {
            text = "";
            String[] folderFileElements = Directory.GetFileSystemEntries(selectedPath);// элементы папки
            int progressLength = 2*folderFileElements.Length;
            int progress = 0;
            // Считывает подпапки из папки
            foreach (String elem in folderFileElements)
            {
                if (Path.GetExtension(elem) == "") text += elem+"\n";
                bw.ReportProgress(++progress * 100/ progressLength);
                
            }
            // Считывает музыкальные файлы
            string ext, str;
            foreach (String elem in folderFileElements)
            {
                ext = Path.GetExtension(elem);
                if (ext==".mp3" || ext==".flac")
                {
                    text += elem + "\n";
                    infoField.Invoke(new Action(() => { start = infoField.TextLength + text.Length; })); // Старт строки <Артис>-<Название>
                    str = showTags(elem) + "\n";
                    text += str;
                    infoField.Invoke(new Action(() => { length = infoField.TextLength + text.Length - start; })); // Длина строки <Артис>-<Название>                    
                    if (str.Contains("Пусто"))
                        words.Add(new PainterWord(start, length, 0));
                    else
                        words.Add(new PainterWord(start, length, 1));
                }
                bw.ReportProgress(++progress * 100 / progressLength);
            }
            // Путь корневой папки
            String[] foldersInPath = selectedPath.Split("\\"); // число папок в пути
            int rootFolderPathLength = 0;
            for (int i = 0; i < foldersInPath.Length - 1; i++)
                rootFolderPathLength += foldersInPath[i].Length + 1; // с учетом \
            updateStartPath(selectedPath.Substring(0, rootFolderPathLength));
            e.Cancel = true;
        }
       // Событие проверки тегов
        private void checkTagsMenuItem_Click(object sender, EventArgs e)
        {
            text = "";
            // файла
            if (isSelectedFile)
            {
                checkStatusLabel.Text = "";
                selectedPath = deleteRedudantSymbols(selectedPath);
                checkTags(selectedPath);
                infoField.Text += text;
                paintWords();
            }
            // папка
            else
            {
                infoField.Text += "    Проверка тегов файлов\n";
                checkStatusLabel.Text = "Выполнение";
                bw = new BackgroundWorker();
                bw.WorkerSupportsCancellation = true;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += progressChangedBW;
                bw.RunWorkerCompleted += runCompletedBW;
                bw.DoWork += checkTagsBW;
                bw.RunWorkerAsync();
                bw.CancelAsync();
            }
        }
        // Фоновая задача проверки тегов
        private void checkTagsBW(object sender, DoWorkEventArgs e)
        {
            String[] folderFileElements = Directory.GetFileSystemEntries(selectedPath);
            for (int i = 0; i < folderFileElements.Length; i++)
            {
                folderFileElements[i] = deleteRedudantSymbols(folderFileElements[i]);
                checkTags(folderFileElements[i]);
                bw.ReportProgress((i+1)*100/folderFileElements.Length);
            }
            e.Cancel = true;
        }
        // Проверяет наличие тегов Артист-Название и заполняет их из имени файла
        private bool checkTags(String file)
        {
            string ext = Path.GetExtension(file);
            if (ext != ".mp3" && ext != ".flac") return false;
            String NameOFTags1 = showTags(file);
            TagLib.File tags = TagLib.File.Create(file);
            // Проверяет название песни
            if (tags.Tag.Title == null)
            {
                int arg1 = file.LastIndexOf("-") + 2; // начало выреза
                int arg2 = file.Length - arg1 - 4; // число символов для выреза
                tags.Tag.Title = file.Substring(arg1, arg2); ;
                tags.Save();
            }
            // Проверяет исполнителя
            if (tags.Tag.FirstPerformer == null)
            {
                String fileName = Path.GetFileName(file);
                tags.Tag.Performers = new String[1] { fileName.Substring(0, fileName.LastIndexOf("-") - 1) };
                tags.Save();
            }
            String NameOFTags2 = showTags(file);
            if (!NameOFTags1.Equals(NameOFTags2))
                text += NameOFTags2;
            else
                text += NameOFTags1 + ": ";

            infoField.Invoke(new Action(() => { start = infoField.TextLength + text.Length; })); // Старт строки <Артис>-<Название>
            text += "правильные теги\n";
            infoField.Invoke(new Action(() => { length = infoField.TextLength + text.Length - start; })); // Длина строки <Артис>-<Название>                    
            words.Add(new PainterWord(start, length, 2));
            return true;
        }
        // Событие Проверка имени файла
        private void checkFileNameMenuItem_Click(object sender, EventArgs e)
        {
            selectedPath = checkFileName_Click(selectedPath, false);
        }
        // Событие Проверка имени файла альбома
        private void checkAlbFileNameMenuItem_Click(object sender, EventArgs e)
        {
            selectedPath = checkFileName_Click(selectedPath, true);
        }
        // Проверяет и изменяет имя файла на соответствие тегам
        private string checkFileName_Click(string filename, bool isAlbum)
        {
            text = "";
            if (isSelectedFile)
            {
                string newname = checkFileName(filename, isAlbum);
                if (newname == null) return filename;
                if (!filename.Equals(newname))
                {
                    filename = newname;
                    print(filename + "\n");
                }
                else
                {
                    print(filename + ": ");
                    start = infoField.TextLength + text.Length;
                    print("название файла соотвествует тегам\n");
                    length = infoField.TextLength + text.Length - start;
                    words.Add(new PainterWord(start, length, 2));
                }
                paintWords();
            }
            else
            {
                infoField.Text += "    Проверка соответствия имен файлов тегам\n";
                checkStatusLabel.Text = "Выполнение";

                bw = new BackgroundWorker();
                bw.DoWork += checkFileNameBW;

                bw.WorkerSupportsCancellation = true;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += progressChangedBW;
                bw.RunWorkerCompleted += runCompletedBW;

                bw.RunWorkerAsync(isAlbum);
                bw.CancelAsync();
            }
            return filename;
        }
        // Фоновая задача проверки имени файла
        private void checkFileNameBW(object sender, DoWorkEventArgs e)
        {
            text = "";
            String[] folderFileElements = Directory.GetFileSystemEntries(selectedPath);
            string newname, ext;
            for (int i = 0; i < folderFileElements.Length; i++)
            {
                ext = Path.GetExtension(folderFileElements[i]);
                if (ext!=".mp3" && ext!=".flac") continue;
                newname = checkFileName(folderFileElements[i], (bool)e.Argument);
                if (!folderFileElements[i].Equals(newname))
                {
                    folderFileElements[i] = newname;
                    text += folderFileElements[i] + "\n";
                }
                else if(newname == null)
                {
                    text += folderFileElements[i];
                }
                else
                {
                    text += folderFileElements[i] + ": ";
                    infoField.Invoke(new Action(() => { start = infoField.TextLength + text.Length; })); // Старт строки <Артис>-<Название>
                    text += "название соотвествует тегам\n";
                    infoField.Invoke(new Action(() => { length = infoField.TextLength + text.Length - start; })); // Длина строки <Артис>-<Название>                    
                    words.Add(new PainterWord(start, length, 2));
                }
                bw.ReportProgress((i+1) * 100 / folderFileElements.Length);
            }
            e.Cancel = true;
        }
        /// <summary>
        /// Проверяет и изменяет имя файла на соответствие тегам
        /// </summary>
        private string checkFileName(String filename, bool isAlbum)
        {
            string newname = filename.Substring(0, filename.Length - Path.GetFileName(filename).Length);
            TagLib.File tags = TagLib.File.Create(filename);
            // Проверяется наличие тегов
            if(tags.Tag.Performers == null)
            {
                print("Есть пустые теги");
                return null;
            }
            if (tags.Tag.Title == null)
            {
                print("Есть пустые теги");
                return null;
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
                 catch (System.IO.DirectoryNotFoundException exc)
                 {
                    print("Не могу создать название файла " + newname + "\n");
                 }
                catch(System.IO.IOException)
                {
                    print("Файл занят другим процессом " + newname + "\n");
                    return null;
                }
                  filename = newname;
            }
            newname = deleteRedudantSymbols(filename);
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
        /// Событие завершения фоновой задачи
        /// </summary>
        private void runCompletedBW(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                infoField.Text += text;
                paintWords();
                checkStatusLabel.Text = "Готово";
            }
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
        /// <summary>
        /// Удаляет лишние символы из названий
        /// </summary>
        private string deleteRedudantSymbols(string filename)
        {
            String newName;
            // Удаляет из имени файла -kissvk.com
            String kissVK = "-kissvk.com";
            if (filename.Contains(kissVK))
            {
                newName = filename.Remove(filename.IndexOf(kissVK), kissVK.Length);
                System.IO.File.Move(filename, newName);
                filename = newName;
            }
            // редактирует " - "
            if (filename.Contains("-") && !filename.Contains(" - "))
            {
                {
                    newName = filename.Replace("-", " - ");
                    System.IO.File.Move(filename, newName);
                    filename = newName;
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
            public int type;
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

    }
}