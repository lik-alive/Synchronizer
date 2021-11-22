using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Synchronizer.Menu
{
    public static class MenuCommands
    {
        private static RoutedUICommand m_new;
        private static RoutedUICommand m_load;
        private static RoutedUICommand m_save;
        private static RoutedUICommand m_analyze;
        private static RoutedUICommand m_synchronize;
        private static RoutedUICommand m_analyzeAll;
        private static RoutedUICommand m_synchronizeAll;
        private static RoutedUICommand m_stop;
        private static RoutedUICommand m_stopAll;

        static MenuCommands()
        {
            m_new = new RoutedUICommand("New", "New", typeof(MenuCommands));
            m_new.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            m_load = new RoutedUICommand("Load", "Load", typeof(MenuCommands));
            m_load.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            m_save = new RoutedUICommand("Save", "Save", typeof(MenuCommands));
            m_save.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));

            m_analyze = new RoutedUICommand("Analyze", "Analyze", typeof(MenuCommands));
            m_analyze.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Alt));
            m_synchronize = new RoutedUICommand("Synchronize", "Synchronize", typeof(MenuCommands));
            m_synchronize.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt));
            m_analyzeAll = new RoutedUICommand("AnalyzeAll", "AnalyzeAll", typeof(MenuCommands));
            m_analyzeAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Alt | ModifierKeys.Shift));
            m_synchronizeAll = new RoutedUICommand("Synchronize", "SynchronizeAll", typeof(MenuCommands));
            m_synchronizeAll.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt | ModifierKeys.Shift));

            m_stop = new RoutedUICommand("Stop", "Stop", typeof(MenuCommands));
            m_stop.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Alt));
            m_stopAll = new RoutedUICommand("StopAll", "StopAll", typeof(MenuCommands));
            m_stopAll.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Alt | ModifierKeys.Shift));
        }

        public static RoutedUICommand New
        {
            get { return m_new; }
        }

        public static RoutedUICommand Load
        {
            get { return m_load; }
        }

        public static RoutedUICommand Save
        {
            get { return m_save; }
        }

        public static RoutedUICommand Analyze
        {
            get { return m_analyze; }
        }

        public static RoutedUICommand Synchronize
        {
            get { return m_synchronize; }
        }

        public static RoutedUICommand AnalyzeAll
        {
            get { return m_analyzeAll; }
        }

        public static RoutedUICommand SynchronizeAll
        {
            get { return m_synchronizeAll; }
        }

        public static RoutedUICommand Stop
        {
            get { return m_stop; }
        }

        public static RoutedUICommand StopAll
        {
            get { return m_stopAll; }
        }
    }
}
