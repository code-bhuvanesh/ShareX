package com.example.sharex.helpers

import android.os.Environment
import android.util.Log
import kotlinx.coroutines.*
import java.io.File
import java.util.*
import kotlin.Comparator

class SendImages(ip: String) {

    private val port = 2347

    init {
        //create separate socket for photo transfer
        val pUtils = Utils(port)

        //get path of DCIM/camera and get image files
        val imgDirPath =
            Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).absolutePath + "/Camera"
        val imgDir = File(imgDirPath)
        var imgList = imgDir.listFiles { file ->
            file.name.contains(".jpg") && file.name.substring(0,1) != "."
        }

        //sorting the list of files
//        if (imgList != null && imgList.size > 1) {
//            imgList.sortedArrayWith{
//                    object1, object2 ->
//                if (object1.lastModified() > object2.lastModified()) object1.lastModified().toInt()
//                else object2.lastModified().toInt()
//            }
//        }


        //starting transferring photos
        CoroutineScope(Dispatchers.IO).launch {
            imgList = sortFiles(imgList.asIterable()).toList().toTypedArray()

            var totalImages = imgList.size
//            totalImages = 10

            if (imgList != null) {
                Log.d("sendImages", "files List : ${imgList.size}")
            }
            pUtils.connectToServer(ip)
            pUtils.sendMsg(totalImages.toString())
            if (imgList != null) {
                for (i in 0 until totalImages) {
                    pUtils.sendFile(imgList[i])
                    Log.d("sendImages", "sent ${i + 1} images")
                }
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

    private class FileWithMetadata(
        val file: File
    ) : Comparable<FileWithMetadata> {
        private val lastModified = file.lastModified()
        override fun compareTo(other: FileWithMetadata): Int {
            return other.lastModified.compareTo(this.lastModified)
        }
    }
}