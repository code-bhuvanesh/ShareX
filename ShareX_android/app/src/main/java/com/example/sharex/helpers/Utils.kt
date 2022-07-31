package com.example.sharex.helpers

import android.content.ContentUris
import android.content.Context
import android.database.Cursor
import android.net.Uri
import android.os.Environment
import android.provider.DocumentsContract
import android.provider.MediaStore
import android.util.Log
import kotlinx.coroutines.*
import java.io.*
import java.net.ServerSocket
import java.net.Socket
import java.nio.ByteBuffer


class Utils(private val port: Int = 6578) {
    lateinit var socket: Socket
    lateinit var server: Socket
    lateinit var serverSocket : ServerSocket
    lateinit var inputStream: InputStream
    lateinit var outputStream: OutputStream

    private val buffer = 1024 * 10000

    suspend fun connectToPC()
    {
        Log.d("server", "connectToPC: waiting for pc")
        serverSocket = ServerSocket(port)
        socket = serverSocket.accept()
        inputStream = socket.getInputStream()
        outputStream = socket.getOutputStream()

        Log.d("server", "connectToPC: Connected to Pc")
    }

    suspend fun connectToServer(ip:String): Boolean
    {
        try {
//            val client = Socket("192.168.29.180", 6578);
            server = Socket(ip, port);
            server.tcpNoDelay = true
            inputStream = server.getInputStream()
            outputStream = server.getOutputStream()

            return true
        }
        catch(e: Exception)
        {
            Log.d("server", "connectToServer: error connecting : ${e.message}")
            return false
        }

    }

    suspend fun onDisconnected(callback:()-> Unit)
    {
        while (true)
        {
            val isReachable = server.inetAddress.isReachable(10)
            Log.d("server", "isReachable = $isReachable")
            if(!isReachable)
                break
            continue
        }
        callback()

    }
    suspend fun sendMsg(msg:String)
    {
        val toSend = msg
        val toSendBytes = toSend.toByteArray()
        val toSendLen = toSendBytes.size
        val toSendLenBytes = ByteArray(4)
        toSendLenBytes[0] = (toSendLen and 0xff).toByte()
        toSendLenBytes[1] = (toSendLen shr 8 and 0xff).toByte()
        toSendLenBytes[2] = (toSendLen shr 16 and 0xff).toByte()
        toSendLenBytes[3] = (toSendLen shr 24 and 0xff).toByte()
        outputStream.write(toSendLenBytes)
        outputStream.write(toSendBytes)
    }

    suspend fun receiveMsg() : String
    {
        val lenBytes = ByteArray(4)
        inputStream.read(lenBytes, 0, 4)
//        Log.d("received bytes", "server: ${String(lenBytes,0,4)} ")
        val len: Int = byteToInt(lenBytes)
//        Log.d("received bytes", "onCreate: $len")
        val receivedBytes = ByteArray(len)
        inputStream.read(receivedBytes, 0, len)
        val received = String(receivedBytes, 0, len)
        Log.d("received bytes", "onCreate: $received")
        return  received
    }

    suspend fun receiveFile(): File
    {
        val fileName = receiveMsg()
        val dir = File(Environment.getExternalStorageDirectory(), "shareX")
        if(!dir.exists())
        {
            dir.mkdir()
        }
        val file = File(dir, fileName)

//        Log.d("server", "receiveFile: file path is ${file.absolutePath}")

        val fileSize = (receiveMsg()).toLong()
        Log.d("server", "receiveFile: file size $fileSize")

        val bytes = ByteArray(buffer)

        val fos = FileOutputStream(file)
        val bos = BufferedOutputStream(fos)

        var fileRead = 0

        while (fileRead < fileSize) {
            val bytesRead = inputStream.read(bytes)
            bos.write(bytes,0, bytesRead)
            fileRead += bytesRead
            var completed = String.format("%.2f", (((fileRead*1.0)/fileSize)*100) ).toDouble()
            completed = if(completed < 100.0) completed else 100.00
//            Log.d("server", "receiveFile: completed $completed" )
        }

        sendMsg("completed")
        receiveMsg()
        bos.close()
        fos.close()

        return file
    }

    lateinit var receiveThread: Job
    suspend fun receiveFile(filesListAdapter: FilesListAdapter, pos:Int): File
    {
        val fileName = receiveMsg()
        val dir = File(Environment.getExternalStorageDirectory(), "shareX")
        if(!dir.exists())
        {
            dir.mkdir()
        }
        val file = File(dir, fileName)

//        Log.d("server", "receiveFile: file path is ${file.absolutePath}")

        val fileSize = (receiveMsg()).toLong()
        Log.d("server", "receiveFile: file size $fileSize")
        val bytes = ByteArray(buffer)

        GlobalScope.launch(Dispatchers.Main)
        {
            filesListAdapter.addFile(fileName, getFileSize(fileSize))
        }

        val fos = FileOutputStream(file)
        val bos = BufferedOutputStream(fos)

        var fileRead = 0

        while (fileRead < fileSize) {
            Log.d("server", "receiveFileashdfsahfklsajklfdsjkfhjksdhfjkdshfkldsfklhdklfjdklfkldjf" )
            val bytesRead = inputStream.read(bytes)
            Log.d("server", "receiveFileashdfsahfklsajklfdsjkfhjksdhfjkdshfkldsfklhdklfjdklfkldjf" )
            bos.write(bytes,0, bytesRead)
            fileRead += bytesRead
            var completed = String.format("%.2f", (((fileRead*1.0)/fileSize)*100) ).toDouble()
            completed = if(completed < 100.0) completed else 100.00
            GlobalScope.launch(Dispatchers.Main)
            {
                filesListAdapter.updateStatus(pos,completed)
            }
//            Log.d("server", "receiveFile: completed $completed" )
        }

        sendMsg("completed")
        receiveMsg()
        bos.close()
        fos.close()
        return file
    }

