using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using System.ComponentModel;
using Synchronizer.CheckTree;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Collections.ObjectModel;

namespace Synchronizer.Tasks
{
    /// <summary>
    /// Interaction logic for SyncPage.xaml
    /// </summary>
    public partial class TaskView : UserControl, INotifyPropertyChanged
    {

        #region Fields

        /// <summary>
        /// Информация о задании
        /// </summary>
        private TaskData m_data;

        /// <summary>
        /// Дерево папок и файлов из левой части
        /// </summary>
        private TreeModel m_leftDir;

        /// <summary>
        /// Дерево папок и файлов из правой части
        /// </summary>
        private TreeModel m_rightDir;

        /// <summary>
        /// Имя файла настроек
        /// </summary>
        private String m_lastPathsFile = System.IO.Path.GetTempPath() + "paths.txt";

        /// <summary>
        /// Список ранее вводимых путей в левое окно
        /// </summary>
        private ObservableCollection<String> m_leftLastPaths;

        /// <summary>
        /// Список ранее вводимых путей в правое окно
        /// </summary>
        private ObservableCollection<String> m_rightLastPaths;

        /// <summary>
        /// Поток выполнения задания
        /// </summary>
        private Thread m_task;

        /// <summary>
        /// Прогресс выполнения задания
        /// </summary>
        private Double m_progress;

        /// <summary>
        /// Величина прироста прогресса выполнения задания
        /// </summary>
        private Double m_progressInc;

        #endregion

        #region Properties

        /// <summary>
        /// Получение данных задания
        /// </summary>
        public TaskData Data
        {
            get { return m_data; }
        }

        /// <summary>
        /// Получение/задание списка объектов из левой части
        /// </summary>
        public List<TreeModel> LeftList
        {
            get 
            {
                List<TreeModel> result = new List<TreeModel>();
                if (m_leftDir != null) result.Add(m_leftDir);
                return result;
            }
        }

        /// <summary>
        /// Получение/задание списка объектов из правой части
        /// </summary>
        public List<TreeModel> RightList
        {
            get
            {
                List<TreeModel> result = new List<TreeModel>();
                if (m_rightDir != null) result.Add(m_rightDir);
                return result;
            }
        }

        /// <summary>
        /// Получение/задание корневой папки из левой части
        /// </summary>
        public String LeftPath
        {
            get { return m_data.LeftPath; }
            set { m_data.LeftPath = value; }
        }

        /// <summary>
        /// Получение/задание корневой папки из правой части
        /// </summary>
        public String RightPath
        {
            get { return m_data.RightPath; }
            set { m_data.RightPath = value; }
        }

        /// <summary>
        /// Получение/задание списка предыдущих папок в левом окне
        /// </summary>
        public ObservableCollection<String> LeftLastPaths
        {
            get { return m_leftLastPaths; }
            set { m_leftLastPaths = value; }
        }

        /// <summary>
        /// Получение/задание списка предыдущих папок в правом окне
        /// </summary>
        public ObservableCollection<String> RightLastPaths
        {
            get { return m_rightLastPaths; }
            set { m_rightLastPaths = value; }
        }

        #endregion

        #region Events
        
        /// <summary>
        /// Событие обновления статуса
        /// </summary>
        public event Action<TaskView, String> StatusUpdate;

        /// <summary>
        /// Событие обновления полосы прогресса
        /// </summary>
        public event Action<TaskView, Double> ProgressUpdate;

        #endregion

        public TaskView(TaskData data)
        {
            m_data = data;
            
            LoadLastPaths();
            LeftPath = data.LeftPath;
            RightPath = data.RightPath;
            
            InitializeComponent();
            
            OnPropertyChanged("LeftPath");
            OnPropertyChanged("RightPath");
            OnPropertyChanged("NameWithFlags");
            OnPropertyChanged("Children");

        }

        /// <summary>
        /// Прерывание выполнения текущего задания
        /// </summary>
        public void Interrupt()
        {
            if (m_task != null && m_task.IsAlive) m_task.Interrupt();
        }

