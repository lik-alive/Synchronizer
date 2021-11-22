using System;
using System.Collections.Generic;
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
    /// Информационное сообщение
    /// </summary>
    public partial class InfoOK : Window
    {
        public String Info { get; private set; }

        public InfoOK(String info)
        {
            Info = info;
            InitializeComponent();
        }

        /// <summary>
        /// Обработка подтверждения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        /// <summary>
        /// Подтверждение с помощью горячих клавиш
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OK_Click(null, null);
            }
        }
    }
}