    suspend fun sendFile(file: File) {
        val fileName = file.name
        val fileSize = file.length()

        sendMsg(fileName)
        sendMsg(fileSize.toString())

        val fis = FileInputStream(file)

        var bytes = ByteArray(buffer)

        Log.d("server", "sendFile: starting file transfer fileName : $fileName, filesize : $fileSize");
        var fileRead = 0
        while(fileRead < fileSize)
        {
            val bytesRead = fis.read(bytes)
            outputStream.write(bytes, 0, bytesRead)
            Log.d("server", "sendFile: .........................................")
            fileRead += bytesRead

            var completed = String.format("%.2f", (((fileRead*1.0)/fileSize)*100) ).toDouble()
            completed = if(completed < 100.0) completed else 100.00

            Log.d("server", "sendFile: completed $completed %")

        }
        receiveMsg()
        Log.d("server", "sendFile: file Sent")



    }

    suspend fun stopReceiving()
    {
        inputStream.close()
    }
    suspend fun stopSending()
    {
        outputStream.close()


    }

    suspend fun disconnet()
    {
        socket.close()
        serverSocket.close()
    }
/*
    fun byteToInt(bytes: ByteArray): Int {
        var result = 0
        for (i in bytes.indices) {
            result = result or (bytes[i].toInt() shl 8 * i)
        }
        return result
    }*/

    fun byteToInt(bytes: ByteArray): Int {
        var result = 0
        var shift = 0
        for (byte in bytes) {
            result = result or (byte.toInt() shl shift)
            shift += 8
        }
        bytes.reverse()
        return ByteBuffer.wrap(bytes).int
    }

    fun getPath(context: Context, uri: Uri): String? {
        // DocumentProvider
        if (DocumentsContract.isDocumentUri(context, uri)) {
            // ExternalStorageProvider
            if (isExternalStorageDocument(uri)) {
                val docId = DocumentsContract.getDocumentId(uri)
                val split = docId.split(":").toTypedArray()
                val type = split[0]
                if ("primary".equals(type, ignoreCase = true)) {
                    return Environment.getExternalStorageDirectory().toString() + "/" + split[1]
                }
                // TODO handle non-primary volumes
            } else if (isDownloadsDocument(uri)) {
                val id = DocumentsContract.getDocumentId(uri)
                val contentUri = ContentUris.withAppendedId(
                    Uri.parse("content://downloads/public_downloads"),
                    java.lang.Long.valueOf(id)
                )
                return getDataColumn(context, contentUri, null, null)
            } else if (isMediaDocument(uri)) {
                val docId = DocumentsContract.getDocumentId(uri)
                val split = docId.split(":").toTypedArray()
                val type = split[0]
                var contentUri: Uri? = null
                if ("image" == type) {
                    contentUri = MediaStore.Images.Media.EXTERNAL_CONTENT_URI
                } else if ("video" == type) {
                    contentUri = MediaStore.Video.Media.EXTERNAL_CONTENT_URI
                } else if ("audio" == type) {
                    contentUri = MediaStore.Audio.Media.EXTERNAL_CONTENT_URI
                }
                val selection = "_id=?"
                val selectionArgs = arrayOf(split[1])
                return getDataColumn(context, contentUri, selection, selectionArgs)
            }
        }
        return null
    }

    fun getDataColumn(
        context: Context,
        uri: Uri?,
        selection: String?,
        selectionArgs: Array<String>?
    ): String? {
        var cursor: Cursor? = null
        val column = "_data"
        val projection = arrayOf(column)
        try {
            cursor =
                context.getContentResolver()
                    .query(uri!!, projection, selection, selectionArgs, null)
            if (cursor != null && cursor.moveToFirst()) {
                val index: Int = cursor.getColumnIndexOrThrow(column)
                return cursor.getString(index)
            }
        } finally {
            if (cursor != null) cursor.close()
        }
        return null
    }

    fun isExternalStorageDocument(uri: Uri): Boolean {
        return "com.android.externalstorage.documents" == uri.authority
    }

    fun isDownloadsDocument(uri: Uri): Boolean {
        return "com.android.providers.downloads.documents" == uri.authority
    }

    fun isMediaDocument(uri: Uri): Boolean {
        return "com.android.providers.media.documents" == uri.authority
    }

    fun isGooglePhotosUri(uri: Uri): Boolean {
        return "com.google.android.apps.photos.content" == uri.authority
    }

    fun getFileSize(size: Long): String{
        return if (size > 1069547520) {
            String.format("%.2f", (size * 1.0 / 1069547520)) + " GB"
        } else if (size > 1048576) {
            String.format("%.2f", (size * 1.0 / 1048576)) + " MB"
        } else if (size > 1024) {
            String.format("%.2f", (size * 1.0 / 1024))+ " KB"
        } else {
            size.toString() + " B"
        }
    }



}