        /// <summary>
        /// Загрузка ранее вводимых путей
        /// </summary>
        private void LoadLastPaths()
        {
            m_leftLastPaths = new ObservableCollection<String>();
            m_rightLastPaths = new ObservableCollection<String>();
            FileInfo info = new FileInfo(m_lastPathsFile);
            if (info.Exists)
            {
                StreamReader sr = new StreamReader(new FileStream(m_lastPathsFile, FileMode.Open));
                while (!sr.EndOfStream)
                {
                    String str = sr.ReadLine();
                    if (str.StartsWith("l_")) m_leftLastPaths.Add(str.Substring(2));
                    else if (str.StartsWith("r_")) m_rightLastPaths.Add(str.Substring(2));
                }
                sr.Close();
            }
        }

        /// <summary>
        /// Сохранение ранее вводимых путей
        /// </summary>
        private void SaveLastPaths()
        {
            StreamWriter sw = new StreamWriter(new FileStream(m_lastPathsFile, FileMode.Create));            
            foreach (String lastPath in m_leftLastPaths)
            {
                sw.WriteLine("l_" + lastPath);
            }
            foreach (String lastPath in m_rightLastPaths)
            {
                sw.WriteLine("r_" + lastPath);
            }
            sw.Close();
        }

        /// <summary>
        /// Вызов обновления статуса
        /// </summary>
        /// <param name="str"></param>
        private void InvokeStatusUpdate(String str)
        {
            if (StatusUpdate != null) StatusUpdate(this, str);
        }

        /// <summary>
        /// Вызов обновления полосы прогресса
        /// </summary>
        /// <param name="value"></param>
        private void InvokeProgressUpdate(Double value)
        {
            if (ProgressUpdate != null) ProgressUpdate(this, value);
        }
                
        #region Analyze

        /// <summary>
        /// Анализ задания
        /// </summary>
        /// <param name="fullRefresh"></param>
        public void Analyze()
        {
            if (m_task != null && m_task.ThreadState == ThreadState.Running)
            {
                InvokeStatusUpdate("Stop current task or wait for results");
                return;
            }
            //Проверка существования директорий
            if (!Directory.Exists(LeftPath) || !Directory.Exists(RightPath))
            {
                InvokeStatusUpdate("Select correct paths");
                return;
            }
            DirectoryInfo leftInfo = new DirectoryInfo(LeftPath);
            DirectoryInfo rightInfo = new DirectoryInfo(RightPath);            
            //Сохранение директорий в файл
            if (m_leftLastPaths.Contains(LeftPath)) m_leftLastPaths.Move(m_leftLastPaths.IndexOf(LeftPath), 0);
            else m_leftLastPaths.Insert(0, LeftPath);
            comboBoxLeft.SelectedIndex = 0;
            if (m_rightLastPaths.Contains(RightPath)) m_rightLastPaths.Move(m_rightLastPaths.IndexOf(RightPath), 0);
            else m_rightLastPaths.Insert(0, RightPath);
            comboBoxRight.SelectedIndex = 0;
            SaveLastPaths();
            OnPropertyChanged("LeftLastPaths");
            OnPropertyChanged("RightLastPaths");
            //Analyze trees
            m_task = new Thread(() =>
            {
                try
                {
                    InvokeStatusUpdate("Analyze started");

                    InvokeProgressUpdate(-1);

                    m_leftDir = new TreeModel(leftInfo, null);
                    m_rightDir = new TreeModel(rightInfo, null);

                    InvokeProgressUpdate(0);

                    m_progress = m_progressInc = 100 / (double)m_leftDir.InsideCount;
                    
                    Analyze_CompareFolders(m_leftDir, m_rightDir);
                    TreeModel.CompareParameters(m_leftDir, m_rightDir);
                    m_data.IsAnalyzed = true;

                    InvokeProgressUpdate(100);
                    InvokeStatusUpdate("Analyze completed");
                }
                catch (Exception ex)
                {
                    m_leftDir = null;
                    m_rightDir = null;
                    m_data.IsAnalyzed = false;
                    InvokeProgressUpdate(0);
                    InvokeStatusUpdate("Analyze stopped");
                }
                OnPropertyChanged("LeftList");
                OnPropertyChanged("RightList");
            });
            m_task.Start();
        }

