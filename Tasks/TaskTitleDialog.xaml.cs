using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Synchronizer.Tasks
{
    /// <summary>
    /// Запрос на ввод названия задания
    /// Добавление задания может быть принято или отклонено
    /// </summary>
    public partial class TaskTitleDialog : Window, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Название задания
        /// </summary>
        private String m_taskTitle;

        #endregion

        #region Properties

        /// <summary>
        /// Свойство чтения/записи названия задания
        /// </summary>
        public String TaskTitle
        {
            get { return m_taskTitle; }
            set {  m_taskTitle = value; }
        }

        #endregion

        public TaskTitleDialog()
        {
            TaskTitle = "<New Task>";
            InitializeComponent();
            textBox1.Focus();
            //Выделение всей строки
            textBox1.SelectAll();
        }

        /// <summary>
        /// Обработка подтверждения добавления задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text == "")
            {
                TaskTitle = "<New Task>";
                OnPropertyChanged("TaskTitle");
                SystemSounds.Hand.Play();
            }
            else
            {   
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Обработка отклонения добавления задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CANCEL_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Принятие/отклонение добавления задания с помощью горячих клавиш
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OK_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                CANCEL_Click(null, null);
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
