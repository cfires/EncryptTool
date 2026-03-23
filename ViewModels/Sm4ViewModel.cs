using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EncryptTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EncryptTool.ViewModels
{
    public partial class Sm4ViewModel : ViewModelBase
    {
        #region 绑定属性
        // 默认密钥（32位HEX）
        private string _key = "a1b2c3d4e5f67890098765f4e3d2c1b0";
        public string Key
        {
            get => _key;
            set
            {
                SetProperty(ref _key, value);
                KeyLength = value?.Length ?? 0;
            }
        }

        // 默认IV（32位HEX，CBC使用）
        private string _iV = "0123456789abcdef0123456789abcdef";
        public string IV
        {
            get => _iV;
            set => SetProperty(ref _iV, value);
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
                SetProperty(ref _selectedMode, value);
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
        #endregion

        #region 命令
        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }
        #endregion

        public Sm4ViewModel()
        {
            EncryptCommand = new RelayCommand(ExecuteEncrypt);
            DecryptCommand = new RelayCommand(ExecuteDecrypt);
            KeyLength = Key.Length;
        }

        #region 加密 / 解密
        private void ExecuteEncrypt()
        {
            try
            {
                // 严格校验
                if (string.IsNullOrWhiteSpace(Key))
                {
                    Output = "错误：请输入32位HEX密钥";
                    return;
                }
                if (Key.Length != 32)
                {
                    Output = "错误：密钥必须是32位HEX字符串";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Input))
                {
                    Output = "错误：请输入需要加密的内容";
                    return;
                }
                if (SelectedMode == "CBC" && string.IsNullOrWhiteSpace(IV))
                {
                    Output = "错误：CBC模式必须填写32位HEX偏移量IV";
                    return;
                }
                if (SelectedMode == "CBC" && IV.Length != 32)
                {
                    Output = "错误：IV必须是32位HEX字符串";
                    return;
                }

                Output = Sm4Helper.Encrypt(
                    Input,
                    Key,
                    IV,
                    SelectedMode,
                    SelectedCharset,
                    SelectedOutputFormat);
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
                if (string.IsNullOrWhiteSpace(Key))
                {
                    Output = "错误：请输入32位HEX密钥";
                    return;
                }
                if (Key.Length != 32)
                {
                    Output = "错误：密钥必须是32位HEX字符串";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Input))
                {
                    Output = "错误：请输入需要解密的内容";
                    return;
                }
                if (SelectedMode == "CBC" && string.IsNullOrWhiteSpace(IV))
                {
                    Output = "错误：CBC模式必须填写32位HEX偏移量IV";
                    return;
                }
                if (SelectedMode == "CBC" && IV.Length != 32)
                {
                    Output = "错误：IV必须是32位HEX字符串";
                    return;
                }

                Output = Sm4Helper.Decrypt(
                    Input,
                    Key,
                    IV,
                    SelectedMode,
                    SelectedCharset,
                    SelectedOutputFormat);
            }
            catch (Exception ex)
            {
                Output = $"解密失败：{ex.Message}";
            }
        }
        #endregion
    }
}