        /// <summary>
        /// Сравнение двух директорий
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void Analyze_CompareFolders(TreeModel left, TreeModel right)
        {
            Thread.Sleep(1);
            foreach (TreeModel l_node in left.Children)
            {
                TreeModel r_node = null;
                foreach (TreeModel node in right.Children)
                {
                    if (l_node.ShortName.ToLower().Equals(node.ShortName.ToLower()))
                    {
                        r_node = node;
                        break;
                    }
                }
                if (r_node != null)
                {
                    if (l_node.IsDirectory) Analyze_CompareFolders(l_node, r_node);
                    TreeModel.CompareParameters(l_node, r_node);
                }
                else m_progress += l_node.InsideCount * m_progressInc;
                //Обновление прогресса
                m_progress += m_progressInc;
                InvokeProgressUpdate(m_progress);
            }
        }

        #endregion

        #region Synchronize

        public void Synchronize()
        {
            if (m_task != null && m_task.ThreadState == ThreadState.Running)
            {
                InvokeStatusUpdate("Stop current task or wait for results");
                return;
            }
            if (!m_data.IsAnalyzed)
            {
                new InfoOK("Execute Analyze first!").ShowDialog();
                return;
            }
            //Запрос на разрешение зеркального удаления файлов
            bool? res = new DeleteFilesDialog().ShowExtendedDialog();
            if (!res.HasValue) return;

            m_data.IsAnalyzed = false;
            bool fromLeftToRight = comboBoxDirection.SelectedIndex == 0;
            m_task = new Thread(() =>
                {
                    try
                    {
                        //копирование файлов
                        InvokeStatusUpdate("Synchronize started");
                        InvokeProgressUpdate(0);
                        if (fromLeftToRight)
                        {
                            m_progress = m_progressInc = 100 / (double)m_leftDir.SelectedCount;
                            Synchronize_CopyObjects(m_leftDir, m_leftDir.FullName, m_rightDir.FullName);
                        }
                        else
                        {
                            m_progress = m_progressInc = 100 / (double)m_rightDir.SelectedCount;
                            Synchronize_CopyObjects(m_rightDir, m_rightDir.FullName, m_leftDir.FullName);
                        }
                        //удаление файлов
                        if (res.Value)
                        {
                            InvokeProgressUpdate(0);
                            if (fromLeftToRight)
                            {
                                m_progress = m_progressInc = 100 / (double)m_rightDir.ExclusiveCount;
                                Synchronize_DeleteObjects(m_rightDir);
                            }
                            else
                            {
                                m_progress = m_progressInc = 100 / (double)m_leftDir.ExclusiveCount;
                                Synchronize_DeleteObjects(m_leftDir);
                            }
                        }

                        InvokeProgressUpdate(100);
                        m_leftDir = new TreeModel(new DirectoryInfo(LeftPath), null);
                        m_rightDir = new TreeModel(new DirectoryInfo(RightPath), null);
                        String result = String.Format("{0,0:N0}B | {1,0:N0}B\nDifference: {2,0:N0}B", m_leftDir.Length, m_rightDir.Length, 
                            Math.Abs(m_leftDir.Length - m_rightDir.Length));
                        Dispatcher.Invoke(new Action(() => new InfoOK(result).ShowDialog()));                        
                        InvokeStatusUpdate("Synchronize completed");
                    }
                    catch (Exception ex)
                    {   
                        m_leftDir = null;
                        m_rightDir = null;                        
                        InvokeStatusUpdate("Synchronize stopped");
                        OnPropertyChanged("LeftList");
                        OnPropertyChanged("RightList");
                    }
                });
            m_task.Start();
        }
        
