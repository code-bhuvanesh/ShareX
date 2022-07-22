package com.example.sharex

import android.os.Bundle
import android.util.Log
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ListView
import android.widget.TextView
import com.example.sharex.model.FileData
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class ReceiveFragment : Fragment() {

    private lateinit var filesStatus: TextView
    private lateinit var filesListView: ListView
    private lateinit var receiveFilesBtn: Button

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
        filesListView = view.findViewById(R.id.receiveFilesList )
        receiveFilesBtn = view.findViewById(R.id.receiveFiles)


        val fileList = ArrayList<FileData>()
        val filesAdapter = FilesListAdapter(requireContext(), fileList)
        filesListView.adapter = filesAdapter

        var isReceiving = false
        receiveFilesBtn.setOnClickListener {
            GlobalScope.launch(Dispatchers.IO) {
                if(!isReceiving)
                {
                    withContext(Dispatchers.Main)
                    {
                        receiveFilesBtn.text = "receiving..."
                    }
                    isReceiving = true
                    Log.d("TAG", "onCreate: receiving file...............")
                    var filesCount = 0
                    try {
                        val count = Utils.receiveMsg()
                        filesCount = count.toInt()
                        Log.d("TAG", "onCreate: files count : $count")
                        for(i in 0 until filesCount)
                        {
                            Log.d("TAG", "onCreate: files $i")
                            val file = Utils.receiveFile(filesAdapter, i)
                            Thread.sleep(2000)
                        }
                    }
                    catch (e :Exception)
                    {
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
//                    isReceiving = false
                }


            }
        }


    }
}