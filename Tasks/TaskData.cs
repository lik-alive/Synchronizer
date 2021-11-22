using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Synchronizer.Tasks
{
    public class TaskData
    {
        #region Fields

        private Boolean m_isAnalyzed;

        private String m_title;

        private String m_lpath;

        private String m_rpath;

        #endregion

        #region Properties

        /// <summary>
        /// Получение/задание статуса проведения анализа данных
        /// </summary>
        public Boolean IsAnalyzed
        {
            get { return m_isAnalyzed; }
            set { m_isAnalyzed = value; }
        }

        /// <summary>
        /// Получение/задание названия задания
        /// </summary>
        public String Title
        {
            get { return m_title; }
        }

        /// <summary>
        /// Получение/задание корневой папки из левой части
        /// </summary>
        public String LeftPath
        {
            get { return m_lpath; }
            set
            {
                if (m_lpath != value)
                {
                    m_lpath = value;
                    m_isAnalyzed = false;
                }
            }
        }

        /// <summary>
        /// Получение/задание корневой папки из правой части
        /// </summary>
        public String RightPath
        {
            get { return m_rpath; }
            set
            {
                if (m_rpath != value)
                {
                    m_rpath = value;
                    m_isAnalyzed = false;
                }
            }
        }

        #endregion

        public TaskData()
        {
            m_isAnalyzed = false;
            m_title = "<New Task>";
            m_lpath = "";
            m_rpath = "";
        }

        public TaskData(String title) : this()
        {
            m_title = title;
        }

        /// <summary>
        /// Сохранение данных в поток
        /// </summary>
        /// <param name="stream"></param>
        public void Save(StreamWriter stream)
        {
            stream.WriteLine(m_title);
            stream.WriteLine(m_lpath);
            stream.WriteLine(m_rpath);
            stream.Flush();
        }

        /// <summary>
        /// Загрузка данных из потока
        /// </summary>
        /// <param name="stream"></param>
        public void Load(StreamReader stream)
        {
            m_title = stream.ReadLine();
            m_lpath = stream.ReadLine();
            m_rpath = stream.ReadLine();

            m_isAnalyzed = false;
        }
    }
}