        /// <summary>
        /// Копирование файлов с перезаписью существующих
        /// </summary>
        /// <param name="node"></param>
        public void Synchronize_CopyObjects(TreeModel node, String fromHead, String toHead)
        {
            Thread.Sleep(1);
            foreach (TreeModel l_node in node.Children)
            {
                if ((!l_node.IsChecked.HasValue) || (l_node.IsChecked.Value))
                {
                    String newpath = "";
                    try
                    {
                        newpath = toHead;
                        newpath += l_node.FullName.Substring(fromHead.Length);
                    }
                    catch (Exception ex)
                    {
                        InvokeStatusUpdate(String.Format("Error in file name: {0}", l_node.FullName));
                    }

                    if (l_node.IsDirectory)
                    {
                        if (!Directory.Exists(newpath))
                        {
                            InvokeStatusUpdate(String.Format("Create directory: \"{0}\"", newpath));
                            try
                            {
                                Directory.CreateDirectory(newpath);
                            }
                            catch (Exception ex)
                            {
                                InvokeStatusUpdate(String.Format("Error create directory: {0}", l_node.FullName));
                            }
                        }
                        Synchronize_CopyObjects(l_node, fromHead, toHead);
                    }
                    else
                    {
                        InvokeStatusUpdate(String.Format("Copy file: \"{0}\" to \"{1}\"", l_node.FullName, newpath));
                        try
                        {
                            File.Copy(l_node.FullName, newpath, true);
                        }
                        catch (Exception ex)
                        {
                            //Если нет возможности перезаписать файл - изменение его атрибутов
                            if (File.Exists(newpath))
                            {
                                try
                                {
                                    File.SetAttributes(newpath, FileAttributes.Normal);
                                    File.Copy(l_node.FullName, newpath, true);
                                }
                                catch (Exception ex1)
                                {
                                    InvokeStatusUpdate(String.Format("Error copy file: {0}", l_node.FullName));
                                }
                            }
                            else
                            {
                                InvokeStatusUpdate(String.Format("Error copy file: {0}", l_node.FullName));
                            }
                        }
                    }
                    m_progress += m_progressInc;
                    InvokeProgressUpdate(m_progress);
                }
            }
        }

        /// <summary>
        /// Дублирование удаления файлов
        /// </summary>
        /// <param name="node"></param>
        /// <param name="count"></param>
        public void Synchronize_DeleteObjects(TreeModel node)
        {
            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                TreeModel r_node = node.Children[i];
                if (r_node.IsExclusiveAny)
                {
                    if ((!r_node.IsChecked.HasValue) || (r_node.IsChecked.Value))
                    {
                        if (r_node.IsDirectory)
                        {
                            Synchronize_DeleteObjects(r_node);

                            if (r_node.IsExclusive)
                            {
                                InvokeStatusUpdate(String.Format("Delete directory: \"{0}\"", r_node.FullName));
                                try
                                {
                                    if (File.GetAttributes(r_node.FullName).HasFlag(FileAttributes.ReadOnly))
                                        File.SetAttributes(r_node.FullName, File.GetAttributes(r_node.FullName) ^ FileAttributes.ReadOnly);
                                    Directory.Delete(r_node.FullName, false);
                                    node.Children.RemoveAt(i);
                                }
                                catch (Exception ex)
                                {
                                    InvokeStatusUpdate(String.Format("Error delete folder: {0}", r_node.FullName));
                                }
                            }
                        }
                        else
                        {
                            InvokeStatusUpdate(String.Format("Delete file: \"{0}\"", r_node.FullName));
                            try
                            {
                                if (File.GetAttributes(r_node.FullName).HasFlag(FileAttributes.ReadOnly))
                                    File.SetAttributes(r_node.FullName, File.GetAttributes(r_node.FullName) ^ FileAttributes.ReadOnly);
                                File.Delete(r_node.FullName);
                                node.Children.RemoveAt(i);
                            }
                            catch (Exception ex)
                            {
                                InvokeStatusUpdate(String.Format("Error delete file: {0}", r_node.FullName));
                            }
                        }
                    }
                    m_progress += m_progressInc;
                    InvokeProgressUpdate(m_progress);
                }
            }
        }

        #endregion

        private void leftBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = LeftPath;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_data.LeftPath = fbd.SelectedPath;
                OnPropertyChanged("LeftPath");
            }
        }

        private void rightBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = RightPath;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_data.RightPath = fbd.SelectedPath;
                OnPropertyChanged("RightPath");
            }
        }

        private void tree_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ((sender as TreeView).Parent as ScrollViewer).LineUp();
                ((sender as TreeView).Parent as ScrollViewer).LineUp();
                ((sender as TreeView).Parent as ScrollViewer).LineUp();
            }
            else
            {
                ((sender as TreeView).Parent as ScrollViewer).LineDown();
                ((sender as TreeView).Parent as ScrollViewer).LineDown();
                ((sender as TreeView).Parent as ScrollViewer).LineDown();
            }

        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
