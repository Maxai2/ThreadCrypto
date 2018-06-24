using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

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

        private ICommand startCom;
        public ICommand StartCom
        {
            get
            {
                if (startCom is null)
                {
                    startCom = new RelayCommand(
                        (param) =>
                        {
                            EcryptDecryptFile(true);
                        });
                }

                return startCom;
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

            MessageBox.Show(fileText);
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