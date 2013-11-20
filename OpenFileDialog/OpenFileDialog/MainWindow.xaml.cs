using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenFileDialog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Static Members
        private static Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
        private static string currentDirectory = null; 
        #endregion

        #region Constants
        private const string OutputFileName = "OutputBGCoderFile";
        private const string DefaultExtension = ".txt";
        private const string DialogFilter = "CSharp Documents  (*.cs;*.txt)|*.cs;*.txt"; 
        #endregion

        #region Files Processing
        private static string GetFilesNames()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in dialog.SafeFileNames)
            {
                builder.Append(item + "; ");
            }

            return builder.ToString().TrimEnd();
        }
        private static void ReadAllFiles(StringBuilder finalResult, List<string> namespaces)
        {
            foreach (var item in dialog.SafeFileNames)
            {
                int parentenceCounter = 0;
                using (var reader = new StreamReader(currentDirectory + item))
                {
                    string currentLine = reader.ReadLine();
                    while (currentLine != null)
                    {
                        if (currentLine.StartsWith("using"))
                        {
                            if (!namespaces.Contains(currentLine))
                            {
                                namespaces.Add(currentLine);
                            }
                        }
                        else if (currentLine.StartsWith("namespace"))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                currentLine = reader.ReadLine();
                            }
                            continue;
                        }
                        else
                        {
                            if (currentLine.Contains('{'))
                            {
                                parentenceCounter++;
                            }

                            if (currentLine.Contains('}'))
                            {
                                parentenceCounter--;
                            }

                            if (parentenceCounter >= 0)
                            {
                                finalResult.AppendLine(currentLine);
                            }
                        }
                        currentLine = reader.ReadLine();
                    }
                }

                finalResult.AppendLine();
            }
        }

        private static void WriteToOutputFile(StringBuilder finalResult, List<string> namespaces, TextBox textBox)
        {
            if (textBox.Text.Length > 0)
            {
                using (StreamWriter writer = new StreamWriter(currentDirectory + OutputFileName + DefaultExtension))
                {
                    if (namespaces.Count > 0)
                    {
                        foreach (var name in namespaces)
                        {
                            writer.WriteLine(name);
                        }
                        writer.WriteLine();
                    }

                    writer.Write(finalResult.ToString().Trim());
                } 
            }
        }

        #endregion

        #region Button Events
        private void FilePicker(object sender, RoutedEventArgs e)
        {
            dialog.Multiselect = true;
            dialog.DefaultExt = DefaultExtension;
            dialog.Filter = DialogFilter;

            Nullable<bool> result = dialog.ShowDialog();
            currentDirectory = (dialog.FileName.Remove(dialog.FileName.Length - dialog.SafeFileName.Length, dialog.SafeFileName.Length));

            if (result == true)
            {
                FileNameTextBox.Text = GetFilesNames();
            }

            if (this.FileNameTextBox.Text.Length > 0)
            {
                this.UnifierButton.IsEnabled = true;
            }
            
        }

        private void UnifyFiles(object sender, RoutedEventArgs e)
        {
            try
            {                
                var finalResult = new StringBuilder();
                List<string> namespaces = new List<string>();

                ReadAllFiles(finalResult, namespaces);
                WriteToOutputFile(finalResult, namespaces,this.FileNameTextBox);

                if (this.FileNameTextBox.Text.Length > 0)
                {
                    MessageBox.Show("Files unified successfully! Please check: " + currentDirectory + OutputFileName + DefaultExtension); 
                }
                else
                {
                    MessageBox.Show("Select some files first!");
                    return;
                }
                
            }
            catch (Exception)
            {
                MessageBox.Show("A problem occured. Sorry!");
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
