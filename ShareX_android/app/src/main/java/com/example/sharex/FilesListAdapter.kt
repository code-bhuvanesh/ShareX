package com.example.sharex

import android.content.Context
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.BaseAdapter
import android.widget.TextView
import com.example.sharex.model.FileData

class FilesListAdapter(private val context: Context, private  val filesList: ArrayList<FileData>) : BaseAdapter() {
    override fun getCount(): Int {
        return filesList.size
    }

    override fun getItem(position: Int): Any {
        return position
    }

    override fun getItemId(position: Int): Long {
        return position.toLong()
    }

    override fun getView(position: Int, convertView: View?, parent: ViewGroup?): View {

        var view = convertView
        if(convertView == null)
            view = LayoutInflater.from(context).inflate(R.layout.file_item_layout, parent, false)

        if(view != null)
        {
            val fileName = view.findViewById<TextView>(R.id.fileName)
            val fileSize = view.findViewById<TextView>(R.id.fileSize)
            val fileCompleted = view.findViewById<TextView>(R.id.fileCompleted)

            fileName.text = filesList[position].filename
            fileSize.text = filesList[position].fileSize
            fileCompleted.text = filesList[position].completed.toString() + " %"

        }
        return view!!
    }

    fun addFile(fileName:String, fileSize:String)
    {
        filesList.add(FileData(fileName,fileSize))
        notifyDataSetChanged()
    }

    fun updateStatus(pos:Int, completed: Double)
    {
        val file = filesList[pos]
        file.update(completed)
        notifyDataSetChanged()
    }
}