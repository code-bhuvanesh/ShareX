package com.example.sharex.model

class FileData(var filename: String, var fileSize: String) {
    var completed: Double = 0.0
    fun update(persentage: Double)
    {
        completed = persentage
    }
}