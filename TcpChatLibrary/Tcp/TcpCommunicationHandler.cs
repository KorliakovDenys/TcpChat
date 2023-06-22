using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace TcpChatLibrary.Tcp;

public static class TcpCommunicationHandler{
    public delegate Task ResponseHandlerCallback(TcpClient client, string response);

    public delegate void ClientDisconnectHandlerCallback(TcpClient client);

    public static async Task ReceiveDataAsync(TcpClient client, CancellationToken token, ResponseHandlerCallback responseHandlerCallback, ClientDisconnectHandlerCallback clientDisconnectHandlerCallback){
        var networkStream = client.GetStream();

        try{
            var headerBuffer = new byte[sizeof(int)];

            while (!token.IsCancellationRequested){
                var bytesReceived = await networkStream.ReadAsync(headerBuffer, 0, 4, token);

                if (bytesReceived != sizeof(int))
                    break;

                var length = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer);

                var buffer = new byte[length];

                var count = 0;

                while (count < length){
                    bytesReceived = await networkStream.ReadAsync(buffer, count, buffer.Length - count, token);
                    count += bytesReceived;
                }

                var receivedData = Encoding.UTF8.GetString(buffer, 0, length);

                var message = receivedData.Trim();

                _ = responseHandlerCallback(client, message).ConfigureAwait(false);
            }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
        finally{
            networkStream.Close();
            client.Close();
            client.Dispose();
            
            clientDisconnectHandlerCallback(client);
            Debug.WriteLine(client.Client.RemoteEndPoint + " disconnected");
        }
    }
    public static async Task SendMessageAsync(Stream networkStream, string message){
             var headerBuffer = new byte[sizeof(int)];
             var buffer = Encoding.UTF8.GetBytes(message);
             BinaryPrimitives.WriteInt32LittleEndian(headerBuffer, buffer.Length);
     
             await networkStream.WriteAsync(headerBuffer, 0, headerBuffer.Length);
             await networkStream.WriteAsync(buffer, 0, buffer.Length);
             await networkStream.FlushAsync().ConfigureAwait(false);
         }

    public static void Disconnect(TcpClient client){
        try{
            client.GetStream().Close();
            client.Close();
            Debug.WriteLine("Disconnect() called");
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }
}