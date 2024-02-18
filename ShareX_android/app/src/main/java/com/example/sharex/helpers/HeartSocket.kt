package com.example.sharex.helpers

import android.util.Log
import com.example.sharex.MainActivity
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import java.net.Socket
import java.net.SocketException

object HeartSocket {
    var heartSocket = SocketHelper(1233)
    var isConnected = false
    public fun connect(ip : String, main:MainActivity){
        CoroutineScope(Dispatchers.IO).launch {
            while (!isConnected)
            {
                Log.d("heartSocket", "waiting for connection")
                isConnected = heartSocket.connectToServer(ip)
                if (isConnected)
                    Log.d("heartSocket", "heartSocket connected")
                else
                    Log.d("heartSocket", "heartSocket not connected")
            }

            while (true)
            {
                try {
                    Thread.sleep(1000)
//                    var b = ByteArray(1)
//                    Log.d("hearSocket", " is alive : ${heartSocket.inputStream.read(b)}")
//                    Log.d("hearSocket", " is alive 1 : ${heartSocket.inputStream.available()}")
//                    heartSocket.outputStream.write(byteArrayOf(1),0,1);
//                    val rcvMsg = heartSocket.receiveMsg();
//                    Log.d("hearSocket", "rcv msg is $rcvMsg ")
//                    heartSocket.sendMsg(rcvMsg)
//                    Log.d("hearSocket", "msg sent")
                    Log.d("hearSocket", "is connected = ${checkConnection(heartSocket.server)}")

                }
                catch (e : SocketException)
                {
                    isConnected = false
                    Log.d("hearSocket", "hearSocket error: ${e.message} ")
                    break
                }

            }
            main.disconnectPc()
            main.connectToPC(ip)
        }


    }
    suspend fun checkConnection(socket : Socket): Boolean{
        return socket.getInputStream().read() != -1
    }
}