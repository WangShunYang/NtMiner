﻿using NTMiner.Vms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NTMiner.Views.Ucs {
    public partial class MinerProfileDual : UserControl {
        private MinerProfileViewModel Vm {
            get {
                return MinerProfileViewModel.Instance;
            }
        }

        public MinerProfileDual() {
            this.DataContext = MinerProfileViewModel.Instance;
            InitializeComponent();
            this.RunOneceOnLoaded((window) => {
                window.AddEventPath<LocalContextVmsReInitedEvent>("本地上下文视图模型集刷新后刷新界面上的popup", LogEnum.DevConsole,
                    action: message => {
                        UIThread.Execute(() => () => {
                            if (Vm.MineWork != null) {
                                return;
                            }
                            if (this.PopupDualCoinPool.Child != null && this.PopupDualCoinPool.IsOpen) {
                                this.PopupDualCoinPool.IsOpen = false;
                                OpenDualCoinPoolPopup();
                            }
                            if (this.PopupDualCoin.Child != null && this.PopupDualCoin.IsOpen) {
                                this.PopupDualCoin.IsOpen = false;
                                OpenDualCoinPopup();
                            }
                            if (this.PopupDualCoinWallet.Child != null && this.PopupDualCoinWallet.IsOpen) {
                                this.PopupDualCoinWallet.IsOpen = false;
                                OpenDualCoinWalletPopup();
                            }
                        });
                    }, location: this.GetType());
            });
        }

        private void DualCoinWeightSlider_LostFocus(object sender, RoutedEventArgs e) {
            if (Vm.CoinVm == null
                || Vm.CoinVm.CoinKernel == null
                || Vm.CoinVm.CoinKernel.CoinKernelProfile == null) {
                return;
            }
            CoinKernelProfileViewModel coinKernelProfileVm = Vm.CoinVm.CoinKernel.CoinKernelProfile;
            NTMinerRoot.Instance.MinerProfile.SetCoinKernelProfileProperty(coinKernelProfileVm.CoinKernelId, nameof(coinKernelProfileVm.DualCoinWeight), coinKernelProfileVm.DualCoinWeight);
            NTMinerRoot.RefreshArgsAssembly.Invoke();
        }

        #region OpenPopup

        private void OpenDualCoinPoolPopup() {
            var coinVm = Vm.CoinVm.CoinKernel.CoinKernelProfile.SelectedDualCoin;
            if (coinVm == null) {
                return;
            }
            var popup = PopupDualCoinPool;
            popup.IsOpen = true;
            var selected = coinVm.CoinProfile.DualCoinPool;
            var vm = new PoolSelectViewModel(coinVm, selected, onOk: selectedResult => {
                if (selectedResult != null) {
                    if (coinVm.CoinProfile.DualCoinPool != selectedResult) {
                        coinVm.CoinProfile.DualCoinPool = selectedResult;
                    }
                    popup.IsOpen = false;
                }
            }) {
                HideView = new DelegateCommand(() => {
                    popup.IsOpen = false;
                })
            };
            if (popup.Child == null) {
                popup.Child = new PoolSelect(vm);
            }
            else {
                ((PoolSelect)popup.Child).DataContext = vm;
            }
        }

        private void OpenDualCoinPopup() {
            if (Vm.CoinVm == null || Vm.CoinVm.CoinKernel == null) {
                return;
            }
            var popup = PopupDualCoin;
            popup.IsOpen = true;
            var selected = Vm.CoinVm.CoinKernel.CoinKernelProfile.SelectedDualCoin;
            var vm = new CoinSelectViewModel(Vm.CoinVm.CoinKernel.SelectedDualCoinGroup.DualCoinVms, selected, onOk: selectedResult => {
                if (selectedResult != null) {
                    if (Vm.CoinVm.CoinKernel.CoinKernelProfile.SelectedDualCoin != selectedResult) {
                        Vm.CoinVm.CoinKernel.CoinKernelProfile.SelectedDualCoin = selectedResult;
                    }
                    popup.IsOpen = false;
                }
            }) {
                HideView = new DelegateCommand(() => {
                    popup.IsOpen = false;
                })
            };
            if (popup.Child == null) {
                popup.Child = new CoinSelect(vm);
            }
            else {
                ((CoinSelect)popup.Child).DataContext = vm;
            }
        }

        private void OpenDualCoinWalletPopup() {
            var coinVm = Vm.CoinVm.CoinKernel.CoinKernelProfile.SelectedDualCoin;
            if (coinVm == null) {
                return;
            }
            var popup = PopupDualCoinWallet;
            popup.IsOpen = true;
            var selected = coinVm.CoinProfile.SelectedDualCoinWallet;
            bool isDualCoin = true;
            var vm = new WalletSelectViewModel(coinVm, isDualCoin, selected, onOk: selectedResult => {
                if (selectedResult != null) {
                    if (coinVm.CoinProfile.SelectedDualCoinWallet != selectedResult) {
                        coinVm.CoinProfile.SelectedDualCoinWallet = selectedResult;
                    }
                    else {
                        coinVm.CoinProfile.OnPropertyChanged(nameof(coinVm.CoinProfile.SelectedDualCoinWallet));
                        selectedResult.OnPropertyChanged(nameof(selectedResult.Name));
                        selectedResult.OnPropertyChanged(nameof(selectedResult.Address));
                    }
                    popup.IsOpen = false;
                }
            }) {
                HideView = new DelegateCommand(() => {
                    popup.IsOpen = false;
                })
            };
            if (popup.Child == null) {
                popup.Child = new WalletSelect(vm);
            }
            else {
                ((WalletSelect)popup.Child).DataContext = vm;
            }
        }

        #endregion

        private void KbButtonDualCoinPool_Clicked(object sender, RoutedEventArgs e) {
            OpenDualCoinPoolPopup();
            UserActionHappend();
            e.Handled = true;
        }

        private void KbButtonDualCoin_Clicked(object sender, RoutedEventArgs e) {
            OpenDualCoinPopup();
            UserActionHappend();
            e.Handled = true;
        }

        private void KbButtonDualCoinWallet_Clicked(object sender, RoutedEventArgs e) {
            var coinVm = Vm.CoinVm.CoinKernel.CoinKernelProfile.SelectedDualCoin;
            if (coinVm == null) {
                return;
            }
            if (coinVm.Wallets.Count == 0) {
                coinVm.CoinProfile.AddDualCoinWallet.Execute(null);
            }
            else {
                OpenDualCoinWalletPopup();
                UserActionHappend();
            }
            e.Handled = true;
        }

        private static void UserActionHappend() {
            if (!DevMode.IsDevMode) {
                VirtualRoot.RaiseEvent(new UserActionEvent());
            }
        }

        private void ScrollViewer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            WpfUtil.ScrollViewer_PreviewMouseDown(sender, e);
        }
    }
}
