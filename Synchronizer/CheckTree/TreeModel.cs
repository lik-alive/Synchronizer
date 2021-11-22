using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;

namespace Synchronizer.CheckTree
{
    /// <summary>
    /// Перечень флагов, характеризующих файл
    /// </summary>
    public enum ObjectFlags
    {   
        Newer, //Новее
        Older, //Старше
        Larger, //Больше
        Shorter //Меньше
    }

    public class TreeModel : INotifyPropertyChanged
    {
        #region Data

        /// <summary>
        /// Информация об объекте
        /// </summary>
        private FileSystemInfo m_fsobject;

        /// <summary>
        /// Флаги данного объекта
        /// </summary>
        private Boolean[] m_flags;

        /// <summary>
        /// Флаг уникальности объекта
        /// </summary>
        private Boolean m_isExclusive;

        /// <summary>
        /// Флаг уникальности вложенных объектов
        /// </summary>
        private Boolean m_isExclusiveNested;

        /// <summary>
        /// Статус выбора объекта
        /// </summary>
        private bool? m_isChecked = false;
        
        /// <summary>
        /// Ссылка на копию
        /// </summary>
        private TreeModel m_copy;
        
        /// <summary>
        /// Ссылка на родительский объект
        /// </summary>
        private TreeModel m_parent;

        /// <summary>
        /// Ссылка на дочерние объекты
        /// </summary>
        private List<TreeModel> m_children;
                
        #endregion

        #region Properties

        /// <summary>
        /// Ссылка на дочерние объекты
        /// </summary>
        public List<TreeModel> Children
        {
            get { return m_children; }
            set { m_children = value; }
        }
        
        /// <summary>
        /// Получение расширенного имени папки/файла (с перечислением флагов)
        /// </summary>
        public String NameWithFlags
        {
            get
            {
                String status = m_fsobject.Name;
                for (int i = 0; i < m_flags.Length; i++)
                {
                    if (m_flags[i])
                        status += "  " + "(" + Enum.GetName(typeof(ObjectFlags), i) + ")";
                }
                if (IsExclusiveAny)
                    status += "  " + "(" + "Exclusive" + ")";
                return status;
            }
        }

        /// <summary>
        /// Получение краткого имени папки/файла
        /// </summary>
        public String ShortName
        {
            get { return m_fsobject.Name; }
        }

        /// <summary>
        /// Получение полного имени папки/файла (абсолютный путь)
        /// </summary>
        public String FullName
        {
            get { return m_fsobject.FullName; }
        }

        /// <summary>
        /// Приведение объекта к строке
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return NameWithFlags;
        }

        /// <summary>
        /// Получение фона объекта
        /// </summary>
        public Brush Background
        {
            get
            {
                if (IsExclusiveAny) return Brushes.GreenYellow;
                else if (m_flags[2]) return Brushes.Pink;
                else if (m_flags[3]) return Brushes.Aqua;
                else if (m_flags[0]) return Brushes.Pink;
                else if (m_flags[1]) return Brushes.Aqua;
                else return Brushes.White;
            }
        }

        /// <summary>
        /// Проверка, является ли объект папкой
        /// </summary>
        public Boolean IsDirectory
        {
            get { return m_fsobject.Attributes.HasFlag(FileAttributes.Directory); }
        }

        /// <summary>
        /// Проверка, является ли объект уникальным (новым)
        /// </summary>
        public Boolean IsExclusive
        {
            get { return m_isExclusive; }
        }

        /// <summary>
        /// Проверка, содержит ли объект уникальные объекты
        /// </summary>
        public Boolean IsExclusiveNested
        {
            get { return m_isExclusiveNested; }
        }

        /// <summary>
        /// Проверка, является ли объект уникальным или содержит ли объект уникальные объекты
        /// </summary>
        public Boolean IsExclusiveAny
        {
            get { return m_isExclusive | m_isExclusiveNested; }
        }
        
        /// <summary>
        /// Получение размера объекта
        /// </summary>
        public long Length
        {
            get
            {
                long result = 0;
                m_children.ForEach(c =>
                    {
                        if (c.IsDirectory) result += c.Length;
                        else result += (c.m_fsobject as FileInfo).Length;
                    });

                return result;
            }
        }

        /// <summary>
        /// Получение количества выделенных папок/файлов, содержащихся в данном объекте
        /// </summary>
        public Int32 SelectedCount
        {
            get
            {
                int result = 0;
                m_children.ForEach(c => result += c.SelectedCount);
                if ((!IsChecked.HasValue) || ((IsChecked.HasValue) && (IsChecked.Value)))
                {
                    result++;
                }
                return result;
            }
        }
        
        /// <summary>
        /// Получение количества всех папок/файлов, содержащихся в данном объекте
        /// </summary>
        public Int32 InsideCount
        {
            get
            {
                int result = 0;
                m_children.ForEach(c => result += (1 + c.InsideCount));
                return result;
            }
        }

        /// <summary>
        /// Получение количества уникальных папок/файлов, содержащихся в данном объекте
        /// </summary>
        public Int32 ExclusiveCount
        {
            get
            {
                int result = 0;
                m_children.ForEach(c => result += c.ExclusiveCount);
                if (IsExclusiveAny)
                {
                    result++;
                }
                return result;
            }
        }

        #region IsChecked

