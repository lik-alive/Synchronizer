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
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using Synchronizer.Menu;
using Synchronizer.Tasks;
using WpfControls;

namespace Synchronizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Список добавленных заданий
        /// </summary>
        private List<TaskView> m_tasks = new List<TaskView>();
                
        #endregion

        #region Properties

        /// <summary>
        /// Получение статуса создания проекта
        /// </summary>
        public Boolean ProjCreated
        {
            get { return m_tasks.Count != 0; }
        }
        
        #endregion

        public MainWindow()
        {   
            InitializeComponent();

            this.Loaded += new RoutedEventHandler((object sender, RoutedEventArgs e) => { LoadLastState(); });
            this.Closed += new EventHandler((object sender, EventArgs e) => { SaveLastState(); });
        }


        /// <summary>
        /// Загрузка последнего состояния
        /// </summary>
        private void LoadLastState()
        {            
            String lastStateFile = System.IO.Path.GetTempPath() + "lastState.inf";
            if (File.Exists(lastStateFile)) LoadFrom(lastStateFile);
        }

        /// <summary>
        /// Сохранение последнего состояния
        /// </summary>
        private void SaveLastState()
        {
            String lastStateFile = System.IO.Path.GetTempPath() + "lastState.inf";
            SaveTo(lastStateFile);
        }
        
        #region ProjFile Commands
        
        /// <summary>
        /// Обработка события создания нового проекта
        /// </summary>
        private void Menu_New(object sender, ExecutedRoutedEventArgs e)
        {
            ClearDesk();
        }

        /// <summary>
        /// Обработка события загрузки проекта
        /// </summary>
        private void Menu_Load(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Project files (*.synp)|*.synp";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) LoadFrom(ofd.FileName);
        }


        /// <summary>
        /// Открытие заданий из файла
        /// </summary>
        /// <param name="filename"></param>
        private void LoadFrom(String filename)
        {
            ClearDesk();
            StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open));
            while (!sr.EndOfStream)
            {
                TaskData task = new TaskData();
                task.Load(sr);
                AddTask(task);
            }
            sr.Close();
        }

        /// <summary>
        /// Обработка события сохранения проекта
        /// </summary>
        private void Menu_Save(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "Project files (*.synp)|*.synp";            
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) SaveTo(sfd.FileName);
        }

        /// <summary>
        /// Сохранение заданий в файл
        /// </summary>
        /// <param name="filename"></param>
        private void SaveTo(String filename)
        {
            StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create));
            m_tasks.ForEach(d => d.Data.Save(sw));
            sw.Close();
        }

        #endregion

        #region Project Menu

        /// <summary>
        /// Обработка события анализа текущего задания
        /// </summary>
        private void Menu_Analyze(object sender, RoutedEventArgs e)
        {
            ((tasksPanel.SelectedItem as TabItem).Content as TaskView).Analyze();
            if (ProjCreated) progressComboBox.SelectedIndex = tasksPanel.SelectedIndex;
        }

        /// <summary>
        /// Обработка события синхронизации текущего задания
        /// </summary>
        private void Menu_Synchronize(object sender, RoutedEventArgs e)
        {
            ((tasksPanel.SelectedItem as TabItem).Content as TaskView).Synchronize();
            if (ProjCreated) progressComboBox.SelectedIndex = tasksPanel.SelectedIndex;
        }

        /// <summary>
        /// Обработка события остановки текущего действия
        /// </summary>
        private void Menu_Stop(object sender, RoutedEventArgs e)
        {
            if (ProjCreated)
            {
                ((tasksPanel.SelectedItem as TabItem).Content as TaskView).Interrupt();
                progressComboBox.SelectedIndex = tasksPanel.SelectedIndex;
            }
        }

        /// <summary>
        /// Обработка события анализа текущего задания
        /// </summary>
        private void Menu_AnalyzeAll(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < tasksPanel.Items.Count-1; i++ )
            {
                ((tasksPanel.Items[i] as TabItem).Content as TaskView).Analyze();
            }
            if (ProjCreated) progressComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Обработка события синхронизации текущего задания
        /// </summary>
        private void Menu_SynchronizeAll(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < tasksPanel.Items.Count - 1; i++)
            {
                ((tasksPanel.Items[i] as TabItem).Content as TaskView).Synchronize();
            }
            if (ProjCreated) progressComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Обработка события остановки текущего действия
        /// </summary>
        private void Menu_StopAll(object sender, RoutedEventArgs e)
        {
            if (ProjCreated)
            {
                for (int i = 0; i < tasksPanel.Items.Count - 1; i++)
                {
                    ((tasksPanel.Items[i] as TabItem).Content as TaskView).Interrupt();
                }
                progressComboBox.SelectedIndex = 0;
            }
        }
        
        #endregion

        #region Helpers

        /// <summary>
        /// Очистка листа заданий
        /// </summary>
        private void ClearDesk()
        {
            Menu_Stop(null, null);
            m_tasks.Clear();
            while (tasksPanel.Items.Count > 1)
            {
                progressComboBox.Items.RemoveAt(0);
                tasksPanel.Items.RemoveAt(0);
            }
        }

        #endregion
        
        #region Tasks Control

        /// <summary>
        /// Обработка нажатия на кнопку добавления заданий
        /// Запрос названия задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void taskAdd_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TaskTitleDialog dialog = new TaskTitleDialog();
            bool? accepted = dialog.ShowDialog();
            if (accepted.HasValue && accepted.Value) AddTask(new TaskData(dialog.TaskTitle));
        }

        /// <summary>
        /// Добавление задания в очередь
        /// </summary>
        /// <param name="data"></param>
        private void AddTask(TaskData data)
        {   
            //формирование окна с заданием
            CloseableTabItem item = new CloseableTabItem();
            item.Header = data.Title;
            TaskView content = new TaskView(data);
            content.StatusUpdate += content_StatusUpdate;
            content.ProgressUpdate += content_ProgressUpdate;
            item.Content = content;
            item.OnClose += delete_Click;
            m_tasks.Add(content);
            //добавление полосы прогресса
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = progressBar.Maximum;
            progressBar.Width = 180;
            progressBar.Height = 14;
            progressComboBox.Items.Add(progressBar);
            //активация добавленного окна в списке закладок
            tasksPanel.Items.Insert(tasksPanel.Items.Count - 1, item);
            tasksPanel.SelectedIndex = tasksPanel.Items.Count - 2;
            OnPropertyChanged("ProjCreated");
        }

        /// <summary>
        /// Обновление полосы прогресса
        /// </summary>
        /// <param name="obj"></param>
        private void content_ProgressUpdate(TaskView task, Double obj)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ProgressBar progressBar = (ProgressBar)progressComboBox.Items[m_tasks.IndexOf(task)];
                progressBar.IsIndeterminate = false;
                if (obj == -1) progressBar.IsIndeterminate = true;
                progressBar.Value = obj;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Обновление статуса
        /// </summary>
        /// <param name="obj"></param>
        private void content_StatusUpdate(TaskView task, String obj)
        {
            Dispatcher.Invoke(new Action(() =>
                {   
                    statusLabel.Content = task.Data.Title + ": " + obj;
                }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Удаление задания из очереди
        /// </summary>
        /// <param name="sender"></param>
        private void delete_Click(CloseableTabItem sender)
        {
            bool? accepted = new TaskCloseDialog().ShowDialog();
            if (accepted.HasValue && accepted.Value)
            {
                int pos = m_tasks.IndexOf(sender.Content as TaskView);
                m_tasks.RemoveAt(pos);
                //Удаление полосы прогресса и окна задания
                progressComboBox.Items.RemoveAt(pos);
                tasksPanel.Items.Remove(sender);
            }
            OnPropertyChanged("ProjCreated");
        }

        /// <summary>
        /// Выбор задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tasksPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tasksPanel.SelectedIndex == tasksPanel.Items.Count - 1)
                tasksPanel.SelectedIndex = tasksPanel.Items.Count - 2;
        }

        #endregion
              
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
