// DllCppSort.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "DllCppSort.h"

void __stdcall MergeSort(int arr[], int size) {
	MergeSortHelper(arr, 0, size - 1);
}

void MergeSortHelper(int arr[], int left, int right) {
	if (left >= right){
		return;
	}
	int mid = (left + right) / 2;
	MergeSortHelper(arr, left, mid);
	MergeSortHelper(arr, mid + 1, right);
	Merge(arr, left, mid, right);
}

// 合并两个有序数组
void Merge(int src[], int left, int mid,int right) {
	int len = right - left + 1;
	// 辅助中间数组
	int* tmpArr = new int[len]();

	int p1 = left;
	int p2 = mid + 1;
	for (int i = 0; i < len; i++) {
		if (p1 > mid){
			tmpArr[i] = src[p2];
			p2++;
		} else if (p2 > right){
			// right part ends
			tmpArr[i] = src[p1];
			p1++;
		} else {
			if (src[p1] <= src[p2]){
				tmpArr[i] = src[p1];
				p1++;
			} else {
				tmpArr[i] = src[p2];
				p2++;
			}
		}
	}
		for(int i = 0; i < len; i++) {
			src[left + i] = tmpArr[i];
		}
}