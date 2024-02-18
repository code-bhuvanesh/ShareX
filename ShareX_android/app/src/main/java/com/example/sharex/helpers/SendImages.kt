package com.example.sharex.helpers

import android.os.Environment
import android.util.Log
import kotlinx.coroutines.*
import java.io.File
import java.net.SocketException

object SendImages{

    private val port = 7586

    public var isConnected = false
    lateinit var pSocketHelper :SocketHelper
    fun startSendingPhotos( ip: String)
    {
//        create separate socket for photo transfer
        pSocketHelper = SocketHelper(port, "photo_server")

        //get path of DCIM/camera and get image files
        val imgDirPath =
            Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).absolutePath + "/Camera"
        val imgDir = File(imgDirPath)
        val imgList = imgDir.listFiles { file ->
            file.name.contains(".jpg") && file.name.substring(0,1) != "."
        }

        CoroutineScope(Dispatchers.IO).launch {
            val imgList1 = sortFiles(imgList!!.asIterable()).toMutableList()

            var totalImages = imgList1.size
            totalImages = 200

            Log.d("photo_server", "files List : ${imgList1.size}")

            while (!isConnected)
            {
                isConnected = pSocketHelper.connectToServer(ip)
                Log.d("photo_server", "isConnected $isConnected")
            }
//            Log.d("photo_server", "startSendingPhotos: test msg is ${pSocketHelper.receiveMsg()}")
            pSocketHelper.sendMsg(totalImages.toString())
            try {
                for (i in 0 until totalImages) {
                    pSocketHelper.sendMsg(imgList1[i].name)
                    if(pSocketHelper.receiveMsg() != "success")
                    {
                        pSocketHelper.sendFile(imgList1[i])
                    }
                }
            }
            catch (e : SocketException)
            {
                try{
                    pSocketHelper.disconnect()
                    isConnected = false;
                }
                catch (e : SocketException){}
                isConnected = false;
            }

        }


    }

    suspend fun sortFiles(files: Iterable<File>): List<File> {
        val metadataReadTasks: List<Deferred<FileWithMetadata>> = withContext(Dispatchers.IO)
        {
            files.map { file ->
                async {
                    FileWithMetadata(file)
                }
            }
        }
        val metadatas: List<FileWithMetadata> = metadataReadTasks.awaitAll()
        return metadatas
            .sorted()
            .map {
                it.file
            }
    }
    public fun disconnect() {
        CoroutineScope(Dispatchers.IO).launch {
            pSocketHelper.disconnect()
        }
    }


    private class FileWithMetadata(
        val file: File
    ) : Comparable<FileWithMetadata> {
        private val lastModified = file.lastModified()
        override fun compareTo(other: FileWithMetadata): Int {
            return other.lastModified.compareTo(this.lastModified)
        }
    }
}