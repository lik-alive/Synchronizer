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
    /// Запрос на удаление задания
    /// Удаление задания может быть принято или отклонено
    /// </summary>
    public partial class DeleteFilesDialog : Window
    {

        private Boolean? m_status;

        public DeleteFilesDialog()
        {
            m_status = null;
            InitializeComponent();
        }

        public Boolean? ShowExtendedDialog()
        {
            ShowDialog();
            return m_status;
        }

        /// <summary>
        /// Обработка подтверждения удаления задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            m_status = true;
            this.Close();
        }

        /// <summary>
        /// Обработка отклонения удаления задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NO_Click(object sender, RoutedEventArgs e)
        {
            m_status = false;
            this.Close();
        }

        /// <summary>
        /// Обработка отмены синхронизации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CANCEL_Click(object sender, RoutedEventArgs e)
        {
            m_status = null;
            this.Close();
        }

        /// <summary>
        /// Принятие/отклонение удаления задания с помощью горячих клавиш
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
    }
}
