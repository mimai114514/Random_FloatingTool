using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Random_FloatingTool
{
    class RemoteControl
    {
        private WebSocketServer server;
        private List<Fleck.IWebSocketConnection> allSockets = new List<Fleck.IWebSocketConnection>();

        public void Start()
        {
            allSockets = new List<IWebSocketConnection>();
            // 监听所有网络接口的 8181 端口
            // 使用 "ws://0.0.0.0:8181" 确保局域网内的设备可以访问
            server = new WebSocketServer("ws://0.0.0.0:8181");

            // 使用 Task.Run 在后台线程启动服务器，避免阻塞UI
            Task.Run(() =>
            {
                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        Console.WriteLine("A client connected!");
                        allSockets.Add(socket);
                        Broadcast("Hello Client");
                    };
                    socket.OnClose = () =>
                    {
                        Console.WriteLine("A client disconnected!");
                        allSockets.Remove(socket);
                    };
                    socket.OnMessage = message =>
                    {
                        Console.WriteLine($"Received command: {message}");

                        // 重要：从后台线程安全地更新UI
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 在这里处理来自手机的命令
                            // 例如，((MainWindow)Application.Current.MainWindow).HandleCommand(message);
                            // 为了演示，我们只是广播消息
                            Broadcast($"Echo from WPF: {message}");
                        });
                    };
                });
            });
        }

        public void Stop()
        {
            // 关闭所有连接并停止服务器
            foreach (var socket in allSockets.ToList())
            {
                socket.Close();
            }
            server?.Dispose();
        }

        // 向所有连接的手机客户端广播消息
        public void Broadcast(string message)
        {
            foreach (var socket in allSockets)
            {
                socket.Send(message);
            }
        }
    }
}
