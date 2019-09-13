#pragma once

extern "C" _declspec(dllexport) void __stdcall MergeSort(int arr[], int size);
void MergeSortHelper(int arr[], int left, int right);
void Merge(int src[], int left, int mid, int right);