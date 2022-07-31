package com.example.sharex.helpers

import android.content.res.Resources
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import androidx.viewpager2.adapter.FragmentStateAdapter
import com.example.sharex.fragments.ReceiveFragment
import com.example.sharex.fragments.SendFragment

class VPAdapter(activity: AppCompatActivity, u: Utils) : FragmentStateAdapter(activity) {

    val utils: Utils = u

    override fun getItemCount(): Int {
        return  2
    }

    override fun createFragment(position: Int): Fragment {
        return  when(position){
            0 -> {
                SendFragment(utils)
            }
            1 -> {
                ReceiveFragment(utils)
            }
            else->{throw Resources.NotFoundException("position not found")}
        }
    }
}