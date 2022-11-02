using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Mp3RenamerV2
{
    public partial class MainFrame : Form
    {
        const int COLOR_RED = 0;
        const int COLOR_BLUE = 1;
        const int COLOR_GREEN = 2;                         
        /// <summary>
        /// список открытых музыкальных файлов
        /// </summary>
        List<String> files = new List<String>();
        /// <summary>
        /// Список закрашенных слов
        /// </summary>
        List<PainterWord> words = new List<PainterWord>();
        String textBuffer = ""; // буфер

        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog openFolderDialog;
        private String startPath = "";
        private BackgroundWorker openBW;
        private BackgroundWorker checkTagsBW;
        private BackgroundWorker checkFilenameBW;

        public MainFrame()
        {
            InitializeComponent();
            MaximizeBox = false;
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
            openBW.DoWork += openExplorerElementTask;
            checkTagsBW.DoWork += checkTagsTask;
            checkFilenameBW.DoWork += checkFileNameTask;
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
            infoField.Text += textBuffer;
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
        /// <summary>
        /// Обновляет точку открытия Проводника
        /// </summary>
        private void updateStartPath(String value)
        {
            startPath = value;
            openFileDialog.InitialDirectory = startPath;

        }
        /// <summary>
        /// Выводит информацию в infoField
        /// </summary>
        private void print(String text)
        {
            infoField.Invoke(new Action(() => { infoField.Text += text; }));
        }
        /// <summary>
        /// Очищает infoField
        /// </summary>
        private void clearInfoField_Click(object sender, EventArgs e)
        {
            infoField.Text = "";
            words.Clear();
        }

        // Событие 'Открыть файл'
        private void openFileMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            openBW.RunWorkerAsync(openFileDialog.FileName);  
        }
        // Событие 'Открыть папку'
        private void openFolderMenuItem_Click(object sender, EventArgs e)
        {
            if (openFolderDialog.ShowDialog() != DialogResult.OK) return;
            print("    Открыта папка: " + openFolderDialog.SelectedPath + "\n");
            openBW.RunWorkerAsync(openFolderDialog.SelectedPath);
        }
        // Фоновая задача открытия папки или файла
        private void openExplorerElementTask(object sender, DoWorkEventArgs e)
        {
            checkStatusLabel.Invoke(new Action(() => { 
                checkStatusLabel.Text = "";
                checkTagsMenuItem.Enabled = true;
                checkNameMenuItem.Enabled = true;
            }));
            files.Clear();
            textBuffer = "";

            string path = (String)e.Argument;
            string pathExt = Path.GetExtension(path);
            string[] folderFileElements;
            // открыта папка
            if (pathExt == "")
            {
                folderFileElements = Directory.GetFileSystemEntries(path);
            }
            // открыт файл
            else
            {
                folderFileElements = new string[1];
                folderFileElements[0] = path;
            }
            int progressLength = folderFileElements.Length;
            int progress = 0;
            // Считывает папки и музыкальные файлы
            string ext, tagsName;
            foreach (String elem in folderFileElements)
            {
                ext = Path.GetExtension(elem);
                if (ext=="")
                    textBuffer += elem + "\n";
                else if (ext==".mp3" || ext==".flac")
                {
                    files.Add(elem);
                    textBuffer += elem + "\n";
                    tagsName = showTags(elem);
                    printAndAddWordForPainting(tagsName, tagsName.Contains("Пусто") ? COLOR_RED : COLOR_BLUE, true);
                    textBuffer += "\n";
                }
                openBW.ReportProgress(++progress * 100 / progressLength);
            }
            // Обновляет место открытия проводника
            string[] foldersInPath = path.Split('\\'); // число папок в пути
            int rootFolderPathLength = 0;
            for (int i = 0; i < foldersInPath.Length - 1; i++)
                rootFolderPathLength += foldersInPath[i].Length + 1; // с учетом \
            updateStartPath(path.Substring(0, rootFolderPathLength));
        }
        
        // Событие 'Проверка тегов'
        private void checkTagsMenuItem_Click(object sender, EventArgs e)
        {
            if (files.Count > 1)
                print("    Проверка тегов файлов\n");
            else if (files.Count == 0)
            {
                print("    Нет файлов\n");
                return;
            }
            checkStatusLabel.Text = "Выполнение";
            checkTagsBW.RunWorkerAsync();
        }
        // Фоновая задача проверки тегов
        private void checkTagsTask(object sender, DoWorkEventArgs e)
        {
            textBuffer = "";
            int start, length;
            string name;
            for (int i = 0; i < files.Count; i++)
            {
                // удаление -kissvk.com из названия
                if (files[i].Contains("-kissvk.com"))
                {
                    name = files[i].Remove(files[i].IndexOf("-kissvk.com"), 11);
                    System.IO.File.Move(files[i], name);
                    files[i] = name;
                }
                // замена '-' на ' - '
                if (files[i].Contains("-") && !files[i].Contains(" - "))
                {
                    {
                        name = files[i].Replace("-", " - ");
                        if (!System.IO.File.Exists(name))
                        {
                            System.IO.File.Move(files[i], name);
                            files[i] = name;
                        }
                        else
                        {
                            print("Текущее имя: " + files[i] + "\n");
                            printAndAddWordForPainting(name + " уже существует\n", COLOR_RED, true);
                            return;
                        }
                    }
                }
                // Проверяет название песни, вырезает из названия файла при отстутствии
                try
                {
                    TagLib.File tags = TagLib.File.Create(files[i]);
                    if (tags.Tag.Title == null)
                    {
                        start = files[i].LastIndexOf("-") + 2; // начало выреза
                        length = files[i].Length - start - 4; // число символов для выреза, 4 - длина расширения
                        tags.Tag.Title = files[i].Substring(start, length);
                        tags.Save();
                    }
                    // Проверяет исполнителя, вырезает из короткого названия файла
                    if (tags.Tag.FirstPerformer == null)
                    {
                        name = Path.GetFileName(files[i]);
                        tags.Tag.Performers = new String[1] { name.Substring(0, name.LastIndexOf("-") - 1) };
                        tags.Save();
                    }
                    textBuffer += showTags(files[i]);
                    printAndAddWordForPainting("   правильные теги", COLOR_GREEN, true);
                    textBuffer += "\n";
                    checkTagsBW.ReportProgress((i + 1) * 100 / files.Count);
                }
                catch (System.IO.FileNotFoundException)
                {
                    printAndAddWordForPainting(files[i] + ": не могу найти файл\n", COLOR_RED, true);
                }
            }
        }
        
        // Событие 'Проверка имени файла'
        private void checkFileNameMenuItem_Click(object sender, EventArgs e)
        {
            checkFileName_Click(false);
        }
        // Событие 'Проверка имени файла альбома'
        private void checkAlbFileNameMenuItem_Click(object sender, EventArgs e)
        {
            checkFileName_Click(true);
        }
        // Общее событие 'Проверка имени файла'
        private void checkFileName_Click(bool isAlbum)
        {
            if (files.Count > 1)
                print("    Проверка соответствия имен файлов тегам\n");
            else if (files.Count == 0)
            {
                print("    Нет файлов\n");
                return;
            }
            checkStatusLabel.Text = "Выполнение";
            checkFilenameBW.RunWorkerAsync(isAlbum);
        }
        // Фоновая задача проверки имени файла
        private void checkFileNameTask(object sender, DoWorkEventArgs e)
        {
            textBuffer = "";
            bool isAlbum = (bool)e.Argument;
            string newFilename;
            int progress = 0;
            for (int i = 0; i < files.Count; i++)
            {
                newFilename = files[i].Substring(0, files[i].Length - Path.GetFileName(files[i]).Length); // копируется корень пути
                try
                {
                    TagLib.File tags = TagLib.File.Create(files[i]);
                    // Проверяется наличие тегов
                    if (tags.Tag.Performers == null || tags.Tag.Title == null)
                    {
                        textBuffer += files[i];
                        printAndAddWordForPainting("   пустые теги\n", COLOR_RED, true);
                        continue;
                    }
                    // Записывается номер трека для файла из альбома
                    if (isAlbum)
                    {
                        if (tags.Tag.Track < 10)
                            newFilename += "0" + tags.Tag.Track + ". ";
                        else
                            newFilename += tags.Tag.Track + ". ";
                    }
                    // Дописывается в имя файла Исполнитель, Название, Формат
                    newFilename += tags.Tag.Performers[0] + " - " + tags.Tag.Title;
                    newFilename += Path.GetExtension(files[i]) == ".mp3" ? ".mp3" : ".flac";
                }
                catch (System.IO.FileNotFoundException)
                {
                    printAndAddWordForPainting(files[i] + ": не могу найти файл\n", COLOR_RED, true);
                    continue;
                }
                // Переименование
                if (!files[i].Equals(newFilename))
                {
                    try
                    {
                        System.IO.File.Move(files[i], newFilename);
                        files[i] = newFilename;
                        textBuffer += files[i] + "\n";
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        textBuffer += files[i];
                        printAndAddWordForPainting("   DirectoryNotFoundException\n", COLOR_RED, true);
                    }
                    catch (System.IO.IOException)
                    {
                        textBuffer += files[i];
                        printAndAddWordForPainting("   System.IO.IOException: возможно, файл открыт в другой программе\n", COLOR_RED, true);
                    }
                }
                else
                {
                    textBuffer += files[i];
                    printAndAddWordForPainting("   название соответствует тегам\n", COLOR_GREEN, true);
                }
                checkFilenameBW.ReportProgress(++progress * 100 / files.Count);
            }
        }

        /// <summary>
        /// Класс закрашенного слова
        /// </summary>
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
        /// <summary>
        /// Красит слова
        /// </summary>
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
            infoField.Invoke(new Action(() => { start = infoField.TextLength + textBuffer.Length; })); // Старт строки
            words.Add(new PainterWord(start, word.Length, color));
            if (isBuffer)
                textBuffer += word;
            else
                print(word + "\n");
        }
    }
}