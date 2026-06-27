using CommunityToolkit.Mvvm.Input;
using EncryptTool.Services;
using System;
using System.Windows.Input;

namespace EncryptTool.ViewModels
{
    public sealed class Sm2ViewModel : ViewModelBase
    {
        private string _publicKey = string.Empty;
        public string PublicKey
        {
            get => _publicKey;
            set
            {
                if (SetProperty(ref _publicKey, value))
                    PublicKeyLength = value?.Length ?? 0;
            }
        }

        private string _privateKey = string.Empty;
        public string PrivateKey
        {
            get => _privateKey;
            set
            {
                if (SetProperty(ref _privateKey, value))
                    PrivateKeyLength = value?.Length ?? 0;
            }
        }

        private int _publicKeyLength;
        public int PublicKeyLength
        {
            get => _publicKeyLength;
            private set => SetProperty(ref _publicKeyLength, value);
        }

        private int _privateKeyLength;
        public int PrivateKeyLength
        {
            get => _privateKeyLength;
            private set => SetProperty(ref _privateKeyLength, value);
        }

        private string _input = string.Empty;
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private string _output = string.Empty;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        private string _selectedCipherMode = "C1C3C2";
        public string SelectedCipherMode
        {
            get => _selectedCipherMode;
            set => SetProperty(ref _selectedCipherMode, value);
        }

        private string _selectedCharset = "UTF-8";
        public string SelectedCharset
        {
            get => _selectedCharset;
            set => SetProperty(ref _selectedCharset, value);
        }

        private string _selectedOutputFormat = "Base64";
        public string SelectedOutputFormat
        {
            get => _selectedOutputFormat;
            set => SetProperty(ref _selectedOutputFormat, value);
        }

        public ICommand GenerateKeyPairCommand { get; }
        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }

        public Sm2ViewModel()
        {
            GenerateKeyPairCommand = new RelayCommand(GenerateKeyPair);
            EncryptCommand = new RelayCommand(Encrypt);
            DecryptCommand = new RelayCommand(Decrypt);
            GenerateKeyPair();
        }

        private void GenerateKeyPair()
        {
            try
            {
                Sm2KeyPair pair = Sm2Helper.GenerateKeyPair();
                PublicKey = pair.PublicKey;
                PrivateKey = pair.PrivateKey;
                Output = "SM2密钥对已生成";
            }
            catch (Exception ex)
            {
                Output = $"生成密钥对失败：{ex.Message}";
            }
        }

        private void Encrypt()
        {
            try
            {
                if (!Sm2Helper.IsValidPublicKey(PublicKey))
                {
                    Output = "错误：请输入有效的SM2公钥";
                    return;
                }

                Output = Sm2Helper.Encrypt(
                    Input,
                    PublicKey,
                    SelectedCipherMode,
                    SelectedCharset,
                    SelectedOutputFormat);
            }
            catch (Exception ex)
            {
                Output = $"加密失败：{ex.Message}";
            }
        }

        private void Decrypt()
        {
            try
            {
                if (!Sm2Helper.IsValidPrivateKey(PrivateKey))
                {
                    Output = "错误：请输入有效的64位HEX格式SM2私钥";
                    return;
                }

                Output = Sm2Helper.Decrypt(
                    Input,
                    PrivateKey,
                    SelectedCipherMode,
                    SelectedCharset,
                    SelectedOutputFormat);
            }
            catch (Exception ex)
            {
                Output = $"解密失败：{ex.Message}";
            }
        }
    }
}
