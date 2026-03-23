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
    public partial class AesViewModel : ViewModelBase
    {
        #region 绑定属性
        private string _key = "n9g7jM3a1QhfQZ6G"; // 默认密钥
        public string Key
        {
            get => _key;
            set
            {
                SetProperty(ref _key, value);
                KeyLength = value?.Length ?? 0;
            }
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

        private bool _isOpenSslMode = true;
        public bool IsOpenSslMode
        {
            get => _isOpenSslMode;
            set
            {
                SetProperty(ref _isOpenSslMode, value);
                IsCustomModeEnabled = !value;
            }
        }

        private bool _isCustomModeEnabled;
        public bool IsCustomModeEnabled
        {
            get => _isCustomModeEnabled;
            set => SetProperty(ref _isCustomModeEnabled, value);
        }

        // 下拉选中项
        private string _selectedMode = "CBC";
        public string SelectedMode
        {
            get => _selectedMode;
            set => SetProperty(ref _selectedMode, value);
        }

        private string _selectedPadding = "PKCS7";
        public string SelectedPadding
        {
            get => _selectedPadding;
            set => SetProperty(ref _selectedPadding, value);
        }

        private string _selectedCharset = "UTF-8";
        public string SelectedCharset
        {
            get => _selectedCharset;
            set => SetProperty(ref _selectedCharset, value);
        }
        #endregion

        #region 命令
        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }
        #endregion

        public AesViewModel()
        {
            // 初始化命令
            EncryptCommand = new RelayCommand(ExecuteEncrypt);
            DecryptCommand = new RelayCommand(ExecuteDecrypt);

            // 初始化长度
            KeyLength = Key.Length;
            // 初始化自定义模式启用状态
            IsCustomModeEnabled = !IsOpenSslMode;
        }

        #region 业务方法
        private void ExecuteEncrypt()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                {
                    Output = "错误：请输入密钥";
                    return;
                }

                if (IsOpenSslMode)
                {
                    Output = AesHelper.EncryptOpenSsl(Input, Key, "UTF-8");
                }
                else
                {
                    Output = AesHelper.Encrypt(Input, Key, SelectedMode, SelectedPadding, SelectedCharset);
                }
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
                    Output = "错误：请输入密钥";
                    return;
                }

                if (IsOpenSslMode)
                {
                    Output = AesHelper.DecryptOpenSsl(Input, Key, "UTF-8");
                }
                else
                {
                    Output = AesHelper.Decrypt(Input, Key, SelectedMode, SelectedPadding, SelectedCharset);
                }
            }
            catch (Exception ex)
            {
                Output = $"解密失败：{ex.Message}";
            }
        }
        #endregion
    }
}
