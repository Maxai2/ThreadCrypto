using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
            get => filePath;
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }

        //--------------------------------------------------------------------

        private string encryptKey;
        public string EncryptKey
        {
            get => encryptKey; 
            set
            {
                encryptKey = value;
                OnPropertyChanged();
            }
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

        private double progBarMaxVal = 1;
        public double ProgBarMaxVal
        {
            get => progBarMaxVal;
            set
            {
                progBarMaxVal = value;
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
                            var task = new Thread(() =>
                            {
                                EcryptDecryptFile(IsEncrypt);
                            });
                            
                            task.Start();
                        }, 
                        (param) =>
                        {
                            if (EncryptKey == null || FilePath == null)
                                return false;

                            return true;
                        });
                }

                return startCom;
            }
        }

        //--------------------------------------------------------------------

        string fileText;

        void EcryptDecryptFile(bool mode)
        {
            //var att = File.GetAttributes(FilePath);

            //if (att == FileAttributes.Encrypted)
            //{

            //}

            using (FileStream fstream = File.OpenRead(FilePath))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(EncryptKey);

                byte[] array = new byte[fstream.Length];
                
                fstream.Read(array, 0, array.Length);

                ProgBarMaxVal = array.Length;

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (byte)(array[i] ^ bytes[i % bytes.Length]);

                    Thread.Sleep(50);

                    Dispatcher.Invoke(() =>
                    {
                        ProgressValue += (i * array.Length) / 100;
                    });
                }

                fileText = Encoding.Default.GetString(array);
            }

            File.WriteAllText(FilePath, String.Empty);

            using (FileStream fstream = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(fileText);

                fstream.Write(array, 0, array.Length);
            }

            MessageBox.Show("Done");

            //File.SetAttributes(FilePath, FileAttributes.Encrypted);
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