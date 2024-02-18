package com.example.sharex

import android.Manifest
import android.app.Activity
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.content.pm.PackageManager
import android.content.res.Resources
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.os.Environment
import android.provider.Settings
import android.util.Log
import android.widget.TextView
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.viewpager2.widget.ViewPager2
import com.example.sharex.helpers.*
import com.google.android.material.tabs.TabLayout
import com.google.android.material.tabs.TabLayoutMediator
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.File


class MainActivity : AppCompatActivity() {

    private lateinit var tabLayout: TabLayout
    private lateinit var viewPager: ViewPager2
    private lateinit var statusText: TextView

    private val STORAGE_PERMISSION_CODE = 100
    private val  TAG = "PERMISSION_TAG"

    private var isConnected = false
    private var isSendSocketConnected = false
    private var isReceiveSocketConnected = false

    private lateinit var sendSocket : SocketHelper
    private lateinit var receiveSocket : SocketHelper
    private lateinit var heartSocket : HeartSocket

    public fun connectToPC(ip: String)
    {

        if(!HeartSocket.isConnected)
        {
            HeartSocket.connect(ip, this)
        }
        if(!MainSocket.isConnected)
        {
            MainSocket.connectMainSocket(ip)
        }
        if(!SendImages.isConnected)
        {
            SendImages.startSendingPhotos(ip)
        }

        CoroutineScope(Dispatchers.IO).launch {
            while (!isSendSocketConnected)
            {
                isSendSocketConnected = sendSocket.connectToServer(ip)
            }
        }
        CoroutineScope(Dispatchers.IO).launch {
            while (!isReceiveSocketConnected)
            {
                isReceiveSocketConnected = receiveSocket.connectToServer(ip)
            }
        }
        CoroutineScope(Dispatchers.IO).launch {
            while(true)
            {

                if (isConnected) {

                    withContext(Dispatchers.Main)
                    {
                        statusText.text = "connected"
                    }
                    break
                }else{
                    isConnected = isSendSocketConnected && isReceiveSocketConnected && MainSocket.isConnected && SendImages.isConnected && HeartSocket.isConnected

                }
            }
        }
    }

    public fun disconnectPc()
    {

        if(MainSocket.isConnected)
        {
            MainSocket.disconnect()
            MainSocket.isConnected = false
        }
        if(SendImages.isConnected)
        {
            SendImages.disconnect()
            SendImages.isConnected = false
        }

        CoroutineScope(Dispatchers.IO).launch {
            while (isSendSocketConnected)
            {
                sendSocket.disconnect()
                isSendSocketConnected = false
            }
        }
        CoroutineScope(Dispatchers.IO).launch {
            while (!isReceiveSocketConnected)
            {
                receiveSocket.disconnect()
                isReceiveSocketConnected = false
            }
        }
        CoroutineScope(Dispatchers.IO).launch {
            isConnected = false
            withContext(Dispatchers.Main)
            {
                statusText.text = "not connected"
            }
        }
    }

    private var nReceiver: NotificationReceiver? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        if (!checkPermission()){
            Log.d(TAG, "onCreate: Permission was not granted, request")
            requestPermission()
        }





        tabLayout = findViewById(R.id.tabLayout)
        viewPager = findViewById(R.id.viewPager)
        statusText = findViewById(R.id.statusText)

        sendSocket = SocketHelper(3243, "SendSocket")
        receiveSocket = SocketHelper(3233, "ReceiveSocket")

        viewPager.adapter = VPAdapter(this,sendSocket, receiveSocket)
        TabLayoutMediator(tabLayout, viewPager){tab,index ->
            tab.text = when(index){
                0->{"notification"}
                1->{"send"}
                2->{"receive"}
                else -> {throw Resources.NotFoundException("page not found")}
            }
        }.attach()

        val sharedPreferences = getSharedPreferences("hostInfo", MODE_PRIVATE)

        var hostIp = ""// sharedPreferences.getString("IP","").toString()
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
                            connectToPC(PCip)

                        }
                        connectToPC(PCip)

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
                if(!isConnected)
                {
                    connectToPC(hostIp)
                }
            }
            connectToPC(hostIp)
        }

        //notification services
        nReceiver = NotificationReceiver()
        val filter = IntentFilter()
        filter.addAction("com.example.sharex.NOTIFICATION_LISTENER_EXAMPLE")
        registerReceiver(nReceiver, filter)


    }

    internal class NotificationReceiver : BroadcastReceiver() {
        override fun onReceive(context: Context?, intent: Intent) {

            val temp = """
               ${intent.getStringExtra("notification_event")}
               """.trimIndent()
            Log.d("notification", "*********************************notification posted ")

        }
    }

    override fun onDestroy() {
        super.onDestroy()
        unregisterReceiver(nReceiver)
    }


    private fun requestPermission(){
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R){
            //Android is 11(R) or above
            try {
                Log.d("TAG", "requestPermission: try")
                val intent = Intent()
                intent.action = Settings.ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION
                val uri = Uri.fromParts("package", this.packageName, null)
                intent.data = uri
                storageActivityResultLauncher.launch(intent)
            }
            catch (e: Exception){
                Log.e("TAG", "requestPermission: ", e)
                val intent = Intent()
                intent.action = Settings.ACTION_MANAGE_ALL_FILES_ACCESS_PERMISSION
                storageActivityResultLauncher.launch(intent)
            }
        }
        else{
            //Android is below 11(R)
            ActivityCompat.requestPermissions(this,
                arrayOf(Manifest.permission.WRITE_EXTERNAL_STORAGE, Manifest.permission.READ_EXTERNAL_STORAGE),
                STORAGE_PERMISSION_CODE
            )
        }
    }

    private val storageActivityResultLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()){
        Log.d("TAG", "storageActivityResultLauncher: ")
        //here we will handle the result of our intent
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R){
            //Android is 11(R) or above
            if (Environment.isExternalStorageManager()){
                //Manage External Storage Permission is granted
                Log.d("TAG", "storageActivityResultLauncher: Manage External Storage Permission is granted")
            }
            else{
                //Manage External Storage Permission is denied....
                Log.d("TAG", "storageActivityResultLauncher: Manage External Storage Permission is denied....")
            }
        }
        else{
            //Android is below 11(R)
        }
    }

    private fun checkPermission(): Boolean{
        return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R){
            //Android is 11(R) or above
            Environment.isExternalStorageManager()
        }
        else{
            //Android is below 11(R)
            val write = ContextCompat.checkSelfPermission(this, Manifest.permission.WRITE_EXTERNAL_STORAGE)
            val read = ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
            write == PackageManager.PERMISSION_GRANTED && read == PackageManager.PERMISSION_GRANTED
        }
    }


    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == STORAGE_PERMISSION_CODE){
            if (grantResults.isNotEmpty()){
                //check each permission if granted or not
                val write = grantResults[0] == PackageManager.PERMISSION_GRANTED
                val read = grantResults[1] == PackageManager.PERMISSION_GRANTED
                if (write && read){
                    //External Storage Permission granted
                    Log.d(TAG, "onRequestPermissionsResult: External Storage Permission granted")
                }
                else{
                    //External Storage Permission denied...
                    Log.d(TAG, "onRequestPermissionsResult: External Storage Permission denied...")
                }
            }
        }
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