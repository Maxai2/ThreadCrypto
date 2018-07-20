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
        private object pause = new object();
        private bool interrupt = false;

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

        private string filePath = "";
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

        private string encryptKey = "";
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

        private double progBarMaxVal = 100;
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

        private bool filePathIsEnable = true;
        public bool FilePathIsEnable
        {
            get => filePathIsEnable;
            set
            {
                filePathIsEnable = value;
                OnPropertyChanged();
            }
        }

        //--------------------------------------------------------------------

        private bool keyEncDecIsEnable = true;
        public bool KeyEncDecIsEnable
        {
            get => keyEncDecIsEnable;
            set
            {
                keyEncDecIsEnable = value;
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

        Thread task;

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
                            FilePathIsEnable = false;
                            KeyEncDecIsEnable = false;

                            this.interrupt = false;

                            task = new Thread(() =>
                            {
                                EcryptDecryptFile(IsEncrypt);
                            });

                            task.Start();

                        },
                        (param) =>
                        {
                            if (EncryptKey == "" || FilePath == "")
                            {
                                return false;
                            }

                            if (task != null && task.IsAlive)
                            {
                                return false;
                            }

                            return true;
                        });
                }

                return startCom;
            }
        }

        //--------------------------------------------------------------------

        private ICommand cancelCom;
        public ICommand CancelCom
        {
            get
            {
                if (cancelCom is null)
                {
                    cancelCom = new RelayCommand(
                        (param) =>
                        {
                            if (task == null)
                                return;

                            lock (pause)
                            {
                                var result = MessageBox.Show("Do u want to cancel ecrypt?", "Alert", MessageBoxButton.YesNo);

                                if (result == MessageBoxResult.Yes)
                                {
                                    // task.Interrupt();
                                    this.interrupt = true;
                                    this.task = null;
                                }
                            }
                            DefaultState();

                        },
                        (param) =>
                        {
                            if (task == null)
                                return false;

                            if (task.IsAlive)
                                return true;
                            else
                                return false;
                        });
                }

                return cancelCom;
            }
        }

        //--------------------------------------------------------------------

        void EcryptDecryptFile(bool mode)
        {
            var att = File.GetAttributes(FilePath);

            if ((att == FileAttributes.Archive && IsEncrypt == true) || (att == FileAttributes.Normal && IsEncrypt == false))
            {
                using (FileStream fstream = File.OpenRead(FilePath))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(EncryptKey);

                    byte[] array = new byte[fstream.Length];

                    fstream.Read(array, 0, array.Length);

                    ProgBarMaxVal = array.Length;

                    //var step = array.Length / 100.0;

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (interrupt)
                        {
                            FilePathIsEnable = true;
                            KeyEncDecIsEnable = true;

                            DefaultState();

                            return;
                        }
                        lock (pause)
                        {
                            array[i] = (byte)(array[i] ^ bytes[i % bytes.Length]);

                            Dispatcher.Invoke(() =>
                            {
                                ProgressValue += 100;
                            });
                        }
                        Thread.Sleep(2);
                    }

                    fileText = Encoding.Default.GetString(array);
                }

                File.WriteAllText(FilePath, String.Empty);

                using (FileStream fstream = new FileStream(FilePath, FileMode.OpenOrCreate))
                {
                    byte[] array = System.Text.Encoding.Default.GetBytes(fileText);

                    fstream.Write(array, 0, array.Length);
                }

                if (IsEncrypt)
                {
                    att = RemoveAttribute(att, FileAttributes.Archive);
                    File.SetAttributes(FilePath, att);

                    File.SetAttributes(FilePath, FileAttributes.Normal);
                    MessageBox.Show("File is encrypted");
                }
                else
                {
                    att = RemoveAttribute(att, FileAttributes.Normal);
                    File.SetAttributes(FilePath, att);

                    File.SetAttributes(FilePath, FileAttributes.Archive);
                    MessageBox.Show("File is decrypted");
                }

                DefaultState();

            }
            else
            if (att == FileAttributes.Normal && IsEncrypt == true)
            {
                MessageBox.Show("File already encrypted");
            }
            else
            if (att == FileAttributes.Archive && IsEncrypt == false)
            {
                MessageBox.Show("File is not encrypted");
            }

            FilePathIsEnable = true;
            KeyEncDecIsEnable = true;
        }

        //--------------------------------------------------------------------

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        //--------------------------------------------------------------------

        void DefaultState()
        {
            EncryptKey = "";
            FilePath = "";

            Dispatcher.Invoke(() =>
            {
                ProgressValue = 0;
            });
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