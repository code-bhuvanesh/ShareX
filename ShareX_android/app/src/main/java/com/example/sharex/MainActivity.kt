package com.example.sharex

import android.app.Activity
import android.content.Intent
import android.content.res.Resources
import android.net.Uri
import android.os.Bundle
import android.os.Environment
import android.util.Log
import android.widget.TextView
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.viewpager2.widget.ViewPager2
import com.example.sharex.helpers.SendImages
import com.example.sharex.helpers.Utils
import com.example.sharex.helpers.VPAdapter
import com.google.android.material.tabs.TabLayout
import com.google.android.material.tabs.TabLayoutMediator
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.File


class MainActivity : AppCompatActivity() {

    private lateinit var tabLayout: TabLayout
    private lateinit var viewPager: ViewPager2
    private lateinit var statusText: TextView

    fun connectToPC(utils:Utils, ip: String)
    {
        GlobalScope.launch(Dispatchers.IO) {
//            utils.connectToPC()
            val connected = utils.connectToServer(ip)
            Log.d("TAG", "onCreate: connected $connected")
            if (connected) {

                withContext(Dispatchers.Main)
                {
                    statusText.text = "connected"
                }
                utils.onDisconnected {
                    Log.d("TAG","disconnected from host")
                }

            }
            else{
                Log.d("TAG","retrying .....")
                connectToPC(utils,ip)
            }
            SendImages(ip)

        }
    }

    val SP_HOST_DATA_NAMME = "hostInfo"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)


        tabLayout = findViewById(R.id.tabLayout)
        viewPager = findViewById(R.id.viewPager)
        statusText = findViewById(R.id.statusText)

        val utils = Utils(6578)

        viewPager.adapter = VPAdapter(this,utils)
        TabLayoutMediator(tabLayout, viewPager){tab,index ->
            tab.text = when(index){
                0->{"send"}
                1->{"receive"}
                else -> {throw Resources.NotFoundException("page not found")}
            }
        }.attach()

        val sharedPreferences = getSharedPreferences("hostInfo", MODE_PRIVATE)

        var hostIp = sharedPreferences.getString("IP","").toString()
        var PCip = ""


        val resultLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == Activity.RESULT_OK) {
                val data: Intent? = result.data
                if (data != null) {
                    Log.d("TAG", "onCreate: data  not null")
                    if(data.getStringExtra("IP") != null){
                        PCip = data.getStringExtra("IP")!!
                        val spEditor =  sharedPreferences.edit()
                        spEditor.putString("IP",PCip)
                        spEditor.apply()
                        statusText.setOnClickListener{
                            connectToPC(utils, PCip)
                        }
                        connectToPC(utils, PCip)
                        Log.d("TAG", "IP is $PCip")
                    }else{
                        Log.d("TAG", "onCreate: IP is null")
                    }
                }
            }
        }


        if(hostIp.isEmpty())
        {
            val intent = Intent(this,QRScannerActivity::class.java)
            resultLauncher.launch(intent)
        }
        else
        {
            statusText.setOnClickListener{
                connectToPC(utils, hostIp)
            }

            connectToPC(utils, hostIp)
        }






//        val sendBtn = findViewById<Button>(R.id.send_btn)
//        val connectBtn = findViewById<Button>(R.id.connect_btn)
//        val inputText = findViewById<EditText>(R.id.input_text)
//        val receiveText = findViewById<TextView>(R.id.received_txt)
//
//        /*sendBtn.setOnClickListener {
//            GlobalScope.launch(Dispatchers.IO) {
//                utils.sendMsg(inputText.text.toString());
//                withContext(Dispatchers.Main)
//                {
//                    inputText.setText("")
//                }
//            }
//        }*/
//        connectBtn.setOnClickListener {
//            GlobalScope.launch(Dispatchers.IO) {
////            utils.connectToPC()
//                val connected = utils.connectToServer()
//                Log.d("TAG", "onCreate: connected $connected")
//                if (connected) {
//                    withContext(Dispatchers.Main)
//                    {
//                        receiveText.text = "connected"
//                    }
//                    utils.sendMsg("hello")
//                    withContext(Dispatchers.Main)
//                    {
//                        receiveText.text = "recieving"
//                    }
////                    utils.receiveFile(receiveText)
////                    while (true) {
////                        val rcv = utils.receiveMsg()
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
////                        utils.sendFile(file)
////                    }
////                }
//
//                if (null != data.clipData) {
//                    GlobalScope.launch(Dispatchers.IO) {
//                        utils.sendMsg(data.clipData!!.itemCount.toString())
//                        for (i in 0 until data.clipData!!.itemCount) {
//                            val uri = data.clipData!!.getItemAt(i).uri
//                            val filePath = utils.getPath(this@MainActivity, uri);
//                            val file = File(filePath)
//                            Log.d("TAG", "onCreate: file path is............. : $filePath")
//                            utils.sendFile(file)
//                            Thread.sleep(500)
//                        }
//                    }
//                } else {
//                    val filePath = utils.getPath(this, data.data!!);
//                    val file = File(filePath)
//                    Log.d("TAG", "onCreate: file path is : $filePath")
//                    GlobalScope.launch(Dispatchers.IO) {
//                        utils.sendMsg("1")
//                        utils.sendFile(file)
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
//                        val a = utils.receiveMsg()
//                        filesCount = a.toInt()
//                        Log.d("TAG", "onCreate: files count : $a")
//                        for(i in 0 until filesCount)
//                        {
//                            Log.d("TAG", "onCreate: files $i")
//                            utils.receiveFile(receiveText)
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