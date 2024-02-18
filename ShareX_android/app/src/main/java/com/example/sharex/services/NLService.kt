package com.example.sharex.services

import android.app.Notification
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.content.pm.ApplicationInfo
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.drawable.BitmapDrawable
import android.graphics.drawable.Drawable
import android.service.notification.NotificationListenerService
import android.service.notification.StatusBarNotification
import android.util.Log
import com.example.sharex.helpers.MainSocket
import com.example.sharex.helpers.SocketHelper
import com.example.sharex.helpers.SocketMode
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch


class NLService : NotificationListenerService() {
    private val TAG = this.javaClass.simpleName
    private var nlservicereciver: NLServiceReceiver? = null
    private val port = 5467
    private lateinit var nSocketHelper: SocketHelper;

    override fun onCreate() {
        super.onCreate()
        nlservicereciver = NLServiceReceiver()
        nSocketHelper = SocketHelper(port, "NotificationSocket")
        val sharedPreferences = getSharedPreferences("hostInfo", MODE_PRIVATE)
        val hostIp = sharedPreferences.getString("IP","").toString()
        Log.d("notification", "**********  started")

        val filter = IntentFilter()
        filter.addAction("com.example.sharex.NOTIFICATION_LISTENER_SERVICE_EXAMPLE")
        registerReceiver(nlservicereciver, filter)
        CoroutineScope(Dispatchers.IO).launch {
            nSocketHelper.connectToServer(hostIp)
        }


    }

    override fun onDestroy() {
        super.onDestroy()
//        unregisterReceiver(nlservicereciver)
    }

    override fun onNotificationPosted(sbn: StatusBarNotification) {
        Log.d("notification", "**********  onNotificationPosted")


        val pack = sbn.packageName
        var ticker = ""
        if (sbn.notification.tickerText != null) {
            ticker = sbn.notification.tickerText.toString()
        }
        val extras = sbn.notification.extras
        val title = extras.getString("android.title").toString()
        val text = extras.getCharSequence("android.text").toString()
        var icon: Drawable? = null
        try {
            icon = packageManager.getApplicationIcon(pack)
        } catch (e: Exception) {
            e.printStackTrace()
        }

        val pm = applicationContext.packageManager
        val ai: ApplicationInfo?
        ai = try {
            pm.getApplicationInfo(pack, 0)
        } catch (e: PackageManager.NameNotFoundException) {
            null
        }
        val applicationName =
            (if (ai != null) pm.getApplicationLabel(ai) else "(unknown)") as String



//        CoroutineScope(Dispatchers.IO).launch {
//            MainSocket.sendMsg(SocketMode.Notification)
//            if (icon != null) {
//                val iconBitmap = drawableToBitmap(icon);
//                Log.d(TAG, "onNotificationPosted: notification icon true")
//                nSocketHelper.sendMsg("true")
//                nSocketHelper.sendBitmap(iconBitmap!!);
//
//
//                if (extras.containsKey(Notification.EXTRA_PICTURE)) {
//                    // this bitmap contain the picture attachment
//                    val bmp = extras[Notification.EXTRA_PICTURE] as Bitmap?
//                    nSocketHelper.sendMsg("true")
//                    nSocketHelper.sendBitmap(bmp!!);
//                }
//
//            }
//            else
//            {
//                Log.d(TAG, "onNotificationPosted: notification icon false")
//                nSocketHelper.sendMsg("false")
//            }
//            try {
//                nSocketHelper.sendMsg(pack)
//                nSocketHelper.sendMsg(ticker)
//                nSocketHelper.sendMsg(title)
//                nSocketHelper.sendMsg(text)
//                nSocketHelper.sendMsg(applicationName)
//            }
//            catch (e : Exception)
//            {
//                e.printStackTrace()
//            }
//
//        }
        Log.i("", "*")
        Log.i("Package", pack)
        Log.i("Ticker", ticker)
        Log.i("Title", title)
        Log.i("Text", text)


    }



    private fun drawableToBitmap(drawable: Drawable): Bitmap? {
        var bitmap: Bitmap? = null
        if (drawable is BitmapDrawable) {
            val bitmapDrawable = drawable
            if (bitmapDrawable.bitmap != null) {
                return bitmapDrawable.bitmap
            }
        }
        bitmap = if (drawable.intrinsicWidth <= 0 || drawable.intrinsicHeight <= 0) {
            Bitmap.createBitmap(
                1,
                1,
                Bitmap.Config.ARGB_8888
            ) // Single color bitmap will be created of 1x1 pixel
        } else {
            Bitmap.createBitmap(
                drawable.intrinsicWidth,
                drawable.intrinsicHeight,
                Bitmap.Config.ARGB_8888
            )
        }
        val canvas = Canvas(bitmap)
        drawable.setBounds(0, 0, canvas.getWidth(), canvas.getHeight())
        drawable.draw(canvas)
        return bitmap
    }

    override fun onNotificationRemoved(sbn: StatusBarNotification) {
//        Log.i(TAG, "********** onNOtificationRemoved")
//        Log.i(TAG, "ID :" + sbn.id + "\t" + sbn.notification.tickerText + "\t" + sbn.packageName)
//        val i = Intent("com.example.sharex.NOTIFICATION_LISTENER_EXAMPLE")
//        i.putExtra(
//            "notification_event", """
//     onNotificationRemoved :${sbn.packageName}
//
//     """.trimIndent()
//        )
//        sendBroadcast(i)
    }

    internal inner class NLServiceReceiver : BroadcastReceiver() {
        override fun onReceive(context: Context, intent: Intent) {
            Log.d("notification", "*********************************notification posted ")
            if (intent.getStringExtra("command") == "clearall") {
                cancelAllNotifications()
            } else if (intent.getStringExtra("command") == "list") {
                val i1 = Intent("com.example.sharex.NOTIFICATION_LISTENER_EXAMPLE")
                i1.putExtra("notification_event", "=====================")
                sendBroadcast(i1)
                var i = 1
                for (sbn in this@NLService.activeNotifications) {
                    val i2 = Intent("com.example.sharex.NOTIFICATION_LISTENER_EXAMPLE")
                    i2.putExtra(
                        "notification_event", """$i ${sbn.packageName}
"""
                    )
                    sendBroadcast(i2)
                    i++
                }
                val i3 = Intent("com.example.sharex.NOTIFICATION_LISTENER_EXAMPLE")
                i3.putExtra("notification_event", "===== Notification List ====")
                sendBroadcast(i3)
            }
        }
    }
}