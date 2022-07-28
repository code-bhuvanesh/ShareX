package com.example.sharex

import android.content.res.Resources
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import androidx.viewpager2.adapter.FragmentStateAdapter

class VPAdapter(activity: AppCompatActivity) : FragmentStateAdapter(activity) {
    override fun getItemCount(): Int {
        return  2
    }

    override fun createFragment(position: Int): Fragment {
        return  when(position){
            0 -> {SendFragment()}
            1 -> {ReceiveFragment()}
            else->{throw Resources.NotFoundException("position not found")}
        }
    }
}