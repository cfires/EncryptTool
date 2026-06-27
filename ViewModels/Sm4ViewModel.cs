using CommunityToolkit.Mvvm.Input;
using EncryptTool.Services;
using System;
using System.Windows.Input;

namespace EncryptTool.ViewModels
{
    public partial class Sm4ViewModel : ViewModelBase
    {
        private string _key = "a1b2c3d4e5f67890098765f4e3d2c1b0";
        public string Key
        {
            get => _key;
            set
            {
                if (SetProperty(ref _key, value))
                    KeyLength = value?.Length ?? 0;
            }
        }

        private string _iv = "0123456789abcdef0123456789abcdef";
        public string IV
        {
            get => _iv;
            set => SetProperty(ref _iv, value);
        }

        private int _keyLength;
        public int KeyLength
        {
            get => _keyLength;
            set => SetProperty(ref _keyLength, value);
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

        private string _selectedMode = "CBC";
        public string SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (SetProperty(ref _selectedMode, value))
                    IsIvEnabled = value == "CBC";
            }
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

        private bool _isIvEnabled = true;
        public bool IsIvEnabled
        {
            get => _isIvEnabled;
            set => SetProperty(ref _isIvEnabled, value);
        }

        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }

        public Sm4ViewModel()
        {
            EncryptCommand = new RelayCommand(ExecuteEncrypt);
            DecryptCommand = new RelayCommand(ExecuteDecrypt);
            KeyLength = Key.Length;
        }

        private void ExecuteEncrypt()
        {
            try
            {
                if (!ValidateInput(isDecrypt: false))
                    return;

                Output = Sm4Helper.Encrypt(Input, Key, IV, SelectedMode, SelectedCharset, SelectedOutputFormat);
            }
            catch (Exception ex)
            {
                Output = $"加密失败：{ex.Message}";
            }
        }

        private void ExecuteDecrypt()
        {
            try
            {
                if (!ValidateInput(isDecrypt: true))
                    return;

                Output = Sm4Helper.Decrypt(Input, Key, IV, SelectedMode, SelectedCharset, SelectedOutputFormat);
            }
            catch (Exception ex)
            {
                Output = $"解密失败：{ex.Message}";
            }
        }

        private bool ValidateInput(bool isDecrypt)
        {
            if (!Sm4Helper.IsHexString(Key, 32))
            {
                Output = "错误：密钥必须是32位HEX字符串";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Input))
            {
                Output = isDecrypt ? "错误：请输入需要解密的内容" : "错误：请输入需要加密的内容";
                return false;
            }

            if (SelectedMode == "CBC" && !Sm4Helper.IsHexString(IV, 32))
            {
                Output = "错误：CBC模式必须填写32位HEX偏移量IV";
                return false;
            }

            return true;
        }
    }
}
