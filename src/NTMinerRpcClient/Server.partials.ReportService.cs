﻿using NTMiner.Controllers;
using NTMiner.MinerClient;
using NTMiner.MinerServer;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NTMiner {
    public static partial class Server {
        public partial class ReportServiceFace {
            public static readonly ReportServiceFace Instance = new ReportServiceFace();
            private static readonly string SControllerName = ControllerUtil.GetControllerName<IReportController>();

            private ReportServiceFace() { }

            public void ReportSpeedAsync(string host, SpeedData data, Action<ReportResponse> callback) {
                Task.Factory.StartNew(() => {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(3);
                    try {
                        using (HttpClient client = new HttpClient()) {
                            // 可能超过3秒钟，查查原因。因为我的网络不稳经常断线。
                            client.Timeout = timeSpan;
                            Task<HttpResponseMessage> getHttpResponse = client.PostAsJsonAsync($"http://{host}:{NTKeyword.ControlCenterPort}/api/{SControllerName}/{nameof(IReportController.ReportSpeed)}", data);
                            if (callback != null) {
                                callback.Invoke(getHttpResponse.Result.Content.ReadAsAsync<ReportResponse>().Result);
                            }
                            else {
                                Write.DevDebug($"{nameof(ReportSpeedAsync)} {getHttpResponse.Result.ReasonPhrase}");
                            }
                        }
                    }
                    catch (Exception e) {
                        Write.DevException(e);
                    }
                });
            }

            public void ReportStateAsync(string host, Guid clientId, bool isMining) {
                Task.Factory.StartNew(() => {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(3);
                    try {
                        using (HttpClient client = new HttpClient()) {
                            client.Timeout = timeSpan;
                            ReportState request = new ReportState {
                                ClientId = clientId,
                                IsMining = isMining
                            };
                            Task<HttpResponseMessage> getHttpResponse = client.PostAsJsonAsync($"http://{host}:{NTKeyword.ControlCenterPort}/api/{SControllerName}/{nameof(IReportController.ReportState)}", request);
                            Write.DevDebug($"{nameof(ReportStateAsync)} {getHttpResponse.Result.ReasonPhrase}");
                        }
                    }
                    catch (Exception e) {
                        Write.DevException(e);
                    }
                });
            }
        }
    }
}