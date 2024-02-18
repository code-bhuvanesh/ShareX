package com.example.sharex.helpers

import android.util.Log
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

enum class SocketMode {
    SendingFile,
    ReceivingFile,
    SendPhotos,
    ReceivePhotos,
    SendMessage,
    ReceiveMessage,
    GetImage,
    Notification,
    Any
}

object MainSocket {

    private const val port = 6774

    private var receiveFile: (() -> Unit)? = null
    private lateinit var socket: SocketHelper

    public var isConnected = false
    public fun connectMainSocket(ip: String): Boolean {
        socket = SocketHelper(port, "Main_server")
        CoroutineScope(Dispatchers.IO).launch {

            while (!isConnected) {
                Log.d("mainSocket1", "mainSocket waiting")
                isConnected = socket.connectToServer(ip)
                if (isConnected)
                    Log.d("mainSocket1", "mainSocket connected")
                else
                    Log.d("mainSocket1", "mainSocket not connected")
            }

            receiveThread()
        }
        return isConnected
    }

    private suspend fun receiveThread() {
        var currentMode = SocketMode.Any;
        while (true) {
            if(isConnected)
            {
                try {
                    val rcvMsg = socket.receiveMsg();
                    if (rcvMsg.isEmpty())
                        continue
                    Log.d("rcvmsg", "rcvmsg is $rcvMsg")
                    var rcvInt: Int? = -1;
                    if (rcvMsg != "") {
                        if (rcvInt != null) {
                            rcvInt = rcvMsg.toIntOrNull()
                            currentMode = SocketMode.valueOf(rcvMsg);
                            Log.d("mainSocket1", "$currentMode is not in socket mode ENUM 111")

                            when (currentMode) {
                                SocketMode.SendingFile -> {
                                    if (receiveFile != null) {
                                        Log.d(
                                            "mainSocket1",
                                            "$currentMode is not in socket mode ENUM 111"
                                        )
                                        receiveFile?.invoke()
                                    }
                                }
                                else -> {
                                    Log.d("mainSocket1", "$currentMode is not in socket mode ENUM")
                                }
                            }
                        } else {
                            //Debug.WriteLine($"rcv msg is {rcvMsg}");
                        }
                    }
                } catch (e: Exception) {
//                    Log.d("rcvmsg", "rcvmsg is error ${e.message}")
                }
            }
        }

    }

    public fun sendMsg(mode: SocketMode) {
        CoroutineScope(Dispatchers.IO).launch {
            if(isConnected)
            {
                Log.d("mainSocket1", "message sent from main socket")
                socket.sendMsg(mode.ordinal.toString())
            }
        }
    }

    public fun disconnect() {
        CoroutineScope(Dispatchers.IO).launch {
            socket.disconnect()
        }
    }

    public fun receiveCallback(receiveFun: () -> Unit) {
        receiveFile = receiveFun
    }
}