package com.example.sharex.fragments

import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.os.Bundle
import android.util.Log
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ListView
import android.widget.TextView
import androidx.activity.result.ActivityResult
import androidx.activity.result.ActivityResultCallback
import androidx.activity.result.contract.ActivityResultContracts
import com.example.sharex.helpers.FilesListAdapter
import com.example.sharex.R
import com.example.sharex.helpers.Utils
import com.example.sharex.model.FileData
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.File
import java.net.SocketException

class SendFragment(u: Utils) : Fragment() {

    val utils = u

    private lateinit var filesStatus: TextView
    private lateinit var filesListView:ListView
    private lateinit var selectFilesBtn:Button
    private lateinit var sendFilesBtn:Button

    val MyPREFERENCES = "dataTransfer"
    val TagName = "transferring"

    var transferring = false

    var sending = false


    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

    }

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        return inflater.inflate(R.layout.fragment_send, container, false)
    }


    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        filesStatus = view.findViewById(R.id.filesStatus)
        filesListView = view.findViewById(R.id.filesList)
        selectFilesBtn = view.findViewById(R.id.selectFiles)
        sendFilesBtn = view.findViewById(R.id.SendFiles)

        val fileList = ArrayList<FileData>()
        val filesToSend = ArrayList<File>()
        val filesAdapter = FilesListAdapter(requireContext(), fileList)
        filesListView.adapter = filesAdapter

        val resultLauncher = registerForActivityResult(
            ActivityResultContracts.StartActivityForResult(),
            ActivityResultCallback<ActivityResult> { result ->
                // Initialize result data
                val data: Intent = result.getData()!!
                // check condition

                if (null != data.clipData) {
                    for (i in 0 until data.clipData!!.itemCount) {
                        val uri = data.clipData!!.getItemAt(i).uri
                        val filePath = utils.getPath(requireContext(), uri);
                        val file = File(filePath!!)
                        if(!filesToSend.contains(file))
                        {
                            filesToSend.add(file)
                            val fileName = file.name
                            val fileSize = utils.getFileSize((file.length()))
                            fileList.add(FileData(fileName, fileSize))
                            filesAdapter.notifyDataSetChanged()
                            Log.d("TAG", "onCreate: file path is............. : $filePath")
                        }


                    }
                } else {
                    val filePath = utils.getPath(requireContext(), data.data!!);
                    val file = File(filePath!!)
                    if(!filesToSend.contains(file))
                    {
                        filesToSend.add(file)
                        val fileName = file.name
                        val fileSize = utils.getFileSize((file.length()))
                        fileList.add(FileData(fileName, fileSize))
                        filesAdapter.notifyDataSetChanged()
                        Log.d("TAG", "onCreate: file path is : $filePath")
                    }
                }
                if(fileList.size > 0)
                {
                    filesStatus.visibility = View.GONE
                }

            })

        selectFilesBtn.setOnClickListener {
            val intent = Intent(Intent.ACTION_GET_CONTENT)
            intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true)
            intent.type = "*/*"
            resultLauncher.launch(intent)
        }

        val sharedPreferences: SharedPreferences =
            requireContext().getSharedPreferences(MyPREFERENCES, Context.MODE_PRIVATE)
        val prefs: SharedPreferences = requireContext().getSharedPreferences(MyPREFERENCES,
            Context.MODE_PRIVATE
        )
        transferring = prefs.getBoolean(TagName, false)
        sendFilesBtn.setOnClickListener {
            GlobalScope.launch(Dispatchers.IO){
                sendFiles(sharedPreferences,filesToSend,fileList,filesAdapter)
            }
        }
    }

    private suspend fun sendFiles(sharedPreferences:SharedPreferences, filesToSend:ArrayList<File>, fileList:ArrayList<FileData>, filesAdapter: FilesListAdapter)
    {
        if(!sending && !transferring)
        {
            val editor = sharedPreferences.edit()
            editor.putBoolean(TagName, true)
            editor.apply()
            sending = true
            utils.sendMsg(filesToSend.size.toString())
            val countError = utils.receiveMsg()
            if(countError == "success")
            {
                for (i in 0 until filesToSend.size) {
                    try {
                        utils.sendFile(filesToSend[0])
                        Thread.sleep(100)
                        filesToSend.removeFirst()
                        fileList.removeFirst()
                        withContext(Dispatchers.Main)
                        {
                            filesAdapter.notifyDataSetChanged()
                        }
                    }
                    catch (e:SocketException)
                    {
                        e.printStackTrace()
                    }

                }
                editor.putBoolean(TagName, false)
                editor.apply()
                sending = false
            }
            else{
                sendFiles(sharedPreferences,filesToSend,fileList,filesAdapter)
                return
            }


        }
    }


}