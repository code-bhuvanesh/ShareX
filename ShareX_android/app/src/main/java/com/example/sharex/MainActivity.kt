package com.example.sharex

import android.app.Activity
import android.content.ContentUris
import android.content.Context
import android.content.Intent
import android.content.res.Resources
import android.database.Cursor
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.os.Environment
import android.provider.DocumentsContract
import android.provider.MediaStore
import android.util.Log
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.TextView
import androidx.activity.result.ActivityResult
import androidx.activity.result.ActivityResultCallback
import androidx.activity.result.contract.ActivityResultContracts
import androidx.activity.result.contract.ActivityResultContracts.StartActivityForResult
import androidx.appcompat.app.AppCompatActivity
import androidx.viewpager.widget.ViewPager
import androidx.viewpager2.widget.ViewPager2
import com.google.android.material.tabs.TabLayout
import com.google.android.material.tabs.TabLayoutMediator
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.File
import java.lang.Exception


class MainActivity : AppCompatActivity() {

    private lateinit var tabLayout: TabLayout
    private lateinit var viewPager: ViewPager2
    private lateinit var statusText: TextView

    fun connectToPC(ip: String)
    {
        GlobalScope.launch(Dispatchers.IO) {
//            Utils.connectToPC()
            val connected = Utils.connectToServer(ip)
            Log.d("TAG", "onCreate: connected $connected")
            if (connected) {
                withContext(Dispatchers.Main)
                {
                    statusText.text = "connected"
                }

            }
            else{
                Log.d("TAG","retrying .....")
                connectToPC(ip)
            }

        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        tabLayout = findViewById(R.id.tabLayout)
        viewPager = findViewById(R.id.viewPager)
        statusText = findViewById(R.id.statusText)


        viewPager.adapter = VPAdapter(this)
        TabLayoutMediator(tabLayout, viewPager){tab,index ->
            tab.text = when(index){
                0->{"send"}
                1->{"receive"}
                else -> {throw Resources.NotFoundException("page not found")}
            }
        }.attach()
        var PCip = ""
        var resultLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == Activity.RESULT_OK) {
                val data: Intent? = result.data
                if (data != null) {
                    Log.d("TAG", "onCreate: data  not null")
                    if(data.getStringExtra("IP") != null){
                        PCip = data.getStringExtra("IP")!!
                        statusText.setOnClickListener{
                            connectToPC(PCip)
                        }

                        connectToPC(PCip)
                        Log.d("TAG", "IP is $PCip ...........................................")
                    }else{
                        Log.d("TAG", "onCreate: IP is null")
                    }
                }
            }
        }


        val intent = Intent(this,QRScannerActivity::class.java)
        resultLauncher.launch(intent)





//        val sendBtn = findViewById<Button>(R.id.send_btn)
//        val connectBtn = findViewById<Button>(R.id.connect_btn)
//        val inputText = findViewById<EditText>(R.id.input_text)
//        val receiveText = findViewById<TextView>(R.id.received_txt)
//
//        /*sendBtn.setOnClickListener {
//            GlobalScope.launch(Dispatchers.IO) {
//                Utils.sendMsg(inputText.text.toString());
//                withContext(Dispatchers.Main)
//                {
//                    inputText.setText("")
//                }
//            }
//        }*/
//        connectBtn.setOnClickListener {
//            GlobalScope.launch(Dispatchers.IO) {
////            Utils.connectToPC()
//                val connected = Utils.connectToServer()
//                Log.d("TAG", "onCreate: connected $connected")
//                if (connected) {
//                    withContext(Dispatchers.Main)
//                    {
//                        receiveText.text = "connected"
//                    }
//                    Utils.sendMsg("hello")
//                    withContext(Dispatchers.Main)
//                    {
//                        receiveText.text = "recieving"
//                    }
////                    Utils.receiveFile(receiveText)
////                    while (true) {
////                        val rcv = Utils.receiveMsg()
////                        withContext(Dispatchers.Main){
////                            receiveText.text = rcv
////                        }
////                    }
//                }
//
//            }
//        }
//
//
//        val fileList = ArrayList<File>()
//
//        val resultLauncher = registerForActivityResult(
//            StartActivityForResult(),
//            ActivityResultCallback<ActivityResult> { result ->
//                // Initialize result data
//                val data: Intent = result.getData()!!
//                // check condition
//
////                if (data != null) {
////                    val filePath = getPath(this,data.data!!);
////                    val file = File(filePath)
////                    Log.d("TAG", "onCreate: file path is : $filePath")
////                    GlobalScope.launch(Dispatchers.IO) {
////                        Utils.sendFile(file)
////                    }
////                }
//
//                if (null != data.clipData) {
//                    GlobalScope.launch(Dispatchers.IO) {
//                        Utils.sendMsg(data.clipData!!.itemCount.toString())
//                        for (i in 0 until data.clipData!!.itemCount) {
//                            val uri = data.clipData!!.getItemAt(i).uri
//                            val filePath = Utils.getPath(this@MainActivity, uri);
//                            val file = File(filePath)
//                            Log.d("TAG", "onCreate: file path is............. : $filePath")
//                            Utils.sendFile(file)
//                            Thread.sleep(500)
//                        }
//                    }
//                } else {
//                    val filePath = Utils.getPath(this, data.data!!);
//                    val file = File(filePath)
//                    Log.d("TAG", "onCreate: file path is : $filePath")
//                    GlobalScope.launch(Dispatchers.IO) {
//                        Utils.sendMsg("1")
//                        Utils.sendFile(file)
//                    }
//                }
//            })
//
//        sendBtn.setOnClickListener {
//            val intent = Intent(Intent.ACTION_GET_CONTENT)
//            intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true)
//            intent.type = "*/*"
//            resultLauncher.launch(intent)
//        }
//
//
//        val receiveBtn = findViewById<Button>(R.id.receive_btn)
//        var receive = false;
//        receiveBtn.setOnClickListener {
//            GlobalScope.launch(Dispatchers.IO) {
//                if(!receive)
//                {
//                    receive = true
//                    Log.d("TAG", "onCreate: receiving file...............")
//                    var filesCount = 0
//                    try {
//                        val a = Utils.receiveMsg()
//                        filesCount = a.toInt()
//                        Log.d("TAG", "onCreate: files count : $a")
//                        for(i in 0 until filesCount)
//                        {
//                            Log.d("TAG", "onCreate: files $i")
//                            Utils.receiveFile(receiveText)
//                            Thread.sleep(500)
//                        }
//                    }
//                    catch (e :Exception)
//                    {
//                        e.printStackTrace()
//                    }
//                    finally {
//                        receive = false
//                    }
//                }
//
//
//            }
//        }


    }

    fun uriToPath(uri: Uri?): String? {

        if (uri!!.path != null) {
            val file = File(uri.path!!)

            val split = file.path.split(":").toTypedArray() //split the path.

            return Environment.getExternalStorageDirectory().absolutePath + "/" + split[1] //assign it to a string(your choice).
        }
        return null;

    }




}