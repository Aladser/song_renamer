using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mp3RenamerV2
{
    public partial class MainFrame : Form
    {
        const int COLOR_RED = 0;
        const int COLOR_BLUE = 1;
        const int COLOR_GREEN = 2;                         
        /// <summary>
        /// ������ �������� ����������� ������
        /// </summary>
        List<String> files = new List<String>();
        /// <summary>
        /// ������ ����������� ����
        /// </summary>
        List<PainterWord> words = new List<PainterWord>();
        String textBuffer = ""; // �����

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
            openFileDialog.Filter = "mp3-����� (*.mp3)|*.mp3|flac-����� (*.flac)|*.flac|��� ����� (*.*)|*.*";
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
        /// ���������� �������� ������� ������
        /// </summary>
        private void progressChangedBW(object sender, ProgressChangedEventArgs e)
        {
            progressLabel.Text = (e.ProgressPercentage.ToString() + "%");
        }

        /// <summary>
        /// ������� ���������� ������� ������
        /// </summary>
        private void runCompletedBW(object sender, RunWorkerCompletedEventArgs e)
        {
            infoField.Text += textBuffer;
            paintWords();
            checkStatusLabel.Text = "������";
        }

        /// <summary>
        /// ���������� ���� �����
        /// </summary>
        private String showTags(String file)
        {
            String rslt = "";
            try
            {
                TagLib.File tagSong = TagLib.File.Create(file);
                rslt += "{";
                rslt += tagSong.Tag.FirstPerformer == null ? "�����" : tagSong.Tag.FirstPerformer;
                rslt += "} - {";
                rslt += tagSong.Tag.Title == null ? "�����" : tagSong.Tag.Title;
                rslt += "}";
                return rslt;
            } catch(TagLib.CorruptFileException) {
                return "";
            }
        }

        /// <summary>
        /// ��������� ����� �������� ����������
        /// </summary>
        private void updateStartPath(String value)
        {
            startPath = value;
            openFileDialog.InitialDirectory = startPath;

        }
        /// <summary>
        /// ������� ���������� � infoField
        /// </summary>
        private void print(String text)
        {
            infoField.Invoke(new Action(() => { infoField.Text += text; }));
        }
        /// <summary>
        /// ������� infoField
        /// </summary>
        private void clearInfoField_Click(object sender, EventArgs e)
        {
            infoField.Text = "";
            words.Clear();
        }

        // ������� '������� ����'
        private void openFileMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            openBW.RunWorkerAsync(openFileDialog.FileName);  
        }
        // ������� '������� �����'
        private void openFolderMenuItem_Click(object sender, EventArgs e)
        {
            if (openFolderDialog.ShowDialog() != DialogResult.OK) return;
            print("    ������� �����: " + openFolderDialog.SelectedPath + "\n");
            openBW.RunWorkerAsync(openFolderDialog.SelectedPath);
        }
        // ������� ������ �������� ����� ��� �����
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
            // ������� �����
            if (pathExt == "")
            {
                folderFileElements = Directory.GetFileSystemEntries(path);
            }
            // ������ ����
            else
            {
                folderFileElements = new string[1];
                folderFileElements[0] = path;
            }
            int progressLength = folderFileElements.Length;
            int progress = 0;
            // ��������� ����� � ����������� �����
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
                    printAndAddWordForPainting(tagsName, tagsName.Contains("�����") ? COLOR_RED : COLOR_BLUE, true);
                    textBuffer += "\n";
                }
                openBW.ReportProgress(++progress * 100 / progressLength);
            }
            // ��������� ����� �������� ����������
            string[] foldersInPath = path.Split('\\'); // ����� ����� � ����
            int rootFolderPathLength = 0;
            for (int i = 0; i < foldersInPath.Length - 1; i++)
                rootFolderPathLength += foldersInPath[i].Length + 1; // � ������ \
            updateStartPath(path.Substring(0, rootFolderPathLength));
        }
        
        // ������� '�������� �����'
        private void checkTagsMenuItem_Click(object sender, EventArgs e)
        {
            if (files.Count > 1)
                print("    �������� ����� ������\n");
            else if (files.Count == 0)
            {
                print("    ��� ������\n");
                return;
            }
            checkStatusLabel.Text = "����������";
            checkTagsBW.RunWorkerAsync();
        }
        // ������� ������ �������� �����
        private void checkTagsTask(object sender, DoWorkEventArgs e)
        {
            textBuffer = "";
            int start, length;
            string name;
            for (int i = 0; i < files.Count; i++)
            {
                // �������� -kissvk.com �� ��������
                if (files[i].Contains("-kissvk.com"))
                {
                    name = files[i].Remove(files[i].IndexOf("-kissvk.com"), 11);
                    System.IO.File.Move(files[i], name);
                    files[i] = name;
                }
                // ������ '-' �� ' - '
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
                            print("������� ���: " + files[i] + "\n");
                            printAndAddWordForPainting(name + " ��� ����������\n", COLOR_RED, true);
                            return;
                        }
                    }
                }
                // ��������� �������� �����, �������� �� �������� ����� ��� �����������
                try
                {
                    TagLib.File tags = TagLib.File.Create(files[i]);
                    if (tags.Tag.Title == null)
                    {
                        start = files[i].LastIndexOf("-") + 2; // ������ ������
                        length = files[i].Length - start - 4; // ����� �������� ��� ������, 4 - ����� ����������
                        tags.Tag.Title = files[i].Substring(start, length);
                        tags.Save();
                    }
                    // ��������� �����������, �������� �� ��������� �������� �����
                    if (tags.Tag.FirstPerformer == null)
                    {
                        name = Path.GetFileName(files[i]);
                        tags.Tag.Performers = new String[1] { name.Substring(0, name.LastIndexOf("-") - 1) };
                        tags.Save();
                    }
                    textBuffer += showTags(files[i]);
                    printAndAddWordForPainting("   ���������� ����", COLOR_GREEN, true);
                    textBuffer += "\n";
                    checkTagsBW.ReportProgress((i + 1) * 100 / files.Count);
                }
                catch (System.IO.FileNotFoundException)
                {
                    printAndAddWordForPainting(files[i] + ": �� ���� ����� ����\n", COLOR_RED, true);
                }
                catch(System.ArgumentOutOfRangeException)
                {
                    continue;
                }
            }
        }
        
        // ������� '�������� ����� �����'
        private void checkFileNameMenuItem_Click(object sender, EventArgs e)
        {
            checkFileName_Click(false);
        }
        // ������� '�������� ����� ����� �������'
        private void checkAlbFileNameMenuItem_Click(object sender, EventArgs e)
        {
            checkFileName_Click(true);
        }
        // ����� ������� '�������� ����� �����'
        private void checkFileName_Click(bool isAlbum)
        {
            if (files.Count > 1)
                print("    �������� ������������ ���� ������ �����\n");
            else if (files.Count == 0)
            {
                print("    ��� ������\n");
                return;
            }
            checkStatusLabel.Text = "����������";
            checkFilenameBW.RunWorkerAsync(isAlbum);
        }
        // ������� ������ �������� ����� �����
        private void checkFileNameTask(object sender, DoWorkEventArgs e)
        {
            textBuffer = "";
            bool isAlbum = (bool)e.Argument;
            string newFilepath;
            int progress = 0;
            bool isTags;
            for (int i = 0; i < files.Count; i++)
            {
                isTags = false;
                newFilepath = files[i].Substring(0, files[i].Length - Path.GetFileName(files[i]).Length); // ���������� ������ ����
                try
                {
                    TagLib.File tags = TagLib.File.Create(files[i]);
                    // ����������� ������� �����
                    if (tags.Tag.Performers == null || tags.Tag.Title == null) {
                        // ������� �������� �����

                        string newFilename = Path.GetFileName(files[i]).Replace("-kissvk.com", "");
                        newFilepath += newFilename;
                    } else {
                        // �������� ����� ����� �� �����

                        isTags = true;
                        // ������������ ����� ����� ��� ����� �� �������
                        if (isAlbum)
                        {
                            if (tags.Tag.Track < 10)
                                newFilepath += "0" + tags.Tag.Track + ". ";
                            else
                                newFilepath += tags.Tag.Track + ". ";
                        }
                        // ������������ � ��� ����� �����������, ��������, ������
                        newFilepath += tags.Tag.Performers[0] + " - " + tags.Tag.Title;
                        newFilepath += Path.GetExtension(files[i]) == ".mp3" ? ".mp3" : ".flac";
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    printAndAddWordForPainting(files[i] + ": �� ���� ����� ����\n", COLOR_RED, true);
                    continue;
                }
                catch(TagLib.CorruptFileException)
                {
                    printAndAddWordForPainting(files[i] + ": ��� ��������� ����� � �����\n", COLOR_RED, true);
                    continue;
                }
                // ��������������
                if (!files[i].Equals(newFilepath))
                {
                    try
                    {
                        System.IO.File.Move(files[i], newFilepath);
                        files[i] = newFilepath;
                        textBuffer += files[i] + "\n";
                        if (!isTags)
                        {
                            printAndAddWordForPainting("������ ����\n", COLOR_RED, true);
                        }
                    } 
                    catch (DirectoryNotFoundException) {
                        textBuffer += files[i];
                        printAndAddWordForPainting("  ����� �� �������\n", COLOR_RED, true);
                    } 
                    catch (IOException) 
                    {
                        textBuffer += files[i];
                        printAndAddWordForPainting("   ���� ������ � ������ ���������\n", COLOR_RED, true);
                    } 
                    catch(NotSupportedException)
                    {
                        textBuffer += files[i];
                        printAndAddWordForPainting("  ������ �������������� �����\n", COLOR_RED, true);
                    }
                    catch (System.ArgumentException) 
                    {
                        textBuffer += files[i];
                        printAndAddWordForPainting("  ������������ ����� � ����� �����\n", COLOR_RED, true);
                    }
                }
                else
                {
                    textBuffer += files[i];
                    if (!isTags) {
                        printAndAddWordForPainting("  -  ������ ����\n", COLOR_GREEN, true);
                    } else {
                        printAndAddWordForPainting("   -   �������� ������������� �����\n", COLOR_GREEN, true);
                    }
                }
                checkFilenameBW.ReportProgress(++progress * 100 / files.Count);
            }
        }

        /// <summary>
        /// ����� ������������ �����
        /// </summary>
        private struct PainterWord {
            public int start;
            public int length;
            public int type; // 0-������� ���� 1-����� ���� 2-������� ����
            public PainterWord(int start, int length, int type)
            {
                this.start = start;
                this.length = length;
                this.type = type;
            }
        }
        /// <summary>
        /// ������ �����
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
            // ����������� ����������� ����
            infoField.SelectionStart = infoField.TextLength;
            infoField.SelectionLength = 0;
        }
        /// <summary>
        /// ��������� ����� ��� ���������
        /// </summary>
        /// <param name="word"> ����� </param>
        /// <param name="isBuffer"> true - ���� ����� ��� ������ </param>
        private void printAndAddWordForPainting(string word, int color, bool isBuffer)
        {
            int start=0;
            infoField.Invoke(new Action(() => { start = infoField.TextLength + textBuffer.Length; })); // ����� ������
            words.Add(new PainterWord(start, word.Length, color));
            if (isBuffer)
                textBuffer += word;
            else
                print(word + "\n");
        }
    }
}