package com.example.sharex.fragments

import android.content.*
import android.content.Context.MODE_PRIVATE
import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ListView
import android.widget.TextView
import androidx.fragment.app.Fragment
import androidx.localbroadcastmanager.content.LocalBroadcastManager
import com.example.sharex.helpers.FilesListAdapter
import com.example.sharex.R
import com.example.sharex.helpers.MainSocket
import com.example.sharex.helpers.SocketHelper
import com.example.sharex.model.FileData
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext


class ReceiveFragment(u: SocketHelper) : Fragment() {

    val utils = u

    private lateinit var filesStatus: TextView
    private lateinit var filesListView: ListView
    private lateinit var receiveFilesBtn: Button

    private lateinit var sharedpreferences: SharedPreferences
    val MyPREFERENCES = "dataTransfer"
    val TagName = "transferring"

    var isReceiving = false
    var transferring = false

    var recFilesCount = 0
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

    }

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_receive, container, false)
    }




    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)




        filesStatus = view.findViewById(R.id.filesStatus1)
        filesListView = view.findViewById(R.id.receiveFilesList)
        receiveFilesBtn = view.findViewById(R.id.receiveFiles)


        val fileList = ArrayList<FileData>()
        val filesAdapter = FilesListAdapter(requireContext(), fileList)
        filesListView.adapter = filesAdapter



        sharedpreferences = requireContext().getSharedPreferences(MyPREFERENCES, Context.MODE_PRIVATE)
        val prefs: SharedPreferences = requireContext().getSharedPreferences(MyPREFERENCES, MODE_PRIVATE)
        transferring = prefs.getBoolean(TagName, false)

        MainSocket.receiveCallback { receiveFiles(fileList, filesAdapter) }

        receiveFilesBtn.setOnClickListener {
            receiveFiles(fileList, filesAdapter)
        }
    }

    fun receiveFiles(fileList : ArrayList<FileData>, filesAdapter : FilesListAdapter)
    {

        GlobalScope.launch(Dispatchers.IO) {
            if(!isReceiving)
            {
                val editor = sharedpreferences.edit()
                editor.putBoolean(TagName, true)
                editor.apply()
                withContext(Dispatchers.Main)
                {
                    receiveFilesBtn.text = "receiving..."
                }
                isReceiving = true
                Log.d("TAG", "onCreate: receiving file...............")
                var filesCount : Int? = 0
                try {
                    val count = utils.receiveMsg()
                    filesCount = count.toIntOrNull()
                    Log.d("TAG", "onCreate: files count : $count")
                    if(filesCount != null)
                    {
                        for(i in 0 until filesCount)
                        {
                            Log.d("TAG", "onCreate: files $recFilesCount")
                            val file = utils.receiveFile(filesAdapter, recFilesCount)
                            recFilesCount++
                            Thread.sleep(2000)
                        }
                        editor.putBoolean(TagName, false)
                        editor.apply()
                    }
                    else
                    {
                        receiveFiles(fileList, filesAdapter)
                    }
                }
                catch (e :Exception)
                {
                    editor.putBoolean(TagName, false)
                    editor.apply()
                    e.printStackTrace()
                }
                finally {
                    isReceiving = false
                    withContext(Dispatchers.Main)
                    {
                        receiveFilesBtn.text = "receive"
                        if(fileList.size > 0 )
                        {
                            filesStatus.visibility = View.GONE
                        }
                    }
                }
            }
            else{
                isReceiving = false
                val editor = sharedpreferences.edit()
                editor.putBoolean(TagName, false)
                editor.apply()
                withContext(Dispatchers.Main)
                {
                    receiveFilesBtn.text = "receive"
                }
            }
        }
    }
}