        /// <summary>
        /// Получение/задание статуса выбора объекта (объект выбран, объект не выбран, выбрана часть дочерних объектов)
        /// </summary>
        public bool? IsChecked
        {
            get { return m_isChecked; }
            set
            {
                SetIsChecked(value, true, true);
                if (m_copy != null)
                {
                    m_copy.SetIsChecked(value, true, true);
                }
            }
        }

        /// <summary>
        /// Установка статуса выбора объекта
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateChildren"></param>
        /// <param name="updateParent"></param>
        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == m_isChecked) return;

            m_isChecked = value;

            if (updateChildren)
                m_children.ForEach(c => c.SetIsChecked(value, true, false));

            if (updateParent && m_parent != null)
                m_parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }
        
        /// <summary>
        /// Проверка корректности статуса
        /// </summary>
        private void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < m_children.Count; i++)
            {
                bool? current = m_children[i].IsChecked;
                if (i == 0) state = current;
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        public TreeModel(FileSystemInfo fsobject, TreeModel parent)
        {
            if (parent != null)
            {
                m_parent = parent;
                m_parent.Children.Add(this);
            }
            m_fsobject = fsobject;
            m_copy = null;

            m_flags = new Boolean[Enum.GetValues(typeof(ObjectFlags)).Length];
            for (Int32 i = 0; i < m_flags.Length; i++) m_flags[i] = false;
            m_isExclusive = true;

            m_children = new List<TreeModel>();
            m_isChecked = true;

            if (IsDirectory) BuildTree();
        }

        /// <summary>
        /// Построение дерева объектов
        /// </summary>
        private void BuildTree()
        {
            Thread.Sleep(1);
            DirectoryInfo dirInfo = new DirectoryInfo(FullName);

            DirectoryInfo[] directories;
            directories = dirInfo.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                if (!File.GetAttributes(directory.FullName).HasFlag(FileAttributes.System))
                {
                    TreeModel model = new TreeModel(directory, this);
                }
            }

            FileInfo[] files;
            files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (!File.GetAttributes(file.FullName).HasFlag(FileAttributes.System))
                {
                    TreeModel model = new TreeModel(file, this);
                }
            }
        }
        
        /// <summary>
        /// Сравнение параметров двух объектов
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        public static void CompareParameters(TreeModel node1, TreeModel node2)
        {
            node1.m_copy = node2;
            node2.m_copy = node1;
            //Exclusive
            if (node1.IsDirectory)
            {
                bool val = false;
                node1.Children.ForEach(c => val |= c.IsExclusiveAny);
                node1.m_isExclusiveNested = val;
                bool isChecked = false;
                node1.Children.ForEach(c => isChecked |= c.IsChecked.HasValue ? c.IsChecked.Value : true);
                if (!isChecked) node1.m_isChecked = false;
            }
            node1.m_isExclusive = false;

            if (node2.IsDirectory)
            {
                bool val = false;
                node2.Children.ForEach(c => val |= c.IsExclusiveAny);
                node2.m_isExclusiveNested = val;
                bool isChecked = false;
                node2.Children.ForEach(c => isChecked |= c.IsChecked.HasValue ? c.IsChecked.Value : true);
                if (!isChecked) node2.m_isChecked = false;
            }
            node2.m_isExclusive = false;
            //If both IsFile
            if (!node1.IsDirectory && !node2.IsDirectory)
            {
                Boolean equals = true;
                //Length
                if ((node1.m_fsobject as FileInfo).Length > (node2.m_fsobject as FileInfo).Length)
                {
                    equals = false;
                    node1.SetFlag(ObjectFlags.Larger, true);
                    node2.SetFlag(ObjectFlags.Shorter, true);
                }
                else if ((node1.m_fsobject as FileInfo).Length < (node2.m_fsobject as FileInfo).Length)
                {
                    equals = false;
                    node1.SetFlag(ObjectFlags.Shorter, true);
                    node2.SetFlag(ObjectFlags.Larger, true);
                }
                //WriteTime
                if ((node1.m_fsobject.LastWriteTime - node2.m_fsobject.LastWriteTime).TotalSeconds > 20)
                {
                    equals = false;
                    node1.SetFlag(ObjectFlags.Newer, true);
                    node2.SetFlag(ObjectFlags.Older, true);
                }
                else if ((node1.m_fsobject.LastWriteTime - node2.m_fsobject.LastWriteTime).TotalSeconds < -20)
                {
                    equals = false;
                    node1.SetFlag(ObjectFlags.Older, true);
                    node2.SetFlag(ObjectFlags.Newer, true);
                }
                if (equals)
                {
                    node1.IsChecked = false;
                }
            }
            //Duplicate flags to parent
            if (node1.m_parent != null)
            {
                for (int i = 0; i < node1.m_flags.Length; i++)
                    if (node1.m_flags[i]) node1.m_parent.m_flags[i] = true;
            }
            if (node2.m_parent != null)
            {
                for (int i = 0; i < node2.m_flags.Length; i++)
                    if (node2.m_flags[i]) node2.m_parent.m_flags[i] = true;
            }
        }

        /// <summary>
        /// Установка флага
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public void SetFlag(ObjectFlags flag, Boolean value)
        {
            m_flags[(int)flag] = value;
        }

        /// <summary>
        /// Получение флага
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public Boolean GetFlag(ObjectFlags flag)
        {
            return m_flags[(int)flag];
        }

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}