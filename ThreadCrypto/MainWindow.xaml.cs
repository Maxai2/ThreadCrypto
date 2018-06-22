using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

//--------------------------------------------------------------------

namespace ThreadCrypto
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool isEncrypt = true;
        public bool IsEncrypt
        {
            get => isEncrypt;
            set
            {
                isEncrypt = value;
                OnPropertyChanged();
            }
        }

        //--------------------------------------------------------------------

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; OnPropertyChanged(); }
        }

        //--------------------------------------------------------------------

        private string encryptKey;
        public string EncryptKey
        {
            get { return encryptKey; }
            set { encryptKey = value; OnPropertyChanged(); }
        }

        //--------------------------------------------------------------------

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        //--------------------------------------------------------------------

        OpenFileDialog OpenFile;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            OpenFile = new OpenFileDialog();
            OpenFile.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            OpenFile.RestoreDirectory = true;
        }

        //--------------------------------------------------------------------

        private ICommand fileSelect;
        public ICommand FileSelect
        {
            get
            {
                if (fileSelect is null)
                {
                    fileSelect = new RelayCommand(
                        (param) =>
                        {
                            if (OpenFile.ShowDialog() == true)
                            {
                                FilePath = OpenFile.FileName;
                            }        
                        });
                }

                return fileSelect;
            }
        }

        //--------------------------------------------------------------------

        string fileText;

        void EcryptDecryptFile(bool mode)
        {
            using (FileStream fstream = File.OpenRead(FilePath))
            {
                
                byte[] array = new byte[fstream.Length];
                
                fstream.Read(array, 0, array.Length);

                fileText = Encoding.Default.GetString(array);
            }


        }

        //--------------------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //--------------------------------------------------------------------

    }
}
//--------------------------------------------------------------------