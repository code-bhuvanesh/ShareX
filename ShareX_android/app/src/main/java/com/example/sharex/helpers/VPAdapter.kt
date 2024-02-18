package com.example.sharex.helpers

import android.content.res.Resources
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import androidx.viewpager2.adapter.FragmentStateAdapter
import com.example.sharex.fragments.NotificationFragment
import com.example.sharex.fragments.ReceiveFragment
import com.example.sharex.fragments.SendFragment

class VPAdapter(activity: AppCompatActivity, socketHelper1: SocketHelper, socketHelper2: SocketHelper) : FragmentStateAdapter(activity) {

    val socket1: SocketHelper = socketHelper1
    val socket2: SocketHelper = socketHelper2

    override fun getItemCount(): Int {
        return  3
    }

    override fun createFragment(position: Int): Fragment {
        return  when(position){
            0->
            {
                NotificationFragment()
            }
            1 -> {
                SendFragment(socket1)
            }
            2 -> {
                ReceiveFragment(socket2)
            }
            else->{throw Resources.NotFoundException("position not found")}
        }
    }
}